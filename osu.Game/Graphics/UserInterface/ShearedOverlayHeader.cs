// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Graphics.UserInterface
{
    public partial class ShearedOverlayHeader : CompositeDrawable
    {
        public const float HEIGHT = main_area_height + 2 * corner_radius;

        public LocalisableString Title
        {
            set => titleSpriteText.Text = value;
        }

        public LocalisableString Description
        {
            set => descriptionText.Text = value;
        }

        public Action? Close
        {
            get => closeButton.Action;
            set => closeButton.Action = value;
        }

        private const float corner_radius = 14;
        private const float main_area_height = 70;

        private readonly Container underlayContainer;
        private readonly Box underlayBackground;
        private readonly Container contentContainer;
        private readonly Box contentBackground;
        private readonly OsuSpriteText titleSpriteText;
        private readonly OsuTextFlowContainer descriptionText;
        private readonly IconButton closeButton;

        public ShearedOverlayHeader()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding
                {
                    Horizontal = 70,
                    Top = -corner_radius
                },
                Children = new Drawable[]
                {
                    underlayContainer = new InputBlockingContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = HEIGHT,
                        CornerRadius = corner_radius,
                        Masking = true,
                        BorderThickness = 2,
                        Child = underlayBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        }
                    },
                    contentContainer = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = main_area_height + corner_radius,
                        CornerRadius = corner_radius,
                        Masking = true,
                        BorderThickness = 2,
                        EdgeEffect = new EdgeEffectParameters
                        {
                            Type = EdgeEffectType.Shadow,
                            Colour = Colour4.Black.Opacity(0.1f),
                            Offset = new Vector2(0, 1),
                            Radius = 3
                        },
                        Children = new Drawable[]
                        {
                            contentBackground = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Margin = new MarginPadding { Top = corner_radius },
                                Padding = new MarginPadding { Horizontal = 100 },
                                Children = new Drawable[]
                                {
                                    titleSpriteText = new OsuSpriteText
                                    {
                                        Font = OsuFont.TorusAlternate.With(size: 20)
                                    },
                                    descriptionText = new OsuTextFlowContainer(t =>
                                    {
                                        t.Font = OsuFont.Default.With(size: 12);
                                    })
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y
                                    }
                                }
                            },
                            closeButton = new IconButton
                            {
                                Icon = FontAwesome.Solid.Times,
                                Scale = new Vector2(0.6f),
                                Anchor = Anchor.CentreRight,
                                Origin = Anchor.CentreRight,
                                Margin = new MarginPadding
                                {
                                    Right = 21,
                                    Top = corner_radius
                                }
                            }
                        }
                    }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            underlayContainer.BorderColour = ColourInfo.GradientVertical(Colour4.Black, colourProvider.Dark4);
            underlayBackground.Colour = colourProvider.Dark4;

            contentContainer.BorderColour = ColourInfo.GradientVertical(colourProvider.Dark3, colourProvider.Dark1);
            contentBackground.Colour = colourProvider.Dark3;

            closeButton.IconHoverColour = colourProvider.Highlight1;
        }
    }
}
