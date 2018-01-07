// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A <see cref="ScrollingContainer"/> which scrolls linearly relative to the <see cref="MultiplierControlPoint"/> start time.
    /// </summary>
    public class LinearScrollingContainer : ScrollingContainer
    {
        private readonly MultiplierControlPoint controlPoint;

        public LinearScrollingContainer(MultiplierControlPoint controlPoint)
        {
            this.controlPoint = controlPoint;
        }

        protected override void Update()
        {
            base.Update();

            if ((ScrollingAxes & Axes.X) > 0) X = (float)(controlPoint.StartTime - Time.Current);
            if ((ScrollingAxes & Axes.Y) > 0) Y = (float)(controlPoint.StartTime - Time.Current);
        }
    }
}
