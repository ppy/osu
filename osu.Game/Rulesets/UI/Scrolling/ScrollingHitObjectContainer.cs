// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Layout;
using osu.Framework.Threading;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osuTK;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public class ScrollingHitObjectContainer : HitObjectContainer
    {
        private readonly IBindable<double> timeRange = new BindableDouble();
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();
        private readonly Dictionary<DrawableHitObject, InitialState> hitObjectInitialStateCache = new Dictionary<DrawableHitObject, InitialState>();

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        // Responds to changes in the layout. When the layout changes, all hit object states must be recomputed.
        private readonly LayoutValue layoutCache = new LayoutValue(Invalidation.RequiredParentSizeToFit | Invalidation.DrawInfo);

        // A combined cache across all hit object states to reduce per-update iterations.
        // When invalidated, one or more (but not necessarily all) hitobject states must be re-validated.
        private readonly Cached combinedObjCache = new Cached();

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

        public override void Add(DrawableHitObject hitObject)
        {
            combinedObjCache.Invalidate();
            hitObject.DefaultsApplied += onDefaultsApplied;
            base.Add(hitObject);
        }

        public override bool Remove(DrawableHitObject hitObject)
        {
            var result = base.Remove(hitObject);

            if (result)
            {
                combinedObjCache.Invalidate();
                hitObjectInitialStateCache.Remove(hitObject);

                hitObject.DefaultsApplied -= onDefaultsApplied;
            }

            return result;
        }

        public override void Clear(bool disposeChildren = true)
        {
            foreach (var h in Objects)
                h.DefaultsApplied -= onDefaultsApplied;

            base.Clear(disposeChildren);

            combinedObjCache.Invalidate();
            hitObjectInitialStateCache.Clear();
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

        private void onDefaultsApplied(DrawableHitObject drawableObject)
        {
            // The cache may not exist if the hitobject state hasn't been computed yet (e.g. if the hitobject was added + defaults applied in the same frame).
            // In such a case, combinedObjCache will take care of updating the hitobject.
            if (hitObjectInitialStateCache.TryGetValue(drawableObject, out var state))
            {
                combinedObjCache.Invalidate();
                state.Cache.Invalidate();
            }
        }

        private float scrollLength;

        protected override void Update()
        {
            base.Update();

            if (!layoutCache.IsValid)
            {
                foreach (var state in hitObjectInitialStateCache.Values)
                    state.Cache.Invalidate();
                combinedObjCache.Invalidate();

                scrollingInfo.Algorithm.Reset();

                layoutCache.Validate();
            }

            if (!combinedObjCache.IsValid)
            {
                switch (direction.Value)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        scrollLength = DrawSize.Y;
                        break;

                    default:
                        scrollLength = DrawSize.X;
                        break;
                }

                foreach (var obj in Objects)
                {
                    if (!hitObjectInitialStateCache.TryGetValue(obj, out var state))
                        state = hitObjectInitialStateCache[obj] = new InitialState(new Cached());

                    if (state.Cache.IsValid)
                        continue;

                    state.ScheduledComputation?.Cancel();
                    state.ScheduledComputation = computeInitialStateRecursive(obj);

                    computeLifetimeStartRecursive(obj);

                    state.Cache.Validate();
                }

                combinedObjCache.Validate();
            }
        }

        private void computeLifetimeStartRecursive(DrawableHitObject hitObject)
        {
            hitObject.LifetimeStart = computeOriginAdjustedLifetimeStart(hitObject);

            foreach (var obj in hitObject.NestedHitObjects)
                computeLifetimeStartRecursive(obj);
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

        private ScheduledDelegate computeInitialStateRecursive(DrawableHitObject hitObject) => hitObject.Schedule(() =>
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
                computeInitialStateRecursive(obj);

                // Nested hitobjects don't need to scroll, but they do need accurate positions
                updatePosition(obj, hitObject.HitObject.StartTime);
            }
        });

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            // We need to calculate hitobject positions as soon as possible after lifetimes so that hitobjects get the final say in their positions
            foreach (var obj in AliveObjects)
                updatePosition(obj, Time.Current);
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

        private class InitialState
        {
            [NotNull]
            public readonly Cached Cache;

            [CanBeNull]
            public ScheduledDelegate ScheduledComputation;

            public InitialState(Cached cache)
            {
                Cache = cache;
            }
        }
    }
}
