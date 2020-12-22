// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Ranking;

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

        private readonly TaskCompletionSource<bool> resultsReady = new TaskCompletionSource<bool>();
        private readonly ManualResetEventSlim startedEvent = new ManualResetEventSlim();

        public RealtimePlayer(PlaylistItem playlistItem)
            : base(playlistItem, false)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            if (Token == null)
                return; // Todo: Somehow handle token retrieval failure.

            client.MatchStarted += onMatchStarted;
            client.ResultsReady += onResultsReady;
            client.ChangeState(MultiplayerUserState.Loaded);

            if (!startedEvent.Wait(TimeSpan.FromSeconds(30)))
            {
                Logger.Log("Failed to start the multiplayer match in time.", LoggingTarget.Runtime, LogLevel.Important);

                Schedule(() =>
                {
                    ValidForResume = false;
                    this.Exit();
                });
            }
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
