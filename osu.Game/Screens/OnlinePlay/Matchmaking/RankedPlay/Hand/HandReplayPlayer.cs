// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RankedPlay;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand
{
    public partial class HandReplayPlayer : Component
    {
        /// <summary>
        /// Maximum amount of frames that can get queued up at the same time
        /// </summary>
        public int MaxQueuedFrames { get; set; } = 20;

        private readonly int userId;
        private readonly OpponentHandOfCards handOfCards;

        private int queuedFrames;
        private double? lastPlayback;

        public HandReplayPlayer(int userId, OpponentHandOfCards handOfCards)
        {
            this.userId = userId;
            this.handOfCards = handOfCards;
        }

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            client.MatchEvent += onMatchEvent;
        }

        private void onMatchEvent(MatchServerEvent e)
        {
            if (e is not RankedPlayCardHandReplayEvent replayEvent || replayEvent.UserId != userId)
                return;

            foreach (var frame in replayEvent.Frames)
            {
                if (queuedFrames >= MaxQueuedFrames)
                    return;

                queuedFrames++;

                double delay = Math.Max(lastPlayback != null ? lastPlayback.Value + frame.Delay - Time.Current : 0, 0);
                lastPlayback = Time.Current + delay;

                Scheduler.AddDelayed(() =>
                {
                    queuedFrames--;

                    handOfCards.SetState(frame.Cards);
                }, delay);
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            client.MatchEvent -= onMatchEvent;

            base.Dispose(isDisposing);
        }
    }
}
