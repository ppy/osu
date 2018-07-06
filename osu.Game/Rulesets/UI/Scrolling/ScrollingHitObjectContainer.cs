// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;
using osu.Game.Rulesets.UI.Scrolling.Visualisers;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public class ScrollingHitObjectContainer : HitObjectContainer
    {
        /// <summary>
        /// The duration required to scroll through one length of the <see cref="ScrollingHitObjectContainer"/> before any control point adjustments.
        /// </summary>
        public readonly BindableDouble TimeRange = new BindableDouble
        {
            MinValue = 0,
            MaxValue = double.MaxValue
        };

        /// <summary>
        /// The control points that adjust the scrolling speed.
        /// </summary>
        protected readonly SortedList<MultiplierControlPoint> ControlPoints = new SortedList<MultiplierControlPoint>();

        public readonly Bindable<ScrollingDirection> Direction = new Bindable<ScrollingDirection>();

        private Cached initialStateCache = new Cached();

        public ScrollingHitObjectContainer()
        {
            RelativeSizeAxes = Axes.Both;

            TimeRange.ValueChanged += _ => initialStateCache.Invalidate();
            Direction.ValueChanged += _ => initialStateCache.Invalidate();
        }

        private ISpeedChangeVisualiser speedChangeVisualiser;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            switch (config.Get<SpeedChangeVisualisationMethod>(OsuSetting.SpeedChangeVisualisation))
            {
                case SpeedChangeVisualisationMethod.Sequential:
                    speedChangeVisualiser = new SequentialSpeedChangeVisualiser(ControlPoints);
                    break;
                case SpeedChangeVisualisationMethod.Overlapping:
                    speedChangeVisualiser = new OverlappingSpeedChangeVisualiser(ControlPoints);
                    break;
            }
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

            if (!initialStateCache.IsValid)
            {
                speedChangeVisualiser.ComputeInitialStates(Objects, Direction, TimeRange, DrawSize);
                initialStateCache.Validate();
            }
        }

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            // We need to calculate this as soon as possible after lifetimes so that hitobjects get the final say in their positions
            speedChangeVisualiser.UpdatePositions(AliveObjects, Direction, Time.Current, TimeRange, DrawSize);
        }
    }
}
