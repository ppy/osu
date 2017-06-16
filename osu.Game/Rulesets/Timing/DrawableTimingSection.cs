// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using OpenTK;
using osu.Game.Rulesets.Objects.Types;

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
    /// This container will auto-size to the total duration of the contained hit objects along the desired auto-sizing axes such that the resulting size
    /// of this container will be a value representing the total duration of all contained hit objects.
    /// </para>
    ///
    /// <para>
    /// This container is and must always be relatively-sized and positioned to its such that the parent can utilise <see cref="Container{T}.RelativeChildSize"/>
    /// and <see cref="Container{T}.RelativeChildOffset"/> to apply further time offsets to this collection of hit objects.
    /// </para>
    /// </summary>
    public abstract class DrawableTimingSection : Container<DrawableHitObject>
    {
        private readonly BindableDouble visibleTimeRange = new BindableDouble();
        /// <summary>
        /// Gets or sets the range of time that is visible by the length of this container.
        /// </summary>
        public BindableDouble VisibleTimeRange
        {
            get { return visibleTimeRange; }
            set { visibleTimeRange.BindTo(value); }
        }

        /// <summary>
        /// Axes through which this timing section scrolls. This is set by the <see cref="SpeedAdjustmentContainer"/>.
        /// </summary>
        internal Axes ScrollingAxes;

        /// <summary>
        /// The control point that provides the speed adjustments for this container. This is set by the <see cref="SpeedAdjustmentContainer"/>.
        /// </summary>
        internal MultiplierControlPoint ControlPoint;

        protected override IComparer<Drawable> DepthComparer => new HitObjectReverseStartTimeComparer();

        /// <summary>
        /// Creates a new <see cref="DrawableTimingSection"/>.
        /// </summary>
        protected DrawableTimingSection()
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

        private Cached<double> durationBacking = new Cached<double>();
        /// <summary>
        /// The maximum duration of any one hit object inside this <see cref="DrawableTimingSection"/>. This is calculated as the maximum
        /// end time between all hit objects relative to this <see cref="DrawableTimingSection"/>'s <see cref="MultiplierControlPoint.StartTime"/>.
        /// </summary>
        public double Duration => durationBacking.EnsureValid()
            ? durationBacking.Value
            : durationBacking.Refresh(() =>
        {
            if (!Children.Any())
                return 0;

            double baseDuration = Children.Max(c => (c.HitObject as IHasEndTime)?.EndTime ?? c.HitObject.StartTime) - ControlPoint.StartTime;

            // If we have a singular hit object at the timing section's start time, let's set a sane default duration
            if (baseDuration == 0)
                baseDuration = 1;

            // Scrolling ruleset hit objects typically have anchors+origins set to the hit object's start time, but if the hit object doesn't implement IHasEndTime and lies on the control point
            // then the baseDuration above will be 0. This will cause problems with masking when it is further set as the value for Size in Update(). We _want_ the timing section bounds to
            // completely enclose the hit object to avoid the masking optimisations.
            //
            // To do this we need to find a duration that corresponds to the absolute size of the element that extrudes beyond the timing section's bounds and add that to baseDuration.
            // We can utilize the fact that the Size and RelativeChildSpace are 1:1, meaning that an change in duration for the timing section has no change to the hit object's positioning
            // and simply find the largest absolutely-sized element in this timing section. This introduces a little bit of error, but will never under-estimate the duration.

            // Find the largest element that is absolutely-sized along ScrollingAxes
            float maxAbsoluteSize = Children.Where(c => (c.RelativeSizeAxes & ScrollingAxes) == 0)
                                            .Select(c => (ScrollingAxes & Axes.X) > 0 ? c.Width : c.Height)
                                            .DefaultIfEmpty().Max();

            float ourAbsoluteSize = (ScrollingAxes & Axes.X) > 0 ? DrawWidth : DrawHeight;

            // Add the extra duration to account for the absolute size
            baseDuration *= 1 + maxAbsoluteSize / ourAbsoluteSize;

            return baseDuration;
        });

        protected override void Update()
        {
            base.Update();

            // We want our size and position-space along ScrollingAxes to span our duration to completely enclose all the hit objects
            Size = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)Duration : Size.X, (ScrollingAxes & Axes.Y) > 0 ? (float)Duration : Size.Y);
            // And we need to make sure the hit object's position-space doesn't change due to our resizing
            RelativeChildSize = Size;
        }
    }
}
