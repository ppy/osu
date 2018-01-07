// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public class GlobalScrollingHitObjectContainer : ScrollingHitObjectContainer
    {
        public GlobalScrollingHitObjectContainer(ScrollingDirection direction)
            : base(direction)
        {
        }

        protected override IScrollingAlgorithm CreateScrollingAlgorithm() => new GlobalScrollingAlgorithm(ControlPoints);
    }
}
