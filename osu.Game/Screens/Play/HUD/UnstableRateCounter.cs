// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Skinning;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class UnstableRateCounter : RollingCounter<int>, ISerialisableDrawable
    {
        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Font), nameof(SkinnableComponentStrings.FontDescription))]
        public Bindable<Typeface> Font { get; } = new Bindable<Typeface>(Typeface.Venera);

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Colour), nameof(SkinnableComponentStrings.ColourDescription))]
        public BindableColour4 TextColour { get; } = new BindableColour4(Color4Extensions.FromHex(@"ddffff"));

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel), nameof(SkinnableComponentStrings.ShowLabelDescription))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        public bool UsesFixedAnchor { get; set; }

        protected override double RollingDuration => 375;

        private const float alpha_when_invalid = 0.3f;
        private readonly Bindable<bool> valid = new Bindable<bool>();

        private HitEventExtensions.UnstableRateCalculationResult? unstableRateResult;

        [Resolved]
        private ScoreProcessor scoreProcessor { get; set; } = null!;

        public UnstableRateCounter()
        {
            Current.Value = 0;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            TextColour.BindValueChanged(c => Colour = TextColour.Value, true);
            valid.BindValueChanged(e =>
                DrawableCount.FadeTo(e.NewValue ? 1 : alpha_when_invalid, 1000, Easing.OutQuint));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoreProcessor.NewJudgement += updateDisplay;
            scoreProcessor.JudgementReverted += updateDisplay;
            updateDisplay();
        }

        private void updateDisplay(JudgementResult result)
        {
            if (HitEventExtensions.AffectsUnstableRate(result.HitObject, result.Type))
                Scheduler.AddOnce(updateDisplay);
        }

        private void updateDisplay()
        {
            unstableRateResult = scoreProcessor.HitEvents.CalculateUnstableRate(unstableRateResult);

            double? unstableRate = unstableRateResult?.Result;

            valid.Value = unstableRate != null;

            if (unstableRate != null)
                Current.Value = (int)Math.Round(unstableRate.Value);
        }

        protected override IHasText CreateText() => new TextComponent
        {
            Alpha = alpha_when_invalid,
            ShowLabel = { BindTarget = ShowLabel },
            Font = { BindTarget = Font },
        };

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (scoreProcessor.IsNotNull())
            {
                scoreProcessor.NewJudgement -= updateDisplay;
                scoreProcessor.JudgementReverted -= updateDisplay;
            }
        }

        private partial class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = value;
            }

            public Bindable<bool> ShowLabel { get; } = new BindableBool();
            public Bindable<Typeface> Font { get; } = new Bindable<Typeface>();

            private readonly OsuSpriteText text;
            private readonly OsuSpriteText label;

            public TextComponent()
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Spacing = new Vector2(2),
                    Children = new Drawable[]
                    {
                        text = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 16, fixedWidth: true)
                        },
                        label = new OsuSpriteText
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Font = OsuFont.Numeric.With(size: 8, fixedWidth: true),
                            Text = @"UR",
                            Padding = new MarginPadding { Bottom = 1.5f }, // align baseline better
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ShowLabel.BindValueChanged(s =>
                {
                    label.Alpha = s.NewValue ? 1 : 0;
                }, true);

                Font.BindValueChanged(typeface =>
                {
                    // We only have bold weight for venera, so let's force that.
                    FontWeight fontWeight = typeface.NewValue == Typeface.Venera ? FontWeight.Bold : FontWeight.Regular;

                    FontUsage f = OsuFont.GetFont(typeface.NewValue, weight: fontWeight);

                    // Fixed width looks better on venera only in my opinion.
                    text.Font = f.With(size: 16, fixedWidth: typeface.NewValue == Typeface.Venera);
                    label.Font = f.With(size: 8, fixedWidth: typeface.NewValue == Typeface.Venera);
                }, true);
            }
        }
    }
}
