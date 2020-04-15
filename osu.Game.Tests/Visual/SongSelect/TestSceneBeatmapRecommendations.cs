// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Visual.Navigation;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.SongSelect
{
    public class TestSceneBeatmapRecommendations : OsuGameTestScene
    {
        [Resolved]
        private DifficultyRecommender recommender { get; set; }

        [SetUpSteps]
        public new void SetUpSteps()
        {
            AddStep("register request handling", () =>
            {
                Logger.Log($"Registering request handling for {(DummyAPIAccess)API}");
                ((DummyAPIAccess)API).HandleRequest = req =>
                {
                    Logger.Log($"New request {req}");

                    switch (req)
                    {
                        case GetUserRequest userRequest:
                            userRequest.TriggerSuccess(new User
                            {
                                Username = @"Dummy",
                                Id = 1001,
                                Statistics = new UserStatistics
                                {
                                    PP = 928 // Expected recommended star difficulty is 2.999
                                }
                            });
                            break;
                    }
                };
                // Force recommender to calculate its star ratings again
                recommender.APIStateChanged(API, APIState.Online);
            });
        }

        [Test]
        public void TestPresentedBeatmapIsRecommended()
        {
            var importFunctions = importBeatmaps(5);

            for (int i = 0; i < 5; i++)
            {
                presentAndConfirm(importFunctions[i], i);
            }
        }

        private List<Func<BeatmapSetInfo>> importBeatmaps(int amount, RulesetInfo ruleset = null)
        {
            var importFunctions = new List<Func<BeatmapSetInfo>>();

            for (int i = 0; i < amount; i++)
            {
                importFunctions.Add(importBeatmap(i, ruleset));
            }

            return importFunctions;
        }

        private Func<BeatmapSetInfo> importBeatmap(int i, RulesetInfo ruleset = null)
        {
            BeatmapSetInfo imported = null;
            AddStep($"import beatmap {i * 1000}", () =>
            {
                var difficulty = new BeatmapDifficulty();
                var metadata = new BeatmapMetadata
                {
                    Artist = "SomeArtist",
                    AuthorString = "SomeAuthor",
                    Title = $"import {i * 1000}"
                };

                var beatmaps = new List<BeatmapInfo>();

                for (int j = 1; j <= 5; j++)
                {
                    beatmaps.Add(new BeatmapInfo
                    {
                        OnlineBeatmapID = j * 1024 + i * 5,
                        Metadata = metadata,
                        BaseDifficulty = difficulty,
                        Ruleset = ruleset ?? new OsuRuleset().RulesetInfo,
                        StarDifficulty = j,
                    });
                }

                imported = Game.BeatmapManager.Import(new BeatmapSetInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    OnlineBeatmapSetID = i,
                    Metadata = metadata,
                    Beatmaps = beatmaps,
                }).Result;
            });

            AddAssert($"import {i * 1000} succeeded", () => imported != null);

            return () => imported;
        }

        private void presentAndConfirm(Func<BeatmapSetInfo> getImport, int importedID)
        {
            AddStep("present beatmap", () => Game.PresentBeatmap(getImport()));

            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is Screens.Select.SongSelect);
            AddUntilStep("recommended beatmap displayed", () => Game.Beatmap.Value.BeatmapInfo.OnlineBeatmapID == importedID * 5 + 1024 * 3);
            AddAssert("correct ruleset selected", () => Game.Ruleset.Value.ID == getImport().Beatmaps.First().Ruleset.ID);
        }
    }
}
