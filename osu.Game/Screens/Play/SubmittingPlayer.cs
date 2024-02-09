// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Online.Solo;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A player instance which supports submitting scores to an online store.
    /// </summary>
    public abstract partial class SubmittingPlayer : Player
    {
        /// <summary>
        /// The token to be used for the current submission. This is fetched via a request created by <see cref="CreateTokenRequest"/>.
        /// </summary>
        private long? token;

        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private SpectatorClient spectatorClient { get; set; }

        [Resolved]
        private SessionStatics statics { get; set; }

        [Resolved(canBeNull: true)]
        [CanBeNull]
        private SoloStatisticsWatcher soloStatisticsWatcher { get; set; }

        private readonly object scoreSubmissionLock = new object();
        private TaskCompletionSource<bool> scoreSubmissionSource;

        protected SubmittingPlayer(PlayerConfiguration configuration = null)
            : base(configuration)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (DrawableRuleset == null)
            {
                // base load must have failed (e.g. due to an unknown mod); bail.
                return;
            }

            AddInternal(new PlayerTouchInputDetector());

            // We probably want to move this display to something more global.
            // Probably using the OSD somehow.
            AddInternal(new GameplayOffsetControl
            {
                Margin = new MarginPadding(20),
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
            });
        }

        protected override GameplayClockContainer CreateGameplayClockContainer(WorkingBeatmap beatmap, double gameplayStart) => new MasterGameplayClockContainer(beatmap, gameplayStart)
        {
            ShouldValidatePlaybackRate = true,
        };

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            handleTokenRetrieval();
        }

        private bool handleTokenRetrieval()
        {
            // Token request construction should happen post-load to allow derived classes to potentially prepare DI backings that are used to create the request.
            var tcs = new TaskCompletionSource<bool>();

            if (Mods.Value.Any(m => !m.UserPlayable))
            {
                handleTokenFailure(new InvalidOperationException("Non-user playable mod selected."));
                return false;
            }

            if (!api.IsLoggedIn)
            {
                handleTokenFailure(new InvalidOperationException("API is not online."));
                return false;
            }

            var req = CreateTokenRequest();

            if (req == null)
            {
                handleTokenFailure(new InvalidOperationException("Request could not be constructed."));
                return false;
            }

            req.Success += r =>
            {
                Logger.Log($"Score submission token retrieved ({r.ID})");
                token = r.ID;
                tcs.SetResult(true);
            };
            req.Failure += handleTokenFailure;

            api.Queue(req);

            // Generally a timeout would not happen here as APIAccess will timeout first.
            if (!tcs.Task.Wait(30000))
                req.TriggerFailure(new InvalidOperationException("Token retrieval timed out (request never run)"));

            return true;

            void handleTokenFailure(Exception exception)
            {
                tcs.SetResult(false);

                if (HandleTokenRetrievalFailure(exception))
                {
                    if (string.IsNullOrEmpty(exception.Message))
                        Logger.Error(exception, "Failed to retrieve a score submission token.");
                    else
                    {
                        switch (exception.Message)
                        {
                            case "expired token":
                                Logger.Log("Score submission failed because your system clock is set incorrectly. Please check your system time, date and timezone.", level: LogLevel.Important);
                                break;

                            default:
                                Logger.Log($"You are not able to submit a score: {exception.Message}", level: LogLevel.Important);
                                break;
                        }
                    }

                    Schedule(() =>
                    {
                        ValidForResume = false;
                        this.Exit();
                    });
                }
                else
                {
                    // Gameplay is allowed to continue, but we still should keep track of the error.
                    // In the future, this should be visible to the user in some way.
                    Logger.Log($"Score submission token retrieval failed ({exception.Message})");
                }
            }
        }

        /// <summary>
        /// Called when a token could not be retrieved for submission.
        /// </summary>
        /// <param name="exception">The error causing the failure.</param>
        /// <returns>Whether gameplay should be immediately exited as a result. Returning false allows the gameplay session to continue. Defaults to true.</returns>
        protected virtual bool HandleTokenRetrievalFailure(Exception exception) => true;

        protected override async Task PrepareScoreForResultsAsync(Score score)
        {
            await base.PrepareScoreForResultsAsync(score).ConfigureAwait(false);

            score.ScoreInfo.Date = DateTimeOffset.Now;

            await submitScore(score).ConfigureAwait(false);
            spectatorClient.EndPlaying(GameplayState);
            soloStatisticsWatcher?.RegisterForStatisticsUpdateAfter(score.ScoreInfo);
        }

        [Resolved]
        private RealmAccess realm { get; set; }

        protected override void StartGameplay()
        {
            base.StartGameplay();

            // User expectation is that last played should be updated when entering the gameplay loop
            // from multiplayer / playlists / solo.
            realm.WriteAsync(r =>
            {
                var realmBeatmap = r.Find<BeatmapInfo>(Beatmap.Value.BeatmapInfo.ID);
                if (realmBeatmap != null)
                    realmBeatmap.LastPlayed = DateTimeOffset.Now;
            });

            spectatorClient.BeginPlaying(token, GameplayState, Score);
        }

        protected override void OnFail()
        {
            base.OnFail();

            submitFromFailOrQuit();
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            bool exiting = base.OnExiting(e);
            submitFromFailOrQuit();
            statics.SetValue(Static.LastLocalUserScore, Score?.ScoreInfo.DeepClone());
            return exiting;
        }

        private void submitFromFailOrQuit()
        {
            if (LoadedBeatmapSuccessfully)
            {
                Task.Run(async () =>
                {
                    await submitScore(Score.DeepClone()).ConfigureAwait(false);
                    spectatorClient.EndPlaying(GameplayState);
                }).FireAndForget();
            }
        }

        /// <summary>
        /// Construct a request to be used for retrieval of the score token.
        /// Can return null, at which point <see cref="HandleTokenRetrievalFailure"/> will be fired.
        /// </summary>
        [CanBeNull]
        protected abstract APIRequest<APIScoreToken> CreateTokenRequest();

        /// <summary>
        /// Construct a request to submit the score.
        /// Will only be invoked if the request constructed via <see cref="CreateTokenRequest"/> was successful.
        /// </summary>
        /// <param name="score">The score to be submitted.</param>
        /// <param name="token">The submission token.</param>
        protected abstract APIRequest<MultiplayerScore> CreateSubmissionRequest(Score score, long token);

        private Task submitScore(Score score)
        {
            var masterClock = GameplayClockContainer as MasterGameplayClockContainer;

            if (masterClock?.PlaybackRateValid.Value != true)
            {
                Logger.Log("Score submission cancelled due to audio playback rate discrepancy.");
                return Task.CompletedTask;
            }

            // token may be null if the request failed but gameplay was still allowed (see HandleTokenRetrievalFailure).
            if (token == null)
            {
                Logger.Log("No token, skipping score submission");
                return Task.CompletedTask;
            }

            lock (scoreSubmissionLock)
            {
                if (scoreSubmissionSource != null)
                    return scoreSubmissionSource.Task;

                scoreSubmissionSource = new TaskCompletionSource<bool>();
            }

            // if the user never hit anything, this score should not be counted in any way.
            if (!score.ScoreInfo.Statistics.Any(s => s.Key.IsHit() && s.Value > 0))
                return Task.CompletedTask;

            Logger.Log($"Beginning score submission (token:{token.Value})...");
            var request = CreateSubmissionRequest(score, token.Value);

            request.Success += s =>
            {
                score.ScoreInfo.OnlineID = s.ID;
                score.ScoreInfo.Position = s.Position;

                scoreSubmissionSource.SetResult(true);
                Logger.Log($"Score submission completed! (token:{token.Value} id:{s.ID})");
            };

            request.Failure += e =>
            {
                Logger.Error(e, $"Failed to submit score (token:{token.Value}): {e.Message}");
                scoreSubmissionSource.SetResult(false);
            };

            api.Queue(request);
            return scoreSubmissionSource.Task;
        }
    }
}
