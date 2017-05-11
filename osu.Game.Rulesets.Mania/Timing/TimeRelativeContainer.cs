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

        private readonly List<DrawableTimingSection> drawableTimingSections;

        public TimeRelativeContainer(IEnumerable<TimingSection> timingSections)
        {
            drawableTimingSections = timingSections.Select(t => new DrawableTimingSection(t)).ToList();

            Children = drawableTimingSections;
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

            var section = drawableTimingSections.LastOrDefault(t => t.CanContain(drawable)) ?? drawableTimingSections.FirstOrDefault();

            if (section == null)
                throw new Exception("Could not find suitable timing section to add object to.");

            section.Add(drawable);
        }

        /// <summary>
        /// A container that contains drawables within the time span of a timing section.
        /// <para>
        /// The content of this container will scroll relative to the current time.
        /// </para>
        /// </summary>
        private class DrawableTimingSection : Container
        {
            protected override Container<Drawable> Content => content;
            /// <summary>
            /// The container which will scroll relative to the current time.
            /// </summary>
            private readonly Container content;

            private readonly TimingSection section;

            /// <summary>
            /// Creates a drawable timing section. The height of this container will be proportional
            /// to the beat length of the timing section and the timespan of its parent at all times.
            /// <para>
            /// This is so that, e.g. a beat length of 500ms results in this container being twice as high as its parent,
            /// which means that the content container will scroll at twice the normal rate.
            /// </para>
            /// </summary>
            /// <param name="section">The section to create the drawable timing section for.</param>
            public DrawableTimingSection(TimingSection section)
            {
                this.section = section;

                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;

                RelativeSizeAxes = Axes.Both;

                AddInternal(content = new Container
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativePositionAxes = Axes.Both,
                    RelativeSizeAxes = Axes.Both,
                    Y = -(float)section.StartTime,
                    Height = (float)section.Duration,
                    RelativeCoordinateSpace = new Vector2(1, (float)section.Duration)
                });
            }

            protected override void Update()
            {
                var parent = (TimeRelativeContainer)Parent;

                // Adjust our height to account for the speed changes
                Height = (float)(parent.TimeSpan * 1000 / section.BeatLength);
                RelativeCoordinateSpace = new Vector2(1, (float)parent.TimeSpan);

                // Scroll the content
                content.Y = (float)(Time.Current - section.StartTime);
            }

            public override void Add(Drawable drawable)
            {
                // The previously relatively-positioned drawable will now become relative to us, but since the drawable has no knowledge of us,
                // we need to offset it back by our position so that it becomes correctly relatively-positioned to us
                // This can be removed if hit objects were stored such that either their StartTime or their "beat offset" was relative to the timing section
                // they belonged to, but this requires a radical change to the beatmap format which we're not ready to do just yet
                drawable.Y += (float)section.StartTime;

                base.Add(drawable);
            }

            public bool CanContain(Drawable drawable) => content.Y >= drawable.Y;
        }
    }
}