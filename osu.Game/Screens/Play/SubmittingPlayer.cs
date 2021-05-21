// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Screens.Play
{
    /// <summary>
    /// A player instance which supports submitting scores to an online store.
    /// </summary>
    public abstract class SubmittingPlayer : Player
    {
        /// <summary>
        /// The token to be used for the current submission. This is fetched via a request created by <see cref="CreateTokenRequest"/>.
        /// </summary>
        private long? token;

        [Resolved]
        private IAPIProvider api { get; set; }

        protected SubmittingPlayer(PlayerConfiguration configuration = null)
            : base(configuration)
        {
        }

        protected override void LoadAsyncComplete()
        {
            base.LoadAsyncComplete();
            handleTokenRetrieval();
        }

        private bool handleTokenRetrieval()
        {
            // Token request construction should happen post-load to allow derived classes to potentially prepare DI backings that are used to create the request.
            var tcs = new TaskCompletionSource<bool>();

            if (Mods.Value.Any(m => m is ModAutoplay))
            {
                handleTokenFailure(new InvalidOperationException("Autoplay loaded."));
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
                token = r.ID;
                tcs.SetResult(true);
            };
            req.Failure += handleTokenFailure;

            api.Queue(req);

            tcs.Task.Wait();
            return true;

            void handleTokenFailure(Exception exception)
            {
                if (HandleTokenRetrievalFailure(exception))
                {
                    if (string.IsNullOrEmpty(exception.Message))
                        Logger.Error(exception, "Failed to retrieve a score submission token.");
                    else
                        Logger.Log($"You are not able to submit a score: {exception.Message}", level: LogLevel.Important);

                    Schedule(() =>
                    {
                        ValidForResume = false;
                        this.Exit();
                    });
                }

                tcs.SetResult(false);
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

            // token may be null if the request failed but gameplay was still allowed (see HandleTokenRetrievalFailure).
            if (token == null)
                return;

            var tcs = new TaskCompletionSource<bool>();
            var request = CreateSubmissionRequest(score, token.Value);

            request.Success += s =>
            {
                score.ScoreInfo.OnlineScoreID = s.ID;
                tcs.SetResult(true);
            };

            request.Failure += e =>
            {
                Logger.Error(e, "Failed to submit score");
                tcs.SetResult(false);
            };

            api.Queue(request);
            await tcs.Task.ConfigureAwait(false);
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
    }
}
