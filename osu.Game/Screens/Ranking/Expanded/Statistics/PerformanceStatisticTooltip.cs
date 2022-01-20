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

        private Drawable createAttributeItem(PerformanceDisplayAttribute attribute, PerformanceDisplayAttribute perfectAttribute)
        {
            bool isTotal = attribute.PropertyName == nameof(PerformanceAttributes.Total);
            float fraction = (float)(attribute.Value / perfectAttribute.Value);
            if (float.IsNaN(fraction))
                fraction = 0;
            string text = fraction.ToLocalisableString("0%").ToString();

            if (isTotal)
                text = (int)Math.Round(attribute.Value, MidpointRounding.AwayFromZero) + "/" + (int)Math.Round(perfectAttribute.Value, MidpointRounding.AwayFromZero);

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
                            Colour = isTotal ? titleColor : textColour
                        },
                        new Bar
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Width = 130,
                            Height = 5,
                            BackgroundColour = Color4.White.Opacity(0.5f),
                            Colour = isTotal ? titleColor : textColour,
                            Length = fraction,
                            Margin = new MarginPadding { Left = 5, Right = 5 }
                        },
                        new OsuSpriteText
                        {
                            Origin = Anchor.CentreLeft,
                            Anchor = Anchor.CentreLeft,
                            Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                            Text = text,
                            Colour = isTotal ? titleColor : textColour
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
                Content.Add(createAttributeItem(attr, perfectDisplayAttributes.First(a => a.PropertyName == attr.PropertyName)));
            }
        }

        public void Move(Vector2 pos) => Position = pos;
    }
}
