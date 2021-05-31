// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Layout;
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

        public override void Clear()
        {
            base.Clear();

            layoutComputed.Clear();
        }

        /// <summary>
        /// Given a position in screen space, return the time within this column.
        /// </summary>
        public double TimeAtScreenSpacePosition(Vector2 screenSpacePosition)
        {
            // convert to local space of column so we can snap and fetch correct location.
            Vector2 localPosition = ToLocalSpace(screenSpacePosition);

            float position = 0;

            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Up:
                case ScrollingDirection.Down:
                    position = localPosition.Y;
                    break;

                case ScrollingDirection.Right:
                case ScrollingDirection.Left:
                    position = localPosition.X;
                    break;
            }

            flipPositionIfRequired(ref position);

            return scrollingInfo.Algorithm.TimeAt(position, Time.Current, scrollingInfo.TimeRange.Value, scrollLength);
        }

        /// <summary>
        /// Given a time, return the screen space position within this column.
        /// </summary>
        public Vector2 ScreenSpacePositionAtTime(double time)
        {
            var pos = scrollingInfo.Algorithm.PositionAt(time, Time.Current, scrollingInfo.TimeRange.Value, scrollLength);

            flipPositionIfRequired(ref pos);

            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Up:
                case ScrollingDirection.Down:
                    return ToScreenSpace(new Vector2(getBreadth() / 2, pos));

                default:
                    return ToScreenSpace(new Vector2(pos, getBreadth() / 2));
            }
        }

        private float scrollLength
        {
            get
            {
                switch (scrollingInfo.Direction.Value)
                {
                    case ScrollingDirection.Left:
                    case ScrollingDirection.Right:
                        return DrawWidth;

                    default:
                        return DrawHeight;
                }
            }
        }

        private float getBreadth()
        {
            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Up:
                case ScrollingDirection.Down:
                    return DrawWidth;

                default:
                    return DrawHeight;
            }
        }

        private void flipPositionIfRequired(ref float position)
        {
            // We're dealing with screen coordinates in which the position decreases towards the centre of the screen resulting in an increase in start time.
            // The scrolling algorithm instead assumes a top anchor meaning an increase in time corresponds to an increase in position,
            // so when scrolling downwards the coordinates need to be flipped.

            switch (scrollingInfo.Direction.Value)
            {
                case ScrollingDirection.Down:
                    position = DrawHeight - position;
                    break;

                case ScrollingDirection.Right:
                    position = DrawWidth - position;
                    break;
            }
        }

        protected override void OnAdd(DrawableHitObject drawableHitObject)
        {
            invalidateHitObject(drawableHitObject);
            drawableHitObject.DefaultsApplied += invalidateHitObject;
        }

        protected override void OnRemove(DrawableHitObject drawableHitObject)
        {
            layoutComputed.Remove(drawableHitObject);

            drawableHitObject.DefaultsApplied -= invalidateHitObject;
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

            foreach (var hitObject in Objects)
            {
                if (hitObject.HitObject != null)
                    invalidateHitObject(hitObject);
            }

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
                switch (direction.Value)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        hitObject.Height = scrollingInfo.Algorithm.GetLength(hitObject.HitObject.StartTime, e.EndTime, timeRange.Value, scrollLength);
                        break;

                    case ScrollingDirection.Left:
                    case ScrollingDirection.Right:
                        hitObject.Width = scrollingInfo.Algorithm.GetLength(hitObject.HitObject.StartTime, e.EndTime, timeRange.Value, scrollLength);
                        break;
                }
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
            switch (direction.Value)
            {
                case ScrollingDirection.Up:
                    hitObject.Y = scrollingInfo.Algorithm.PositionAt(hitObject.HitObject.StartTime, currentTime, timeRange.Value, scrollLength);
                    break;

                case ScrollingDirection.Down:
                    hitObject.Y = -scrollingInfo.Algorithm.PositionAt(hitObject.HitObject.StartTime, currentTime, timeRange.Value, scrollLength);
                    break;

                case ScrollingDirection.Left:
                    hitObject.X = scrollingInfo.Algorithm.PositionAt(hitObject.HitObject.StartTime, currentTime, timeRange.Value, scrollLength);
                    break;

                case ScrollingDirection.Right:
                    hitObject.X = -scrollingInfo.Algorithm.PositionAt(hitObject.HitObject.StartTime, currentTime, timeRange.Value, scrollLength);
                    break;
            }
        }
    }
}
