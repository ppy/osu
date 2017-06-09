﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A collection of hit objects which scrolls within a <see cref="SpeedAdjustmentContainer"/>.
    ///
    /// <para>
    /// This container handles the conversion between time and position through <see cref="Container{T}.RelativeChildSize"/> and
    /// <see cref="Container{T}.RelativeChildOffset"/> such that hit objects added to this container should have time values set as their
    /// positions/sizes to make proper use of this container.
    /// </para>
    ///
    /// <para>
    /// This container will auto-size to the total size of its children along the desired auto-sizing axes such that the reasulting size
    /// of this container will also be a time value.
    /// </para>
    ///
    /// <para>
    /// This container will always be relatively-sized and positioned to its parent through the use of <see cref="Drawable.RelativeSizeAxes"/>
    /// and <see cref="Drawable.RelativePositionAxes"/> such that the parent can utilise <see cref="Container{T}.RelativeChildSize"/> and
    /// <see cref="Container{T}.RelativeChildOffset"/> to apply further time offsets to this collection of hit objects.
    /// </para>
    /// </summary>
    public abstract class DrawableTimingSection : Container<DrawableHitObject>
    {
        private readonly BindableDouble visibleTimeRange = new BindableDouble();
        public BindableDouble VisibleTimeRange
        {
            get { return visibleTimeRange; }
            set { visibleTimeRange.BindTo(value); }
        }

        protected override IComparer<Drawable> DepthComparer => new HitObjectReverseStartTimeComparer();

        private readonly Axes autoSizingAxes;

        private Cached layout = new Cached();

        /// <summary>
        /// Creates a new <see cref="DrawableTimingSection"/>.
        /// </summary>
        /// <param name="autoSizingAxes">The axes on which to auto-size to the total size of items in the container.</param>
        protected DrawableTimingSection(Axes autoSizingAxes)
        {
            this.autoSizingAxes = autoSizingAxes;

            // We need a default size since RelativeSizeAxes is overridden
            Size = Vector2.One;
        }

        public override Axes AutoSizeAxes { set { throw new InvalidOperationException($"{nameof(DrawableTimingSection)} must always be relatively-sized."); } }

        public override Axes RelativeSizeAxes
        {
            get { return Axes.Both; }
            set { throw new InvalidOperationException($"{nameof(DrawableTimingSection)} must always be relatively-sized."); }
        }

        public override Axes RelativePositionAxes
        {
            get { return Axes.Both; }
            set { throw new InvalidOperationException($"{nameof(DrawableTimingSection)} must always be relatively-positioned."); }
        }

        public override void InvalidateFromChild(Invalidation invalidation)
        {
            // We only want to re-compute our size when a child's size or position has changed
            if ((invalidation & Invalidation.RequiredParentSizeToFit) == 0)
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

                    //double maxDuration = Children.Select(c => (c.HitObject as IHasEndTime)?.EndTime ?? c.HitObject.StartTime).Max();
                    //float width = (float)maxDuration - RelativeChildOffset.X;
                    //float height = (float)maxDuration - RelativeChildOffset.Y;


                    // Auto-size to the total size of our children
                    // This ends up being the total duration of our children, however for now this is a more sure-fire way to calculate this
                    // than the above due to some undesired masking optimisations causing some hit objects to be culled...
                    // Todo: When this is investigated more we should use the above method as it is a little more exact
                    float width = Children.Select(child => child.X + child.Width).Max() - RelativeChildOffset.X;
                    float height = Children.Select(child => child.Y + child.Height).Max() - RelativeChildOffset.Y;

                    // Consider that width/height are time values. To have ourselves span these time values 1:1, we first need to set our size
                    Size = new Vector2((autoSizingAxes & Axes.X) > 0 ? width : Size.X, (autoSizingAxes & Axes.Y) > 0 ? height : Size.Y);
                    // Then to make our position-space be time values again, we need our relative child size to follow our size
                    RelativeChildSize = Size;
                });
            }
        }
    }
}
