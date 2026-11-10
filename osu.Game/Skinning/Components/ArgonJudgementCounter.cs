// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Play.HUD.JudgementCounter;
using osuTK.Graphics;

namespace osu.Game.Skinning.Components
{
    public sealed partial class ArgonJudgementCounter : VisibilityContainer
    {
        public readonly JudgementCount Result;

        public IBindable<float> WireframeOpacity => textComponent.WireframeOpacity;

        public IBindable<int?> WireframeDigits { get; } = new Bindable<int?>();

        public IBindable<bool> ShowLabel => textComponent.ShowLabel;

        private readonly ArgonCounterTextComponent textComponent;
        private readonly BindableInt displayedValue = new BindableInt();

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public ArgonJudgementCounter(JudgementCount result)
        {
            Result = result;

            AutoSizeAxes = Axes.Both;
            AddInternal(textComponent = new ArgonCounterTextComponent(Anchor.TopLeft, result.DisplayName.ToUpper()));
        }

        private void updateWireframe()
        {
            textComponent.WireframeTemplate = new string('#', WireframeDigits.Value ?? Math.Max(2, textComponent.Text.ToString().Length));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            WireframeDigits.BindValueChanged(_ => updateWireframe());

            displayedValue.BindTo(Result.ResultCount);
            displayedValue.BindValueChanged(v =>
            {
                textComponent.Text = v.NewValue.ToString();
                updateWireframe();
            }, true);

            var result = Result.Types.First();
            textComponent.LabelColour.Value = getJudgementColor(result);
            textComponent.ShowLabel.BindValueChanged(v => textComponent.TextColour.Value = !v.NewValue ? getJudgementColor(result) : Color4.White, true);
        }

        private Color4 getJudgementColor(HitResult result)
        {
            return result.IsBasic() ? colours.ForHitResult(result) : !result.IsBonus() ? colours.PurpleLight : colours.PurpleLighter;
        }

        protected override void PopIn() => this.FadeIn(JudgementCounterDisplay.TRANSFORM_DURATION, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut();
    }
}
