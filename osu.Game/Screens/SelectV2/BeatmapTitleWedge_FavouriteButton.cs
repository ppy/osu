// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapTitleWedge
    {
        public partial class FavouriteButton : OsuClickableContainer
        {
            private readonly BindableBool isFavourite = new BindableBool();

            private Box background = null!;
            private OsuSpriteText valueText = null!;
            private LoadingSpinner loading = null!;
            private Box hoverLayer = null!;
            private Box flashLayer = null!;
            private SpriteIcon icon = null!;

            private LocalisableString? text;

            public LocalisableString? Text
            {
                get => text;
                set
                {
                    text = value;
                    Scheduler.AddOnce(updateDisplay);
                }
            }

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public FavouriteButton()
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Masking = true;
                CornerRadius = 5;
                Shear = OsuGame.SHEAR;

                AddRange(new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.2f),
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Margin = new MarginPadding { Left = 10, Right = 10, Vertical = 5f },
                        Spacing = new Vector2(4f, 0f),
                        Shear = -OsuGame.SHEAR,
                        Children = new Drawable[]
                        {
                            icon = new SpriteIcon
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Icon = OsuIcon.Heart,
                                Size = new Vector2(OsuFont.Style.Heading2.Size),
                                Colour = colourProvider.Content2,
                            },
                            new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                AutoSizeAxes = Axes.X,
                                Height = 20,
                                Children = new Drawable[]
                                {
                                    loading = new LoadingSpinner
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Size = new Vector2(12f),
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
                                            new Dimension(GridSizeMode.AutoSize, minSize: 25),
                                        },
                                        Content = new[]
                                        {
                                            new[]
                                            {
                                                valueText = new OsuSpriteText
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Font = OsuFont.Style.Heading2,
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
                    },
                    hoverLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Colour = Colour4.White.Opacity(0.1f),
                        Blending = BlendingParameters.Additive,
                    },
                    flashLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        Colour = Colour4.White,
                    }
                });
                Action = isFavourite.Toggle;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Scheduler.AddOnce(updateDisplay);
                isFavourite.BindValueChanged(_ =>
                {
                    if (isFavourite.Value)
                        flashLayer.FadeOutFromOne(500, Easing.Out);
                    Scheduler.AddOnce(updateDisplay);
                });
            }

            protected override bool OnHover(HoverEvent e)
            {
                hoverLayer.FadeIn(500, Easing.OutQuint);
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                hoverLayer.FadeOut(500, Easing.OutQuint);
            }

            private void updateDisplay()
            {
                loading.State.Value = text != null ? Visibility.Hidden : Visibility.Visible;

                if (text != null)
                {
                    valueText.Text = text.Value;
                    valueText.FadeIn(120, Easing.OutQuint);
                }
                else
                    valueText.FadeOut(120, Easing.OutQuint);

                background.FadeColour(isFavourite.Value ? colours.Pink1 : Colour4.Black.Opacity(0.2f), 500, Easing.OutQuint);
                icon.Icon = isFavourite.Value ? FontAwesome.Solid.Heart : FontAwesome.Regular.Heart;
            }
        }
    }
}
