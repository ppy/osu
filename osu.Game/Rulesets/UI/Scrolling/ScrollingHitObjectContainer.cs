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
using OpenTK;

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

        private readonly Dictionary<DrawableHitObject, Vector2> hitObjectPositions = new Dictionary<DrawableHitObject, Vector2>();

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

                var endPosition = positionAt(endTime.EndTime);

                float length = Vector2.Distance(startPosition, endPosition);

                switch (direction)
                {
                    case ScrollingDirection.Up:
                    case ScrollingDirection.Down:
                        obj.Height = length;
                        break;
                    case ScrollingDirection.Left:
                    case ScrollingDirection.Right:
                        obj.Width = length;
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
                var finalPosition = hitObjectPositions[obj];

                switch (direction)
                {
                    case ScrollingDirection.Up:
                        obj.Y = finalPosition.Y - timelinePosition.Y;
                        break;
                    case ScrollingDirection.Down:
                        obj.Y = -finalPosition.Y + timelinePosition.Y;
                        break;
                    case ScrollingDirection.Left:
                        obj.X = finalPosition.X - timelinePosition.X;
                        break;
                    case ScrollingDirection.Right:
                        obj.X = -finalPosition.X + timelinePosition.X;
                        break;
                }
            }
        }

        private Vector2 positionAt(double time)
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

                length += (float)(Math.Min(currentDuration, time - current.StartTime) * current.Multiplier / TimeRange);
            }

            return length * DrawSize;
        }
    }
}
