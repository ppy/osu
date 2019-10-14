// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    public class EditorBeatmap<T> : IEditorBeatmap<T>
        where T : HitObject
    {
        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is added to this <see cref="EditorBeatmap{T}"/>.
        /// </summary>
        public event Action<HitObject> HitObjectAdded;

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is removed from this <see cref="EditorBeatmap{T}"/>.
        /// </summary>
        public event Action<HitObject> HitObjectRemoved;

        /// <summary>
        /// Invoked when the start time of a <see cref="HitObject"/> in this <see cref="EditorBeatmap{T}"/> was changed.
        /// </summary>
        public event Action<HitObject> StartTimeChanged;

        private readonly Dictionary<T, Bindable<double>> startTimeBindables = new Dictionary<T, Bindable<double>>();
        private readonly Beatmap<T> beatmap;

        public EditorBeatmap(Beatmap<T> beatmap)
        {
            this.beatmap = beatmap;

            foreach (var obj in HitObjects)
                trackStartTime(obj);
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

        public IReadOnlyList<T> HitObjects => beatmap.HitObjects;

        IReadOnlyList<HitObject> IBeatmap.HitObjects => beatmap.HitObjects;

        public IEnumerable<BeatmapStatistic> GetStatistics() => beatmap.GetStatistics();

        public IBeatmap Clone() => (EditorBeatmap<T>)MemberwiseClone();

        /// <summary>
        /// Adds a <see cref="HitObject"/> to this <see cref="EditorBeatmap{T}"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Add(T hitObject)
        {
            trackStartTime(hitObject);

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
            {
                var bindable = startTimeBindables[hitObject];
                bindable.UnbindAll();

                startTimeBindables.Remove(hitObject);
                HitObjectRemoved?.Invoke(hitObject);
            }
        }

        private void trackStartTime(T hitObject)
        {
            startTimeBindables[hitObject] = hitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindables[hitObject].ValueChanged += _ =>
            {
                // For now we'll remove and re-add the hitobject. This is not optimal and can be improved if required.
                beatmap.HitObjects.Remove(hitObject);

                var insertionIndex = beatmap.HitObjects.FindLastIndex(h => h.StartTime <= hitObject.StartTime);
                beatmap.HitObjects.Insert(insertionIndex + 1, hitObject);

                StartTimeChanged?.Invoke(hitObject);
            };
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
