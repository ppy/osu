// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public class ScrollingHitObjectContainer : HitObjectContainer
    {
        private readonly IBindable<double> timeRange = new BindableDouble();
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        /// <summary>
        /// Whether the scrolling direction is horizontal or vertical.
        /// </summary>
        private Direction scrollingAxis => direction.Value == ScrollingDirection.Left || direction.Value == ScrollingDirection.Right ? Direction.Horizontal : Direction.Vertical;

        /// <summary>
        /// Whether the scrolling direction is the positive-to-negative direction in the local coordinate.
        /// </summary>
        private bool axisInverted => direction.Value == ScrollingDirection.Down || direction.Value == ScrollingDirection.Right;

        /// <summary>
        /// A set of top-level <see cref="DrawableHitObject"/>s which have an up-to-date layout.
        /// </summary>
        private readonly HashSet<DrawableHitObject> layoutComputed = new HashSet<DrawableHitObject>();

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        // Responds to changes in the layout. When the layout changes, all hit object states must be recomputed.
        private readonly LayoutValue layoutCache = new LayoutValue(Invalidation.RequiredParentSizeToFit | Invalidation.DrawInfo);

        public ScrollingHitObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;

            AddLayout(layoutCache);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            direction.BindTo(scrollingInfo.Direction);
            timeRange.BindTo(scrollingInfo.TimeRange);

            direction.ValueChanged += _ => layoutCache.Invalidate();
            timeRange.ValueChanged += _ => layoutCache.Invalidate();
        }

        /// <summary>
        /// Given a position at <paramref name="currentTime"/>, return the time of the object corresponding the position.
        /// </summary>
        /// <remarks>
        /// If there are multiple valid time values, one arbitrary time is returned.
        /// </remarks>
        public double TimeAtPosition(float localPosition, double currentTime)
        {
            float scrollPosition = axisInverted ? scrollLength - localPosition : localPosition;
            return scrollingInfo.Algorithm.TimeAt(scrollPosition, currentTime, timeRange.Value, scrollLength);
        }

        /// <summary>
        /// Given a position at the current time in screen space, return the time of the object corresponding the position.
        /// </summary>
        /// <remarks>
        /// If there are multiple valid time values, one arbitrary time is returned.
        /// </remarks>
        public double TimeAtScreenSpacePosition(Vector2 screenSpacePosition)
        {
            Vector2 localPosition = ToLocalSpace(screenSpacePosition);
            return TimeAtPosition(scrollingAxis == Direction.Horizontal ? localPosition.X : localPosition.Y, Time.Current);
        }

        /// <summary>
        /// Given a time, return the position along the scrolling axis within this <see cref="HitObjectContainer"/> at time <paramref name="currentTime"/>.
        /// </summary>
        public float PositionAtTime(double time, double currentTime)
        {
            float scrollPosition = scrollingInfo.Algorithm.PositionAt(time, currentTime, timeRange.Value, scrollLength);
            return axisInverted ? scrollLength - scrollPosition : scrollPosition;
        }

        /// <summary>
        /// Given a time, return the position along the scrolling axis within this <see cref="HitObjectContainer"/> at the current time.
        /// </summary>
        public float PositionAtTime(double time) => PositionAtTime(time, Time.Current);

        /// <summary>
        /// Given a time, return the screen space position within this <see cref="HitObjectContainer"/>.
        /// In the non-scrolling axis, the center of this <see cref="HitObjectContainer"/> is returned.
        /// </summary>
        public Vector2 ScreenSpacePositionAtTime(double time)
        {
            float localPosition = PositionAtTime(time, Time.Current);
            return scrollingAxis == Direction.Horizontal
                ? ToScreenSpace(new Vector2(localPosition, DrawHeight / 2))
                : ToScreenSpace(new Vector2(DrawWidth / 2, localPosition));
        }

        /// <summary>
        /// Given a start time and end time of a scrolling object, return the length of the object along the scrolling axis.
        /// </summary>
        public float LengthAtTime(double startTime, double endTime)
        {
            return scrollingInfo.Algorithm.GetLength(startTime, endTime, timeRange.Value, scrollLength);
        }

        private float scrollLength => scrollingAxis == Direction.Horizontal ? DrawWidth : DrawHeight;

        protected override void AddDrawable(HitObjectLifetimeEntry entry, DrawableHitObject drawable)
        {
            base.AddDrawable(entry, drawable);

            invalidateHitObject(drawable);
            drawable.DefaultsApplied += invalidateHitObject;
        }

        protected override void RemoveDrawable(HitObjectLifetimeEntry entry, DrawableHitObject drawable)
        {
            base.RemoveDrawable(entry, drawable);

            drawable.DefaultsApplied -= invalidateHitObject;
            layoutComputed.Remove(drawable);
        }

        private void invalidateHitObject(DrawableHitObject hitObject)
        {
            hitObject.LifetimeStart = computeOriginAdjustedLifetimeStart(hitObject);
            layoutComputed.Remove(hitObject);
        }

        protected override void Update()
        {
            base.Update();

            if (layoutCache.IsValid) return;

            layoutComputed.Clear();

            // Reset lifetime to the conservative estimation.
            // If a drawable becomes alive by this lifetime, its lifetime will be updated to a more precise lifetime in the next update.
            foreach (var entry in Entries)
                entry.SetInitialLifetime();

            scrollingInfo.Algorithm.Reset();

            layoutCache.Validate();
        }

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            // We need to calculate hit object positions (including nested hit objects) as soon as possible after lifetimes
            // to prevent hit objects displayed in a wrong position for one frame.
            // Only AliveObjects need to be considered for layout (reduces overhead in the case of scroll speed changes).
            foreach (var obj in AliveObjects)
            {
                updatePosition(obj, Time.Current);

                if (layoutComputed.Contains(obj))
                    continue;

                updateLayoutRecursive(obj);

                layoutComputed.Add(obj);
            }
        }

        private double computeOriginAdjustedLifetimeStart(DrawableHitObject hitObject)
        {
            // Origin position may be relative to the parent size
            Debug.Assert(hitObject.Parent != null);

            float originAdjustment = 0.0f;

            // calculate the dimension of the part of the hitobject that should already be visible
            // when the hitobject origin first appears inside the scrolling container
            switch (direction.Value)
            {
                case ScrollingDirection.Up:
                    originAdjustment = hitObject.OriginPosition.Y;
                    break;

                case ScrollingDirection.Down:
                    originAdjustment = hitObject.DrawHeight - hitObject.OriginPosition.Y;
                    break;

                case ScrollingDirection.Left:
                    originAdjustment = hitObject.OriginPosition.X;
                    break;

                case ScrollingDirection.Right:
                    originAdjustment = hitObject.DrawWidth - hitObject.OriginPosition.X;
                    break;
            }

            return scrollingInfo.Algorithm.GetDisplayStartTime(hitObject.HitObject.StartTime, originAdjustment, timeRange.Value, scrollLength);
        }

        private void updateLayoutRecursive(DrawableHitObject hitObject)
        {
            if (hitObject.HitObject is IHasDuration e)
            {
                float length = LengthAtTime(hitObject.HitObject.StartTime, e.EndTime);
                if (scrollingAxis == Direction.Horizontal)
                    hitObject.Width = length;
                else
                    hitObject.Height = length;
            }

            foreach (var obj in hitObject.NestedHitObjects)
            {
                updateLayoutRecursive(obj);

                // Nested hitobjects don't need to scroll, but they do need accurate positions
                updatePosition(obj, hitObject.HitObject.StartTime);
            }
        }

        private void updatePosition(DrawableHitObject hitObject, double currentTime)
        {
            float position = PositionAtTime(hitObject.HitObject.StartTime, currentTime);

            // The position returned from `PositionAtTime` is assuming the `TopLeft` anchor.
            // A correction is needed because the hit objects are using a different anchor for each direction (e.g. `BottomCentre` for `Bottom` direction).
            float anchorCorrection = axisInverted ? scrollLength : 0;

            if (scrollingAxis == Direction.Horizontal)
                hitObject.X = position - anchorCorrection;
            else
                hitObject.Y = position - anchorCorrection;
        }
    }
}
