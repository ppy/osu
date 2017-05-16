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
    /// A container in which added drawables are put into a relative coordinate space spanned by a length of time.
    /// <para>
    /// This container contains <see cref="ControlPoint"/>s which scroll inside this container.
    /// Drawables added to this container are moved inside the relevant <see cref="ControlPoint"/>,
    /// and as such, will scroll along with the <see cref="ControlPoint"/>s.
    /// </para>
    /// </summary>
    public class ControlPointContainer : Container<Drawable>
    {
        /// <summary>
        /// The amount of time which this container spans.
        /// </summary>
        public double TimeSpan { get; set; }

        private readonly List<DrawableControlPoint> drawableControlPoints;

        public ControlPointContainer(IEnumerable<ControlPoint> timingChanges)
        {
            drawableControlPoints = timingChanges.Select(t => new DrawableControlPoint(t)).ToList();
            Children = drawableControlPoints;
        }

        /// <summary>
        /// Adds a drawable to this container. Note that the drawable added must have its Y-position be
        /// an absolute unit of time that is _not_ relative to <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="drawable">The drawable to add.</param>
        public override void Add(Drawable drawable)
        {
            // Always add timing sections to ourselves
            if (drawable is DrawableControlPoint)
            {
                base.Add(drawable);
                return;
            }

            var controlPoint = drawableControlPoints.LastOrDefault(t => t.CanContain(drawable)) ?? drawableControlPoints.FirstOrDefault();

            if (controlPoint == null)
                throw new Exception("Could not find suitable timing section to add object to.");

            controlPoint.Add(drawable);
        }

        /// <summary>
        /// A container that contains drawables within the time span of a timing section.
        /// <para>
        /// The content of this container will scroll relative to the current time.
        /// </para>
        /// </summary>
        private class DrawableControlPoint : Container
        {
            private readonly ControlPoint timingChange;

            protected override Container<Drawable> Content => content;
            private readonly Container content;

            /// <summary>
            /// Creates a drawable control point. The height of this container will be proportional
            /// to the beat length of the control point it is initialized with such that, e.g. a beat length
            /// of 500ms results in this container being twice as high as its parent, which further means that
            /// the content container will scroll at twice the normal rate.
            /// </summary>
            /// <param name="timingChange">The control point to create the drawable control point for.</param>
            public DrawableControlPoint(ControlPoint timingChange)
            {
                this.timingChange = timingChange;

                RelativeSizeAxes = Axes.Both;

                AddInternal(content = new AutoTimeRelativeContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Y = (float)timingChange.Time
                });
            }

            protected override void Update()
            {
                var parent = (ControlPointContainer)Parent;

                // Adjust our height to account for the speed changes
                Height = (float)(1000 / timingChange.BeatLength / timingChange.SpeedMultiplier);
                RelativeCoordinateSpace = new Vector2(1, (float)parent.TimeSpan);

                // Scroll the content
                content.Y = (float)(timingChange.Time - Time.Current);
            }

            public override void Add(Drawable drawable)
            {
                // The previously relatively-positioned drawable will now become relative to content, but since the drawable has no knowledge of content,
                // we need to offset it back by content's position position so that it becomes correctly relatively-positioned to content
                // This can be removed if hit objects were stored such that either their StartTime or their "beat offset" was relative to the timing change
                // they belonged to, but this requires a radical change to the beatmap format which we're not ready to do just yet
                drawable.Y -= (float)timingChange.Time;

                base.Add(drawable);
            }

            /// <summary>
            /// Whether this control point can contain a drawable. This control point can contain a drawable if the drawable is positioned "after" this control point.
            /// </summary>
            /// <param name="drawable">The drawable to check.</param>
            public bool CanContain(Drawable drawable) => content.Y <= drawable.Y;

            /// <summary>
            /// A container which always keeps its height and relative coordinate space "auto-sized" to its children.
            /// <para>
            /// This is used in the case where children are relatively positioned/sized to time values (e.g. notes/bar lines) to keep
            /// such children wrapped inside a container, otherwise they would disappear due to container flattening.
            /// </para>
            /// </summary>
            private class AutoTimeRelativeContainer : Container
            {
                public override void InvalidateFromChild(Invalidation invalidation)
                {
                    // We only want to re-compute our size when a child's size or position has changed
                    if ((invalidation & Invalidation.Geometry) == 0)
                    {
                        base.InvalidateFromChild(invalidation);
                        return;
                    }

                    if (!Children.Any())
                        return;

                    float height = Children.Select(child => child.Y + child.Height).Max();

                    Height = height;
                    RelativeCoordinateSpace = new Vector2(1, height);

                    base.InvalidateFromChild(invalidation);
                }
            }
        }
    }
}