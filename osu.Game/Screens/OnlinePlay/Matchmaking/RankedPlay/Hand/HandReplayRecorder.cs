// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RankedPlay;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand
{
    public partial class HandReplayRecorder : Component
    {
        /// <summary>
        /// Interval at which buffered frames get collected and emitted
        /// </summary>
        public double FlushInterval { get; init; } = 1000;

        /// <summary>
        /// Minimum interval between individual replay frames
        /// </summary>
        public double RecordInterval { get; init; } = 25;

        /// <summary>
        /// Max amount of frames to collect per <see cref="FlushInterval"/>
        /// </summary>
        public int MaxBufferSize = 20;

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private readonly PlayerHandOfCards handOfCards;

        private readonly List<RankedPlayCardHandReplayFrame> buffer = new List<RankedPlayCardHandReplayFrame>();
        private bool hasChanges;
        private double? lastFrameTime;

        public HandReplayRecorder(PlayerHandOfCards handOfCards)
        {
            this.handOfCards = handOfCards;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Scheduler.AddDelayed(recordFrame, RecordInterval, true);
            Scheduler.AddDelayed(tryFlush, FlushInterval, true);

            handOfCards.StateChanged += onHandOfCardsStateChanged;
        }

        private void onHandOfCardsStateChanged() => hasChanges = true;

        private void recordFrame()
        {
            if (!hasChanges || buffer.Count >= MaxBufferSize)
                return;

            double delay = lastFrameTime != null ? Time.Current - lastFrameTime.Value : 0;

            buffer.Add(new RankedPlayCardHandReplayFrame
            {
                Delay = delay,
                Cards = handOfCards.State,
            });

            lastFrameTime = Time.Current;
            hasChanges = false;
        }

        private void tryFlush()
        {
            if (buffer.Count == 0)
                return;

            var frames = compress(buffer).ToArray();
            buffer.Clear();

            if (frames.Length > 0)
                Flush(frames);
        }

        /// <summary>
        /// Compresses a list of <see cref="RankedPlayCardHandReplayFrame"/>s by only keeping values that have changed between each frame
        /// </summary>
        private IEnumerable<RankedPlayCardHandReplayFrame> compress(IReadOnlyList<RankedPlayCardHandReplayFrame> frames)
        {
            if (frames.Count == 0)
                yield break;

            // The first frame always contains the full state since the replay player may drop frames starting from the end for each message.
            yield return frames[0];

            var lastFrame = frames[0];

            foreach (var frame in frames.Skip(1))
            {
                yield return frame.RelativeTo(lastFrame);

                lastFrame = frame;
            }
        }

        protected virtual void Flush(RankedPlayCardHandReplayFrame[] frames)
        {
            if (frames.Length == 0)
                return;

            client.SendMatchRequest(new RankedPlayCardHandReplayRequest
            {
                Frames = frames,
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            handOfCards.StateChanged -= onHandOfCardsStateChanged;

            base.Dispose(isDisposing);
        }
    }
}
