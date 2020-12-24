// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking;
using osuTK;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    // Todo: The "room" part of TimeshiftPlayer should be split out into an abstract player class to be inherited instead.
    public class RealtimePlayer : TimeshiftPlayer
    {
        protected override bool PauseOnFocusLost => false;

        // Disallow fails in multiplayer for now.
        protected override bool CheckModsAllowFailure() => false;

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        private IBindable<bool> isConnected;

        private readonly TaskCompletionSource<bool> resultsReady = new TaskCompletionSource<bool>();
        private readonly ManualResetEventSlim startedEvent = new ManualResetEventSlim();

        [CanBeNull]
        private MultiplayerGameplayLeaderboard leaderboard;

        private readonly int[] userIds;

        /// <summary>
        /// Construct a multiplayer player.
        /// </summary>
        /// <param name="playlistItem">The playlist item to be played.</param>
        /// <param name="userIds">The users which are participating in this game.</param>
        public RealtimePlayer(PlaylistItem playlistItem, int[] userIds)
            : base(playlistItem, new PlayerConfiguration
            {
                AllowPause = false,
                AllowRestart = false,
                AllowSkippingIntro = false,
            })
        {
            this.userIds = userIds;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (Token == null)
                return; // Todo: Somehow handle token retrieval failure.

            client.MatchStarted += onMatchStarted;
            client.ResultsReady += onResultsReady;

            isConnected = client.IsConnected.GetBoundCopy();
            isConnected.BindValueChanged(connected =>
            {
                if (!connected.NewValue)
                {
                    // messaging to the user about this disconnect will be provided by the RealtimeMatchSubScreen.
                    failAndBail();
                }
            }, true);

            client.ChangeState(MultiplayerUserState.Loaded)
                  .ContinueWith(task => failAndBail(task.Exception?.Message ?? "Server error"), TaskContinuationOptions.NotOnRanToCompletion);

            if (!startedEvent.Wait(TimeSpan.FromSeconds(30)))
            {
                failAndBail("Failed to start the multiplayer match in time.");
                return;
            }

            Debug.Assert(client.Room != null);

            // todo: this should be implemented via a custom HUD implementation, and correctly masked to the main content area.
            LoadComponentAsync(leaderboard = new MultiplayerGameplayLeaderboard(ScoreProcessor, userIds), HUDOverlay.Add);
        }

        private void failAndBail(string message = null)
        {
            if (!string.IsNullOrEmpty(message))
                Logger.Log(message, LoggingTarget.Runtime, LogLevel.Important);

            startedEvent.Set();
            Schedule(() => PerformExit(false));
        }

        protected override void Update()
        {
            base.Update();
            adjustLeaderboardPosition();
        }

        private void adjustLeaderboardPosition()
        {
            if (leaderboard == null)
                return;

            const float padding = 44; // enough margin to avoid the hit error display.

            leaderboard.Position = new Vector2(
                padding,
                padding + HUDOverlay.TopScoringElementsHeight);
        }

        private void onMatchStarted() => startedEvent.Set();

        private void onResultsReady() => resultsReady.SetResult(true);

        protected override async Task SubmitScore(Score score)
        {
            await base.SubmitScore(score);

            await client.ChangeState(MultiplayerUserState.FinishedPlay);

            // Await up to 30 seconds for results to become available (3 api request timeouts).
            // This is arbitrary just to not leave the player in an essentially deadlocked state if any connection issues occur.
            await Task.WhenAny(resultsReady.Task, Task.Delay(TimeSpan.FromSeconds(30)));
        }

        protected override ResultsScreen CreateResults(ScoreInfo score)
        {
            Debug.Assert(RoomId.Value != null);
            return new RealtimeResultsScreen(score, RoomId.Value.Value, PlaylistItem);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (client != null)
            {
                client.MatchStarted -= onMatchStarted;
                client.ResultsReady -= onResultsReady;
            }
        }
    }
}
