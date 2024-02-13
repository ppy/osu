// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Timing;
using osu.Game.Screens.Edit;

namespace osu.Game.Screens.Play
{
    public class LatencyAssumptionClock : FramedOffsetClock
    {
        public new double Offset => base.Offset;

        public LatencyAssumptionClock(IClock source)
            : base(source)
        {
        }

        public override void ProcessFrame()
        {
            base.ProcessFrame();
            updateOffset();
        }

        private void updateOffset()
        {
            // The latency assumption baked into beatmaps should be adjusted by the difference in playback rate.
            // There should only need to be one clock that performs this calculation.
            base.Offset = Editor.WAVEFORM_VISUAL_OFFSET * (Rate - 1.0);
        }
    }
}
