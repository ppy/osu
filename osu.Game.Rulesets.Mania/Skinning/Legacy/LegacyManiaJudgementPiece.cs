// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Animations;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Skinning;

namespace osu.Game.Rulesets.Mania.Skinning.Legacy
{
    public partial class LegacyManiaJudgementPiece : CompositeDrawable, IAnimatableJudgement
    {
        private readonly HitResult result;
        private readonly Drawable animation;

        public LegacyManiaJudgementPiece(HitResult result, Drawable animation)
        {
            this.result = result;
            this.animation = animation;

            Origin = Anchor.Centre;

            AutoSizeAxes = Axes.Both;
        }

        private IBindable<ScrollingDirection> direction = null!;

        [Resolved]
        private ISkinSource skin { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction = scrollingInfo.Direction.GetBoundCopy();
            direction.BindValueChanged(_ => onDirectionChanged(), true);

            InternalChild = animation.With(d =>
            {
                d.Anchor = Anchor.Centre;
                d.Origin = Anchor.Centre;
            });
        }

        private void onDirectionChanged()
        {
            float hitPosition = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.HitPosition)?.Value ?? 0;
            float scorePosition = skin.GetManiaSkinConfig<float>(LegacyManiaSkinConfigurationLookups.ScorePosition)?.Value ?? 0;

            float hitPositionFromTop = 480f * LegacyManiaSkinConfiguration.POSITION_SCALE_FACTOR - hitPosition;

            if (scorePosition > hitPositionFromTop / 2f)
            {
                Anchor = direction.Value == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
                Y = direction.Value == ScrollingDirection.Up ? hitPositionFromTop - scorePosition : scorePosition - hitPositionFromTop;
            }
            else
            {
                Anchor = direction.Value == ScrollingDirection.Up ? Anchor.BottomCentre : Anchor.TopCentre;
                Y = direction.Value == ScrollingDirection.Up ? -scorePosition : scorePosition;
            }
        }

        public void PlayAnimation()
        {
            (animation as IFramedAnimation)?.GotoFrame(0);

            this.FadeInFromZero(20, Easing.Out)
                .Then().Delay(160)
                .FadeOutFromOne(40, Easing.In);

            switch (result)
            {
                case HitResult.None:
                    break;

                case HitResult.Miss:
                    animation.ScaleTo(1.2f).Then().ScaleTo(1, 100, Easing.Out);

                    animation.RotateTo(0);
                    animation.RotateTo(RNG.NextSingle(-5.73f, 5.73f), 100, Easing.Out);
                    break;

                default:
                    animation.ScaleTo(0.8f)
                             .Then().ScaleTo(1, 40)
                             // this is actually correct to match stable; there were overlapping transforms.
                             .Then().ScaleTo(0.85f)
                             .Then().ScaleTo(0.7f, 40)
                             .Then().Delay(100)
                             .Then().ScaleTo(0.4f, 40, Easing.In);
                    break;
            }
        }

        public Drawable? GetAboveHitObjectsProxiedContent() => null;
    }
}
