// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Rulesets.Mania.Timing
{
    public class DrawableTimingSection : Container
    {
        private readonly TimingSection section;

        public DrawableTimingSection(TimingSection section)
        {
            this.section = section;

            RelativePositionAxes = Axes.Y;
            Y = -(float)section.StartTime;

            RelativeSizeAxes = Axes.Both;
            Height = (float)section.Duration;

            RelativeCoordinateSpace = new Vector2(1, Height);
        }
    }
}