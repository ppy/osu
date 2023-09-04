// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Play.HUD.JudgementCounter
{
    public partial class JudgementCounter : VisibilityContainer
    {
        public BindableBool ShowName = new BindableBool();
        public Bindable<FillDirection> Direction = new Bindable<FillDirection>();

        public readonly JudgementCountController.JudgementCount Result;

        public JudgementCounter(JudgementCountController.JudgementCount result) => Result = result;

        public OsuSpriteText ResultName = null!;
        private FillFlowContainer flowContainer = null!;
        private JudgementRollingCounter counter = null!;

        [BackgroundDependencyLoader]
        private void load(OsuColour colours, IBindable<RulesetInfo> ruleset)
        {
            AutoSizeAxes = Axes.Both;

            InternalChild = flowContainer = new FillFlowContainer
            {
                AutoSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    counter = new JudgementRollingCounter
                    {
                        Current = Result.ResultCount
                    },
                    ResultName = new OsuSpriteText
                    {
                        Alpha = 0,
                        Font = OsuFont.Numeric.With(size: 8),
                        Text = ruleset.Value.CreateInstance().GetDisplayNameForHitResult(Result.Type)
                    }
                }
            };

            var result = Result.Type;

            Colour = result.IsBasic() ? colours.ForHitResult(Result.Type) : !result.IsBonus() ? colours.PurpleLight : colours.PurpleLighter;
        }

        protected override void LoadComplete()
        {
            ShowName.BindValueChanged(value =>
                ResultName.FadeTo(value.NewValue ? 1 : 0, JudgementCounterDisplay.TRANSFORM_DURATION, Easing.OutQuint), true);

            Direction.BindValueChanged(direction =>
            {
                flowContainer.Direction = direction.NewValue;
                changeAnchor(direction.NewValue == FillDirection.Vertical ? Anchor.TopLeft : Anchor.BottomLeft);

                void changeAnchor(Anchor anchor) => counter.Anchor = ResultName.Anchor = counter.Origin = ResultName.Origin = anchor;
            }, true);

            base.LoadComplete();
        }

        protected override void PopIn() => this.FadeIn(JudgementCounterDisplay.TRANSFORM_DURATION, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(100);

        private sealed partial class JudgementRollingCounter : RollingCounter<int>
        {
            protected override OsuSpriteText CreateSpriteText()
                => base.CreateSpriteText().With(s => s.Font = s.Font.With(fixedWidth: true, size: 16));
        }
    }
}
