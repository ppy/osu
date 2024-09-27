// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Lists;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Difficulty
{
    /// <summary>
    /// Used to calculate timed difficulty attributes, where only a subset of hitobjects should be visible at any point in time.
    /// </summary>
    internal class ProgressiveCalculationBeatmap : IDifficultyCalculatorBeatmap
    {
        private readonly IBeatmap baseBeatmap;

        public ProgressiveCalculationBeatmap(IBeatmap baseBeatmap)
        {
            this.baseBeatmap = baseBeatmap;
        }

        private readonly List<HitObject> hitObjects = new List<HitObject>();

        IReadOnlyList<HitObject> IBeatmap.HitObjects => hitObjects;

        private int maxCombo;

        public int GetMaxCombo() => maxCombo;

        private readonly Dictionary<Type, int> hitObjectsCounts = new Dictionary<Type, int>();

        public int GetHitObjectCountOf(Type type) => hitObjectsCounts.GetValueOrDefault(type);

        public void AddHitObject(HitObject hitObject)
        {
            hitObjects.Add(hitObject);

            var objectType = hitObject.GetType();
            hitObjectsCounts[objectType] = hitObjectsCounts.GetValueOrDefault(objectType, 0) + 1;

            addCombo(hitObject);
        }

        private void addCombo(HitObject hitObject)
        {
            if (hitObject.Judgement.MaxResult.AffectsCombo())
                maxCombo++;

            foreach (var nested in hitObject.NestedHitObjects)
                addCombo(nested);
        }

        #region Delegated IBeatmap implementation

        public IReadOnlyList<HitObject> HitObjects => hitObjects;

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
