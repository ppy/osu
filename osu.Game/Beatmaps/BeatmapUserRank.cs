// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;
using osu.Game.Scoring;
using Realms;

namespace osu.Game.Beatmaps
{
    public class BeatmapUserRank : EmbeddedObject
    {
        public int OsuRank { get; set; } = (int)ScoreRank.F - 1;
        public int ManiaRank { get; set; } = (int)ScoreRank.F - 1;
        public int TaikoRank { get; set; } = (int)ScoreRank.F - 1;
        public int FruitsRank { get; set; } = (int)ScoreRank.F - 1;

        public void ResetRanks()
        {
            OsuRank = (int)ScoreRank.F - 1;
            ManiaRank = (int)ScoreRank.F - 1;
            TaikoRank = (int)ScoreRank.F - 1;
            FruitsRank = (int)ScoreRank.F - 1;
        }

        public void SetRankByRulesetInfo(IRulesetInfo rulesetInfo, ScoreRank rank)
        {
            switch (rulesetInfo.ShortName)
            {
                case @"osu":
                    OsuRank = (int)rank;
                    break;
                case @"taiko":
                    TaikoRank = (int)rank;
                    break;
                case @"fruits":
                    FruitsRank = (int)rank;
                    break;
                case @"mania":
                    ManiaRank = (int)rank;
                    break;
            }
        }

        public ScoreRank? GetRankByRulesetInfo(IRulesetInfo rulesetInfo)
        {
            switch (rulesetInfo.ShortName)
            {
                case @"osu":
                    return OsuRank >= (int)ScoreRank.F ? (ScoreRank)OsuRank : null;
                case @"taiko":
                    return TaikoRank >= (int)ScoreRank.F ? (ScoreRank)TaikoRank : null;
                case @"fruits":
                    return FruitsRank >= (int)ScoreRank.F ? (ScoreRank)FruitsRank : null;
                case @"mania":
                    return ManiaRank >= (int)ScoreRank.F ? (ScoreRank)ManiaRank : null;
                default: return null;
            }
        }
    }
}
