// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Timing;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.UI
{
    public class TimingSectionContainer : Container<DrawableTimingSection>
    {
        /// <summary>
        /// The amount of time which the length of this container spans.
        /// </summary>
        public double TimeSpan
        {
            get { return RelativeCoordinateSpace.Y; }
            set { RelativeCoordinateSpace = new Vector2(1, (float)value); }
        }

        public TimingSectionContainer(IEnumerable<TimingSection> timingSections)
        {
            Children = timingSections.Select(t => new DrawableTimingSection(t));
        }

        public void Add(Drawable drawable)
        {
            var section = Children.LastOrDefault(t => t.TimingSection.StartTime <= drawable.Y) ?? Children.First();
            drawable.Y -= (float)section.TimingSection.StartTime;
            section.Add(drawable);
        }
    }
}