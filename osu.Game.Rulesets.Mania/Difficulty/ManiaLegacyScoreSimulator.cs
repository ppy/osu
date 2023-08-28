// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Difficulty
{
    internal class ManiaLegacyScoreSimulator : ILegacyScoreSimulator
    {
        public int AccuracyScore => 0;
        public int ComboScore { get; private set; }
        public double BonusScoreRatio => 0;

        public void Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods)
        {
            IBeatmap baseBeatmap = workingBeatmap.Beatmap;

            int countNormal = 0;
            int countSlider = 0;
            int countSpinner = 0;

            foreach (HitObject obj in baseBeatmap.HitObjects)
            {
                switch (obj)
                {
                    case IHasPath:
                        countSlider++;
                        break;

                    case IHasDuration:
                        countSpinner++;
                        break;

                    default:
                        countNormal++;
                        break;
                }
            }

            int objectCount = countNormal + countSlider + countSpinner;

            double multiplier = new ManiaRuleset().GetLegacyScoreMultiplier(mods, new LegacyBeatmapConversionDifficultyInfo
            {
                IsForTargetRuleset = baseBeatmap.BeatmapInfo.Ruleset.OnlineID == 3,
                CircleSize = baseBeatmap.Difficulty.CircleSize,
                OverallDifficulty = baseBeatmap.Difficulty.OverallDifficulty,
                CircleCount = countNormal,
                TotalObjectCount = objectCount
            });

            ComboScore = (int)(1000000 * multiplier);
        }
    }
}
