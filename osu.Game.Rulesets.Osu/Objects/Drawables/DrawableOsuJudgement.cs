// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Osu.Objects.Drawables
{
    public partial class DrawableOsuJudgement : DrawableJudgement
    {
        internal SkinnableLighting Lighting { get; private set; }

        [Resolved]
        private OsuConfigManager config { get; set; }

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

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Lighting.ResetAnimation();
            Lighting.SetColourFrom(JudgedObject, Result);

            if (JudgedObject is DrawableOsuHitObject osuObject)
            {
                Position = osuObject.ToSpaceOfOtherDrawable(osuObject.OriginPosition, Parent!);
                Scale = new Vector2(osuObject.HitObject.Scale);
            }
        }

        protected override void Update()
        {
            base.Update();

            if (JudgedObject is DrawableOsuHitObject osuObject && Parent != null && osuObject.HitObject != null)
            {
                Position = osuObject.ToSpaceOfOtherDrawable(osuObject.OriginPosition, Parent!);
                Scale = new Vector2(osuObject.HitObject.Scale);
            }
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

        protected override Drawable CreateDefaultJudgement(HitResult result) => new OsuJudgementPiece(result);

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
