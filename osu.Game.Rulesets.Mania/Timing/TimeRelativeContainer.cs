// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Timing
{
    /// <summary>
    /// A container in which the Y-relative coordinate space is spanned by a length of time.
    /// <para>
    /// This container contains <see cref="TimingSection"/>s which scroll inside this container.
    /// Drawables added to this container are moved inside the relevant <see cref="TimingSection"/>,
    /// and as such, will scroll along with the <see cref="TimingSection"/>s.
    /// </para>
    /// </summary>
    public class TimeRelativeContainer : Container<Drawable>
    {
        /// <summary>
        /// The amount of time which the height of this container spans.
        /// </summary>
        public double TimeSpan
        {
            get { return RelativeCoordinateSpace.Y; }
            set { RelativeCoordinateSpace = new Vector2(1, (float)value); }
        }

        public TimeRelativeContainer(IEnumerable<TimingSection> timingSections)
        {
            Children = timingSections.Select(t => new DrawableTimingSection(t));
        }

        /// <summary>
        /// Adds a drawable to this container. Note that the drawable added must have a
        /// Y-position as a time relative to this container.
        /// </summary>
        /// <param name="drawable">The drawable to add.</param>
        public override void Add(Drawable drawable)
        {
            // Always add timing sections to ourselves
            if (drawable is DrawableTimingSection)
            {
                base.Add(drawable);
                return;
            }

            var section = (Children.LastOrDefault(t => t.Y >= drawable.Y) ?? Children.First()) as DrawableTimingSection;

            if (section == null)
                throw new Exception("Could not find suitable timing section to add object to.");

            section.Add(drawable);
        }

        /// <summary>
        /// A container that contains drawables within the time span of a timing section.
        /// <para>
        /// Scrolls relative to the current time.
        /// </para>
        /// </summary>
        private class DrawableTimingSection : Container
        {
            private readonly TimingSection section;

            public DrawableTimingSection(TimingSection section)
            {
                this.section = section;

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
                Y = (float)(Time.Current - section.StartTime);
            }

            public override void Add(Drawable drawable)
            {
                // The previously relatively-positioned drawable will now become relative to us, but since the drawable has no knowledge of us,
                // we need to offset it back by our position so that it becomes correctly relatively-positioned to us
                // This can be removed if hit objects were stored such that either their StartTime or their "beat offset" was relative to the timing section
                // they belonged to, but this requires a radical change to the beatmap format which we're not ready to do just yet
                drawable.Y -= Y;

                base.Add(drawable);
            }
        }
    }
}