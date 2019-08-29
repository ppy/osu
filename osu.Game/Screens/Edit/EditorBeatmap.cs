// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    public class EditorBeatmap<T> : IEditorBeatmap<T>
        where T : HitObject
    {
        public event Action<HitObject> HitObjectAdded;
        public event Action<HitObject> HitObjectRemoved;

        private readonly Beatmap<T> beatmap;

        public EditorBeatmap(Beatmap<T> beatmap)
        {
            this.beatmap = beatmap;
        }

        public BeatmapInfo BeatmapInfo
        {
            get => beatmap.BeatmapInfo;
            set => beatmap.BeatmapInfo = value;
        }

        public BeatmapMetadata Metadata => beatmap.Metadata;

        public ControlPointInfo ControlPointInfo => beatmap.ControlPointInfo;

        public List<BreakPeriod> Breaks => beatmap.Breaks;

        public double TotalBreakTime => beatmap.TotalBreakTime;

        IReadOnlyList<T> IBeatmap<T>.HitObjects => beatmap.HitObjects;

        IReadOnlyList<HitObject> IBeatmap.HitObjects => beatmap.HitObjects;

        public IEnumerable<BeatmapStatistic> GetStatistics() => beatmap.GetStatistics();

        public IBeatmap Clone() => (EditorBeatmap<T>)MemberwiseClone();

        /// <summary>
        /// Adds a <see cref="HitObject"/> to this <see cref="EditorBeatmap{T}"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Add(T hitObject)
        {
            // Preserve existing sorting order in the beatmap
            var insertionIndex = beatmap.HitObjects.FindLastIndex(h => h.StartTime <= hitObject.StartTime);
            beatmap.HitObjects.Insert(insertionIndex + 1, hitObject);

            HitObjectAdded?.Invoke(hitObject);
        }

        /// <summary>
        /// Removes a <see cref="HitObject"/> from this <see cref="EditorBeatmap{T}"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Remove(T hitObject)
        {
            if (beatmap.HitObjects.Remove(hitObject))
                HitObjectRemoved?.Invoke(hitObject);
        }

        /// <summary>
        /// Adds a <see cref="HitObject"/> to this <see cref="EditorBeatmap{T}"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Add(HitObject hitObject) => Add((T)hitObject);

        /// <summary>
        /// Removes a <see cref="HitObject"/> from this <see cref="EditorBeatmap{T}"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Remove(HitObject hitObject) => Remove((T)hitObject);
    }
}
