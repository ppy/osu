﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        public IBeatmap BaseBeatmap { get; }

        public DifficultyCalculatorBeatmap(IBeatmap baseBeatmap)
        {
            BaseBeatmap = baseBeatmap;
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

        public IReadOnlyList<HitObject> HitObjects => BaseBeatmap.HitObjects;

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
