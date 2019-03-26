// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Profile.Header;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestCaseRankGraph : OsuTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(RankGraph),
            typeof(LineGraph)
        };

        public TestCaseRankGraph()
        {
            RankGraph graph;

            var data = new int[89];
            var dataWithZeros = new int[89];
            var smallData = new int[89];

            for (int i = 0; i < 89; i++)
                data[i] = dataWithZeros[i] = (i + 1) * 1000;

            for (int i = 20; i < 60; i++)
                dataWithZeros[i] = 0;

            for (int i = 79; i < 89; i++)
                smallData[i] = 100000 - i * 1000;

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

            AddStep("null user", () => graph.User.Value = null);
            AddStep("rank only", () =>
            {
                graph.User.Value = new User
                {
                    Statistics = new UserStatistics
                    {
                        Ranks = new UserStatistics.UserRanks { Global = 123456 },
                        PP = 12345,
                    }
                };
            });

            AddStep("with rank history", () =>
            {
                graph.User.Value = new User
                {
                    Statistics = new UserStatistics
                    {
                        Ranks = new UserStatistics.UserRanks { Global = 89000 },
                        PP = 12345,
                    },
                    RankHistory = new User.RankHistoryData
                    {
                        Data = data,
                    }
                };
            });

            AddStep("with zero values", () =>
            {
                graph.User.Value = new User
                {
                    Statistics = new UserStatistics
                    {
                        Ranks = new UserStatistics.UserRanks { Global = 89000 },
                        PP = 12345,
                    },
                    RankHistory = new User.RankHistoryData
                    {
                        Data = dataWithZeros,
                    }
                };
            });

            AddStep("small amount of data", () =>
            {
                graph.User.Value = new User
                {
                    Statistics = new UserStatistics
                    {
                        Ranks = new UserStatistics.UserRanks { Global = 12000 },
                        PP = 12345,
                    },
                    RankHistory = new User.RankHistoryData
                    {
                        Data = smallData,
                    }
                };
            });
        }
    }
}
