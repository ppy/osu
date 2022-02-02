// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Difficulty;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class PerformanceStatisticTooltip : VisibilityContainer, ITooltip<PerformanceBreakdown>
    {
        private readonly Box background;
        private Colour4 titleColor;
        private Colour4 textColour;

        protected override Container<Drawable> Content { get; }

        public PerformanceStatisticTooltip()
        {
            AutoSizeAxes = Axes.Both;
            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                Content = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = 10, Right = 10, Top = 5, Bottom = 5 }
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            background.Colour = colours.Gray3;
            titleColor = colours.Blue;
            textColour = colours.BlueLighter;
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);

        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        private PerformanceBreakdown currentPerformance;

        public void SetContent(PerformanceBreakdown performance)
        {
            if (performance == currentPerformance)
                return;

            currentPerformance = performance;

            UpdateDisplay(performance);
        }

        private Drawable createItemForTotal(PerformanceDisplayAttribute attribute, PerformanceDisplayAttribute perfectAttribute)
        {
            return
                new GridContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Margin = new MarginPadding { Bottom = 10 },
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.Absolute, 250),
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(weight: FontWeight.Regular),
                                Text = attribute.DisplayName,
                                Colour = titleColor
                            },
                            new OsuSpriteText
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                Text = Math.Round(attribute.Value, MidpointRounding.AwayFromZero).ToLocalisableString(),
                                Colour = titleColor
                            }
                        },
                        new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(weight: FontWeight.Regular),
                                Text = "Maximum",
                                Colour = OsuColour.Gray(0.7f)
                            },
                            new OsuSpriteText
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                Font = OsuFont.GetFont(weight: FontWeight.Regular),
                                Text = Math.Round(perfectAttribute.Value, MidpointRounding.AwayFromZero).ToLocalisableString(),
                                Colour = OsuColour.Gray(0.7f)
                            }
                        }
                    }
                };
        }

        private Drawable createItemForAttribute(PerformanceDisplayAttribute attribute, PerformanceDisplayAttribute perfectAttribute)
        {
            float percentage = (float)(attribute.Value / perfectAttribute.Value);
            if (float.IsNaN(percentage))
                return null;

            return new GridContainer
            {
                AutoSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(GridSizeMode.Absolute, 110),
                    new Dimension(GridSizeMode.Absolute, 140),
                    new Dimension(GridSizeMode.AutoSize)
                },
                RowDimensions = new[]
                {
                    new Dimension(GridSizeMode.AutoSize)
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(weight: FontWeight.Regular),
                            Text = attribute.DisplayName,
                            Colour = textColour
                        },
                        new Bar
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Width = 130,
                            Height = 5,
                            BackgroundColour = Color4.White.Opacity(0.5f),
                            Colour = textColour,
                            Length = percentage,
                            Margin = new MarginPadding { Left = 5, Right = 5 }
                        },
                        new OsuSpriteText
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                            Text = percentage.ToLocalisableString("0%"),
                            Colour = textColour
                        }
                    }
                }
            };
        }

        protected virtual void UpdateDisplay(PerformanceBreakdown performance)
        {
            Content.Clear();

            var displayAttributes = performance.Performance.GetAttributesForDisplay();

            var perfectDisplayAttributes = performance.PerfectPerformance.GetAttributesForDisplay();

            foreach (PerformanceDisplayAttribute attr in displayAttributes)
            {
                var attributeItem = attr.PropertyName == nameof(PerformanceAttributes.Total)
                    ? createItemForTotal(attr, perfectDisplayAttributes.First(a => a.PropertyName == attr.PropertyName))
                    : createItemForAttribute(attr, perfectDisplayAttributes.First(a => a.PropertyName == attr.PropertyName));

                if (attributeItem != null)
                    Content.Add(attributeItem);
            }
        }

        public void Move(Vector2 pos) => Position = pos;
    }
}
