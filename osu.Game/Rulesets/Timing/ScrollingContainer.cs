// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Timing
{
    /// <summary>
    /// A container that scrolls relative to the current time. Will autosize to the total duration of all contained hit objects along the scrolling axes.
    /// </summary>
    public abstract class ScrollingContainer : Container<DrawableHitObject>
    {
        /// <summary>
        /// Gets or sets the range of time that is visible by the length of the scrolling axes.
        /// </summary>
        public readonly BindableDouble VisibleTimeRange = new BindableDouble { Default = 1000 };

        /// <summary>
        /// The axes through which this <see cref="ScrollingContainer"/> scrolls. This is set by the <see cref="SpeedAdjustmentContainer"/>.
        /// </summary>
        internal Axes ScrollingAxes;

        /// <summary>
        /// The control point that defines the speed adjustments for this container. This is set by the <see cref="SpeedAdjustmentContainer"/>.
        /// </summary>
        internal MultiplierControlPoint ControlPoint;

        private Cached<double> durationBacking;

        /// <summary>
        /// Creates a new <see cref="ScrollingContainer"/>.
        /// </summary>
        protected ScrollingContainer()
        {
            RelativeSizeAxes = Axes.Both;
            RelativePositionAxes = Axes.Both;
        }

        public override void InvalidateFromChild(Invalidation invalidation)
        {
            // We only want to re-compute our size when a child's size or position has changed
            if ((invalidation & Invalidation.RequiredParentSizeToFit) == 0)
            {
                base.InvalidateFromChild(invalidation);
                return;
            }

            durationBacking.Invalidate();

            base.InvalidateFromChild(invalidation);
        }

        private double computeDuration()
        {
            if (!Children.Any())
                return 0;

            double baseDuration = Children.Max(c => (c.HitObject as IHasEndTime)?.EndTime ?? c.HitObject.StartTime) - ControlPoint.StartTime;

            // If we have a singular hit object at the timing section's start time, let's set a sane default duration
            if (baseDuration == 0)
                baseDuration = 1;

            // This container needs to resize such that it completely encloses the hit objects to avoid masking optimisations. This is done by converting the largest
            // absolutely-sized element along the scrolling axes and adding a corresponding duration value. This introduces a bit of error, but will never under-estimate.ion.

            // Find the largest element that is absolutely-sized along ScrollingAxes
            float maxAbsoluteSize = Children.Where(c => (c.RelativeSizeAxes & ScrollingAxes) == 0)
                                            .Select(c => (ScrollingAxes & Axes.X) > 0 ? c.Width : c.Height)
                                            .DefaultIfEmpty().Max();

            float ourAbsoluteSize = (ScrollingAxes & Axes.X) > 0 ? DrawWidth : DrawHeight;

            // Add the extra duration to account for the absolute size
            baseDuration *= 1 + maxAbsoluteSize / ourAbsoluteSize;

            return baseDuration;
        }

        /// <summary>
        /// The maximum duration of any one hit object inside this <see cref="ScrollingContainer"/>. This is calculated as the maximum
        /// duration of all hit objects relative to this <see cref="ScrollingContainer"/>'s <see cref="MultiplierControlPoint.StartTime"/>.
        /// </summary>
        public double Duration => durationBacking.IsValid ? durationBacking : (durationBacking.Value = computeDuration());

        protected override void Update()
        {
            base.Update();

            // We want our size and position-space along the scrolling axes to span our duration to completely enclose all the hit objects
            Size = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)Duration : Size.X, (ScrollingAxes & Axes.Y) > 0 ? (float)Duration : Size.Y);
            // And we need to make sure the hit object's position-space doesn't change due to our resizing
            RelativeChildSize = Size;
        }
    }
}
