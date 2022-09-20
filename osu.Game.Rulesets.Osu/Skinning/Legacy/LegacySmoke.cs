// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacySmoke : Smoke
    {
        // fade values
        private const double initial_fade_out_duration = 4000;

        private const double re_fade_in_speed = 3;
        private const double re_fade_in_duration = 50;

        private const double final_fade_out_speed = 2;
        private const double final_fade_out_duration = 8000;

        private const float initial_alpha = 0.6f;
        private const float re_fade_in_alpha = 1f;

        // scale values
        private const double scale_duration = 1200;

        private const float initial_scale = 0.65f;
        private const float final_scale = 1f;

        // rotation values
        private const double rotation_duration = 500;

        private const float max_rotation = 0.25f;

        protected int RotationSeed { get; set; } = RNG.Next();

        protected override double LifetimeAfterSmokeEnd
        {
            get
            {
                double initialFadeOutDurationTrunc = Math.Min(initial_fade_out_duration, SmokeEndTime - SmokeStartTime);
                return final_fade_out_duration + initialFadeOutDurationTrunc / re_fade_in_speed + initialFadeOutDurationTrunc / final_fade_out_speed;
            }
        }

        private readonly ISkin skin;

        public LegacySmoke(ISkin skin)
        {
            this.skin = skin;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Texture = skin.GetTexture("cursor-smoke");
        }

        protected override DrawNode CreateDrawNode() => new LegacySmokeDrawNode(this);

        protected class LegacySmokeDrawNode : SmokeDrawNode
        {
            protected new LegacySmoke Source => (LegacySmoke)base.Source;

            private double initialFadeOutDurationTrunc;
            private double initialFadeOutTime;
            private double reFadeInTime;
            private double finalFadeOutTime;

            private int rotationSeed;
            private Random rotationRNG = new Random();

            public LegacySmokeDrawNode(ITexturedShaderDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                initialFadeOutDurationTrunc = Math.Min(initial_fade_out_duration, SmokeEndTime - SmokeStartTime);
                rotationSeed = Source.RotationSeed;

                rotationRNG = new Random(rotationSeed);
                initialFadeOutTime = Math.Min(CurrentTime, SmokeEndTime);
                reFadeInTime = re_fade_in_speed * (CurrentTime - SmokeEndTime) + SmokeEndTime - initialFadeOutDurationTrunc;
                finalFadeOutTime = final_fade_out_speed * (CurrentTime - SmokeEndTime) + SmokeEndTime - initialFadeOutDurationTrunc * (1 + 1 / re_fade_in_speed);
            }

            protected override Color4 PointColour(SmokePoint point)
            {
                var color = Color4.White;

                double timeDoingInitialFadeOut = initialFadeOutTime - point.Time;

                if (timeDoingInitialFadeOut > 0)
                {
                    float fraction = Math.Clamp((float)(timeDoingInitialFadeOut / initial_fade_out_duration), 0, 1);
                    color.A = (1 - fraction) * initial_alpha;
                }

                if (color.A > 0)
                {
                    double timeDoingReFadeIn = reFadeInTime - point.Time;
                    double timeDoingFinalFadeOut = finalFadeOutTime - point.Time;

                    if (timeDoingFinalFadeOut > 0)
                    {
                        float fraction = Math.Clamp((float)(timeDoingFinalFadeOut / final_fade_out_duration), 0, 1);
                        fraction = MathF.Pow(fraction, 5);
                        color.A = (1 - fraction) * re_fade_in_alpha;
                    }
                    else if (timeDoingReFadeIn > 0)
                    {
                        float fraction = Math.Clamp((float)(timeDoingReFadeIn / re_fade_in_duration), 0, 1);
                        fraction = 1 - MathF.Pow(1 - fraction, 5);
                        color.A = fraction * (re_fade_in_alpha - color.A) + color.A;
                    }
                }

                return color;
            }

            protected override float PointScale(SmokePoint point)
            {
                double timeDoingScale = CurrentTime - point.Time;
                float fraction = Math.Clamp((float)(timeDoingScale / scale_duration), 0, 1);
                fraction = 1 - MathF.Pow(1 - fraction, 5);
                return fraction * (final_scale - initial_scale) + initial_scale;
            }

            protected override Vector2 PointDirection(SmokePoint point)
            {
                float initialAngle = MathF.Atan2(point.Direction.Y, point.Direction.X);
                float finalAngle = initialAngle + nextRotation();

                double timeDoingRotation = CurrentTime - point.Time;
                float fraction = Math.Clamp((float)(timeDoingRotation / rotation_duration), 0, 1);
                fraction = 1 - MathF.Pow(1 - fraction, 5);
                float angle = fraction * (finalAngle - initialAngle) + initialAngle;

                return new Vector2(MathF.Sin(angle), -MathF.Cos(angle));
            }

            private float nextRotation() => max_rotation * ((float)rotationRNG.NextDouble() * 2 - 1);
        }
    }
}
