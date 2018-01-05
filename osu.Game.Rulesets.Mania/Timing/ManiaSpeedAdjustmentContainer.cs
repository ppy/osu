// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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

        protected override ScrollingContainer CreateScrollingContainer()
        {
            switch (scrollingAlgorithm)
            {
                default:
                    return base.CreateScrollingContainer();
                case ScrollingAlgorithm.Gravity:
                    return new GravityScrollingContainer(ControlPoint);
            }
        }
    }
}
