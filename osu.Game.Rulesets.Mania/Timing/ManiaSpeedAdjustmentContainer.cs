// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.Timing
{
    public class ManiaSpeedAdjustmentContainer : SpeedAdjustmentContainer
    {
        private readonly ScrollingAlgorithm scrollingAlgorithm;

        public ManiaSpeedAdjustmentContainer(MultiplierControlPoint timingSection, ScrollingAlgorithm scrollingAlgorithm)
            : base(timingSection)
        {
            this.scrollingAlgorithm = scrollingAlgorithm;
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