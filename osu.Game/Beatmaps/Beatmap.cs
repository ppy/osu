// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps.ControlPoints;
using Newtonsoft.Json;
using osu.Game.IO.Serialization.Converters;
using osu.Game.Utils;

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

                if (beatmapInfo != null)
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

                if (beatmapInfo?.Difficulty != null)
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

        public List<BreakPeriod> Breaks { get; set; } = new List<BreakPeriod>();

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

        IBeatmap IDeepCloneable<IBeatmap>.DeepClone(IDictionary<object, object> referenceLookup) => DeepClone(referenceLookup);

        public Beatmap<T> DeepClone(IDictionary<object, object> referenceLookup)
        {
            if (referenceLookup.TryGetValue(this, out object existing))
                return (Beatmap<T>)existing;

            var clone = (Beatmap<T>)MemberwiseClone();
            referenceLookup[this] = clone;

            clone.beatmapInfo = beatmapInfo?.DeepClone(referenceLookup);
            clone.difficulty = clone.beatmapInfo?.Difficulty;
            clone.ControlPointInfo = ControlPointInfo.DeepClone(referenceLookup);
            clone.Breaks = Breaks.Select(b => b.DeepClone(referenceLookup)).ToList();
            clone.HitObjects = HitObjects.Select(h => (T)h.DeepClone(referenceLookup)).ToList();

            return clone;
        }
    }

    public class Beatmap : Beatmap<HitObject>
    {
        public new Beatmap DeepClone(IDictionary<object, object> referencesLookup) => (Beatmap)base.DeepClone(referencesLookup);

        public override string ToString() => BeatmapInfo?.ToString() ?? base.ToString();
    }
}
