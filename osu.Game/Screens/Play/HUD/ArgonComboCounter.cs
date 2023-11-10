// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonComboCounter : ComboCounter
    {
        protected override double RollingDuration => 500;
        protected override Easing RollingEasing => Easing.OutQuint;

        [SettingSource("Wireframe opacity", "Controls the opacity of the wire frames behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.4f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            Current.BindTo(scoreProcessor.Combo);
            Current.BindValueChanged(combo =>
            {
                bool wasIncrease = combo.NewValue > combo.OldValue;
                DrawableCount.ScaleTo(new Vector2(1, wasIncrease ? 1.2f : 0.8f))
                             .ScaleTo(Vector2.One, 600, Easing.OutQuint);
            });
        }

        protected override LocalisableString FormatCount(int count) => $@"{count}x";

        protected override IHasText CreateText() => new ArgonCounterTextComponent(Anchor.TopLeft, "COMBO")
        {
            WireframeOpacity = { BindTarget = WireframeOpacity },
        };
    }
}
