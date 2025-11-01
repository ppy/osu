﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        public IBeatmap BaseBeatmap { get; }

        public ProgressiveCalculationBeatmap(IBeatmap baseBeatmap)
        {
            BaseBeatmap = baseBeatmap;
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
            get => BaseBeatmap.BeatmapInfo;
            set => BaseBeatmap.BeatmapInfo = value;
        }

        public ControlPointInfo ControlPointInfo
        {
            get => BaseBeatmap.ControlPointInfo;
            set => BaseBeatmap.ControlPointInfo = value;
        }

        public BeatmapMetadata Metadata => BaseBeatmap.Metadata;

        public BeatmapDifficulty Difficulty
        {
            get => BaseBeatmap.Difficulty;
            set => BaseBeatmap.Difficulty = value;
        }

        public SortedList<BreakPeriod> Breaks
        {
            get => BaseBeatmap.Breaks;
            set => BaseBeatmap.Breaks = value;
        }

        public List<string> UnhandledEventLines => BaseBeatmap.UnhandledEventLines;

        public double TotalBreakTime => BaseBeatmap.TotalBreakTime;
        public IEnumerable<BeatmapStatistic> GetStatistics() => BaseBeatmap.GetStatistics();
        public double GetMostCommonBeatLength() => BaseBeatmap.GetMostCommonBeatLength();
        public double AudioLeadIn { get => BaseBeatmap.AudioLeadIn; set => BaseBeatmap.AudioLeadIn = value; }
        public float StackLeniency { get => BaseBeatmap.StackLeniency; set => BaseBeatmap.StackLeniency = value; }
        public bool SpecialStyle { get => BaseBeatmap.SpecialStyle; set => BaseBeatmap.SpecialStyle = value; }
        public bool LetterboxInBreaks { get => BaseBeatmap.LetterboxInBreaks; set => BaseBeatmap.LetterboxInBreaks = value; }
        public bool WidescreenStoryboard { get => BaseBeatmap.WidescreenStoryboard; set => BaseBeatmap.WidescreenStoryboard = value; }
        public bool EpilepsyWarning { get => BaseBeatmap.EpilepsyWarning; set => BaseBeatmap.EpilepsyWarning = value; }
        public bool SamplesMatchPlaybackRate { get => BaseBeatmap.SamplesMatchPlaybackRate; set => BaseBeatmap.SamplesMatchPlaybackRate = value; }
        public double DistanceSpacing { get => BaseBeatmap.DistanceSpacing; set => BaseBeatmap.DistanceSpacing = value; }
        public int GridSize { get => BaseBeatmap.GridSize; set => BaseBeatmap.GridSize = value; }
        public double TimelineZoom { get => BaseBeatmap.TimelineZoom; set => BaseBeatmap.TimelineZoom = value; }
        public CountdownType Countdown { get => BaseBeatmap.Countdown; set => BaseBeatmap.Countdown = value; }
        public int CountdownOffset { get => BaseBeatmap.CountdownOffset; set => BaseBeatmap.CountdownOffset = value; }
        public int[] Bookmarks { get => BaseBeatmap.Bookmarks; set => BaseBeatmap.Bookmarks = value; }
        public int BeatmapVersion => BaseBeatmap.BeatmapVersion;
        public IBeatmap Clone() => new DifficultyCalculatorBeatmap(BaseBeatmap.Clone());

        #endregion
    }
}
