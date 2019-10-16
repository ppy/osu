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
            float dx = Math.Max(centrePosition.X, DrawWidth - centrePosition.X);
            float dy = Math.Max(centrePosition.Y, DrawHeight - centrePosition.Y);
            float maxDistance = new Vector2(dx, dy).Length;

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
