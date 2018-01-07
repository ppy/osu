// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Framework.Caching;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Lists;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI.Scrolling
{
    public class ScrollingHitObjectContainer : Playfield.HitObjectContainer
    {
        public readonly BindableDouble TimeRange = new BindableDouble
        {
            MinValue = 0,
            MaxValue = double.MaxValue
        };

        private readonly ScrollingDirection direction;

        private Cached positionCache = new Cached();

        public ScrollingHitObjectContainer(ScrollingDirection direction)
        {
            this.direction = direction;

            RelativeSizeAxes = Axes.Both;

            TimeRange.ValueChanged += v => positionCache.Invalidate();
        }

        public override void Add(DrawableHitObject hitObject)
        {
            positionCache.Invalidate();
            base.Add(hitObject);
        }

        public override bool Remove(DrawableHitObject hitObject)
        {
            var result = base.Remove(hitObject);
            if (result)
                positionCache.Invalidate();
            return result;
        }

        private readonly SortedList<MultiplierControlPoint> controlPoints = new SortedList<MultiplierControlPoint>();

        public void AddControlPoint(MultiplierControlPoint controlPoint)
        {
            controlPoints.Add(controlPoint);
            positionCache.Invalidate();
        }

        public bool RemoveControlPoint(MultiplierControlPoint controlPoint)
        {
            var result = controlPoints.Remove(controlPoint);
            if (result)
                positionCache.Invalidate();
            return result;
        }

        public override bool Invalidate(Invalidation invalidation = Invalidation.All, Drawable source = null, bool shallPropagate = true)
        {
            if ((invalidation & (Invalidation.RequiredParentSizeToFit | Invalidation.DrawInfo)) > 0)
                positionCache.Invalidate();

            return base.Invalidate(invalidation, source, shallPropagate);
        }

        private readonly Dictionary<DrawableHitObject, double> hitObjectPositions = new Dictionary<DrawableHitObject, double>();

        protected override void Update()
        {
            base.Update();

            if (positionCache.IsValid)
                return;

            foreach (var obj in Objects)
            {
                var startPosition = hitObjectPositions[obj] = positionAt(obj.HitObject.StartTime);

                obj.LifetimeStart = obj.HitObject.StartTime - TimeRange - 1000;
                obj.LifetimeEnd = ((obj.HitObject as IHasEndTime)?.EndTime ?? obj.HitObject.StartTime) + TimeRange + 1000;

                if (!(obj.HitObject is IHasEndTime endTime))
                    continue;

                var length = positionAt(endTime.EndTime) - startPosition;

                switch (direction)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        obj.Height = (float)(length * DrawHeight);
                        break;
                    case ScrollingDirection.Left:
                    case ScrollingDirection.Right:
                        obj.Width = (float)(length * DrawWidth);
                        break;
                }
            }

            positionCache.Validate();
        }

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            // We need to calculate this as soon as possible after lifetimes so that hitobjects
            // get the final say in their positions

            var timelinePosition = positionAt(Time.Current);

            foreach (var obj in AliveObjects)
            {
                var finalPosition = hitObjectPositions[obj] - timelinePosition;

                switch (direction)
                {
                    case ScrollingDirection.Up:
                        obj.Y = (float)(finalPosition * DrawHeight);
                        break;
                    case ScrollingDirection.Down:
                        obj.Y = (float)(-finalPosition * DrawHeight);
                        break;
                    case ScrollingDirection.Left:
                        obj.X = (float)(finalPosition * DrawWidth);
                        break;
                    case ScrollingDirection.Right:
                        obj.X = (float)(-finalPosition * DrawWidth);
                        break;
                }
            }
        }

        private double positionAt(double time)
        {
            double length = 0;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                var current = controlPoints[i];
                var next = i < controlPoints.Count - 1 ? controlPoints[i + 1] : null;

                if (i > 0 && current.StartTime > time)
                    continue;

                // Duration of the current control point
                var currentDuration = (next?.StartTime ?? double.PositiveInfinity) - current.StartTime;

                length += Math.Min(currentDuration, time - current.StartTime) * current.Multiplier / TimeRange;
            }

            return length;
        }
    }
}
