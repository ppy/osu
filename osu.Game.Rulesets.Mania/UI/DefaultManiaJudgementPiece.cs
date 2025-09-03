// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Mania.UI
{
    public partial class DefaultManiaJudgementPiece : DefaultJudgementPiece
    {
        private const float judgement_y_position = -180f;

        private IBindable<ScrollingDirection> direction = null!;

        public DefaultManiaJudgementPiece(HitResult result)
            : base(result)
        {
        }

        [BackgroundDependencyLoader]
        private void load(IScrollingInfo scrollingInfo)
        {
            direction = scrollingInfo.Direction.GetBoundCopy();
            direction.BindValueChanged(_ => onDirectionChanged(), true);
        }

        private void onDirectionChanged()
        {
            Anchor = direction.Value == ScrollingDirection.Up ? Anchor.TopCentre : Anchor.BottomCentre;
            Y = direction.Value == ScrollingDirection.Up ? -judgement_y_position : judgement_y_position;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            JudgementText.Font = JudgementText.Font.With(size: 25);
        }

        public override void PlayAnimation()
        {
            switch (Result)
            {
                case HitResult.None:
                    this.FadeOutFromOne(800);
                    break;

                case HitResult.Miss:
                    this.ScaleTo(1.6f);
                    this.ScaleTo(1, 100, Easing.In);

                    this.MoveToY(direction.Value == ScrollingDirection.Up ? -judgement_y_position : judgement_y_position);
                    this.MoveToOffset(new Vector2(0, 100), 800, Easing.InQuint);

                    this.RotateTo(0);
                    this.RotateTo(40, 800, Easing.InQuint);

                    this.FadeOutFromOne(800);
                    break;

                default:
                    this.ScaleTo(0.8f);
                    this.ScaleTo(1, 250, Easing.OutElastic);

                    this.Delay(50)
                        .ScaleTo(0.75f, 250)
                        .FadeOut(200);

                    // osu!mania uses a custom fade length, so the base call is intentionally omitted.
                    break;
            }
        }
    }
}
