// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public class ConstantSpeedChangeVisualiser : ISpeedChangeVisualiser
    {
        public double TimeRange { get; set; }

        public float ScrollLength { get; set; }

        public void ComputeInitialStates(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction)
        {
            foreach (var obj in hitObjects)
            {
                obj.LifetimeStart = GetDisplayStartTime(obj.HitObject.StartTime);

                if (obj.HitObject is IHasEndTime endTime)
                {
                    switch (direction)
                    {
                        case ScrollingDirection.Up:
                        case ScrollingDirection.Down:
                            obj.Height = GetLength(obj.HitObject.StartTime, endTime.EndTime);
                            break;
                        case ScrollingDirection.Left:
                        case ScrollingDirection.Right:
                            obj.Height = GetLength(obj.HitObject.StartTime, endTime.EndTime);
                            break;
                    }
                }

                ComputeInitialStates(obj.NestedHitObjects, direction);

                // Nested hitobjects don't need to scroll, but they do need accurate positions
                UpdatePositions(obj.NestedHitObjects, direction, obj.HitObject.StartTime);
            }
        }

        public void UpdatePositions(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double currentTime)
        {
            foreach (var obj in hitObjects)
            {
                switch (direction)
                {
                    case ScrollingDirection.Up:
                        obj.Y = PositionAt(currentTime, obj.HitObject.StartTime);
                        break;
                    case ScrollingDirection.Down:
                        obj.Y = -PositionAt(currentTime, obj.HitObject.StartTime);
                        break;
                    case ScrollingDirection.Left:
                        obj.X = PositionAt(currentTime, obj.HitObject.StartTime);
                        break;
                    case ScrollingDirection.Right:
                        obj.X = -PositionAt(currentTime, obj.HitObject.StartTime);
                        break;
                }
            }
        }

        public double GetDisplayStartTime(double startTime) => startTime - TimeRange;

        public float GetLength(double startTime, double endTime)
        {
            // At the hitobject's end time, the hitobject will be positioned such that its end rests at the origin.
            // This results in a negative-position value, and the absolute of it indicates the length of the hitobject.
            return -PositionAt(endTime, startTime);
        }

        public float PositionAt(double currentTime, double startTime) => (float)((startTime - currentTime) / TimeRange * ScrollLength);
    }
}
