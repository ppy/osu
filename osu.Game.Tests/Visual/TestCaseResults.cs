// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Users;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseResults : OsuTestCase
    {
        private BeatmapManager beatmaps;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Score),
            typeof(Results),
            typeof(ResultsPage),
            typeof(ResultsPageScore),
            typeof(ResultsPageRanking)
        };

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;
        }

        private WorkingBeatmap beatmap;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (beatmap == null)
            {
                var beatmapInfo = beatmaps.QueryBeatmap(b => b.RulesetID == 0);
                if (beatmapInfo != null)
                    beatmap = beatmaps.GetWorkingBeatmap(beatmapInfo);
            }

            Add(new Results(new Score
            {
                TotalScore = 2845370,
                Accuracy = 0.98,
                MaxCombo = 123,
                Rank = ScoreRank.A,
                Date = DateTimeOffset.Now,
                Statistics = new Dictionary<HitResult, dynamic>
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
            })
            {
                InitialBeatmap = beatmap
            });
        }
    }
}
