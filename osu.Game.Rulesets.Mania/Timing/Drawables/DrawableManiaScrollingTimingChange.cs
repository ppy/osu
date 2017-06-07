// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    /// <summary>
    /// A basic timing change which scrolls along with a timing change.
    /// </summary>
    public class DrawableManiaScrollingTimingChange : DrawableManiaTimingChange
    {
        public DrawableManiaScrollingTimingChange(TimingChange timingChange)
            : base(timingChange)
        {
        }

        protected override void Update()
        {
            base.Update();

            Content.Y = (float)(TimingChange.Time - Time.Current);
        }
    }
}