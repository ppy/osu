// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;
using Newtonsoft.Json;
using osu.Framework.Lists;
using osu.Game.Beatmaps.Formats;
using osu.Game.IO.Serialization.Converters;

namespace osu.Game.Beatmaps
{
    public class Beatmap<T> : IBeatmap<T>
        where T : HitObject
    {
        private BeatmapDifficulty difficulty = new BeatmapDifficulty();

        public BeatmapDifficulty Difficulty
        {
            get => difficulty;
            set
            {
                difficulty = value;

                beatmapInfo.Difficulty = difficulty.Clone();
            }
        }

        private BeatmapInfo beatmapInfo;

        public BeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                beatmapInfo = value;

                Difficulty = beatmapInfo.Difficulty.Clone();
            }
        }

        public Beatmap()
        {
            beatmapInfo = new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Artist = @"Unknown",
                    Title = @"Unknown",
                    Author = { Username = @"Unknown Creator" },
                },
                DifficultyName = @"Normal",
                Difficulty = Difficulty,
            };
        }

        [JsonIgnore]
        public BeatmapMetadata Metadata => BeatmapInfo.Metadata;

        public ControlPointInfo ControlPointInfo { get; set; } = new ControlPointInfo();

        public SortedList<BreakPeriod> Breaks { get; set; } = new SortedList<BreakPeriod>(Comparer<BreakPeriod>.Default);

        public List<string> UnhandledEventLines { get; set; } = new List<string>();

        [JsonIgnore]
        public double TotalBreakTime => Breaks.Sum(b => b.Duration);

        [JsonConverter(typeof(TypedListConverter<HitObject>))]
        public List<T> HitObjects { get; set; } = new List<T>();

        IReadOnlyList<T> IBeatmap<T>.HitObjects => HitObjects;

        IReadOnlyList<HitObject> IBeatmap.HitObjects => HitObjects;

        public virtual IEnumerable<BeatmapStatistic> GetStatistics() => Enumerable.Empty<BeatmapStatistic>();

        public double GetMostCommonBeatLength()
        {
            double lastTime;

            // The last playable time in the beatmap - the last timing point extends to this time.
            // Note: This is more accurate and may present different results because osu-stable didn't have the ability to calculate slider durations in this context.
            if (!HitObjects.Any())
                lastTime = ControlPointInfo.TimingPoints.LastOrDefault()?.Time ?? 0;
            else
                lastTime = this.GetLastObjectTime();

            var mostCommon =
                // Construct a set of (beatLength, duration) tuples for each individual timing point.
                ControlPointInfo.TimingPoints.Select((t, i) =>
                                {
                                    if (t.Time > lastTime)
                                        return (beatLength: t.BeatLength, 0);

                                    // osu-stable forced the first control point to start at 0.
                                    // This is reproduced here to maintain compatibility around osu!mania scroll speed and song select display.
                                    double currentTime = i == 0 ? 0 : t.Time;
                                    double nextTime = i == ControlPointInfo.TimingPoints.Count - 1 ? lastTime : ControlPointInfo.TimingPoints[i + 1].Time;

                                    return (beatLength: t.BeatLength, duration: nextTime - currentTime);
                                })
                                // Aggregate durations into a set of (beatLength, duration) tuples for each beat length
                                .GroupBy(t => Math.Round(t.beatLength * 1000) / 1000)
                                .Select(g => (beatLength: g.Key, duration: g.Sum(t => t.duration)))
                                // Get the most common one, or 0 as a suitable default (see handling below)
                                .OrderByDescending(i => i.duration).FirstOrDefault();

            if (mostCommon.beatLength == 0)
                return TimingControlPoint.DEFAULT_BEAT_LENGTH;

            return mostCommon.beatLength;
        }

        public double AudioLeadIn { get; set; }

        public float StackLeniency { get; set; } = 0.7f;

        public bool SpecialStyle { get; set; }

        public bool LetterboxInBreaks { get; set; }

        public bool WidescreenStoryboard { get; set; } = true;

        public bool EpilepsyWarning { get; set; }

        public bool SamplesMatchPlaybackRate { get; set; }

        public double DistanceSpacing { get; set; } = 1.0;

        public int GridSize { get; set; }

        public double TimelineZoom { get; set; } = 1.0;

        public CountdownType Countdown { get; set; } = CountdownType.None;

        public int CountdownOffset { get; set; }

        public int[] Bookmarks { get; set; } = Array.Empty<int>();

        public int BeatmapVersion { get; set; } = LegacyBeatmapEncoder.FIRST_LAZER_VERSION;

        IBeatmap IBeatmap.Clone() => Clone();

        public Beatmap<T> Clone() => (Beatmap<T>)MemberwiseClone();

        public override string ToString() => BeatmapInfo.ToString();
    }

    public class Beatmap : Beatmap<HitObject>
    {
    }
}
