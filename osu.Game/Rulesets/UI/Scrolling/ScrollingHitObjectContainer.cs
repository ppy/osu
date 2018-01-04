// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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

        protected override void UpdateAfterChildrenLife()
        {
            base.UpdateAfterChildrenLife();

            // We need to calculate this as soon as possible so that hitobjects
            // get the final say in their positions

            var currentMultiplier = controlPointAt(Time.Current);

            foreach (var obj in AliveObjects)
            {
                // Todo: We may need to consider scale here
                var relativePosition = (Time.Current - obj.HitObject.StartTime) * currentMultiplier.Multiplier / TimeRange;
                var finalPosition = (float)relativePosition * DrawSize;

                switch (direction)
                {
                    case ScrollingDirection.Up:
                        obj.Y = -finalPosition.Y;
                        break;
                    case ScrollingDirection.Down:
                        obj.Y = finalPosition.Y;
                        break;
                    case ScrollingDirection.Left:
                        obj.X = -finalPosition.X;
                        break;
                    case ScrollingDirection.Right:
                        obj.X = finalPosition.X;
                        break;
                }

                if (!(obj.HitObject is IHasEndTime endTime))
                    continue;

                // Todo: We may need to consider scale here
                var relativeEndPosition = (Time.Current - endTime.EndTime) * currentMultiplier.Multiplier / TimeRange;
                var finalEndPosition = (float)relativeEndPosition * DrawSize;

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

        private readonly MultiplierControlPoint searchingPoint = new MultiplierControlPoint();
        private MultiplierControlPoint controlPointAt(double time)
        {
            if (ControlPoints.Count == 0)
                return new MultiplierControlPoint(double.MinValue);

            if (time < ControlPoints[0].StartTime)
                return ControlPoints[0];

            searchingPoint.StartTime = time;

            int index = ControlPoints.BinarySearch(searchingPoint);
            if (index < 0)
                index = ~index - 1;

            return ControlPoints[index];
        }
    }
}
