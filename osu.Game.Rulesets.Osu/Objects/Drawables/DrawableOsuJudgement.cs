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

        public DrawableOsuJudgement()
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            if (config.Get<bool>(OsuSetting.HitLighting))
            {
                AddInternal(lighting = new SkinnableSprite("lighting")
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Blending = BlendingParameters.Additive,
                    Depth = float.MaxValue
                });
            }
        }

        public override void Apply(JudgementResult result, DrawableHitObject judgedObject)
        {
            base.Apply(result, judgedObject);

            if (judgedObject?.HitObject is OsuHitObject osuObject)
            {
                Position = osuObject.StackedPosition;
                Scale = new Vector2(osuObject.Scale);
            }
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            lightingColour?.UnbindAll();

            if (lighting != null)
            {
                if (JudgedObject != null)
                {
                    lightingColour = JudgedObject.AccentColour.GetBoundCopy();
                    lightingColour.BindValueChanged(colour => lighting.Colour = Result.Type == HitResult.Miss ? Color4.Transparent : colour.NewValue, true);
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
                JudgementBody.FadeIn().Delay(FadeInDuration).FadeOut(400);

                lighting.ScaleTo(0.8f).ScaleTo(1.2f, 600, Easing.Out);
                lighting.FadeIn(200).Then().Delay(200).FadeOut(1000);
            }

            JudgementText?.TransformSpacingTo(Vector2.Zero).Then().TransformSpacingTo(new Vector2(14, 0), 1800, Easing.OutQuint);
            base.ApplyHitAnimations();
        }
    }
}
