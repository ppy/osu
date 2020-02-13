// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osuTK;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuJudgement : DrawableJudgement
    {
        private SkinnableSprite lighting;
        private Bindable<Color4> lightingColour;

        public DrawableOsuJudgement(JudgementResult result, DrawableHitObject judgedObject)
            : base(result, judgedObject)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            if (config.Get<bool>(OsuSetting.HitLighting) && Result.Type != HitResult.Miss)
            {
                AddInternal(lighting = new SkinnableSprite("lighting")
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive,
                    Depth = float.MaxValue
                });

                if (JudgedObject != null)
                {
                    lightingColour = JudgedObject.AccentColour.GetBoundCopy();
                    lightingColour.BindValueChanged(colour => lighting.Colour = colour.NewValue, true);
                }
                else
                {
                    lighting.Colour = Color4.White;
                }
            }
        }

        protected override double FadeOutDelay => lighting == null ? base.FadeOutDelay : 1400;

        protected override void ApplyHitAnimations()
        {
            if (lighting != null)
            {
                JudgementBody.Delay(FadeInDuration).FadeOut(400);

                lighting.ScaleTo(0.8f).ScaleTo(1.2f, 600, Easing.Out);
                lighting.FadeIn(200).Then().Delay(200).FadeOut(1000);
            }

            JudgementText?.TransformSpacingTo(new Vector2(14, 0), 1800, Easing.OutQuint);
            base.ApplyHitAnimations();
        }
    }
}
