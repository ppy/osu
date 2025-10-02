// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osuTK;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapMetadataWedge
    {
        private partial class SuccessRateDisplay : CompositeDrawable, IHasTooltip
        {
            private readonly OsuSpriteText valueText;
            private readonly Circle backgroundBar;
            private readonly Circle valueBar;

            private (int passes, int plays) data;

            public (int passes, int plays) Data
            {
                get => data;
                set
                {
                    data = value;

                    float ratio = value.plays == 0 ? 0 : (float)value.passes / value.plays;

                    valueText.Text = ratio.ToLocalisableString(@"0.##%");
                    valueText.MoveToX(Math.Clamp(ratio, 0.05f, 0.95f), 300, Easing.OutQuint);
                    valueBar.ResizeWidthTo(ratio, 300, Easing.OutQuint);
                }
            }

            public LocalisableString TooltipText => $"{data.passes:N0} / {data.plays:N0}";

            public SuccessRateDisplay()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new[]
                {
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0f, 2f),
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Text = BeatmapsetsStrings.ShowInfoSuccessRate,
                                Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Margin = new MarginPadding { Top = 10f },
                                Child = valueText = new OsuSpriteText
                                {
                                    Origin = Anchor.TopCentre,
                                    RelativePositionAxes = Axes.X,
                                    Font = OsuFont.Style.Caption1,
                                }
                            },
                            new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Children = new[]
                                {
                                    backgroundBar = new Circle
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Height = 4f,
                                    },
                                    valueBar = new Circle
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Width = 0f,
                                        Height = 4f,
                                    },
                                },
                            }
                        },
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours, OverlayColourProvider colourProvider)
            {
                backgroundBar.Colour = colourProvider.Background6;
                valueBar.Colour = colours.Lime1;
                valueText.Colour = colourProvider.Content2;
            }
        }
    }
}
