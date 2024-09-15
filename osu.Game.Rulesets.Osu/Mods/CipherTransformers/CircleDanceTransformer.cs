// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;

namespace osu.Game.Rulesets.Osu.Mods.CipherTransformers
{
    public class CircleDanceTransformer : CipherTransformer
    {
        public CircleDanceTransformer(float circleRadius, float speed)
        {
            this.speed = speed;
            this.circleRadius = circleRadius;
        }

        private float arc;
        private readonly float circleRadius;
        private readonly float speed;

        public override Vector2 Transform(Vector2 mousePosition)
        {
            arc += speed;
            float x = (float)(circleRadius * Math.Cos(arc));
            float y = (float)(circleRadius * Math.Sin(arc));
            return mousePosition + new Vector2(x, y);
        }
    }
}
