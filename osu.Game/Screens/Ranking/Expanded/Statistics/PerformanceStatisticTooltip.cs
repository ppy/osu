// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.
using System;
using System.Globalization;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Difficulty;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded.Statistics
{
    public class PerformanceStatisticTooltip : VisibilityContainer, ITooltip<PerformanceAttributes>
    {
        private readonly Box background;
        private Colour4 textColor;

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
            textColor = colours.BlueLighter;
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        private PerformanceAttributes lastAttributes;

        public void SetContent(PerformanceAttributes attributes)
        {
            if (attributes == lastAttributes)
                return;

            lastAttributes = attributes;

            UpdateDisplay(attributes);
        }

        protected virtual void UpdateDisplay(PerformanceAttributes attributes)
        {
            Content.Clear();

            foreach (PerformanceDisplayAttribute attr in attributes.GetAttributesForDisplay())
            {
                Content.Add(new GridContainer
                {
                    AutoSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
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
                                Font = OsuFont.GetFont(weight: FontWeight.Regular),
                                Text = attr.DisplayName,
                                Colour = textColor
                            },
                            new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                Text = ((int)Math.Round(attr.Value, MidpointRounding.AwayFromZero)).ToString(CultureInfo.CurrentCulture)
                            }
                        }
                    }
                });
            }
        }

        public void Move(Vector2 pos) => Position = pos;
    }
}
