// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableOsuJudgement : DrawableJudgement
    {
        internal Color4 AccentColour { get; private set; }

        internal SkinnableLighting Lighting { get; private set; } = null!;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private Vector2? screenSpacePosition;

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(Lighting = new SkinnableLighting
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Blending = BlendingParameters.Additive,
                Depth = float.MaxValue,
                Alpha = 0
            });
        }

        public override void Apply(JudgementResult result, DrawableHitObject? judgedObject)
        {
            base.Apply(result, judgedObject);

            if (judgedObject is not DrawableOsuHitObject osuObject)
                return;

            AccentColour = osuObject.AccentColour.Value;

            switch (osuObject)
            {
                case DrawableSlider slider:
                    screenSpacePosition = slider.TailCircle.ToScreenSpace(slider.TailCircle.OriginPosition);
                    break;

                default:
                    screenSpacePosition = osuObject.ToScreenSpace(osuObject.OriginPosition);
                    break;
            }

            Scale = new Vector2(osuObject.HitObject.Scale);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Lighting.ResetAnimation();
            Lighting.SetColourFrom(this, Result);

            if (screenSpacePosition != null)
                Position = Parent!.ToLocalSpace(screenSpacePosition.Value);
        }

        protected override void ApplyHitAnimations()
        {
            bool hitLightingEnabled = config.Get<bool>(OsuSetting.HitLighting);

            Lighting.Alpha = 0;

            if (hitLightingEnabled)
            {
                // todo: this animation changes slightly based on new/old legacy skin versions.
                Lighting.ScaleTo(0.8f).ScaleTo(1.2f, 600, Easing.Out);
                Lighting.FadeIn(200).Then().Delay(200).FadeOut(1000);

                // extend the lifetime to cover lighting fade
                LifetimeEnd = Lighting.LatestTransformEndTime;
            }

            base.ApplyHitAnimations();
        }

        protected override Drawable CreateDefaultJudgement(HitResult result) =>
            // Tick hits don't show a judgement by default
            result.IsHit() && result.IsTick() ? Empty() : new OsuJudgementPiece(result);

        private partial class OsuJudgementPiece : DefaultJudgementPiece
        {
            public OsuJudgementPiece(HitResult result)
                : base(result)
            {
            }

            public override void PlayAnimation()
            {
                if (Result != HitResult.Miss)
                {
                    JudgementText
                        .ScaleTo(new Vector2(0.8f, 1))
                        .ScaleTo(new Vector2(1.2f, 1), 1800, Easing.OutQuint);
                }

                base.PlayAnimation();
            }
        }
    }
}
