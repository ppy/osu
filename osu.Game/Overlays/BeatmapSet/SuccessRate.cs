// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Select.Details;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class SuccessRate : Container
    {
        protected readonly FailRetryGraph Graph;

        private readonly FillFlowContainer header;
        private readonly SuccessRatePercentage successPercent;
        private readonly Bar successRate;
        private readonly Container percentContainer;

        private APIBeatmap beatmap;

        public APIBeatmap Beatmap
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
            int passCount = beatmap?.PassCount ?? 0;
            int playCount = beatmap?.PlayCount ?? 0;

            float rate = playCount != 0 ? (float)passCount / playCount : 0;
            successPercent.Text = rate.ToLocalisableString(@"0.#%");
            successPercent.TooltipText = $"{passCount} / {playCount}";
            successRate.Length = rate;
            percentContainer.ResizeWidthTo(successRate.Length, 250, Easing.InOutCubic);

            Graph.FailTimes = beatmap?.FailTimes;
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
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = BeatmapsetsStrings.ShowInfoSuccessRate,
                            Font = OsuFont.GetFont(size: 12)
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
                            Child = successPercent = new SuccessRatePercentage
                            {
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopCentre,
                                Font = OsuFont.GetFont(size: 12),
                            },
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = BeatmapsetsStrings.ShowInfoPointsOfFailure,
                            Font = OsuFont.GetFont(size: 12),
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
        private void load(OsuColour colours, OverlayColourProvider colourProvider)
        {
            successRate.AccentColour = colours.Green;
            successRate.BackgroundColour = colourProvider.Background6;

            updateDisplay();
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            Graph.Padding = new MarginPadding { Top = header.DrawHeight };
        }

        private partial class SuccessRatePercentage : OsuSpriteText, IHasTooltip
        {
            public LocalisableString TooltipText { get; set; }
        }
    }
}
