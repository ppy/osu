// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        private Cached initialStateCache = new Cached();

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
                initialStateCache.Invalidate();
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
                    computeInitialStateRecursive(obj);
                initialStateCache.Validate();
            }
        }

        private void computeInitialStateRecursive(DrawableHitObject hitObject)
        {
            hitObject.LifetimeStart = scrollingInfo.Algorithm.GetDisplayStartTime(hitObject.HitObject.StartTime, timeRange.Value);

            if (hitObject.HitObject is IHasEndTime endTime)
            {
                switch (direction.Value)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        hitObject.Height = scrollingInfo.Algorithm.GetLength(hitObject.HitObject.StartTime, endTime.EndTime, timeRange.Value, scrollLength);
                        break;
                    case ScrollingDirection.Left:
                    case ScrollingDirection.Right:
                        hitObject.Width = scrollingInfo.Algorithm.GetLength(hitObject.HitObject.StartTime, endTime.EndTime, timeRange.Value, scrollLength);
                        break;
                }
            }

            foreach (var obj in hitObject.NestedHitObjects)
            {
                computeInitialStateRecursive(obj);

                // Nested hitobjects don't need to scroll, but they do need accurate positions
                updatePosition(obj, hitObject.HitObject.StartTime);
            }
        }

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
