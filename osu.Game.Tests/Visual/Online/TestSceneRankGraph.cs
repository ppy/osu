// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneRankGraph : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        public TestSceneRankGraph()
        {
            RankGraph graph;

            int[] data = new int[89];
            int[] dataWithZeros = new int[89];
            int[] smallData = new int[89];
            int[] edgyData = new int[89];

            for (int i = 0; i < 89; i++)
                data[i] = dataWithZeros[i] = (i + 1) * 1000;

            for (int i = 20; i < 60; i++)
                dataWithZeros[i] = 0;

            for (int i = 79; i < 89; i++)
                smallData[i] = 100000 - i * 1000;

            bool edge = true;

            for (int i = 0; i < 20; i++)
            {
                edgyData[i] = 100000 + (edge ? 1000 : -1000) * (i + 1);
                edge = !edge;
            }

            Add(new Container
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

            AddStep("null user", () => graph.Statistics.Value = null);
            AddStep("rank only", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    GlobalRank = 123456,
                    PP = 12345,
                };
            });

            AddStep("with rank history", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    GlobalRank = 89000,
                    PP = 12345,
                    RankHistory = new APIRankHistory
                    {
                        Data = data,
                    }
                };
            });

            AddStep("with zero values", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    GlobalRank = 89000,
                    PP = 12345,
                    RankHistory = new APIRankHistory
                    {
                        Data = dataWithZeros,
                    }
                };
            });

            AddStep("small amount of data", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    GlobalRank = 12000,
                    PP = 12345,
                    RankHistory = new APIRankHistory
                    {
                        Data = smallData,
                    }
                };
            });

            AddStep("graph with edges", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    GlobalRank = 12000,
                    PP = 12345,
                    RankHistory = new APIRankHistory
                    {
                        Data = edgyData,
                    }
                };
            });
        }
    }
}
