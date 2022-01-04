// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public class TestSceneDifficultySpectrumDisplay : OsuTestScene
    {
        private DifficultySpectrumDisplay display;

        private static APIBeatmapSet createBeatmapSetWith(params (int rulesetId, double stars)[] difficulties) => new APIBeatmapSet
        {
            Beatmaps = difficulties.Select(difficulty => new APIBeatmap
            {
                RulesetID = difficulty.rulesetId,
                StarRating = difficulty.stars
            }).ToArray()
        };

        [Test]
        public void TestSingleRuleset()
        {
            var beatmapSet = createBeatmapSetWith(
                (rulesetId: 0, stars: 2.0),
                (rulesetId: 0, stars: 3.2),
                (rulesetId: 0, stars: 5.6));

            createDisplay(beatmapSet);
        }

        [Test]
        public void TestMultipleRulesets()
        {
            var beatmapSet = createBeatmapSetWith(
                (rulesetId: 0, stars: 2.0),
                (rulesetId: 3, stars: 2.3),
                (rulesetId: 0, stars: 3.2),
                (rulesetId: 1, stars: 4.3),
                (rulesetId: 0, stars: 5.6));

            createDisplay(beatmapSet);
        }

        [Test]
        public void TestUnknownRuleset()
        {
            var beatmapSet = createBeatmapSetWith(
                (rulesetId: 0, stars: 2.0),
                (rulesetId: 3, stars: 2.3),
                (rulesetId: 0, stars: 3.2),
                (rulesetId: 1, stars: 4.3),
                (rulesetId: 0, stars: 5.6),
                (rulesetId: 15, stars: 7.8));

            createDisplay(beatmapSet);
        }

        [Test]
        public void TestMaximumUncollapsed()
        {
            var beatmapSet = createBeatmapSetWith(Enumerable.Range(0, 12).Select(i => (rulesetId: i % 4, stars: 2.5 + i * 0.25)).ToArray());
            createDisplay(beatmapSet);
        }

        [Test]
        public void TestMinimumCollapsed()
        {
            var beatmapSet = createBeatmapSetWith(Enumerable.Range(0, 13).Select(i => (rulesetId: i % 4, stars: 2.5 + i * 0.25)).ToArray());
            createDisplay(beatmapSet);
        }

        [Test]
        public void TestAdjustableDotSize()
        {
            var beatmapSet = createBeatmapSetWith(
                (rulesetId: 0, stars: 2.0),
                (rulesetId: 3, stars: 2.3),
                (rulesetId: 0, stars: 3.2),
                (rulesetId: 1, stars: 4.3),
                (rulesetId: 0, stars: 5.6));

            createDisplay(beatmapSet);

            AddStep("change dot dimensions", () =>
            {
                display.DotSize = new Vector2(8, 12);
                display.DotSpacing = 2;
            });
            AddStep("change dot dimensions back", () =>
            {
                display.DotSize = new Vector2(4, 8);
                display.DotSpacing = 1;
            });
        }

        private void createDisplay(IBeatmapSetInfo beatmapSetInfo) => AddStep("create spectrum display", () => Child = display = new DifficultySpectrumDisplay(beatmapSetInfo)
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            Scale = new Vector2(3)
        });
    }
}
