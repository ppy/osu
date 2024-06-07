// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Scoring;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Play.HUD
{
    public partial class ArgonComboCounter : ComboCounter
    {
        private ArgonCounterTextComponent text = null!;

        protected override double RollingDuration => 250;

        [SettingSource("Wireframe opacity", "Controls the opacity of the wireframes behind the digits.")]
        public BindableFloat WireframeOpacity { get; } = new BindableFloat(0.25f)
        {
            Precision = 0.01f,
            MinValue = 0,
            MaxValue = 1,
        };

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel), nameof(SkinnableComponentStrings.ShowLabelDescription))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            Current.BindTo(scoreProcessor.Combo);
            Current.BindValueChanged(combo =>
            {
                bool wasIncrease = combo.NewValue > combo.OldValue;
                bool wasMiss = combo.OldValue > 1 && combo.NewValue == 0;

                float newScale = Math.Clamp(text.NumberContainer.Scale.X * (wasIncrease ? 1.1f : 0.8f), 0.6f, 1.4f);

                float duration = wasMiss ? 2000 : 500;

                text.NumberContainer
                    .ScaleTo(new Vector2(newScale))
                    .ScaleTo(Vector2.One, duration, Easing.OutQuint);

                if (wasMiss)
                    text.FlashColour(Color4.Red, duration, Easing.OutQuint);
            });
        }

        public override int DisplayedCount
        {
            get => base.DisplayedCount;
            set
            {
                base.DisplayedCount = value;
                updateWireframe();
            }
        }

        private void updateWireframe()
        {
            int digitsRequiredForDisplayCount = getDigitsRequiredForDisplayCount();

            if (digitsRequiredForDisplayCount != text.WireframeTemplate.Length)
                text.WireframeTemplate = new string('#', digitsRequiredForDisplayCount);
        }

        private int getDigitsRequiredForDisplayCount()
        {
            // one for the single presumed starting digit, one for the "x" at the end.
            int digitsRequired = 2;
            long c = DisplayedCount;
            while ((c /= 10) > 0)
                digitsRequired++;
            return digitsRequired;
        }

        protected override LocalisableString FormatCount(int count) => $@"{count}x";

        protected override IHasText CreateText() => text = new ArgonCounterTextComponent(Anchor.TopLeft, MatchesStrings.MatchScoreStatsCombo.ToUpper())
        {
            WireframeOpacity = { BindTarget = WireframeOpacity },
            ShowLabel = { BindTarget = ShowLabel },
        };
    }
}
