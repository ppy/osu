// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Tests.Visual.Navigation;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.SongSelect
{
    [HeadlessTest]
    public class TestSceneBeatmapRecommendations : OsuGameTestScene
    {
        [Resolved]
        private DifficultyRecommender recommender { get; set; }

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [SetUpSteps]
        public new void SetUpSteps()
        {
            AddStep("register request handling", () =>
            {
                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    switch (req)
                    {
                        case GetUserRequest userRequest:
                            userRequest.TriggerSuccess(getUser(userRequest.Ruleset.ID));
                            break;
                    }
                };
            });

            // Force recommender to calculate its star ratings again
            AddStep("calculate recommended SRs", () => recommender.APIStateChanged(API, APIState.Online));

            User getUser(int? rulesetID)
            {
                return new User
                {
                    Username = @"Dummy",
                    Id = 1001,
                    Statistics = new UserStatistics
                    {
                        PP = getNecessaryPP(rulesetID)
                    }
                };
            }

            decimal getNecessaryPP(int? rulesetID)
            {
                switch (rulesetID)
                {
                    case 0:
                        return 336;

                    case 1:
                        return 928;

                    case 2:
                        return 1905;

                    case 3:
                        return 3329;

                    default:
                        return 0;
                }
            }
        }

        [Test]
        public void TestPresentedBeatmapIsRecommended()
        {
            var importFunctions = new List<Func<BeatmapSetInfo>>();

            for (int i = 0; i < 5; i++)
            {
                importFunctions.Add(importBeatmap(i, new List<RulesetInfo> { null, null, null, null, null }));
            }

            for (int i = 0; i < 5; i++)
            {
                presentAndConfirm(importFunctions[i], i, 2);
            }
        }

        [Test]
        public void TestBestRulesetIsRecommended()
        {
            var osuRuleset = rulesets.GetRuleset(0);
            var taikoRuleset = rulesets.GetRuleset(1);
            var catchRuleset = rulesets.GetRuleset(2);
            var maniaRuleset = rulesets.GetRuleset(3);

            var osuImport = importBeatmap(0, new List<RulesetInfo> { osuRuleset });
            var mixedImport = importBeatmap(1, new List<RulesetInfo> { taikoRuleset, catchRuleset, maniaRuleset });

            // Make sure we are on standard ruleset
            presentAndConfirm(osuImport, 0, 1);

            // Present mixed difficulty set, expect ruleset with highest star difficulty
            presentAndConfirm(mixedImport, 1, 3);
        }

        [Test]
        public void TestSecondBestRulesetIsRecommended()
        {
            var osuRuleset = rulesets.GetRuleset(0);
            var taikoRuleset = rulesets.GetRuleset(1);
            var catchRuleset = rulesets.GetRuleset(2);

            var osuImport = importBeatmap(0, new List<RulesetInfo> { osuRuleset });
            var mixedImport = importBeatmap(1, new List<RulesetInfo> { taikoRuleset, catchRuleset, taikoRuleset });

            // Make sure we are on standard ruleset
            presentAndConfirm(osuImport, 0, 1);

            // Present mixed difficulty set, expect ruleset with highest star difficulty
            presentAndConfirm(mixedImport, 1, 2);
        }

        private Func<BeatmapSetInfo> importBeatmap(int importID, List<RulesetInfo> rulesets)
        {
            BeatmapSetInfo imported = null;
            AddStep($"import beatmap {importID}", () =>
            {
                var difficulty = new BeatmapDifficulty();
                var metadata = new BeatmapMetadata
                {
                    Artist = "SomeArtist",
                    AuthorString = "SomeAuthor",
                    Title = $"import {importID}"
                };

                var beatmaps = new List<BeatmapInfo>();
                int difficultyID = 1;

                foreach (RulesetInfo r in rulesets)
                {
                    beatmaps.Add(new BeatmapInfo
                    {
                        OnlineBeatmapID = importID + 1024 * difficultyID,
                        Metadata = metadata,
                        BaseDifficulty = difficulty,
                        Ruleset = r ?? rulesets.First(),
                        StarDifficulty = difficultyID,
                    });
                    difficultyID++;
                }

                imported = Game.BeatmapManager.Import(new BeatmapSetInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    OnlineBeatmapSetID = importID,
                    Metadata = metadata,
                    Beatmaps = beatmaps,
                }).Result;
            });

            AddAssert($"import {importID} succeeded", () => imported != null);

            return () => imported;
        }

        private void presentAndConfirm(Func<BeatmapSetInfo> getImport, int importedID, int expectedDiff)
        {
            AddStep("present beatmap", () => Game.PresentBeatmap(getImport()));

            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is Screens.Select.SongSelect);
            AddUntilStep("recommended beatmap displayed", () => Game.Beatmap.Value.BeatmapInfo.OnlineBeatmapID == importedID + 1024 * expectedDiff);
        }
    }
}
