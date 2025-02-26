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

            TimelineZoom = 1.0;
            DistanceSpacing = 1.0;
            Bookmarks = Array.Empty<int>();
            CountdownOffset = 0;
            Countdown = CountdownType.None;
            AudioLeadIn = 0;
            EpilepsyWarning = false;
            SamplesMatchPlaybackRate = false;
            SpecialStyle = false;
            StackLeniency = 0.7f;
            WidescreenStoryboard = false;
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
            if (!HitObjects.Any())
                lastTime = ControlPointInfo.TimingPoints.LastOrDefault()?.Time ?? 0;
            else
                lastTime = this.GetLastObjectTime();

            var mostCommon =
                ControlPointInfo.TimingPoints.Select((t, i) =>
                {
                    if (t.Time > lastTime)
                        return (beatLength: t.BeatLength, 0);

                    double currentTime = i == 0 ? 0 : t.Time;
                    double nextTime = i == ControlPointInfo.TimingPoints.Count - 1 ? lastTime : ControlPointInfo.TimingPoints[i + 1].Time;

                    return (beatLength: t.BeatLength, duration: nextTime - currentTime);
                })
                .GroupBy(t => Math.Round(t.beatLength * 1000) / 1000)
                .Select(g => (beatLength: g.Key, duration: g.Sum(t => t.duration)))
                .OrderByDescending(i => i.duration).FirstOrDefault();

            if (mostCommon.beatLength == 0)
                return TimingControlPoint.DEFAULT_BEAT_LENGTH;

            return mostCommon.beatLength;
        }

        public CountdownType Countdown { get; set; }
        public int CountdownOffset { get; set; }
        public double DistanceSpacing { get; set; }
        public int[] Bookmarks { get; set; }

        public bool EpilepsyWarning { get; set; }
        public int GridSize { get; set; }
        public bool LetterboxInBreaks { get; set; }
        public bool SamplesMatchPlaybackRate { get; set; }
        public bool SpecialStyle { get; set; }
        public float StackLeniency { get; set; }
        public double TimelineZoom { get; set; }
        public bool WidescreenStoryboard { get; set; }

        private void checkForOldMapVersion()
        {
            if (BeatmapInfo.BeatmapVersion < 8)
            {
                MigrateOldBeatmap();
            }
        }

        public void MigrateOldBeatmap()
        {
            foreach (var timingPoint in ControlPointInfo.TimingPoints)
            {
                if (timingPoint.BeatLength == 0)
                {
                    timingPoint.BeatLength = TimingControlPoint.DEFAULT_BEAT_LENGTH;
                }
            }

            foreach (var hitObject in HitObjects)
            {
                if (hitObject is Sliderf slider)
                {
                    if (slider.RepeatCount > 5)
                    {
                        slider.RepeatCount = 5;
                    }
                }
            }

            if (BeatmapInfo.BeatmapVersion < 8)
            {
                Breaks.Add(new BreakPeriod(10000, 5000));
            }
        }

        IBeatmap IBeatmap.Clone() => Clone();

        public Beatmap<T> Clone() => (Beatmap<T>)MemberwiseClone();

        public override string ToString() => BeatmapInfo.ToString();

        public double AudioLeadIn { get; set; }
    }

    public class Beatmap : Beatmap<HitObject>
    {
    }
}
