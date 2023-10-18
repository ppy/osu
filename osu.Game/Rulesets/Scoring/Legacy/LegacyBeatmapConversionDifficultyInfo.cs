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
        /// <remarks>
        /// When converting from osu! ruleset beatmaps, this is equivalent to the sum of sliders and spinners in the beatmap.
        /// </remarks>
        public int CircleCount { get; set; }

        /// <summary>
        /// The total count of hitobjects in the beatmap.
        /// </summary>
        public int TotalObjectCount { get; set; }

        float IBeatmapDifficultyInfo.DrainRate => 0;
        float IBeatmapDifficultyInfo.ApproachRate => 0;
        double IBeatmapDifficultyInfo.SliderMultiplier => 0;
        double IBeatmapDifficultyInfo.SliderTickRate => 0;

        public static LegacyBeatmapConversionDifficultyInfo FromAPIBeatmap(APIBeatmap apiBeatmap) => new LegacyBeatmapConversionDifficultyInfo
        {
            SourceRuleset = apiBeatmap.Ruleset,
            CircleSize = apiBeatmap.CircleSize,
            OverallDifficulty = apiBeatmap.OverallDifficulty,
            CircleCount = apiBeatmap.CircleCount,
            TotalObjectCount = apiBeatmap.SliderCount + apiBeatmap.SpinnerCount + apiBeatmap.CircleCount
        };

        public static LegacyBeatmapConversionDifficultyInfo FromBeatmap(IBeatmap beatmap) => new LegacyBeatmapConversionDifficultyInfo
        {
            SourceRuleset = beatmap.BeatmapInfo.Ruleset,
            CircleSize = beatmap.Difficulty.CircleSize,
            OverallDifficulty = beatmap.Difficulty.OverallDifficulty,
            CircleCount = beatmap.HitObjects.Count(h => h is not IHasDuration),
            TotalObjectCount = beatmap.HitObjects.Count
        };
    }
}
