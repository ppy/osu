// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.Objects.Drawables;
using osu.Game.Modes.UI;
using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Modes.Osu.UI
{
    public class OsuPlayfield : Playfield
    {
        private Container approachCircles;
        private Container judgementLayer;
        private Container followPointsLayer;

        public override Vector2 Size
        {
            get
            {
                var parentSize = Parent.DrawSize;
                var aspectSize = parentSize.X * 0.75f < parentSize.Y ? new Vector2(parentSize.X, parentSize.X * 0.75f) : new Vector2(parentSize.Y * 4f / 3f, parentSize.Y);

                return new Vector2(aspectSize.X / parentSize.X, aspectSize.Y / parentSize.Y) * base.Size;
            }
        }

        public OsuPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            Size = new Vector2(0.75f);

            Add(new Drawable[]
            {
                followPointsLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 1,
                },
                judgementLayer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = 0,
                },
                approachCircles = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Depth = -1,
                }
            });
        }

        public override void Add(DrawableHitObject h)
        {
            h.Depth = (float)h.HitObject.StartTime;
            DrawableHitCircle c = h as DrawableHitCircle;
            if (c != null)
            {
                approachCircles.Add(c.ApproachCircle.CreateProxy());
            }

            h.OnJudgement += judgement;

            base.Add(h);
        }

        public override void PostProcess()
        {
            AddFollowPoints();
        }

        private void judgement(DrawableHitObject h, JudgementInfo j)
        {
            HitExplosion explosion = new HitExplosion((OsuJudgementInfo)j, (OsuHitObject)h.HitObject);

            judgementLayer.Add(explosion);
        }

        public void AddFollowPoints(int startIndex = 0, int endIndex = -1)
        {
            var followLineDistance = 32;
            var followLinePreEmpt = 800;

            var hitObjects = new List<OsuHitObject>(HitObjects.Children
                .Select(d => (OsuHitObject)d.HitObject)
                .OrderBy(h => h.StartTime));

            if (endIndex < 0)
                endIndex = hitObjects.Count - 1;

            for (int i = startIndex + 1; i <= endIndex; i++)
            {
                var prevHitObject = hitObjects[i - 1];
                var currHitObject = hitObjects[i];

                if (prevHitObject.StartTime > currHitObject.StartTime)
                    throw new Exception();

                if (!currHitObject.NewCombo && !(prevHitObject is Spinner) && !(currHitObject is Spinner))
                {
                    Vector2 startPosition = prevHitObject.EndPosition;
                    Vector2 endPosition = currHitObject.Position;
                    double startTime = prevHitObject.EndTime;
                    double endTime = currHitObject.StartTime;

                    Vector2 distanceVector = endPosition - startPosition;
                    int distance = (int)distanceVector.Length;
                    float rotation = (float)Math.Atan2(distanceVector.Y, distanceVector.X);
                    double duration = endTime - startTime;

                    for (int d = (int)(followLineDistance * 1.5); d < distance - followLineDistance; d += followLineDistance)
                    {
                        float fraction = ((float)d / distance);
                        Vector2 pointStartPosition = startPosition + (fraction - 0.1f) * distanceVector;
                        Vector2 pointEndPosition = startPosition + fraction * distanceVector;
                        double fadeOutTime = startTime + fraction * duration;
                        double fadeInTime = fadeOutTime - followLinePreEmpt;

                        followPointsLayer.Add(new FollowPoint()
                        {
                            StartTime = fadeInTime,
                            EndTime = fadeOutTime,
                            Position = pointStartPosition,
                            EndPosition = pointEndPosition,
                            Rotation = rotation,
                        });
                    }
                }
            }
        }
    }
}