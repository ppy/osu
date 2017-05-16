// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Rulesets.Mania.Timing
{
    /// <summary>
    /// A container in which the Y-relative coordinate space is spanned by a length of time.
    /// <para>
    /// This container contains <see cref="ControlPoint"/>s which scroll inside this container.
    /// Drawables added to this container are moved inside the relevant <see cref="ControlPoint"/>,
    /// and as such, will scroll along with the <see cref="ControlPoint"/>s.
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

        public TimeRelativeContainer(IEnumerable<ControlPoint> timingChanges)
        {
            drawableTimingSections = timingChanges.Select(t => new DrawableTimingSection(t)).ToList();

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
            private readonly ControlPoint timingChange;

            protected override Container<Drawable> Content => content;
            private readonly Container content;

            /// <summary>
            /// Creates a drawable timing section. The height of this container will be proportional
            /// to the beat length of the timing section and the timespan of its parent at all times.
            /// <para>
            /// This is so that, e.g. a beat length of 500ms results in this container being twice as high as its parent,
            /// which means that the content container will scroll at twice the normal rate.
            /// </para>
            /// </summary>
            /// <param name="timingChange">The timing change to create the drawable timing section for.</param>
            public DrawableTimingSection(ControlPoint timingChange)
            {
                this.timingChange = timingChange;

                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;

                RelativeSizeAxes = Axes.Both;

                AddInternal(content = new AutoTimeRelativeContainer
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Y = -(float)timingChange.Time
                });
            }

            protected override void Update()
            {
                var parent = (TimeRelativeContainer)Parent;

                // Adjust our height to account for the speed changes
                Height = (float)(parent.TimeSpan * 1000 / timingChange.BeatLength / timingChange.SpeedMultiplier);
                RelativeCoordinateSpace = new Vector2(1, (float)parent.TimeSpan);

                // Scroll the content
                content.Y = (float)(Time.Current - timingChange.Time);
            }

            public override void Add(Drawable drawable)
            {
                // The previously relatively-positioned drawable will now become relative to us, but since the drawable has no knowledge of us,
                // we need to offset it back by our position so that it becomes correctly relatively-positioned to us
                // This can be removed if hit objects were stored such that either their StartTime or their "beat offset" was relative to the timing section
                // they belonged to, but this requires a radical change to the beatmap format which we're not ready to do just yet
                drawable.Y += (float)timingChange.Time;

                base.Add(drawable);
            }

            /// <summary>
            /// Whether this timing section can contain a drawable. A timing section can contain a drawable if the drawable
            /// can be placed within the timing section's bounds (in this case, from the start of the timing section up to infinity).
            /// </summary>
            /// <param name="drawable">The drawable to check.</param>
            public bool CanContain(Drawable drawable) => content.Y >= drawable.Y;

            private class AutoTimeRelativeContainer : Container
            {
                public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
                {
                    float height = 0;

                    foreach (Drawable child in Children)
                    {
                        // Todo: This is wrong, it won't work for absolute-y-sized children
                        float childEndPos = -child.Y + child.Height;
                        if (childEndPos > height)
                            height = childEndPos;
                    }

                    Height = height;
                    RelativeCoordinateSpace = new Vector2(1, height);

                    return base.Invalidate(invalidation, source, shallPropagate);
                }
            }
        }
    }
}