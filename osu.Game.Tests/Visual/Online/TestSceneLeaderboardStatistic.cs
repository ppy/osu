// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    public partial class TestSceneLeaderboardStatistic : OsuTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        [Cached]
        private OsuColour colours = new OsuColour();

        private FillFlowContainer container = null!;
        private LeaderboardStatistic[] statistics = [];

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
                    new LeaderboardStatistic("COMBO", "123x", false),
                    new LeaderboardStatistic("ACCURACY", "100.00%", true),
                });
            });
            AddStep("change to vertical", () =>
            {
                container.Direction = FillDirection.Vertical;
                container.ScaleTo(0.8f, 200, Easing.OutQuint);
            });
            AddStep("change to horizontal", () =>
            {
                container.Direction = FillDirection.Horizontal;
                container.ScaleTo(1, 200, Easing.OutQuint);
            });
        }

        [Test]
        public void TestThreeElements()
        {
            AddStep("create content", () =>
            {
                container.AddRange(statistics = new[]
                {
                    new LeaderboardStatistic("COMBO", "123x", false),
                    new LeaderboardStatistic("ACCURACY", "100.00%", true),
                    new LeaderboardStatistic("TEST", "12345", false),
                });
            });
            AddStep("change to vertical", () =>
            {
                container.Direction = FillDirection.Vertical;
                container.ScaleTo(0.8f, 200, Easing.OutQuint);

                foreach (var statistic in statistics)
                    statistic.Direction = Direction.Vertical;
            });
            AddStep("change to horizontal", () =>
            {
                container.Direction = FillDirection.Horizontal;
                container.ScaleTo(1, 200, Easing.OutQuint);

                foreach (var statistic in statistics)
                    statistic.Direction = Direction.Horizontal;
            });
        }
    }
}
