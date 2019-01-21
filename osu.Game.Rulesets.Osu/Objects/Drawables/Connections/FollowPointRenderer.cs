// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osuTK;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Types;
using osu.Framework.Allocation;
using osu.Game.Configuration;
using osu.Framework.Configuration;

namespace osu.Game.Rulesets.Osu.Objects.Drawables.Connections
{
    public class FollowPointRenderer : ConnectionRenderer<OsuHitObject>
    {
        private Bindable<int> preEmpt;
        private Bindable<int> offset;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            /// <summary>
            /// Follow points to the next hitobject start appearing for this(preEmpt - offset) many milliseconds before an hitobject's end time.
            /// </summary>
            preEmpt = config.GetBindable<int>(OsuSetting.FollowPointAppearTime);
            offset = config.GetBindable<int>(OsuSetting.FollowPointDelay);
            preEmpt.ValueChanged += _ => update();
            offset.ValueChanged += _ => update();
        }

        private int pointDistance = 32;
        /// <summary>
        /// Determines how much space there is between points.
        /// </summary>
        public int PointDistance
        {
            get { return pointDistance; }
            set
            {
                if (pointDistance == value) return;
                pointDistance = value;
                update();
            }
        }

        private IEnumerable<OsuHitObject> hitObjects;
        public override IEnumerable<OsuHitObject> HitObjects
        {
            get { return hitObjects; }
            set
            {
                hitObjects = value;
                update();
            }
        }

        public override bool RemoveCompletedTransforms => false;

        private void update()
        {
            Clear();

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
                        double fadeInTime = startTime + fraction * duration - preEmpt.Value + offset.Value;

                        FollowPoint fp;

                        Add(fp = new FollowPoint
                        {
                            Position = pointStartPosition,
                            Rotation = rotation,
                            Alpha = 0,
                            Scale = new Vector2(1.5f),
                        });

                        using (fp.BeginAbsoluteSequence(fadeInTime))
                        {
                            fp.FadeIn(currHitObject.TimeFadeIn);
                            fp.ScaleTo(1, currHitObject.TimeFadeIn, Easing.Out);

                            fp.MoveTo(pointEndPosition, currHitObject.TimeFadeIn, Easing.Out);

                            fp.Delay(preEmpt.Value).FadeOut(currHitObject.TimeFadeIn);
                        }

                        fp.Expire(true);
                    }
                }
                prevHitObject = currHitObject;
            }
        }
    }
}
