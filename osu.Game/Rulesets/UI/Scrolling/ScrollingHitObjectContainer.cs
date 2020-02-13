// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public class ScrollingHitObjectContainer : HitObjectContainer
    {
        private readonly IBindable<double> timeRange = new BindableDouble();
        private readonly IBindable<ScrollingDirection> direction = new Bindable<ScrollingDirection>();

        [Resolved]
        private IScrollingInfo scrollingInfo { get; set; }

        private readonly Cached initialStateCache = new Cached();

        public ScrollingHitObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            direction.BindTo(scrollingInfo.Direction);
            timeRange.BindTo(scrollingInfo.TimeRange);

            direction.ValueChanged += _ => initialStateCache.Invalidate();
            timeRange.ValueChanged += _ => initialStateCache.Invalidate();
        }

        public override void Add(DrawableHitObject hitObject)
        {
            initialStateCache.Invalidate();
            base.Add(hitObject);
        }

        public override bool Remove(DrawableHitObject hitObject)
        {
            var result = base.Remove(hitObject);

            if (result)
            {
                initialStateCache.Invalidate();
                hitObjectInitialStateCache.Remove(hitObject);
            }

            return result;
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & (Invalidation.RequiredParentSizeToFit | Invalidation.DrawInfo)) > 0)
                initialStateCache.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        private float scrollLength;

        protected override void Update()
        {
            base.Update();

            if (!initialStateCache.IsValid)
            {
                foreach (var cached in hitObjectInitialStateCache.Values)
                    cached.Invalidate();

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

                scrollingInfo.Algorithm.Reset();

                foreach (var obj in Objects)
                {
                    computeLifetimeStartRecursive(obj);
                    computeInitialStateRecursive(obj);
                }

                initialStateCache.Validate();
            }
        }

        private void computeLifetimeStartRecursive(DrawableHitObject hitObject)
        {
            hitObject.LifetimeStart = computeOriginAdjustedLifetimeStart(hitObject);

            foreach (var obj in hitObject.NestedHitObjects)
                computeLifetimeStartRecursive(obj);
        }

        private readonly Dictionary<DrawableHitObject, Cached> hitObjectInitialStateCache = new Dictionary<DrawableHitObject, Cached>();

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

        // Cant use AddOnce() since the delegate is re-constructed every invocation
        private void computeInitialStateRecursive(DrawableHitObject hitObject) => hitObject.Schedule(() =>
        {
            if (!hitObjectInitialStateCache.TryGetValue(hitObject, out var cached))
                cached = hitObjectInitialStateCache[hitObject] = new Cached();

            if (cached.IsValid)
                return;

            if (hitObject.HitObject is IHasEndTime e)
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

            cached.Validate();
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
    }
}
