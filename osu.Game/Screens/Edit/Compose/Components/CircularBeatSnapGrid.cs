// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public abstract class CircularBeatSnapGrid : BeatSnapGrid
    {
        protected CircularBeatSnapGrid(HitObject hitObject, Vector2 centrePosition)
            : base(hitObject, centrePosition)
        {
        }

        protected override void CreateContent(Vector2 centrePosition)
        {
            float maxDistance = Math.Max(
                Vector2.Distance(centrePosition, Vector2.Zero),
                Math.Max(
                    Vector2.Distance(centrePosition, new Vector2(DrawWidth, 0)),
                    Math.Max(
                        Vector2.Distance(centrePosition, new Vector2(0, DrawHeight)),
                        Vector2.Distance(centrePosition, DrawSize))));

            int requiredCircles = (int)(maxDistance / DistanceSpacing);

            for (int i = 0; i < requiredCircles; i++)
            {
                float radius = (i + 1) * DistanceSpacing * 2;

                AddInternal(new CircularProgress
                {
                    Origin = Anchor.Centre,
                    Position = centrePosition,
                    Current = { Value = 1 },
                    Size = new Vector2(radius),
                    InnerRadius = 4 * 1f / radius,
                    Colour = GetColourForBeatIndex(i)
                });
            }
        }

        public override Vector2 GetSnapPosition(Vector2 position)
        {
            Vector2 direction = position - CentrePosition;
            float distance = direction.Length;

            float radius = DistanceSpacing;
            int radialCount = Math.Max(1, (int)Math.Round(distance / radius));

            if (radialCount <= 0)
                return position;

            Vector2 normalisedDirection = direction * new Vector2(1f / distance);

            return CentrePosition + normalisedDirection * radialCount * radius;
        }
    }
}
