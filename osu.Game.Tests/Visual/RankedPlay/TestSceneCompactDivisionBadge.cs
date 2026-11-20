// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneCompactDivisionBadge : OsuTestScene
    {
        public TestSceneCompactDivisionBadge()
        {
            Child = new GridContainer
            {
                RelativeSizeAxes = Axes.Both,
                ColumnDimensions = new[]
                {
                    new Dimension(),
                    new Dimension(),
                },
                Content = new[]
                {
                    new Drawable[]
                    {
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Y,
                            Width = 150,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Full,
                            Spacing = new Vector2(10),
                            Children = divisions.Select(d => new CompactDivisionBadge(d)).ToArray(),
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(10),
                            ChildrenEnumerable = Enumerable.Range(1, 100).Where(i => i == 1 || i == 100 || i % 5 == 0).Select(i => new CompactDivisionBadge(
                                new APIRankedPlayDivision { Tier = RankingTier.Lustrous, StartRating = 2400, EndRating = 3000 },
                                i
                            )),
                        }
                    }
                },
            };
        }

        private static readonly APIRankedPlayDivision[] divisions = TestSceneTierProgressBar.DIVISIONS[..^1];
    }
}
