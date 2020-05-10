// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Edit
{
    public class EditorBeatmap : Component, IBeatmap, IBeatSnapProvider
    {
        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is added to this <see cref="EditorBeatmap"/>.
        /// </summary>
        public event Action<HitObject> HitObjectAdded;

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is removed from this <see cref="EditorBeatmap"/>.
        /// </summary>
        public event Action<HitObject> HitObjectRemoved;

        /// <summary>
        /// Invoked when a <see cref="HitObject"/> is updated.
        /// </summary>
        public event Action<HitObject> HitObjectUpdated;

        /// <summary>
        /// All currently selected <see cref="HitObject"/>s.
        /// </summary>
        public readonly BindableList<HitObject> SelectedHitObjects = new BindableList<HitObject>();

        /// <summary>
        /// The current placement. Null if there's no active placement.
        /// </summary>
        public readonly Bindable<HitObject> PlacementObject = new Bindable<HitObject>();

        public readonly IBeatmap PlayableBeatmap;

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        private readonly IBeatmapProcessor beatmapProcessor;

        private readonly Dictionary<HitObject, Bindable<double>> startTimeBindables = new Dictionary<HitObject, Bindable<double>>();

        public EditorBeatmap(IBeatmap playableBeatmap)
        {
            PlayableBeatmap = playableBeatmap;

            beatmapProcessor = playableBeatmap.BeatmapInfo.Ruleset?.CreateInstance().CreateBeatmapProcessor(PlayableBeatmap);

            foreach (var obj in HitObjects)
                trackStartTime(obj);
        }

        private readonly HashSet<HitObject> pendingUpdates = new HashSet<HitObject>();
        private ScheduledDelegate scheduledUpdate;

        /// <summary>
        /// Updates a <see cref="HitObject"/>, invoking <see cref="HitObject.ApplyDefaults"/> and re-processing the beatmap.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to update.</param>
        public void UpdateHitObject([NotNull] HitObject hitObject) => updateHitObject(hitObject, false);

        private void updateHitObject([CanBeNull] HitObject hitObject, bool silent)
        {
            scheduledUpdate?.Cancel();

            if (hitObject != null)
                pendingUpdates.Add(hitObject);

            scheduledUpdate = Schedule(() =>
            {
                beatmapProcessor?.PreProcess();

                foreach (var obj in pendingUpdates)
                    obj.ApplyDefaults(ControlPointInfo, BeatmapInfo.BaseDifficulty);

                beatmapProcessor?.PostProcess();

                if (!silent)
                {
                    foreach (var obj in pendingUpdates)
                        HitObjectUpdated?.Invoke(obj);
                }

                pendingUpdates.Clear();
            });
        }

        public BeatmapInfo BeatmapInfo
        {
            get => PlayableBeatmap.BeatmapInfo;
            set => PlayableBeatmap.BeatmapInfo = value;
        }

        public BeatmapMetadata Metadata => PlayableBeatmap.Metadata;

        public ControlPointInfo ControlPointInfo => PlayableBeatmap.ControlPointInfo;

        public List<BreakPeriod> Breaks => PlayableBeatmap.Breaks;

        public double TotalBreakTime => PlayableBeatmap.TotalBreakTime;

        public IReadOnlyList<HitObject> HitObjects => PlayableBeatmap.HitObjects;

        public IEnumerable<BeatmapStatistic> GetStatistics() => PlayableBeatmap.GetStatistics();

        public IBeatmap Clone() => (EditorBeatmap)MemberwiseClone();

        private IList mutableHitObjects => (IList)PlayableBeatmap.HitObjects;

        /// <summary>
        /// Adds a collection of <see cref="HitObject"/>s to this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObjects">The <see cref="HitObject"/>s to add.</param>
        public void AddRange(IEnumerable<HitObject> hitObjects)
        {
            foreach (var h in hitObjects)
                Add(h);
        }

        /// <summary>
        /// Adds a <see cref="HitObject"/> to this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Add(HitObject hitObject)
        {
            // Preserve existing sorting order in the beatmap
            var insertionIndex = findInsertionIndex(PlayableBeatmap.HitObjects, hitObject.StartTime);
            Insert(insertionIndex + 1, hitObject);
        }

        /// <summary>
        /// Inserts a <see cref="HitObject"/> into this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <remarks>
        /// It is the invoker's responsibility to make sure that <see cref="HitObject"/> sorting order is maintained.
        /// </remarks>
        /// <param name="index">The index to insert the <see cref="HitObject"/> at.</param>
        /// <param name="hitObject">The <see cref="HitObject"/> to insert.</param>
        public void Insert(int index, HitObject hitObject)
        {
            trackStartTime(hitObject);

            mutableHitObjects.Insert(index, hitObject);

            HitObjectAdded?.Invoke(hitObject);
            updateHitObject(hitObject, true);
        }

        /// <summary>
        /// Removes a <see cref="HitObject"/> from this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        /// <returns>True if the <see cref="HitObject"/> has been removed, false otherwise.</returns>
        public bool Remove(HitObject hitObject)
        {
            int index = FindIndex(hitObject);

            if (index == -1)
                return false;

            RemoveAt(index);
            return true;
        }

        /// <summary>
        /// Finds the index of a <see cref="HitObject"/> in this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to search for.</param>
        /// <returns>The index of <paramref name="hitObject"/>.</returns>
        public int FindIndex(HitObject hitObject) => mutableHitObjects.IndexOf(hitObject);

        /// <summary>
        /// Removes a <see cref="HitObject"/> at an index in this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="index">The index of the <see cref="HitObject"/> to remove.</param>
        public void RemoveAt(int index)
        {
            var hitObject = (HitObject)mutableHitObjects[index];

            mutableHitObjects.RemoveAt(index);

            var bindable = startTimeBindables[hitObject];
            bindable.UnbindAll();

            startTimeBindables.Remove(hitObject);
            HitObjectRemoved?.Invoke(hitObject);

            updateHitObject(null, true);
        }

        /// <summary>
        /// Clears all <see cref="HitObjects"/> from this <see cref="EditorBeatmap"/>.
        /// </summary>
        public void Clear()
        {
            var removed = HitObjects.ToList();

            mutableHitObjects.Clear();

            foreach (var b in startTimeBindables)
                b.Value.UnbindAll();
            startTimeBindables.Clear();

            foreach (var h in removed)
                HitObjectRemoved?.Invoke(h);

            updateHitObject(null, true);
        }

        private void trackStartTime(HitObject hitObject)
        {
            startTimeBindables[hitObject] = hitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindables[hitObject].ValueChanged += _ =>
            {
                // For now we'll remove and re-add the hitobject. This is not optimal and can be improved if required.
                mutableHitObjects.Remove(hitObject);

                var insertionIndex = findInsertionIndex(PlayableBeatmap.HitObjects, hitObject.StartTime);
                mutableHitObjects.Insert(insertionIndex + 1, hitObject);

                UpdateHitObject(hitObject);
            };
        }

        private int findInsertionIndex(IReadOnlyList<HitObject> list, double startTime)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].StartTime > startTime)
                    return i - 1;
            }

            return list.Count - 1;
        }

        public double SnapTime(double time, double? referenceTime)
        {
            var timingPoint = ControlPointInfo.TimingPointAt(referenceTime ?? time);
            var beatLength = timingPoint.BeatLength / BeatDivisor;

            return timingPoint.Time + (int)Math.Round((time - timingPoint.Time) / beatLength, MidpointRounding.AwayFromZero) * beatLength;
        }

        public double GetBeatLengthAtTime(double referenceTime) => ControlPointInfo.TimingPointAt(referenceTime).BeatLength / BeatDivisor;

        public int BeatDivisor => beatDivisor?.Value ?? 1;
    }
}
