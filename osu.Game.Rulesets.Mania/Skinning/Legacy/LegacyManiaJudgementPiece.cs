// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public class LegacyManiaJudgementPiece : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;
        private readonly Drawable animation;

        public LegacyManiaJudgementPiece(HitResult result, Drawable animation)
        {
            this.result = result;
            this.animation = animation;

            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(ISkinSource skin)
        {
            float? scorePosition = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.ScorePosition)?.Value;

            if (scorePosition != null)
                scorePosition -= Stage.HIT_TARGET_POSITION + 150;

            Y = scorePosition ?? 0;

            if (animation != null)
            {
                InternalChild = animation.With(d =>
                {
                    d.Anchor = Anchor.Centre;
                    d.Origin = Anchor.Centre;
                });
            }
        }

        public void PlayAnimation()
        {
            if (animation == null)
                return;

            (animation as IFramedAnimation)?.GotoFrame(0);

            switch (result)
            {
                case HitResult.None:
                    break;

                case HitResult.Miss:
                    animation.ScaleTo(1.6f);
                    animation.ScaleTo(1, 100, Easing.In);

                    animation.MoveTo(Vector2.Zero);
                    animation.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);

                    animation.RotateTo(0);
                    animation.RotateTo(40, 800, Easing.InQuint);

                    this.FadeOutFromOne(800);
                    break;

                default:
                    animation.ScaleTo(0.8f);
                    animation.ScaleTo(1, 250, Easing.OutElastic);

                    animation.Delay(50).ScaleTo(0.75f, 250);

                    this.Delay(50).FadeOut(200);
                    break;
            }
        }

        public Drawable GetAboveHitObjectsProxiedContent() => null;
    }
}
