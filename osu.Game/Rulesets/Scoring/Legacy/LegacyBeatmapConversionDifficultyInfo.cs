// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Scoring.Legacy
{
    /// <summary>
    /// A set of properties that are required to facilitate beatmap conversion between legacy rulesets.
    /// </summary>
    public class LegacyBeatmapConversionDifficultyInfo : IBeatmapDifficultyInfo
    {
        /// <summary>
        /// The beatmap's ruleset.
        /// </summary>
        public IRulesetInfo SourceRuleset { get; set; } = new RulesetInfo();

        /// <summary>
        /// The beatmap circle size.
        /// </summary>
        public float CircleSize { get; set; }

        /// <summary>
        /// The beatmap overall difficulty.
        /// </summary>
        public float OverallDifficulty { get; set; }

        /// <summary>
        /// The count of hitcircles in the beatmap.
        /// </summary>
        public int CircleCount { get; set; }

        /// <summary>
        /// The total count of hitobjects in the beatmap.
        /// </summary>
        public int TotalObjectCount { get; set; }

        float IBeatmapDifficultyInfo.DrainRate => 0;
        float IBeatmapDifficultyInfo.ApproachRate => 0;
        double IBeatmapDifficultyInfo.SliderMultiplier => 0;
        double IBeatmapDifficultyInfo.SliderTickRate => 0;
        int IBeatmapDifficultyInfo.EndTimeObjectCount => TotalObjectCount - CircleCount;

        public static LegacyBeatmapConversionDifficultyInfo FromAPIBeatmap(APIBeatmap apiBeatmap) => FromBeatmapInfo(apiBeatmap);

        public static LegacyBeatmapConversionDifficultyInfo FromBeatmap(IBeatmap beatmap) => new LegacyBeatmapConversionDifficultyInfo
        {
            SourceRuleset = beatmap.BeatmapInfo.Ruleset,
            CircleSize = beatmap.Difficulty.CircleSize,
            OverallDifficulty = beatmap.Difficulty.OverallDifficulty,
            CircleCount = beatmap.HitObjects.Count - beatmap.HitObjects.Count(h => h is IHasDuration),
            TotalObjectCount = beatmap.HitObjects.Count
        };

        public static LegacyBeatmapConversionDifficultyInfo FromBeatmapInfo(IBeatmapInfo beatmapInfo) => new LegacyBeatmapConversionDifficultyInfo
        {
            SourceRuleset = beatmapInfo.Ruleset,
            CircleSize = beatmapInfo.Difficulty.CircleSize,
            OverallDifficulty = beatmapInfo.Difficulty.OverallDifficulty,
            CircleCount = beatmapInfo.Difficulty.TotalObjectCount - beatmapInfo.Difficulty.EndTimeObjectCount,
            TotalObjectCount = beatmapInfo.Difficulty.TotalObjectCount
        };
    }
}
