// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    public class FollowPointRenderer : ConnectionRenderer<OsuHitObject>
    {
        private int pointDistance = 32;

        /// <summary>
        /// Determines how much space there is between points.
        /// </summary>
        public int PointDistance
        {
            get => pointDistance;
            set
            {
                if (pointDistance == value) return;

                pointDistance = value;
                update();
            }
        }

        private int preEmpt = 800;

        /// <summary>
        /// Follow points to the next hitobject start appearing for this many milliseconds before an hitobject's end time.
        /// </summary>
        public int PreEmpt
        {
            get => preEmpt;
            set
            {
                if (preEmpt == value) return;

                preEmpt = value;
                update();
            }
        }

        private IEnumerable<OsuHitObject> hitObjects;

        public override IEnumerable<OsuHitObject> HitObjects
        {
            get => hitObjects;
            set
            {
                hitObjects = value;
                update();
            }
        }

        public override bool RemoveCompletedTransforms => false;

        private void update()
        {
            ClearInternal();

            if (hitObjects == null)
                return;

            OsuHitObject prevHitObject = null;

            foreach (var currHitObject in hitObjects)
            {
                if (prevHitObject != null && !currHitObject.NewCombo && !(prevHitObject is Spinner) && !(currHitObject is Spinner))
                {
                    Vector2 startPosition = prevHitObject.EndPosition;
                    Vector2 endPosition = currHitObject.Position;
                    double startTime = (prevHitObject as IHasEndTime)?.EndTime ?? prevHitObject.StartTime;
                    double endTime = currHitObject.StartTime;

                    Vector2 distanceVector = endPosition - startPosition;
                    int distance = (int)distanceVector.Length;
                    float rotation = (float)(Math.Atan2(distanceVector.Y, distanceVector.X) * (180 / Math.PI));
                    double duration = endTime - startTime;

                    for (int d = (int)(PointDistance * 1.5); d < distance - PointDistance; d += PointDistance)
                    {
                        float fraction = (float)d / distance;
                        Vector2 pointStartPosition = startPosition + (fraction - 0.1f) * distanceVector;
                        Vector2 pointEndPosition = startPosition + fraction * distanceVector;
                        double fadeOutTime = startTime + fraction * duration;
                        double fadeInTime = fadeOutTime - PreEmpt;

                        FollowPoint fp;

                        AddInternal(fp = new FollowPoint
                        {
                            Position = pointStartPosition,
                            Rotation = rotation,
                            Alpha = 0,
                            Scale = new Vector2(1.5f * currHitObject.Scale),
                        });

                        using (fp.BeginAbsoluteSequence(fadeInTime))
                        {
                            fp.FadeIn(currHitObject.TimeFadeIn);
                            fp.ScaleTo(currHitObject.Scale, currHitObject.TimeFadeIn, Easing.Out);

                            fp.MoveTo(pointEndPosition, currHitObject.TimeFadeIn, Easing.Out);

                            fp.Delay(fadeOutTime - fadeInTime).FadeOut(currHitObject.TimeFadeIn);
                        }

                        fp.Expire(true);
                    }
                }

                prevHitObject = currHitObject;
            }
        }
    }
}
