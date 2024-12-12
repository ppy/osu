// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation.SkinComponents;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Screens.Play.HUD
{
    public partial class LongestComboCounter : ComboCounter
    {
        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Font), nameof(SkinnableComponentStrings.FontDescription))]
        public Bindable<Typeface> Font { get; } = new Bindable<Typeface>(Typeface.Venera);

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.Colour), nameof(SkinnableComponentStrings.ColourDescription))]
        public BindableColour4 TextColour { get; } = new BindableColour4(Color4Extensions.FromHex(@"ffffdd"));

        [SettingSource(typeof(SkinnableComponentStrings), nameof(SkinnableComponentStrings.ShowLabel), nameof(SkinnableComponentStrings.ShowLabelDescription))]
        public Bindable<bool> ShowLabel { get; } = new BindableBool(true);

        [BackgroundDependencyLoader]
        private void load(ScoreProcessor scoreProcessor)
        {
            TextColour.BindValueChanged(c => Colour = TextColour.Value, true);
            Current.BindTo(scoreProcessor.HighestCombo);
        }

        protected override IHasText CreateText() => new TextComponent
        {
            ShowLabel = { BindTarget = ShowLabel },
            Font = { BindTarget = Font },
        };

        private partial class TextComponent : CompositeDrawable, IHasText
        {
            public LocalisableString Text
            {
                get => text.Text;
                set => text.Text = $"{value}x";
            }

            public Bindable<bool> ShowLabel { get; } = new BindableBool();
            public Bindable<Typeface> Font { get; } = new Bindable<Typeface>();

            private readonly FillFlowContainer labelContainer;

            private readonly OsuSpriteText text;
            private readonly OsuSpriteText longestLabel;
            private readonly OsuSpriteText comboLabel;

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
                            Font = OsuFont.Numeric.With(size: 20)
                        },
                        labelContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            Direction = FillDirection.Vertical,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                longestLabel = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Font = OsuFont.Numeric.With(size: 8),
                                    Text = @"longest",
                                },
                                comboLabel = new OsuSpriteText
                                {
                                    Anchor = Anchor.TopLeft,
                                    Origin = Anchor.TopLeft,
                                    Font = OsuFont.Numeric.With(size: 8),
                                    Text = @"combo",
                                    Padding = new MarginPadding { Bottom = 3f }
                                }
                            }
                        }
                    }
                };
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                ShowLabel.BindValueChanged(s =>
                {
                    labelContainer.Alpha = s.NewValue ? 1 : 0;
                }, true);

                Font.BindValueChanged(typeface =>
                {
                    // We only have bold weight for venera, so let's force that.
                    FontWeight fontWeight = typeface.NewValue == Typeface.Venera ? FontWeight.Bold : FontWeight.Regular;

                    FontUsage f = OsuFont.GetFont(typeface.NewValue, weight: fontWeight);

                    // align baseline with fonts that aren't venera
                    comboLabel.Padding = new MarginPadding { Bottom = typeface.NewValue == Typeface.Venera ? 3f : 1f };

                    // Fixed width looks better on venera only in my opinion.
                    text.Font = f.With(size: 16, fixedWidth: typeface.NewValue == Typeface.Venera);
                    longestLabel.Font = f.With(size: 6, fixedWidth: false);
                    comboLabel.Font = f.With(size: 6, fixedWidth: false);
                }, true);
            }
        }
    }
}
