// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Profile.Header.Components;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneRankGraph : OsuTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RankGraph),
            typeof(LineGraph)
        };

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Pink);

        public TestSceneRankGraph()
        {
            RankGraph graph;

            var data = new int[89];
            var dataWithZeros = new int[89];
            var smallData = new int[89];
            var edgyData = new int[89];

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
                    Ranks = new UserStatistics.UserRanks { Global = 123456 },
                    PP = 12345,
                };
            });

            AddStep("with rank history", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    Ranks = new UserStatistics.UserRanks { Global = 89000 },
                    PP = 12345,
                    RankHistory = new User.RankHistoryData
                    {
                        Data = data,
                    }
                };
            });

            AddStep("with zero values", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    Ranks = new UserStatistics.UserRanks { Global = 89000 },
                    PP = 12345,
                    RankHistory = new User.RankHistoryData
                    {
                        Data = dataWithZeros,
                    }
                };
            });

            AddStep("small amount of data", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    Ranks = new UserStatistics.UserRanks { Global = 12000 },
                    PP = 12345,
                    RankHistory = new User.RankHistoryData
                    {
                        Data = smallData,
                    }
                };
            });

            AddStep("graph with edges", () =>
            {
                graph.Statistics.Value = new UserStatistics
                {
                    Ranks = new UserStatistics.UserRanks { Global = 12000 },
                    PP = 12345,
                    RankHistory = new User.RankHistoryData
                    {
                        Data = edgyData,
                    }
                };
            });
        }
    }
}
