// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Layout;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;
using osuTK;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public partial class ScrollingHitObjectContainer : HitObjectContainer
    {
        private readonly IBindable<double> timeRange = new BindableDouble();
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly IBindable<IScrollAlgorithm> algorithm = new Bindable<IScrollAlgorithm>();

        /// <summary>
        /// Whether the scrolling direction is horizontal or vertical.
        /// </summary>
        private Direction scrollingAxis => direction.Value == ScrollingDirection.Left || direction.Value == ScrollingDirection.Right ? Direction.Horizontal : Direction.Vertical;

        /// <summary>
        /// The scrolling axis is inverted if objects temporally farther in the future have a smaller position value across the scrolling axis.
        /// </summary>
        /// <example>
        /// <see cref="ScrollingDirection.Down"/> is inverted, because given two objects, one of which is at the current time and one of which is 1000ms in the future,
        /// in the current time instant the future object is spatially above the current object, and therefore has a smaller value of the Y coordinate of its position.
        /// </example>
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
            algorithm.BindTo(scrollingInfo.Algorithm);

            direction.ValueChanged += _ => layoutCache.Invalidate();
            timeRange.ValueChanged += _ => layoutCache.Invalidate();
            algorithm.ValueChanged += _ => layoutCache.Invalidate();
        }

        /// <summary>
        /// Given a position at <paramref name="currentTime"/>, return the time of the object corresponding to the position.
        /// </summary>
        /// <remarks>
        /// If there are multiple valid time values, one arbitrary time is returned.
        /// </remarks>
        public double TimeAtPosition(float localPosition, double currentTime)
        {
            float scrollPosition = axisInverted ? -localPosition : localPosition;
            return algorithm.Value.TimeAt(scrollPosition, currentTime, timeRange.Value, scrollLength);
        }

        /// <summary>
        /// Given a position at the current time in screen space, return the time of the object corresponding the position.
        /// </summary>
        /// <remarks>
        /// If there are multiple valid time values, one arbitrary time is returned.
        /// </remarks>
        public double TimeAtScreenSpacePosition(Vector2 screenSpacePosition)
        {
            Vector2 pos = ToLocalSpace(screenSpacePosition);
            float localPosition = scrollingAxis == Direction.Horizontal ? pos.X : pos.Y;
            localPosition -= axisInverted ? scrollLength : 0;
            return TimeAtPosition(localPosition, Time.Current);
        }

        /// <summary>
        /// Given a time, return the position along the scrolling axis within this <see cref="HitObjectContainer"/> at time <paramref name="currentTime"/>.
        /// </summary>
        public float PositionAtTime(double time, double currentTime, double? originTime = null)
        {
            float scrollPosition = algorithm.Value.PositionAt(time, currentTime, timeRange.Value, scrollLength, originTime);
            return axisInverted ? -scrollPosition : scrollPosition;
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
            localPosition += axisInverted ? scrollLength : 0;
            return scrollingAxis == Direction.Horizontal
                ? ToScreenSpace(new Vector2(localPosition, DrawHeight / 2))
                : ToScreenSpace(new Vector2(DrawWidth / 2, localPosition));
        }

        /// <summary>
        /// Given a start time and end time of a scrolling object, return the length of the object along the scrolling axis.
        /// </summary>
        public float LengthAtTime(double startTime, double endTime)
        {
            return algorithm.Value.GetLength(startTime, endTime, timeRange.Value, scrollLength);
        }

        private float scrollLength => scrollingAxis == Direction.Horizontal ? DrawWidth : DrawHeight;

        public override void Add(HitObjectLifetimeEntry entry)
        {
            // Scroll info is not available until loaded.
            // The lifetime of all entries will be updated in the first Update.
            if (IsLoaded)
                setComputedLifetimeStart(entry);

            base.Add(entry);
        }

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
            layoutComputed.Remove(hitObject);
        }

        protected override void Update()
        {
            base.Update();

            if (layoutCache.IsValid) return;

            layoutComputed.Clear();

            foreach (var entry in Entries)
                setComputedLifetimeStart(entry);

            algorithm.Value.Reset();

            layoutCache.Validate();
        }

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            // We need to calculate hit object positions (including nested hit objects) as soon as possible after lifetimes
            // to prevent hit objects displayed in a wrong position for one frame.
            // Only AliveEntries need to be considered for layout (reduces overhead in the case of scroll speed changes).
            // We are not using AliveObjects directly to avoid selection/sorting overhead since we don't care about the order at which positions will be updated.
            foreach (var entry in AliveDrawableMap)
            {
                var obj = entry.Value;

                updatePosition(obj, Time.Current);

                if (layoutComputed.Contains(obj))
                    continue;

                updateLayoutRecursive(obj);

                layoutComputed.Add(obj);
            }
        }

        /// <summary>
        /// Get a conservative maximum bounding box of a <see cref="DrawableHitObject"/> corresponding to <paramref name="entry"/>.
        /// It is used to calculate when the hit object appears.
        /// </summary>
        protected virtual RectangleF GetConservativeBoundingBox(HitObjectLifetimeEntry entry) => new RectangleF().Inflate(100);

        private double computeDisplayStartTime(HitObjectLifetimeEntry entry)
        {
            RectangleF boundingBox = GetConservativeBoundingBox(entry);
            float startOffset = 0;

            switch (direction.Value)
            {
                case ScrollingDirection.Right:
                    startOffset = boundingBox.Right;
                    break;

                case ScrollingDirection.Down:
                    startOffset = boundingBox.Bottom;
                    break;

                case ScrollingDirection.Left:
                    startOffset = -boundingBox.Left;
                    break;

                case ScrollingDirection.Up:
                    startOffset = -boundingBox.Top;
                    break;
            }

            return algorithm.Value.GetDisplayStartTime(entry.HitObject.StartTime, startOffset, timeRange.Value, scrollLength);
        }

        private void setComputedLifetimeStart(HitObjectLifetimeEntry entry)
        {
            double computedStartTime = computeDisplayStartTime(entry);

            // always load the hitobject before its first judgement offset
            entry.LifetimeStart = Math.Min(entry.HitObject.StartTime - entry.HitObject.MaximumJudgementOffset, computedStartTime);
        }

        private void updateLayoutRecursive(DrawableHitObject hitObject, double? parentHitObjectStartTime = null)
        {
            parentHitObjectStartTime ??= hitObject.HitObject.StartTime;

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
                updateLayoutRecursive(obj, parentHitObjectStartTime);

                // Nested hitobjects don't need to scroll, but they do need accurate positions and start lifetime
                updatePosition(obj, hitObject.HitObject.StartTime, parentHitObjectStartTime);
                setComputedLifetimeStart(obj.Entry);
            }
        }

        private void updatePosition(DrawableHitObject hitObject, double currentTime, double? parentHitObjectStartTime = null)
        {
            float position = PositionAtTime(hitObject.HitObject.StartTime, currentTime, parentHitObjectStartTime);

            if (scrollingAxis == Direction.Horizontal)
                hitObject.X = position;
            else
                hitObject.Y = position;
        }
    }
}
