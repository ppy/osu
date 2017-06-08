// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.Timing.Drawables;

namespace osu.Game.Rulesets.Mania.Timing.Drawables
{
    public class DrawableManiaTimingSection : DrawableTimingSection
    {
        private readonly ScrollingAlgorithm scrollingAlgorithm;

        public DrawableManiaTimingSection(TimingSection timingSection, ScrollingAlgorithm scrollingAlgorithm)
            : base(timingSection, Axes.Y)
        {
            this.scrollingAlgorithm = scrollingAlgorithm;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var parent = Parent as TimingSectionCollection;

            if (parent == null)
                return;

            // This is very naive and can be improved, but is adequate for now
            LifetimeStart = TimingSection.Time - parent.TimeSpan;
            LifetimeEnd = TimingSection.Time + Content.Height * 2;
        }

        protected override HitObjectCollection CreateHitObjectCollection()
        {
            switch (scrollingAlgorithm)
            {
                default:
                case ScrollingAlgorithm.Basic:
                    return new BasicScrollingHitObjectCollection(TimingSection);
                case ScrollingAlgorithm.Gravity:
                    return new GravityScrollingHitObjectCollection(TimingSection, () => RelativeChildSize.Y);
            }
        }
    }
}