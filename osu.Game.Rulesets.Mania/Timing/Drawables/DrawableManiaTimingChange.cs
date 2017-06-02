// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public class DrawableManiaTimingChange : DrawableTimingChange
    {
        public DrawableManiaTimingChange(TimingChange timingChange)
            : base(timingChange, Axes.Y)
        {
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var parent = Parent as IHasTimeSpan;

            if (parent == null)
                return;

            LifetimeStart = TimingChange.Time - parent.TimeSpan.Y;
            LifetimeEnd = TimingChange.Time + Content.RelativeCoordinateSpace.Y * 2;
        }
    }
}