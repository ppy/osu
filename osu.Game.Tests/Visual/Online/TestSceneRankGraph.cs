// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneRankGraph : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        private RankGraph graph = null!;

        private const int history_length = 89;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create graph", () => Child = new Container
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(300, 150),
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(0.2f)
                    },
                    graph = new RankGraph
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            });
        }

        [Test]
        public void TestNullUser()
        {
            AddStep("null user", () => graph.Statistics.Value = null);
            AddAssert("line graph hidden", () => this.ChildrenOfType<LineGraph>().All(graph => graph.Alpha == 0));
        }

        [Test]
        public void TestRankOnly()
        {
            AddStep("rank only", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    IsRanked = true,
                    GlobalRank = 123456,
                    PP = 12345,
                };
            });
            AddAssert("line graph hidden", () => this.ChildrenOfType<LineGraph>().All(graph => graph.Alpha == 0));
        }

        [Test]
        public void TestWithRankHistory()
        {
            int[] data = new int[history_length];

            for (int i = 0; i < history_length; i++)
                data[i] = (i + 1) * 1000;

            AddStep("with rank history", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    IsRanked = true,
                    GlobalRank = 89000,
                    PP = 12345,
                    RankHistory = new APIRankHistory
                    {
                        Data = data
                    }
                };
            });
            AddAssert("line graph shown", () => this.ChildrenOfType<LineGraph>().All(graph => graph.Alpha == 1));
        }

        [Test]
        public void TestRanksWithZeroValues()
        {
            int[] dataWithZeros = new int[history_length];

            for (int i = 0; i < history_length; i++)
            {
                if (i < 20 || i >= 60)
                    dataWithZeros[i] = (i + 1) * 1000;
            }

            AddStep("with zero values", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    IsRanked = true,
                    GlobalRank = 89000,
                    PP = 12345,
                    RankHistory = new APIRankHistory
                    {
                        Data = dataWithZeros,
                    }
                };
            });
            AddAssert("line graph shown", () => this.ChildrenOfType<LineGraph>().All(graph => graph.Alpha == 1));
        }

        [Test]
        public void TestSmallAmountOfData()
        {
            int[] smallData = new int[history_length];

            for (int i = history_length - 10; i < history_length; i++)
                smallData[i] = 100000 - i * 1000;

            AddStep("small amount of data", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    IsRanked = true,
                    GlobalRank = 12000,
                    PP = 12345,
                    RankHistory = new APIRankHistory
                    {
                        Data = smallData,
                    }
                };
            });
            AddAssert("line graph shown", () => this.ChildrenOfType<LineGraph>().All(graph => graph.Alpha == 1));
        }

        [Test]
        public void TestHistoryWithEdges()
        {
            int[] edgyData = new int[89];

            bool edge = true;

            for (int i = 0; i < 20; i++)
            {
                edgyData[i] = 100000 + (edge ? 1000 : -1000) * (i + 1);
                edge = !edge;
            }

            AddStep("graph with edges", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    IsRanked = true,
                    GlobalRank = 12000,
                    PP = 12345,
                    RankHistory = new APIRankHistory
                    {
                        Data = edgyData,
                    }
                };
            });
            AddAssert("line graph shown", () => this.ChildrenOfType<LineGraph>().All(graph => graph.Alpha == 1));
        }
    }
}
