// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Timing;

namespace osu.Game.Screens.Play
{
    public class OffsetCorrectionClock : FramedOffsetClock
    {
        private readonly BindableDouble pauseRateAdjust;

        private double offset;

        public new double Offset
        {
            get => offset;
            set
            {
                if (value == offset)
                    return;

                offset = value;

                updateOffset();
            }
        }

        public double RateAdjustedOffset => base.Offset;

        public OffsetCorrectionClock(IClock source, BindableDouble pauseRateAdjust)
            : base(source)
        {
            this.pauseRateAdjust = pauseRateAdjust;
        }

        public override void ProcessFrame()
        {
            base.ProcessFrame();
            updateOffset();
        }

        private void updateOffset()
        {
            // changing this during the pause transform effect will cause a potentially large offset to be suddenly applied as we approach zero rate.
            if (pauseRateAdjust.Value == 1)
            {
                // we always want to apply the same real-time offset, so it should be adjusted by the difference in playback rate (from realtime) to achieve this.
                base.Offset = Offset * Rate;
            }
        }
    }
}
