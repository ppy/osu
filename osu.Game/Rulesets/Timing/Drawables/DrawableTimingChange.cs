// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Framework.Caching;
using osu.Framework.Graphics.Primitives;

namespace osu.Game.Rulesets.Timing.Drawables
{
    /// <summary>
    /// Represents a container in which contains hit objects and moves relative to the current time.
    /// </summary>
    public abstract class DrawableTimingChange : Container<DrawableHitObject>
    {
        public readonly TimingChange TimingChange;

        protected override Container<DrawableHitObject> Content => content;
        private readonly Container<DrawableHitObject> content;

        private readonly Axes scrollingAxes;

        /// <summary>
        /// Creates a new drawable timing change which contains hit objects and scrolls relative to the current time.
        /// </summary>
        /// <param name="timingChange">The encapsulated timing change that provides the speed changes.</param>
        /// <param name="scrollingAxes">The axes through which this timing change scrolls.</param>
        protected DrawableTimingChange(TimingChange timingChange, Axes scrollingAxes)
        {
            this.scrollingAxes = scrollingAxes;

            TimingChange = timingChange;

            // We have to proxy the hit objects to an internal container since we're
            // going to be modifying our height to apply speed changes
            AddInternal(content = new RelativeCoordinateAutoSizingContainer(scrollingAxes)
            {
                RelativeSizeAxes = Axes.Both,
                RelativePositionAxes = Axes.Both,
                RelativeCoordinateSpace = new RectangleF((scrollingAxes & Axes.X) > 0 ? (float)TimingChange.Time : 0, (scrollingAxes & Axes.Y) > 0 ? (float)TimingChange.Time : 0, 1, 1)
            });
        }

        public override Axes RelativeSizeAxes
        {
            get { return Axes.Both; }
            set { throw new InvalidOperationException($"{nameof(DrawableTimingChange)} must always be relatively-sized."); }
        }

        protected override void Update()
        {
            var parent = Parent as IHasTimeSpan;

            if (parent == null)
                return;

            // Adjust our size to account for the speed changes
            float speedAdjustedSize = (float)(1000 / TimingChange.BeatLength / TimingChange.SpeedMultiplier);

            Size = new Vector2((scrollingAxes & Axes.X) > 0 ? speedAdjustedSize : 1, (scrollingAxes & Axes.Y) > 0 ? speedAdjustedSize : 1);
            RelativeCoordinateSpace = new RectangleF(0, 0, (scrollingAxes & Axes.X) > 0 ? parent.TimeSpan.X : 1, (scrollingAxes & Axes.Y) > 0 ? parent.TimeSpan.Y : 1);
        }

        /// <summary>
        /// Whether this timing change can contain a hit object. This is true if the hit object occurs "after" after this timing change.
        /// </summary>
        public bool CanContain(DrawableHitObject hitObject) => TimingChange.Time <= hitObject.HitObject.StartTime;

        /// <summary>
        /// A container which cann be relatively-sized while auto-sizing to its children on desired axes. The relative coordinate space of
        /// this container follows its auto-sized height.
        /// </summary>
        private class RelativeCoordinateAutoSizingContainer : Container<DrawableHitObject>
        {
            protected override IComparer<Drawable> DepthComparer => new HitObjectReverseStartTimeComparer();

            private readonly Axes autoSizingAxes;

            private Cached layout = new Cached();

            /// <summary>
            /// The axes which this container should calculate its size from its children on.
            /// Note that this is not the same as <see cref="Container{T}.AutoSizeAxes"/>, because that would not allow this container
            /// to be relatively sized - desired in the case where the playfield re-defines <see cref="Container{T}.RelativeCoordinateSpace"/>.
            /// </summary>
            /// <param name="autoSizingAxes"></param>
            public RelativeCoordinateAutoSizingContainer(Axes autoSizingAxes)
            {
                this.autoSizingAxes = autoSizingAxes;
            }

            public override void InvalidateFromChild(Invalidation invalidation)
            {
                // We only want to re-compute our size when a child's size or position has changed
                if ((invalidation & Invalidation.Geometry) == 0)
                {
                    base.InvalidateFromChild(invalidation);
                    return;
                }

                layout.Invalidate();

                base.InvalidateFromChild(invalidation);
            }

            protected override void UpdateAfterChildren()
            {
                base.UpdateAfterChildren();

                if (!layout.EnsureValid())
                {
                    layout.Refresh(() =>
                    {
                        if (!Children.Any())
                            return;

                        float width = Children.Select(child => child.X + child.Width).Max() - RelativeChildOffset.X;
                        float height = Children.Select(child => child.Y + child.Height).Max() - RelativeChildOffset.Y;

                        Size = new Vector2((autoSizingAxes & Axes.X) > 0 ? width : Size.X, (autoSizingAxes & Axes.Y) > 0 ? height : Size.Y);

                        var space = RelativeCoordinateSpace;

                        if ((autoSizingAxes & Axes.X) > 0)
                            space.Width = width;

                        if ((autoSizingAxes & Axes.Y) > 0)
                            space.Height = height;

                        RelativeCoordinateSpace = space;
                    });
                }
            }
        }
    }
}