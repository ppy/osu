// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.Timing.Drawables;

namespace osu.Game.Rulesets.Mania.Timing
{
    internal class BasicScrollingDrawableTimingSection : DrawableTimingSection
    {
        private readonly SpeedAdjustment timingSection;

        public BasicScrollingDrawableTimingSection(SpeedAdjustment timingSection)
            : base(Axes.Y)
        {
            this.timingSection = timingSection;
        }

        protected override void Update()
        {
            base.Update();

            Y = (float)(timingSection.Time - Time.Current);
        }
    }
}
