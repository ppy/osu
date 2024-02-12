// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Timing;
using osu.Game.Screens.Edit;

namespace osu.Game.Screens.Play
{
    public class OffsetCorrectionClock : FramedOffsetClock
    {
        private double offset;

        public new double Offset
        {
            get => offset;
            set
            {
                if (value == offset)
                    return;

                offset = value;

                UpdateOffset();
            }
        }

        public double RateAdjustedOffset => base.Offset;

        public OffsetCorrectionClock(IClock source)
            : base(source)
        {
        }

        public override void ProcessFrame()
        {
            base.ProcessFrame();
            UpdateOffset();
        }

        protected virtual void UpdateOffset()
        {
            // we always want to apply the same real-time offset, so it should be adjusted by the difference in playback rate (from realtime) to achieve this.
            base.Offset = Offset * Rate;
        }

        protected void UpdateLatencyAssumption()
        {
            // The latency assumption baked into beatmaps should also be adjusted by the difference in playback rate.
            // There should only be one clock that does this.
            base.Offset += Editor.WAVEFORM_VISUAL_OFFSET * (Rate - 1.0);
        }
    }

    public class LatencyAssumptionClock : OffsetCorrectionClock
    {
        public LatencyAssumptionClock(IClock source)
            : base(source)
        {
        }

        protected override void UpdateOffset()
        {
            base.UpdateOffset();
            UpdateLatencyAssumption();
        }
    }
}
