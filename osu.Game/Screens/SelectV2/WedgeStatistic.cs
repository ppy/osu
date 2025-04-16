// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class WedgeStatistic : CompositeDrawable, IHasTooltip
    {
        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        private readonly IconUsage icon;
        private readonly bool background;
        private readonly float leftPadding;
        private readonly float? minSize;

        private OsuSpriteText valueText = null!;
        private LoadingSpinner loading = null!;

        private LocalisableString? value;

        public LocalisableString? Value
        {
            get => value;
            set
            {
                this.value = value;

                Schedule(() =>
                {
                    loading.State.Value = value != null ? Visibility.Hidden : Visibility.Visible;

                    if (value != null)
                    {
                        valueText.Text = value.Value;
                        valueText.FadeIn(120, Easing.OutQuint);
                    }
                    else
                        valueText.FadeOut(120, Easing.OutQuint);
                });
            }
        }

        public LocalisableString TooltipText { get; set; }

        public WedgeStatistic(IconUsage icon, bool background = false, float leftPadding = 10f, float? minSize = null)
        {
            this.icon = icon;
            this.background = background;
            this.leftPadding = leftPadding;
            this.minSize = minSize;

            AutoSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 5;
            Shear = background ? shear : Vector2.Zero;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = background ? 0.2f : 0f,
                },
                new FillFlowContainer
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Margin = new MarginPadding { Left = background ? leftPadding : 0, Right = background ? 10f : 0f, Vertical = 5f },
                    Spacing = new Vector2(4f, 0f),
                    Shear = background ? -shear : Vector2.Zero,
                    Children = new Drawable[]
                    {
                        new SpriteIcon
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Icon = icon,
                            Size = new Vector2(OsuFont.Subheading.Size),
                            Colour = colourProvider.Content2,
                        },
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                loading = new LoadingSpinner
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    Size = new Vector2(14f),
                                    State = { Value = Visibility.Visible },
                                },
                                new GridContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.AutoSize, minSize: minSize ?? 0),
                                    },
                                    Content = new[]
                                    {
                                        new[]
                                        {
                                            valueText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Font = OsuFont.Subheading,
                                                Colour = colourProvider.Content2,
                                                Margin = new MarginPadding { Bottom = 2f },
                                                AlwaysPresent = true,
                                            },
                                        }
                                    }
                                },
                            },
                        },
                    },
                }
            };
        }
    }
}
