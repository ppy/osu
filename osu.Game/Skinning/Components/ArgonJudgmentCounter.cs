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
    public sealed partial class ArgonJudgmentCounter : VisibilityContainer
    {
        public ArgonCounterTextComponent TextComponent;
        private OsuColour colours = null!;
        public readonly JudgementCount JudgementCounter;
        public BindableInt DisplayedValue = new BindableInt();
        public string JudgementName;

        public ArgonJudgmentCounter(JudgementCount judgementCounter)
        {
            JudgementCounter = judgementCounter;
            JudgementName = judgementCounter.DisplayName.ToUpper().ToString();

            AutoSizeAxes = Axes.Both;
            AddInternal(TextComponent = new ArgonCounterTextComponent(Anchor.TopRight, judgementCounter.DisplayName.ToUpper()));
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            this.colours = colours;
        }

        private void updateWireframe()
        {
            int wireframeLenght = Math.Max(2, TextComponent.Text.ToString().Length);
            TextComponent.WireframeTemplate = new string('#', wireframeLenght);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            DisplayedValue.BindValueChanged(v =>
            {
                TextComponent.Text = v.NewValue.ToString();
                updateWireframe();
            }, true);

            var result = JudgementCounter.Types.First();
            TextComponent.LabelColour.Value = getJudgementColor(result);
            TextComponent.ShowLabel.BindValueChanged(v => TextComponent.TextColour.Value = !v.NewValue ? getJudgementColor(result) : Color4.White);
        }

        private Color4 getJudgementColor(HitResult result)
        {
            return result.IsBasic() ? colours.ForHitResult(result) : !result.IsBonus() ? colours.PurpleLight : colours.PurpleLighter;
        }

        protected override void PopIn() => this.FadeIn(JudgementCounterDisplay.TRANSFORM_DURATION, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(100);
    }
}
