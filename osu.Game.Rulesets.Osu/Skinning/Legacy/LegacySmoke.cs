// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Rendering;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Skinning.Legacy
{
    public class LegacySmoke : Smoke
    {
        private const double initial_fade_out_duration = 2500;

        private const double re_fade_in_speed = 3;
        private const double re_fade_in_duration = 50;

        private const double final_fade_out_duration = 7500;

        private const float initial_alpha = 0.8f;
        private const float re_fade_in_alpha = 1.4f;

        protected override double LifetimeAfterSmokeEnd
        {
            get
            {
                double initialFadeOutDurationTrunc = Math.Min(initial_fade_out_duration, SmokeEndTime - SmokeStartTime);
                return final_fade_out_duration + initialFadeOutDurationTrunc * (1 + re_fade_in_speed);
            }
        }

        private ISkin skin;

        public LegacySmoke(ISkin skin)
        {
            this.skin = skin;
            Radius = 3;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Texture = skin.GetTexture("cursor-smoke");
        }

        protected override DrawNode CreateDrawNode() => new LegacySmokeDrawNode(this);

        protected class LegacySmokeDrawNode : SmokeDrawNode
        {
            private double initialFadeOutDurationTrunc;
            private double initialFadeOutTime;
            private double reFadeInTime;
            private double finalFadeOutTime;

            public LegacySmokeDrawNode(ITexturedShaderDrawable source)
                : base(source)
            {
            }

            public override void ApplyState()
            {
                base.ApplyState();

                initialFadeOutDurationTrunc = Math.Min(initial_fade_out_duration, SmokeEndTime - SmokeStartTime);
            }

            protected override void UpdateDrawVariables(IRenderer renderer)
            {
                base.UpdateDrawVariables(renderer);

                initialFadeOutTime = Math.Min(CurrentTime, SmokeEndTime);
                reFadeInTime = re_fade_in_speed * (CurrentTime - SmokeEndTime) + SmokeEndTime - initialFadeOutDurationTrunc;
                finalFadeOutTime = CurrentTime - initialFadeOutDurationTrunc * (1 + 1 / re_fade_in_speed);
            }

            protected override Color4 ColorAtTime(double pointTime)
            {
                var color = Color4.White;

                double timeDoingInitialFadeOut = initialFadeOutTime - pointTime;

                if (timeDoingInitialFadeOut > 0)
                {
                    float fraction = Math.Clamp((float)(timeDoingInitialFadeOut / initial_fade_out_duration), 0, 1);
                    fraction = MathF.Pow(fraction, 5);
                    color.A = (1 - fraction) * initial_alpha;
                }

                if (color.A > 0)
                {
                    double timeDoingReFadeIn = reFadeInTime - pointTime;
                    double timeDoingFinalFadeOut = finalFadeOutTime - pointTime;

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
        }
    }
}
