// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.Timing
{
    public class ManiaSpeedAdjustmentContainer : SpeedAdjustmentContainer
    {
        private readonly ScrollingAlgorithm scrollingAlgorithm;

        public ManiaSpeedAdjustmentContainer(MultiplierControlPoint timingSection, ScrollingAlgorithm scrollingAlgorithm)
            : base(timingSection, Axes.Y)
        {
            this.scrollingAlgorithm = scrollingAlgorithm;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            var parent = Parent as SpeedAdjustmentCollection;

            if (parent == null)
                return;

            // This is very naive and can be improved, but is adequate for now
            LifetimeStart = ControlPoint.StartTime - VisibleTimeRange;
            LifetimeEnd = ControlPoint.StartTime + Content.Height * 2;
        }

        protected override DrawableTimingSection CreateTimingSection()
        {
            switch (scrollingAlgorithm)
            {
                default:
                case ScrollingAlgorithm.Basic:
                    return new BasicScrollingDrawableTimingSection(ControlPoint);
                case ScrollingAlgorithm.Gravity:
                    return new GravityScrollingDrawableTimingSection(ControlPoint);
            }
        }
    }
}