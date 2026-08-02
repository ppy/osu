// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Online.API.Requests.Responses;
using osuTK;

namespace osu.Game.Tests.Visual.Beatmaps
{
    public partial class TestSceneDifficultySpectrumDisplay : OsuTestScene
    {
        private DifficultySpectrumDisplay display = null!;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create spectrum display", () => Child = display = new DifficultySpectrumDisplay
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(3)
            });
        }

        [Test]
        public void TestSingleRuleset()
        {
            var beatmapSet = createBeatmapSetWith(
                (rulesetId: 0, stars: 2.0),
                (rulesetId: 0, stars: 3.2),
                (rulesetId: 0, stars: 5.6));

            AddStep("set beatmap to display", () => display.BeatmapSet = beatmapSet);
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

            AddStep("set beatmap to display", () => display.BeatmapSet = beatmapSet);
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

            AddStep("set beatmap to display", () => display.BeatmapSet = beatmapSet);
        }

        [Test]
        public void TestMaximumUncollapsed()
        {
            var beatmapSet = createBeatmapSetWith(Enumerable.Range(0, 12).Select(i => (rulesetId: i % 4, stars: 2.5 + i * 0.25)).ToArray());
            AddStep("set beatmap to display", () => display.BeatmapSet = beatmapSet);
        }

        [Test]
        public void TestMinimumCollapsed()
        {
            var beatmapSet = createBeatmapSetWith(Enumerable.Range(0, 13).Select(i => (rulesetId: i % 4, stars: 2.5 + i * 0.25)).ToArray());
            AddStep("set beatmap to display", () => display.BeatmapSet = beatmapSet);
        }

        private static APIBeatmapSet createBeatmapSetWith(params (int rulesetId, double stars)[] difficulties) => new APIBeatmapSet
        {
            Beatmaps = difficulties.Select(difficulty => new APIBeatmap
            {
                RulesetID = difficulty.rulesetId,
                StarRating = difficulty.stars
            }).ToArray()
        };
    }
}
