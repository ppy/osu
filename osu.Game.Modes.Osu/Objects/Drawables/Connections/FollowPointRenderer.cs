// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Modes.Osu.Objects.Drawables.Connections;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Osu.Objects.Drawables
{
    public class FollowPointRenderer : ConnectionRenderer<OsuHitObject>
    {
        /// <summary>
        /// Determines how much space there is between points.
        /// </summary>
        public int PointDistance = 32;

        /// <summary>
        /// Follow points to the next hitobject start appearing for this many milliseconds before an hitobject's end time.
        /// </summary>
        public int PreEmpt = 800;

        public override void AddConnections(IEnumerable<OsuHitObject> hitObjects)
        {
            OsuHitObject prevHitObject = null;
            foreach (var currHitObject in hitObjects)
            {
                if (prevHitObject != null && !currHitObject.NewCombo && !(prevHitObject is Spinner) && !(currHitObject is Spinner))
                {
                    Vector2 startPosition = prevHitObject.EndPosition;
                    Vector2 endPosition = currHitObject.Position;
                    double startTime = prevHitObject.EndTime;
                    double endTime = currHitObject.StartTime;

                    Vector2 distanceVector = endPosition - startPosition;
                    int distance = (int)distanceVector.Length;
                    float rotation = (float)Math.Atan2(distanceVector.Y, distanceVector.X);
                    double duration = endTime - startTime;

                    for (int d = (int)(PointDistance * 1.5); d < distance - PointDistance; d += PointDistance)
                    {
                        float fraction = ((float)d / distance);
                        Vector2 pointStartPosition = startPosition + (fraction - 0.1f) * distanceVector;
                        Vector2 pointEndPosition = startPosition + fraction * distanceVector;
                        double fadeOutTime = startTime + fraction * duration;
                        double fadeInTime = fadeOutTime - PreEmpt;

                        Add(new FollowPoint()
                        {
                            StartTime = fadeInTime,
                            EndTime = fadeOutTime,
                            Position = pointStartPosition,
                            EndPosition = pointEndPosition,
                            Rotation = rotation,
                        });
                    }
                }
                prevHitObject = currHitObject;
            }
        }
    }
}
