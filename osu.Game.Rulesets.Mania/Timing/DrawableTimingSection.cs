// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;


namespace osu.Game.Rulesets.Mania.Timing
{
    /// <summary>
    /// A container that contains hit objects within the time span of a timing section.
    /// </summary>
    public class DrawableTimingSection : Container
    {
        public readonly TimingSection TimingSection;

        public DrawableTimingSection(TimingSection section)
        {
            TimingSection = section;

            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;

            RelativePositionAxes = Axes.Y;
            Y = -(float)section.StartTime;

            RelativeSizeAxes = Axes.Both;
            Height = (float)section.Duration;

            RelativeCoordinateSpace = new Vector2(1, Height);
        }

        protected override void Update()
        {
            Y = (float)(Time.Current - TimingSection.StartTime);
        }
    }
}