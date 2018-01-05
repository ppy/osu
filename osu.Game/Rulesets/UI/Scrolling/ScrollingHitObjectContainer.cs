// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Lists;
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

        public readonly SortedList<MultiplierControlPoint> ControlPoints = new SortedList<MultiplierControlPoint>();

        private readonly ScrollingDirection direction;

        public ScrollingHitObjectContainer(ScrollingDirection direction)
        {
            this.direction = direction;

            RelativeSizeAxes = Axes.Both;
        }

        protected override bool UpdateChildrenLife()
        {
            foreach (var obj in Objects)
            {
                obj.LifetimeStart = obj.HitObject.StartTime - TimeRange * 2;
                obj.LifetimeEnd = ((obj.HitObject as IHasEndTime)?.EndTime ?? obj.HitObject.StartTime) + TimeRange * 2;
            }

            return base.UpdateChildrenLife();
        }

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            // We need to calculate this as soon as possible after lifetimes so that hitobjects
            // get the final say in their positions

            var timelinePosition = positionAt(Time.Current);

            foreach (var obj in AliveObjects)
            {
                var finalPosition = positionAt(obj.HitObject.StartTime);

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

                if (!(obj.HitObject is IHasEndTime endTime))
                    continue;

                // Todo: We may need to consider scale here
                var finalEndPosition = positionAt(endTime.EndTime);

                float length = Vector2.Distance(finalPosition, finalEndPosition);

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
        }

        private Vector2 positionAt(double time)
        {
            float length = 0;
            for (int i = 0; i < ControlPoints.Count; i++)
            {
                var current = ControlPoints[i];
                var next = i < ControlPoints.Count - 1 ? ControlPoints[i + 1] : null;

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
