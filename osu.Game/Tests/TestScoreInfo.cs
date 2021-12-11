// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests
{
    public class TestScoreInfo : ScoreInfo
    {
        public TestScoreInfo(RulesetInfo ruleset, bool excessMods = false)
        {
            User = new APIUser
            {
                Id = 2,
                Username = "peppy",
                CoverUrl = "https://osu.ppy.sh/images/headers/profile-covers/c3.jpg",
            };

            BeatmapInfo = new TestBeatmap(ruleset).BeatmapInfo;
            Ruleset = ruleset;
            RulesetID = ruleset.ID ?? 0;

            Mods = excessMods
                ? ruleset.CreateInstance().CreateAllMods().ToArray()
                : new Mod[] { new TestModHardRock(), new TestModDoubleTime() };

            TotalScore = 2845370;
            Accuracy = 0.95;
            MaxCombo = 999;
            Rank = ScoreRank.S;
            Date = DateTimeOffset.Now;

            Statistics[HitResult.Miss] = 1;
            Statistics[HitResult.Meh] = 50;
            Statistics[HitResult.Ok] = 100;
            Statistics[HitResult.Good] = 200;
            Statistics[HitResult.Great] = 300;
            Statistics[HitResult.Perfect] = 320;
            Statistics[HitResult.SmallTickHit] = 50;
            Statistics[HitResult.SmallTickMiss] = 25;
            Statistics[HitResult.LargeTickHit] = 100;
            Statistics[HitResult.LargeTickMiss] = 50;
            Statistics[HitResult.SmallBonus] = 10;
            Statistics[HitResult.SmallBonus] = 50;

            Position = 1;
        }

        private class TestModHardRock : ModHardRock
        {
            public override double ScoreMultiplier => 1;
        }

        private class TestModDoubleTime : ModDoubleTime
        {
            public override double ScoreMultiplier => 1;
        }
    }
}
