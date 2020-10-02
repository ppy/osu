// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osuTK;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Skinning;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public class DrawableOsuJudgement : DrawableJudgement
    {
        protected SkinnableSprite Lighting;

        private Bindable<Color4> lightingColour;

        [Resolved]
        private OsuConfigManager config { get; set; }

        public DrawableOsuJudgement(JudgementResult result, DrawableHitObject judgedObject)
            : base(result, judgedObject)
        {
        }

        public DrawableOsuJudgement()
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(Lighting = new SkinnableSprite("lighting")
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Blending = BlendingParameters.Additive,
                Depth = float.MaxValue,
                Alpha = 0
            });
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

            Lighting.ResetAnimation();

            if (JudgedObject != null)
            {
                lightingColour = JudgedObject.AccentColour.GetBoundCopy();
                lightingColour.BindValueChanged(colour => Lighting.Colour = Result.IsHit ? colour.NewValue : Color4.Transparent, true);
            }
            else
            {
                Lighting.Colour = Color4.White;
            }
        }

        private double fadeOutDelay;
        protected override double FadeOutDelay => fadeOutDelay;

        protected override void ApplyHitAnimations()
        {
            bool hitLightingEnabled = config.Get<bool>(OsuSetting.HitLighting);

            if (hitLightingEnabled)
            {
                JudgementBody.FadeIn().Delay(FadeInDuration).FadeOut(400);

                Lighting.ScaleTo(0.8f).ScaleTo(1.2f, 600, Easing.Out);
                Lighting.FadeIn(200).Then().Delay(200).FadeOut(1000);
            }
            else
            {
                JudgementBody.Alpha = 1;
            }

            fadeOutDelay = hitLightingEnabled ? 1400 : base.FadeOutDelay;

            JudgementText?.TransformSpacingTo(Vector2.Zero).Then().TransformSpacingTo(new Vector2(14, 0), 1800, Easing.OutQuint);
            base.ApplyHitAnimations();
        }
    }
}
