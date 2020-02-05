// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
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
        /// Invoked when the start time of a <see cref="HitObject"/> in this <see cref="EditorBeatmap"/> was changed.
        /// </summary>
        public event Action<HitObject> StartTimeChanged;

        public BindableList<HitObject> SelectedHitObjects { get; } = new BindableList<HitObject>();

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

        private ScheduledDelegate scheduledUpdate;

        /// <summary>
        /// Updates a <see cref="HitObject"/>, invoking <see cref="HitObject.ApplyDefaults"/> and re-processing the beatmap.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to update.</param>
        public void UpdateHitObject(HitObject hitObject)
        {
            scheduledUpdate?.Cancel();
            scheduledUpdate = Scheduler.AddDelayed(() =>
            {
                beatmapProcessor?.PreProcess();
                hitObject?.ApplyDefaults(ControlPointInfo, BeatmapInfo.BaseDifficulty);
                beatmapProcessor?.PostProcess();
            }, 0);
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
        /// Adds a <see cref="HitObject"/> to this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Add(HitObject hitObject)
        {
            trackStartTime(hitObject);

            // Preserve existing sorting order in the beatmap
            var insertionIndex = findInsertionIndex(PlayableBeatmap.HitObjects, hitObject.StartTime);
            mutableHitObjects.Insert(insertionIndex + 1, hitObject);

            HitObjectAdded?.Invoke(hitObject);
        }

        /// <summary>
        /// Removes a <see cref="HitObject"/> from this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Remove(HitObject hitObject)
        {
            if (!mutableHitObjects.Contains(hitObject))
                return;

            mutableHitObjects.Remove(hitObject);

            var bindable = startTimeBindables[hitObject];
            bindable.UnbindAll();

            startTimeBindables.Remove(hitObject);
            HitObjectRemoved?.Invoke(hitObject);
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

                StartTimeChanged?.Invoke(hitObject);
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
