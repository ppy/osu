// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI.Scrolling.Algorithms;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public abstract class ScrollingHitObjectContainer : HitObjectContainer
    {
        public readonly BindableDouble TimeRange = new BindableDouble
        {
            MinValue = 0,
            MaxValue = double.MaxValue
        };

        protected readonly SortedList<MultiplierControlPoint> ControlPoints = new SortedList<MultiplierControlPoint>();

        private readonly ScrollingDirection direction;

        private Cached initialStateCache = new Cached();

        protected ScrollingHitObjectContainer(ScrollingDirection direction)
        {
            this.direction = direction;

            RelativeSizeAxes = Axes.Both;

            TimeRange.ValueChanged += v => initialStateCache.Invalidate();
        }

        private IScrollingAlgorithm scrollingAlgorithm;
        protected override void LoadComplete()
        {
            base.LoadComplete();

            scrollingAlgorithm = CreateScrollingAlgorithm();
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

        public void AddControlPoint(MultiplierControlPoint controlPoint)
        {
            ControlPoints.Add(controlPoint);
            initialStateCache.Invalidate();
        }

        public bool RemoveControlPoint(MultiplierControlPoint controlPoint)
        {
            var result = ControlPoints.Remove(controlPoint);
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

        protected override void Update()
        {
            base.Update();

            if (initialStateCache.IsValid)
                return;

            scrollingAlgorithm.ComputeInitialStates(Objects, direction, TimeRange, DrawSize);

            initialStateCache.Validate();
        }

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            // We need to calculate this as soon as possible after lifetimes so that hitobjects
            // get the final say in their positions

            scrollingAlgorithm.ComputePositions(AliveObjects, direction, Time.Current, TimeRange, DrawSize);
        }

        /// <summary>
        /// Creates the algorithm that will process the positions of the <see cref="DrawableHitObject"/>s.
        /// </summary>
        protected abstract IScrollingAlgorithm CreateScrollingAlgorithm();
    }
}
