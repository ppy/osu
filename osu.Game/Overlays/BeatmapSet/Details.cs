// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays.BeatmapSet.Buttons;
using osu.Game.Rulesets;
using osu.Game.Screens.Select.Details;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet
{
    public partial class Details : FillFlowContainer
    {
        protected readonly UserRatings Ratings;

        private readonly PreviewButton preview;
        private readonly BasicStats basic;
        private readonly AdvancedStats advanced;
        private readonly DetailBox ratingBox;

        private APIBeatmapSet beatmapSet;

        public APIBeatmapSet BeatmapSet
        {
            get => beatmapSet;
            set
            {
                if (value == beatmapSet) return;

                basic.BeatmapSet = preview.BeatmapSet = beatmapSet = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        private IBeatmapInfo beatmapInfo;

        public IBeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (value == beatmapInfo) return;

                basic.BeatmapInfo = advanced.BeatmapInfo = beatmapInfo = value;

                if (IsLoaded)
                    updateDisplay();
            }
        }

        public Details()
        {
            Width = BeatmapSetOverlay.RIGHT_WIDTH;
            AutoSizeAxes = Axes.Y;
            Spacing = new Vector2(1f);

            Children = new Drawable[]
            {
                preview = new PreviewButton
                {
                    RelativeSizeAxes = Axes.X,
                },
                new DetailBox
                {
                    Child = basic = new BasicStats
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Vertical = 10 }
                    },
                },
                new DetailBox
                {
                    Child = advanced = new AdvancedStats
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Margin = new MarginPadding { Vertical = 7.5f },
                    },
                },
                ratingBox = new DetailBox
                {
                    Child = Ratings = new UserRatings
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = 95,
                        Margin = new MarginPadding { Top = 10 },
                    },
                },
            };
        }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            updateDisplay();
        }

        private void updateDisplay()
        {
            Ratings.Ratings = BeatmapSet?.Ratings;
            ratingBox.Alpha = BeatmapSet?.Status > 0 ? 1 : 0;
            advanced.Ruleset.Value = rulesets.GetRuleset(beatmapInfo?.Ruleset.OnlineID ?? 0);
        }

        private partial class DetailBox : Container
        {
            private readonly Container content;
            private readonly Box background;

            protected override Container<Drawable> Content => content;

            public DetailBox()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    background = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.5f
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Padding = new MarginPadding { Horizontal = 15 },
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                background.Colour = colourProvider.Background6;
            }
        }
    }
}
