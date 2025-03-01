// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.SongSelect
{
    public partial class TestSceneBeatmapLeaderboardSorting : OsuTestScene
    {
        private readonly PlayBeatmapDetailArea playBeatmapDetailArea;

        [Cached(typeof(IDialogOverlay))]
        private readonly DialogOverlay dialogOverlay;

        private ScoreManager scoreManager = null!;
        private RulesetStore rulesetStore = null!;
        private BeatmapManager beatmapManager = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.Cache(rulesetStore = new RealmRulesetStore(Realm));
            dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, Realm, null, dependencies.Get<AudioManager>(), Resources, dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(scoreManager = new ScoreManager(rulesetStore, () => beatmapManager, LocalStorage, Realm, API));
            Dependencies.Cache(Realm);

            return dependencies;
        }

        public TestSceneBeatmapLeaderboardSorting()
        {
            AddRange(new Drawable[]
            {
                dialogOverlay = new DialogOverlay
                {
                    Depth = -1
                },
                playBeatmapDetailArea = new PlayBeatmapDetailArea
                {
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Size = new Vector2(550f, 450f),
                }
            });
        }

        [Test]
        public void TestLocalScoresDisplay()
        {
            BeatmapInfo beatmapInfo = null!;

            AddStep(@"Set beatmap", () =>
            {
                beatmapManager.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                beatmapInfo = beatmapManager.GetAllUsableBeatmapSets().First().Beatmaps.First();

                playBeatmapDetailArea.Leaderboard.BeatmapInfo = beatmapInfo;
            });

            AddStep(@"Add sample scores", () =>
            {
                for (int i = 0; i < 10; i++)
                    scoreManager.Import(addRandomScore(beatmapInfo));
            });

            AddStep(@"Add another score", () => scoreManager.Import(addRandomScore(beatmapInfo)));

            clearScores();
        }

        private void clearScores()
        {
            AddStep("Clear all scores", () => scoreManager.Delete());
        }

        private static ScoreInfo addRandomScore(BeatmapInfo beatmapInfo)
        {
            return new ScoreInfo
            {
                Rank = ScoreRank.XH,
                Accuracy = RNG.NextDouble(0, 1),
                MaxCombo = RNG.Next(0, 1500),
                TotalScore = RNG.Next(500000, 1200000),
                Date = DateTime.Now.AddMinutes(RNG.Next(0, 1000) * -1),
                Statistics = new Dictionary<HitResult, int>
                {
                    { HitResult.Miss, RNG.Next(0, 25) },
                },
                Ruleset = new OsuRuleset().RulesetInfo,
                BeatmapInfo = beatmapInfo,
                BeatmapHash = beatmapInfo.Hash,
                User = new APIUser
                {
                    Id = 2,
                    Username = @"peppy",
                    CountryCode = CountryCode.JP,
                },
            };
        }
    }
}
