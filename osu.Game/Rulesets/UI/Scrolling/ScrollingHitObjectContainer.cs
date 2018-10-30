// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Game.Configuration;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
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

        private readonly ISpeedChangeVisualiser visualiser;

        private Cached initialStateCache = new Cached();

        public ScrollingHitObjectContainer(SpeedChangeVisualisationMethod visualisationMethod)
        {
            RelativeSizeAxes = Axes.Both;

            TimeRange.ValueChanged += _ => initialStateCache.Invalidate();
            Direction.ValueChanged += _ => initialStateCache.Invalidate();

            switch (visualisationMethod)
            {
                case SpeedChangeVisualisationMethod.Sequential:
                    visualiser = new SequentialSpeedChangeVisualiser(ControlPoints);
                    break;
                case SpeedChangeVisualisationMethod.Overlapping:
                    visualiser = new OverlappingSpeedChangeVisualiser(ControlPoints);
                    break;
                case SpeedChangeVisualisationMethod.Constant:
                    visualiser = new ConstantSpeedChangeVisualiser();
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

        private float scrollLength;

        protected override void Update()
        {
            base.Update();

            if (!initialStateCache.IsValid)
            {
                switch (Direction.Value)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        scrollLength = DrawSize.Y;
                        break;
                    default:
                        scrollLength = DrawSize.X;
                        break;
                }

                visualiser.Reset();

                foreach (var obj in Objects)
                    computeInitialStateRecursive(obj);
                initialStateCache.Validate();
            }
        }

        private void computeInitialStateRecursive(DrawableHitObject hitObject)
        {
            hitObject.LifetimeStart = visualiser.GetDisplayStartTime(hitObject.HitObject.StartTime, TimeRange);

            if (hitObject.HitObject is IHasEndTime endTime)
            {
                switch (Direction.Value)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        hitObject.Height = visualiser.GetLength(hitObject.HitObject.StartTime, endTime.EndTime, TimeRange, scrollLength);
                        break;
                    case ScrollingDirection.Left:
                    case ScrollingDirection.Right:
                        hitObject.Height = visualiser.GetLength(hitObject.HitObject.StartTime, endTime.EndTime, TimeRange, scrollLength);
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
            switch (Direction.Value)
            {
                case ScrollingDirection.Up:
                    hitObject.Y = visualiser.PositionAt(hitObject.HitObject.StartTime, currentTime, TimeRange, scrollLength);
                    break;
                case ScrollingDirection.Down:
                    hitObject.Y = -visualiser.PositionAt(hitObject.HitObject.StartTime, currentTime, TimeRange, scrollLength);
                    break;
                case ScrollingDirection.Left:
                    hitObject.X = visualiser.PositionAt(hitObject.HitObject.StartTime, currentTime, TimeRange, scrollLength);
                    break;
                case ScrollingDirection.Right:
                    hitObject.X = -visualiser.PositionAt(hitObject.HitObject.StartTime, currentTime, TimeRange, scrollLength);
                    break;
            }
        }
    }
}
