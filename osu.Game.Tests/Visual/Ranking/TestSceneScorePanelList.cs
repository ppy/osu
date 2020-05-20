// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens.Ranking;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Ranking
{
    public class TestSceneScorePanelList : OsuTestScene
    {
        public TestSceneScorePanelList()
        {
            var list = new ScorePanelList
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };

            Add(list);

            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
            list.AddScore(createScore());
        }

        private ScoreInfo createScore() => new ScoreInfo
        {
            User = new User
            {
                Id = 2,
                Username = "peppy",
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            },
            Beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo,
            Mods = new Mod[] { new OsuModHardRock(), new OsuModDoubleTime() },
            TotalScore = 2845370,
            Accuracy = 0.95,
            MaxCombo = 999,
            Rank = ScoreRank.S,
            Date = DateTimeOffset.Now,
            Statistics =
            {
                { HitResult.Miss, 1 },
                { HitResult.Meh, 50 },
                { HitResult.Good, 100 },
                { HitResult.Great, 300 },
            }
        };
    }
}
