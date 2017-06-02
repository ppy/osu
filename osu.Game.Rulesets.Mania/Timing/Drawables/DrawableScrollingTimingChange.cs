// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public class DrawableScrollingTimingChange : DrawableTimingChange
    {
        public DrawableScrollingTimingChange(TimingChange timingChange)
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