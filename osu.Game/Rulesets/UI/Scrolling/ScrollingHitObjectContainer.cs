// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
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
        /// A set containing all <see cref="HitObjectContainer.AliveObjects"/> which have an up-to-date layout.
        /// </summary>
        private readonly HashSet<DrawableHitObject> layoutComputed = new HashSet<DrawableHitObject>();

        /// <summary>
        /// A conservative estimate of maximum bounding box of a <see cref="DrawableHitObject"/>
        /// with respect to the start time position of the hit object.
        /// It is used to calculate when the object appears inbound.
        /// </summary>
        protected virtual RectangleF GetDrawRectangle(HitObjectLifetimeEntry entry) => new RectangleF().Inflate(100);

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

            return scrollingInfo.Algorithm.TimeAt(position, Time.Current, scrollingInfo.TimeRange.Value, getLength());
        }

        /// <summary>
        /// Given a time, return the screen space position within this column.
        /// </summary>
        public Vector2 ScreenSpacePositionAtTime(double time)
        {
            var pos = scrollingInfo.Algorithm.PositionAt(time, Time.Current, scrollingInfo.TimeRange.Value, getLength());

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

        private float getLength()
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

        protected override void OnAdd(DrawableHitObject drawableHitObject) => onAddRecursive(drawableHitObject);

        protected override void OnRemove(DrawableHitObject drawableHitObject) => onRemoveRecursive(drawableHitObject);

        private void onAddRecursive(DrawableHitObject hitObject)
        {
            invalidateHitObject(hitObject);

            hitObject.DefaultsApplied += invalidateHitObject;

            foreach (var nested in hitObject.NestedHitObjects)
                onAddRecursive(nested);
        }

        private void onRemoveRecursive(DrawableHitObject hitObject)
        {
            layoutComputed.Remove(hitObject);

            hitObject.DefaultsApplied -= invalidateHitObject;

            foreach (var nested in hitObject.NestedHitObjects)
                onRemoveRecursive(nested);
        }

        private void invalidateHitObject(DrawableHitObject hitObject)
        {
            if (hitObject.ParentHitObject == null)
                updateLifetime(hitObject.Entry);

            layoutComputed.Remove(hitObject);
        }

        protected override void Update()
        {
            base.Update();

            if (!layoutCache.IsValid)
            {
                foreach (var entry in Entries)
                    updateLifetime(entry);

                layoutComputed.Clear();

                scrollingInfo.Algorithm.Reset();
                layoutCache.Validate();
            }
        }

        // We need to calculate hit object positions (including nested hit objects) as soon as possible after lifetimes
        // to prevent hit objects displayed in a wrong position for one frame.
        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

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

        private void updateLifetime(HitObjectLifetimeEntry entry)
        {
            var rectangle = GetDrawRectangle(entry);
            float startOffset = 0;

            switch (direction.Value)
            {
                case ScrollingDirection.Right:
                    startOffset = rectangle.Right;
                    break;

                case ScrollingDirection.Down:
                    startOffset = rectangle.Bottom;
                    break;

                case ScrollingDirection.Left:
                    startOffset = -rectangle.Left;
                    break;

                case ScrollingDirection.Up:
                    startOffset = -rectangle.Top;
                    break;
            }

            entry.LifetimeStart = scrollingInfo.Algorithm.GetDisplayStartTime(entry.HitObject.StartTime, startOffset, timeRange.Value, getLength());
        }

        private void updateLayoutRecursive(DrawableHitObject hitObject)
        {
            if (hitObject.HitObject is IHasDuration e)
            {
                switch (direction.Value)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        hitObject.Height = scrollingInfo.Algorithm.GetLength(hitObject.HitObject.StartTime, e.EndTime, timeRange.Value, getLength());
                        break;

                    case ScrollingDirection.Left:
                    case ScrollingDirection.Right:
                        hitObject.Width = scrollingInfo.Algorithm.GetLength(hitObject.HitObject.StartTime, e.EndTime, timeRange.Value, getLength());
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
                    hitObject.Y = scrollingInfo.Algorithm.PositionAt(hitObject.HitObject.StartTime, currentTime, timeRange.Value, getLength());
                    break;

                case ScrollingDirection.Down:
                    hitObject.Y = -scrollingInfo.Algorithm.PositionAt(hitObject.HitObject.StartTime, currentTime, timeRange.Value, getLength());
                    break;

                case ScrollingDirection.Left:
                    hitObject.X = scrollingInfo.Algorithm.PositionAt(hitObject.HitObject.StartTime, currentTime, timeRange.Value, getLength());
                    break;

                case ScrollingDirection.Right:
                    hitObject.X = -scrollingInfo.Algorithm.PositionAt(hitObject.HitObject.StartTime, currentTime, timeRange.Value, getLength());
                    break;
            }
        }
    }
}
