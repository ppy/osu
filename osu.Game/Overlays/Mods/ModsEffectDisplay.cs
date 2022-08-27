// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    /// <inheritdoc />
    /// <summary>
    /// Base class for displays of mods effects.
    /// </summary>
    public abstract class ModsEffectDisplay : Container
    {
        public const float HEIGHT = 42;
        private const float value_area_width = 56;
        private const float transition_duration = 200;

        private readonly Box contentBackground;
        private readonly Box labelBackground;
        private readonly Container content;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        /// <summary>
        /// Text to display in the left area of the display.
        /// </summary>
        protected abstract LocalisableString Label { get; }

        protected override Container<Drawable> Content => content;

        protected ModsEffectDisplay()
        {
            Height = HEIGHT;
            AutoSizeAxes = Axes.X;

            InternalChild = new InputBlockingContainer
            {
                RelativeSizeAxes = Axes.Y,
                AutoSizeAxes = Axes.X,
                Masking = true,
                CornerRadius = ModSelectPanel.CORNER_RADIUS,
                Shear = new Vector2(ShearedOverlayContainer.SHEAR, 0),
                Children = new Drawable[]
                {
                    contentBackground = new Box
                    {
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                        RelativeSizeAxes = Axes.Y,
                        Width = value_area_width + ModSelectPanel.CORNER_RADIUS
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Y,
                        AutoSizeAxes = Axes.X,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension(GridSizeMode.Absolute, value_area_width)
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new Container
                                {
                                    RelativeSizeAxes = Axes.Y,
                                    AutoSizeAxes = Axes.X,
                                    Masking = true,
                                    CornerRadius = ModSelectPanel.CORNER_RADIUS,
                                    Children = new Drawable[]
                                    {
                                        labelBackground = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Margin = new MarginPadding { Horizontal = 18 },
                                            Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                            Text = Label,
                                            Font = OsuFont.Default.With(size: 17, weight: FontWeight.SemiBold)
                                        }
                                    }
                                },
                                content = new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0)
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            labelBackground.Colour = colourProvider.Background4;
        }

        /// <summary>
        /// Fades colours of text and its background according to displayed value.
        /// </summary>
        /// <param name="difference">Difference between actual value and default value.
        /// Negative leads to green color, positive to red. Can be obtained via CompareTo(defaultValue).</param>
        protected void SetColours(int difference)
        {
            if (difference == 0)
            {
                contentBackground.FadeColour(colourProvider.Background3, transition_duration, Easing.OutQuint);
                content.FadeColour(Colour4.White, transition_duration, Easing.OutQuint);
            }
            else if (difference < 0)
            {
                contentBackground.FadeColour(colours.ForModType(ModType.DifficultyReduction), transition_duration, Easing.OutQuint);
                content.FadeColour(colourProvider.Background5, transition_duration, Easing.OutQuint);
            }
            else
            {
                contentBackground.FadeColour(colours.ForModType(ModType.DifficultyIncrease), transition_duration, Easing.OutQuint);
                content.FadeColour(colourProvider.Background5, transition_duration, Easing.OutQuint);
            }
        }
    }
}
