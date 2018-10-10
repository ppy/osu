// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public class ConstantSpeedChangeVisualiser : ISpeedChangeVisualiser
    {
        public void ComputeInitialStates(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double timeRange, Vector2 length)
        {
            foreach (var obj in hitObjects)
            {
                obj.LifetimeStart = obj.HitObject.StartTime - timeRange;

                if (obj.HitObject is IHasEndTime endTime)
                {
                    var hitObjectLength = (endTime.EndTime - obj.HitObject.StartTime) / timeRange;

                    switch (direction)
                    {
                        case ScrollingDirection.Up:
                        case ScrollingDirection.Down:
                            obj.Height = (float)(hitObjectLength * length.Y);
                            break;
                        case ScrollingDirection.Left:
                        case ScrollingDirection.Right:
                            obj.Width = (float)(hitObjectLength * length.X);
                            break;
                    }
                }

                ComputeInitialStates(obj.NestedHitObjects, direction, timeRange, length);

                // Nested hitobjects don't need to scroll, but they do need accurate positions
                UpdatePositions(obj.NestedHitObjects, direction, obj.HitObject.StartTime, timeRange, length);
            }
        }

        public void UpdatePositions(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double currentTime, double timeRange, Vector2 length)
        {
            foreach (var obj in hitObjects)
            {
                var position = (obj.HitObject.StartTime - currentTime) / timeRange;

                switch (direction)
                {
                    case ScrollingDirection.Up:
                        obj.Y = (float)(position * length.Y);
                        break;
                    case ScrollingDirection.Down:
                        obj.Y = (float)(-position * length.Y);
                        break;
                    case ScrollingDirection.Left:
                        obj.X = (float)(position * length.X);
                        break;
                    case ScrollingDirection.Right:
                        obj.X = (float)(-position * length.X);
                        break;
                }
            }
        }
    }
}
