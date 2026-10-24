// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Components;
using osuTK;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneDivisionBadge : OsuTestScene
    {
        [Resolved]
        private RulesetStore rulesetStore { get; set; } = null!;

        [TestCase("osu")]
        [TestCase("taiko")]
        [TestCase("fruits")]
        [TestCase("mania")]
        public void TestRuleset(string rulesetName)
        {
            var ruleset = rulesetStore.GetRuleset(rulesetName)!;
            AddStep("create", () =>
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
                                Width = 400,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Full,
                                Spacing = new Vector2(10),
                                ChildrenEnumerable = TestSceneCompactDivisionBadge.DIVISIONS.Select(d => new DivisionBadge(d, ruleset)),
                            },
                            new FillFlowContainer
                            {
                                AutoSizeAxes = Axes.Both,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Direction = FillDirection.Vertical,
                                Spacing = new Vector2(10),
                                ChildrenEnumerable = Enumerable.Range(1, 100).Where(i => i == 1 || i == 100 || i % 20 == 0).Select(i => new DivisionBadge(
                                    new APIRankedPlayDivision { Key = "Lustrous", Tier = RankingTier.Lustrous, DisplayName = "Lustrous", StartRating = 2400, EndRating = 3000 },
                                    ruleset,
                                    i
                                )),
                            },
                        },
                    },
                };
            });
        }
    }
}
