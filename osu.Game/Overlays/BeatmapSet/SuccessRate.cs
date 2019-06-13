// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Select.Details;

namespace osu.Game.Overlays.BeatmapSet
{
    public class SuccessRate : Container
    {
        protected readonly FailRetryGraph Graph;

        private readonly FillFlowContainer header;
        private readonly OsuSpriteText successRateLabel, successPercent, graphLabel;
        private readonly Bar successRate;
        private readonly Container percentContainer;

        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                if (value == beatmap) return;

                beatmap = value;

                updateDisplay();
            }
        }

        private void updateDisplay()
        {
            int passCount = beatmap?.OnlineInfo?.PassCount ?? 0;
            int playCount = beatmap?.OnlineInfo?.PlayCount ?? 0;

            var rate = playCount != 0 ? (float)passCount / playCount : 0;
            successPercent.Text = rate.ToString("P0");
            successRate.Length = rate;
            percentContainer.ResizeWidthTo(successRate.Length, 250, Easing.InOutCubic);

            Graph.Metrics = beatmap?.Metrics;
        }

        public SuccessRate()
        {
            Children = new Drawable[]
            {
                header = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Children = new Drawable[]
                    {
                        successRateLabel = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Success Rate",
                            Font = OsuFont.GetFont(size: 13)
                        },
                        successRate = new Bar
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 5,
                            Margin = new MarginPadding { Top = 5 },
                        },
                        percentContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Width = 0f,
                            Child = successPercent = new OsuSpriteText
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopCentre,
                                Font = OsuFont.GetFont(size: 13),
                            },
                        },
                        graphLabel = new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = "Points of Failure",
                            Font = OsuFont.GetFont(size: 13),
                            Margin = new MarginPadding { Vertical = 20 },
                        },
                    },
                },
                Graph = new FailRetryGraph
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            successRateLabel.Colour = successPercent.Colour = graphLabel.Colour = colours.Gray5;
            successRate.AccentColour = colours.Green;
            successRate.BackgroundColour = colours.GrayD;

            updateDisplay();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Graph.Padding = new MarginPadding { Top = header.DrawHeight };
        }
    }
}
