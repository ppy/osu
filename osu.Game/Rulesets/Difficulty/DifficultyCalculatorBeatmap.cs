// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// Extended <see cref="Beatmap"/> with functions needed in dififculty calculation.
    /// Delegates all other functions to the base beatmap.
    /// </summary>
    internal class DifficultyCalculatorBeatmap : IDifficultyCalculatorBeatmap
    {
        private readonly IBeatmap baseBeatmap;

        public DifficultyCalculatorBeatmap(IBeatmap baseBeatmap)
        {
            this.baseBeatmap = baseBeatmap;
        }

        public int GetMaxCombo()
        {
            int combo = 0;
            foreach (var h in HitObjects)
                addCombo(h, ref combo);
            return combo;

            static void addCombo(HitObject hitObject, ref int combo)
            {
                if (hitObject.Judgement.MaxResult.AffectsCombo())
                    combo++;

                foreach (var nested in hitObject.NestedHitObjects)
                    addCombo(nested, ref combo);
            }
        }

        public int GetHitObjectCountOf(Type type) => HitObjects.Count(h => h.GetType() == type);

        #region Delegated IBeatmap implementation

        public IReadOnlyList<HitObject> HitObjects => baseBeatmap.HitObjects;

        public BeatmapInfo BeatmapInfo
        {
            get => baseBeatmap.BeatmapInfo;
            set => baseBeatmap.BeatmapInfo = value;
        }

        public ControlPointInfo ControlPointInfo
        {
            get => baseBeatmap.ControlPointInfo;
            set => baseBeatmap.ControlPointInfo = value;
        }

        public BeatmapMetadata Metadata => baseBeatmap.Metadata;

        public BeatmapDifficulty Difficulty
        {
            get => baseBeatmap.Difficulty;
            set => baseBeatmap.Difficulty = value;
        }

        public SortedList<BreakPeriod> Breaks
        {
            get => baseBeatmap.Breaks;
            set => baseBeatmap.Breaks = value;
        }

        public List<string> UnhandledEventLines => baseBeatmap.UnhandledEventLines;

        public double TotalBreakTime => baseBeatmap.TotalBreakTime;
        public IEnumerable<BeatmapStatistic> GetStatistics() => baseBeatmap.GetStatistics();
        public double GetMostCommonBeatLength() => baseBeatmap.GetMostCommonBeatLength();
        public IBeatmap Clone() => new DifficultyCalculatorBeatmap(baseBeatmap.Clone());

        #endregion
    }
}
