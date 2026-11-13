// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Overlays;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneLeaderboardStatisticDisplay : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Cached]
        private OsuColour colours = new OsuColour();

        private FillFlowContainer container = null!;
        private BeatmapLeaderboardScore.StatisticDisplay[] statistics = [];

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create container", () =>
            {
                Child = new Container
                {
                    Masking = true,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.X,
                    Height = 50,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colours.Gray3,
                        },
                        container = new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Direction = FillDirection.Horizontal,
                            AutoSizeAxes = Axes.Both,
                            Margin = new MarginPadding { Horizontal = 20 },
                            Spacing = new Vector2(20, 0)
                        },
                    },
                };
            });
        }

        [Test]
        public void TestTwoElements()
        {
            AddStep("create content", () =>
            {
                container.AddRange(statistics = new[]
                {
                    new BeatmapLeaderboardScore.StatisticDisplay("MAX COMBO", "123x", false),
                    new BeatmapLeaderboardScore.StatisticDisplay("ACCURACY", "100.00%", true),
                });
            });
            AddStep("vertical container, vertical stats", () =>
            {
                container.Direction = FillDirection.Vertical;
                container.ScaleTo(0.8f, 200, Easing.OutQuint);

                setStatisticsDirection(Direction.Vertical);
            });
            AddStep("vertical container, horizontal stats", () =>
            {
                container.Direction = FillDirection.Vertical;
                container.ScaleTo(0.8f, 200, Easing.OutQuint);

                setStatisticsDirection(Direction.Horizontal);
            });
            AddStep("horizontal container, vertical stats", () =>
            {
                // This one is not really intended to be used anywhere, as it's not really utilising
                // the vertical space available. Included here for the sake of completeness.
                container.Direction = FillDirection.Horizontal;
                container.ScaleTo(1, 200, Easing.OutQuint);

                setStatisticsDirection(Direction.Vertical);
            });
            AddStep("horizontal container, horizontal stats", () =>
            {
                container.Direction = FillDirection.Horizontal;
                container.ScaleTo(1, 200, Easing.OutQuint);

                setStatisticsDirection(Direction.Horizontal);
            });
        }

        [Test]
        public void TestThreeElements()
        {
            AddStep("create content", () =>
            {
                container.AddRange(statistics = new[]
                {
                    new BeatmapLeaderboardScore.StatisticDisplay("ACCURACY", "100.00%", true),
                    new BeatmapLeaderboardScore.StatisticDisplay("ATTEMPTS", "32", false),
                    new BeatmapLeaderboardScore.StatisticDisplay("COMPLETED", "9", false),
                });
            });
            AddStep("vertical container, vertical stats", () =>
            {
                container.Direction = FillDirection.Vertical;
                container.ScaleTo(0.8f, 200, Easing.OutQuint);

                setStatisticsDirection(Direction.Vertical);
            });
            AddStep("vertical container, horizontal stats", () =>
            {
                // This doesn't really make sense to be used anywhere either, as it requires
                // the scale to be reduced to the point where the statistics are barely readable.
                container.Direction = FillDirection.Vertical;
                container.ScaleTo(0.6f, 200, Easing.OutQuint);

                setStatisticsDirection(Direction.Horizontal);
            });
            AddStep("horizontal container, horizontal stats", () =>
            {
                container.Direction = FillDirection.Horizontal;
                container.ScaleTo(1, 200, Easing.OutQuint);

                setStatisticsDirection(Direction.Horizontal);
            });
            AddStep("horizontal container, vertical stats", () =>
            {
                // Same as its vertical counterpart, this doesn't really make sense to be used
                // anywhere, as it doesn't utilise the vertical space available.
                container.Direction = FillDirection.Horizontal;
                container.ScaleTo(1, 200, Easing.OutQuint);

                setStatisticsDirection(Direction.Vertical);
            });
        }

        private void setStatisticsDirection(Direction direction)
        {
            foreach (var statistic in statistics)
            {
                statistic.Direction = direction;
                statistic.Anchor = statistic.Origin = direction == Direction.Horizontal
                    ? Anchor.TopLeft
                    : Anchor.TopRight;
            }
        }
    }
}
