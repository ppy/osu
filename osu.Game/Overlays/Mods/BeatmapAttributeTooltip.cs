// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Difficulty;
using osuTK;

namespace osu.Game.Overlays.Mods
{
    public partial class BeatmapAttributeTooltip : VisibilityContainer, ITooltip<RulesetBeatmapAttribute?>
    {
        private readonly OverlayColourProvider? colourProvider;

        private Container content = null!;

        private RulesetBeatmapAttribute? attribute;
        private OsuSpriteText adjustedByModsText = null!;
        private OsuTextFlowContainer descriptionText = null!;
        private GridContainer metricsGrid = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        public BeatmapAttributeTooltip(OverlayColourProvider? colourProvider = null)
        {
            this.colourProvider = colourProvider;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AutoSizeAxes = Axes.Both;

            Masking = true;
            CornerRadius = 5;

            InternalChildren = new Drawable[]
            {
                content = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider?.Background4 ?? colours.Gray3,
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Vertical = 10, Horizontal = 15 },
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(10),
                            Children = new Drawable[]
                            {
                                descriptionText = new OsuTextFlowContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    MaximumSize = new Vector2(380, 0),
                                },
                                metricsGrid = new GridContainer
                                {
                                    AutoSizeAxes = Axes.Both,
                                    ColumnDimensions =
                                    [
                                        new Dimension(GridSizeMode.AutoSize),
                                        new Dimension(minSize: 10),
                                        new Dimension(GridSizeMode.AutoSize),
                                    ]
                                },
                                adjustedByModsText = new OsuSpriteText
                                {
                                    Font = OsuFont.Style.Caption1.With(weight: FontWeight.Bold),
                                },
                            }
                        },
                    }
                },
            };

            updateDisplay();
        }

        private void updateDisplay()
        {
            bool shouldShow = false;

            if (attribute != null)
            {
                descriptionText.Text = attribute.Description ?? default;
                shouldShow = attribute.Description != null;

                metricsGrid.Content = attribute.AdditionalMetrics.Select(metric => new[]
                {
                    new OsuSpriteText
                    {
                        Font = OsuFont.Style.Caption1.With(weight: FontWeight.SemiBold),
                        Text = metric.Name,
                        Colour = metric.Colour ?? colourProvider?.Content2 ?? Colour4.White,
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                    },
                    Empty(),
                    new OsuSpriteText
                    {
                        Font = OsuFont.Style.Caption1,
                        Text = metric.Value,
                        Colour = Interpolation.ValueAt<Colour4>(0.85f, colourProvider?.Content1 ?? Colour4.White, metric.Colour ?? colourProvider?.Content1 ?? Colour4.White, 0, 1),
                        Anchor = Anchor.CentreRight,
                        Origin = Anchor.CentreRight,
                    }
                }).ToArray();
                metricsGrid.RowDimensions = Enumerable.Repeat(new Dimension(GridSizeMode.AutoSize), attribute.AdditionalMetrics.Length).ToArray();
                metricsGrid.Alpha = attribute.AdditionalMetrics.Length > 0 ? 1 : 0;
                shouldShow |= attribute.AdditionalMetrics.Length > 0;

                if (!Precision.AlmostEquals(attribute.OriginalValue, attribute.AdjustedValue))
                {
                    adjustedByModsText.Text = $"This value is being adjusted by mods ({attribute.OriginalValue:0.0#} → {attribute.AdjustedValue:0.0#}).";
                    adjustedByModsText.Alpha = 1;
                    shouldShow = true;
                }
                else
                    adjustedByModsText.Alpha = 0;
            }

            if (shouldShow)
                content.Show();
            else
                content.Hide();
        }

        public void SetContent(RulesetBeatmapAttribute? attribute)
        {
            if (this.attribute == attribute)
                return;

            this.attribute = attribute;
            updateDisplay();
        }

        protected override void PopIn() => this.FadeIn(200, Easing.OutQuint);
        protected override void PopOut() => this.FadeOut(200, Easing.OutQuint);

        public void Move(Vector2 pos) => Position = pos;
    }
}
