// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Default
{
    public class DefaultSmoke : Smoke
    {
        private const double fade_out_delay = 8000;
        private const double fade_out_speed = 3;
        private const double fade_out_duration = 50;
        private const float alpha = 0.5f;

        protected override double LifetimeAfterSmokeEnd => fade_out_delay + fade_out_duration + (SmokeEndTime - SmokeStartTime) / fade_out_speed;

        public DefaultSmoke()
        {
            Radius = 2;
        }

        protected override DrawNode CreateDrawNode() => new DefaultSmokeDrawNode(this);

        private class DefaultSmokeDrawNode : SmokeDrawNode
        {
            private double fadeOutTime;

            public DefaultSmokeDrawNode(ITexturedShaderDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                fadeOutTime = SmokeStartTime + fade_out_speed * (CurrentTime - (SmokeEndTime + fade_out_delay));
            }

            protected override Color4 PointColour(SmokePoint point)
            {
                var color = Color4.White;
                color.A = alpha;

                double timeDoingFadeOut = fadeOutTime - point.Time;

                if (timeDoingFadeOut > 0)
                {
                    float fraction = Math.Clamp((float)(1 - (timeDoingFadeOut / fade_out_duration)), 0, 1);
                    fraction = MathF.Pow(fraction, 5);
                    color.A *= fraction;
                }

                return color;
            }

            protected override float PointScale(SmokePoint point) => 1f;

            protected override Vector2 PointDirection(SmokePoint point) => point.Direction;
        }
    }
}
