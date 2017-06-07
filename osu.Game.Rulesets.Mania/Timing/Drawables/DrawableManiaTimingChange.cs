// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.Timing.Drawables;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public abstract class DrawableManiaTimingChange : DrawableTimingChange
    {
        protected DrawableManiaTimingChange(TimingChange timingChange)
            : base(timingChange, Axes.Y)
        {
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var parent = Parent as IHasTimeSpan;

            if (parent == null)
                return;

            // This is very naive and can be improved, but is adequate for now
            LifetimeStart = TimingChange.Time - parent.TimeSpan;
            LifetimeEnd = TimingChange.Time + Content.RelativeChildSize.Y * 2;
        }
    }
}