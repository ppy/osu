// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Taiko;
using osu.Game.Tests.Resources;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneBeatmapRecommendations : OsuGameTestScene
    {
        [Resolved]
        private IRulesetStore rulesetStore { get; set; }

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("populate ruleset statistics", () =>
            {
                Dictionary<string, UserStatistics> rulesetStatistics = new Dictionary<string, UserStatistics>();

                rulesetStore.AvailableRulesets.Where(ruleset => ruleset.IsLegacyRuleset()).ForEach(rulesetInfo =>
                {
                    rulesetStatistics[rulesetInfo.ShortName] = new UserStatistics
                    {
                        PP = getNecessaryPP(rulesetInfo.OnlineID)
                    };
                });

                API.LocalUser.Value.RulesetsStatistics = rulesetStatistics;
            });

            decimal getNecessaryPP(int? rulesetID)
            {
                switch (rulesetID)
                {
                    case 0:
                        return 336; // recommended star rating of 2

                    case 1:
                        return 928; // SR 3

                    case 2:
                        return 1905; // SR 4

                    case 3:
                        return 3329; // SR 5

                    default:
                        return 0;
                }
            }
        }

        [Test]
        public void TestPresentedBeatmapIsRecommended()
        {
            List<BeatmapSetInfo> beatmapSets = null;
            const int import_count = 5;

            AddStep("import 5 maps", () =>
            {
                beatmapSets = new List<BeatmapSetInfo>();

                for (int i = 0; i < import_count; ++i)
                {
                    beatmapSets.Add(importBeatmapSet(Enumerable.Repeat(new OsuRuleset().RulesetInfo, 5)));
                }
            });

            AddAssert("all sets imported", () => ensureAllBeatmapSetsImported(beatmapSets));

            presentAndConfirm(() => beatmapSets[3], 2);
        }

        [Test]
        public void TestCurrentRulesetIsRecommended()
        {
            BeatmapSetInfo catchSet = null, mixedSet = null;

            AddStep("create catch beatmapset", () => catchSet = importBeatmapSet(new[] { new CatchRuleset().RulesetInfo }));
            AddStep("create mixed beatmapset", () => mixedSet = importBeatmapSet(new[] { new TaikoRuleset().RulesetInfo, new CatchRuleset().RulesetInfo, new ManiaRuleset().RulesetInfo }));

            AddAssert("all sets imported", () => ensureAllBeatmapSetsImported(new[] { catchSet, mixedSet }));

            // Switch to catch
            presentAndConfirm(() => catchSet, 1);

            AddAssert("game-wide ruleset changed", () => Game.Ruleset.Value.Equals(catchSet.Beatmaps.First().Ruleset));

            // Present mixed difficulty set, expect current ruleset to be selected
            presentAndConfirm(() => mixedSet, 2);
        }

        [Test]
        public void TestBestRulesetIsRecommended()
        {
            BeatmapSetInfo osuSet = null, mixedSet = null;

            AddStep("create osu! beatmapset", () => osuSet = importBeatmapSet(new[] { new OsuRuleset().RulesetInfo }));
            AddStep("create mixed beatmapset", () => mixedSet = importBeatmapSet(new[] { new TaikoRuleset().RulesetInfo, new CatchRuleset().RulesetInfo, new ManiaRuleset().RulesetInfo }));

            AddAssert("all sets imported", () => ensureAllBeatmapSetsImported(new[] { osuSet, mixedSet }));

            // Make sure we are on standard ruleset
            presentAndConfirm(() => osuSet, 1);

            // Present mixed difficulty set, expect ruleset with highest star difficulty
            presentAndConfirm(() => mixedSet, 3);
        }

        [Test]
        public void TestSecondBestRulesetIsRecommended()
        {
            BeatmapSetInfo osuSet = null, mixedSet = null;

            AddStep("create osu! beatmapset", () => osuSet = importBeatmapSet(new[] { new OsuRuleset().RulesetInfo }));
            AddStep("create mixed beatmapset", () => mixedSet = importBeatmapSet(new[] { new TaikoRuleset().RulesetInfo, new CatchRuleset().RulesetInfo, new TaikoRuleset().RulesetInfo }));

            AddAssert("all sets imported", () => ensureAllBeatmapSetsImported(new[] { osuSet, mixedSet }));

            // Make sure we are on standard ruleset
            presentAndConfirm(() => osuSet, 1);

            // Present mixed difficulty set, expect ruleset with second highest star difficulty
            presentAndConfirm(() => mixedSet, 2);
        }

        [Test]
        public void TestCorrectStarRatingIsUsed()
        {
            BeatmapSetInfo osuSet = null, maniaSet = null;

            AddStep("create osu! beatmapset", () => osuSet = importBeatmapSet(new[] { new OsuRuleset().RulesetInfo }));
            AddStep("create mania beatmapset", () => maniaSet = importBeatmapSet(Enumerable.Repeat(new ManiaRuleset().RulesetInfo, 10)));

            AddAssert("all sets imported", () => ensureAllBeatmapSetsImported(new[] { osuSet, maniaSet }));

            // Make sure we are on standard ruleset
            presentAndConfirm(() => osuSet, 1);

            // Present mania set, expect the difficulty that matches recommended mania star rating
            presentAndConfirm(() => maniaSet, 5);
        }

        private BeatmapSetInfo importBeatmapSet(IEnumerable<RulesetInfo> difficultyRulesets)
        {
            var rulesets = difficultyRulesets.ToArray();

            var beatmapSet = TestResources.CreateTestBeatmapSetInfo(rulesets.Length, rulesets);

            var importedBeatmapSet = Game.BeatmapManager.Import(beatmapSet);

            Debug.Assert(importedBeatmapSet != null);

            importedBeatmapSet.PerformWrite(s =>
            {
                for (int i = 0; i < rulesets.Length; i++)
                {
                    var beatmap = s.Beatmaps[i];

                    beatmap.StarRating = i + 1;
                    beatmap.DifficultyName = $"SR{i + 1}";
                }
            });

            return importedBeatmapSet.Value;
        }

        private bool ensureAllBeatmapSetsImported(IEnumerable<BeatmapSetInfo> beatmapSets) => beatmapSets.All(set => set != null);

        private void presentAndConfirm(Func<BeatmapSetInfo> getImport, int expectedDiff)
        {
            AddStep("present beatmap", () => Game.PresentBeatmap(getImport()));

            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is Screens.Select.SongSelect);
            AddUntilStep("recommended beatmap displayed", () => Game.Beatmap.Value.BeatmapInfo.MatchesOnlineID(getImport().Beatmaps[expectedDiff - 1]));
        }
    }
}
