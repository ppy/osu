// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Scoring;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    public class RealtimePlayer : TimeshiftPlayer
    {
        protected override bool PauseOnFocusLost => false;

        // Disallow fails in multiplayer for now.
        protected override bool CheckModsAllowFailure() => false;

        [Resolved]
        private StatefulMultiplayerClient client { get; set; }

        private readonly TaskCompletionSource<bool> resultsReady = new TaskCompletionSource<bool>();
        private bool started;

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

            while (!started)
                Thread.Sleep(100);
        }

        private void onMatchStarted() => started = true;

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
