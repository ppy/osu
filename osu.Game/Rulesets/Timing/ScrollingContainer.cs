// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
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

        public override bool RemoveWhenNotAlive => false;
        protected override bool RequiresChildrenUpdate => true;

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

        protected override int Compare(Drawable x, Drawable y)
        {
            var hX = (DrawableHitObject)x;
            var hY = (DrawableHitObject)y;

            int result = hY.HitObject.StartTime.CompareTo(hX.HitObject.StartTime);
            if (result != 0)
                return result;
            return base.Compare(y, x);
        }

        public override void Add(DrawableHitObject drawable)
        {
            durationBacking.Invalidate();
            base.Add(drawable);
        }

        public override bool Remove(DrawableHitObject drawable)
        {
            durationBacking.Invalidate();
            return base.Remove(drawable);
        }

        // Todo: This may underestimate the size of the hit object in some cases, but won't be too much of a problem for now
        private double computeDuration() => Math.Max(0, Children.Select(c => (c.HitObject as IHasEndTime)?.EndTime ?? c.HitObject.StartTime).DefaultIfEmpty().Max() - ControlPoint.StartTime) + 1000;

        /// <summary>
        /// An approximate total duration of this scrolling container.
        /// </summary>
        public double Duration => durationBacking.IsValid ? durationBacking : (durationBacking.Value = computeDuration());

        protected override void Update()
        {
            base.Update();

            RelativeChildOffset = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)ControlPoint.StartTime : 0, (ScrollingAxes & Axes.Y) > 0 ? (float)ControlPoint.StartTime : 0);

            // We want our size and position-space along the scrolling axes to span our duration to completely enclose all the hit objects
            Size = new Vector2((ScrollingAxes & Axes.X) > 0 ? (float)Duration : Size.X, (ScrollingAxes & Axes.Y) > 0 ? (float)Duration : Size.Y);
            // And we need to make sure the hit object's position-space doesn't change due to our resizing
            RelativeChildSize = Size;
        }
    }
}
