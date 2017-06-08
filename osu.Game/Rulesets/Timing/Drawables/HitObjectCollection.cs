// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Timing.Drawables
{
    /// <summary>
    /// A collection of hit objects which scrolls within a <see cref="DrawableTimingSection"/>.
    ///
    /// <para>
    /// This container handles the conversion between time and position through <see cref="Container{T}.RelativeChildSize"/> and
    /// <see cref="Container{T}.RelativeChildOffset"/> such that hit objects added to this container should have time values set as their
    /// positions/sizes to make proper use of this container.
    /// </para>
    /// 
    /// <para>
    /// This container will auto-size to the total size of its children along the desired auto-sizing axes such that the size of this container
    /// will also be a time value if hit objects added to this container have time values as their positions/sizes.
    /// </para>
    /// 
    /// <para>
    /// This container will always be relatively-sized to its parent through the use of <see cref="Drawable.RelativeSizeAxes"/> such that the
    /// parent can utilise <see cref="Container{T}.RelativeChildSize"/> and <see cref="Container{T}.RelativeChildOffset"/> to apply further
    /// time offsets to this collection of hit objects.
    /// </para>
    /// </summary>
    public abstract class HitObjectCollection : Container<DrawableHitObject>
    {
        protected override IComparer<Drawable> DepthComparer => new HitObjectReverseStartTimeComparer();

        private readonly Axes autoSizingAxes;

        private Cached layout = new Cached();

        /// <summary>
        /// Creates a new <see cref="HitObjectCollection"/>.
        /// </summary>
        /// <param name="autoSizingAxes">The axes on which to auto-size to the total size of items in the container.</param>
        protected HitObjectCollection(Axes autoSizingAxes)
        {
            this.autoSizingAxes = autoSizingAxes;
        }

        public override Axes AutoSizeAxes { set { throw new InvalidOperationException($"{nameof(HitObjectCollection)} must always be relatively-sized."); } }

        public override Axes RelativeSizeAxes
        {
            get { return Axes.Both; }
            set { throw new InvalidOperationException($"{nameof(HitObjectCollection)} must always be relatively-sized."); }
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

                    // Auto-size to the total size of our children
                    float width = Children.Select(child => child.X + child.Width).Max() - RelativeChildOffset.X;
                    float height = Children.Select(child => child.Y + child.Height).Max() - RelativeChildOffset.Y;

                    // Consider that width/height are time values. To have ourselves span these time values 1:1, we first need to set our size
                    base.Size = new Vector2((autoSizingAxes & Axes.X) > 0 ? width : Size.X, (autoSizingAxes & Axes.Y) > 0 ? height : Size.Y);
                    // Then to make our position-space be time values again, we need our relative child size to follow our size
                    RelativeChildSize = Size;
                });
            }
        }
    }
}
