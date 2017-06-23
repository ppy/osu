// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.Mania.Timing
{
    /// <summary>
    /// A <see cref="DrawableTimingSection"/> which scrolls relative to the control point start time.
    /// </summary>
    internal class BasicScrollingDrawableTimingSection : DrawableTimingSection
    {
        private readonly MultiplierControlPoint controlPoint;

        public BasicScrollingDrawableTimingSection(MultiplierControlPoint controlPoint)
        {
            this.controlPoint = controlPoint;
        }

        protected override void Update()
        {
            base.Update();

            Y = (float)(controlPoint.StartTime - Time.Current);
        }
    }
}
