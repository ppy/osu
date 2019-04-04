// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics.Textures;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;

namespace osu.Game.Tests.Beatmaps
{
    public class TestWorkingBeatmap : WorkingBeatmap
    {
        private readonly TrackVirtualManual track;
        private readonly IBeatmap beatmap;

        /// <summary>
        /// Create an instance which creates a <see cref="TestBeatmap"/> for the provided ruleset when requested.
        /// </summary>
        /// <param name="ruleset">The target ruleset.</param>
        /// <param name="referenceClock">A clock which should be used instead of a stopwatch for virtual time progression.</param>
        public TestWorkingBeatmap(RulesetInfo ruleset, IFrameBasedClock referenceClock)
            : this(new TestBeatmap(ruleset), referenceClock)
        {
        }

        /// <summary>
        /// Create an instance which provides the <see cref="IBeatmap"/> when requested.
        /// </summary>
        /// <param name="beatmap">The beatmap</param>
        /// <param name="referenceClock">An optional clock which should be used instead of a stopwatch for virtual time progression.</param>
        public TestWorkingBeatmap(IBeatmap beatmap, IFrameBasedClock referenceClock = null)
            : base(beatmap.BeatmapInfo)
        {
            this.beatmap = beatmap;

            if (referenceClock != null)
                track = new TrackVirtualManual(referenceClock);
        }

        protected override IBeatmap GetBeatmap() => beatmap;
        protected override Texture GetBackground() => null;
        protected override Track GetTrack() => track;

        /// <summary>
        /// A virtual track which tracks a reference clock.
        /// </summary>
        public class TrackVirtualManual : Track
        {
            private readonly IFrameBasedClock referenceClock;

            private readonly ManualClock clock = new ManualClock();

            private bool running;

            /// <summary>
            /// Local offset added to the reference clock to resolve correct time.
            /// </summary>
            private double offset;

            public TrackVirtualManual(IFrameBasedClock referenceClock)
            {
                this.referenceClock = referenceClock;
                Length = double.PositiveInfinity;
            }

            public override bool Seek(double seek)
            {
                offset = Math.Min(seek, Length);
                lastReferenceTime = null;
                return true;
            }

            public override void Start()
            {
                running = true;
            }

            public override void Reset()
            {
                Seek(0);
                base.Reset();
            }

            public override void Stop()
            {
                if (running)
                {
                    running = false;
                    // on stopping, the current value should be transferred out of the clock, as we can no longer rely on
                    // the referenceClock (which will still be counting time).
                    offset = clock.CurrentTime;
                    lastReferenceTime = null;
                }
            }

            public override bool IsRunning => running;

            private double? lastReferenceTime;

            public override double CurrentTime => clock.CurrentTime;

            protected override void UpdateState()
            {
                base.UpdateState();

                if (running)
                {
                    double refTime = referenceClock.CurrentTime;

                    if (!lastReferenceTime.HasValue)
                    {
                        // if the clock just started running, the current value should be transferred to the offset
                        // (to zero the progression of time).
                        offset -= refTime;
                    }

                    lastReferenceTime = refTime;
                }

                clock.CurrentTime = Math.Min((lastReferenceTime ?? 0) + offset, Length);

                if (CurrentTime >= Length)
                {
                    Stop();
                    RaiseCompleted();
                }
            }
        }
    }
}
