// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Pages;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneResults : ScreenTestScene
    {
        private BeatmapManager beatmaps;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Results),
            typeof(ResultsPage),
            typeof(ScoreResultsPage),
            typeof(RetryButton),
            typeof(ReplayDownloadButton),
            typeof(LocalLeaderboardPage)
        };

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var beatmapInfo = beatmaps.QueryBeatmap(b => b.RulesetID == 0);
            if (beatmapInfo != null)
                Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);

            LoadScreen(new SoloResults(new ScoreInfo
            {
                TotalScore = 2845370,
                Accuracy = 0.98,
                MaxCombo = 123,
                Rank = ScoreRank.A,
                Date = DateTimeOffset.Now,
                Statistics = new Dictionary<HitResult, int>
                {
                    { HitResult.Great, 50 },
                    { HitResult.Good, 20 },
                    { HitResult.Meh, 50 },
                    { HitResult.Miss, 1 }
                },
                User = new User
                {
                    Username = "peppy",
                }
            }));
        }
    }
}
