// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Skinning;
using osu.Game.Utils;

namespace osu.Game.Screens.Edit
{
    public partial class EditorBeatmap : TransactionalCommitComponent, IBeatmap, IBeatSnapProvider
    {
        /// <summary>
        /// Will become <c>true</c> when a new update is queued, and <c>false</c> when all updates have been applied.
        /// </summary>
        /// <remarks>
        /// This is intended to be used to avoid performing operations (like playback of samples)
        /// while mutating hitobjects.
        /// </remarks>
        public IBindable<bool> UpdateInProgress => updateInProgress;

        private readonly BindableBool updateInProgress = new BindableBool();

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
        /// Invoked after any state changes occurred which triggered a beatmap reprocess via an <see cref="IBeatmapProcessor"/>.
        /// </summary>
        /// <remarks>
        /// Beatmap processing may change the order of hitobjects. This event gives external components a chance to handle any changes
        /// not covered by the <see cref="HitObjectAdded"/> / <see cref="HitObjectUpdated"/> / <see cref="HitObjectRemoved"/> events.
        /// </remarks>
        public event Action BeatmapReprocessed;

        /// <summary>
        /// All currently selected <see cref="HitObject"/>s.
        /// </summary>
        public readonly BindableList<HitObject> SelectedHitObjects = new BindableList<HitObject>();

        /// <summary>
        /// The current placement. Null if there's no active placement.
        /// </summary>
        public readonly Bindable<HitObject> PlacementObject = new Bindable<HitObject>();

        private readonly BeatmapInfo beatmapInfo;
        public readonly IBeatmap PlayableBeatmap;

        /// <summary>
        /// Whether at least one timing control point is present and providing timing information.
        /// </summary>
        public IBindable<bool> HasTiming => hasTiming;

        private readonly Bindable<bool> hasTiming = new Bindable<bool>();

        [CanBeNull]
        public readonly EditorBeatmapSkin BeatmapSkin;

        [Resolved]
        private BindableBeatDivisor beatDivisor { get; set; }

        [Resolved]
        private EditorClock editorClock { get; set; }

        public BindableInt PreviewTime { get; }

        private readonly IBeatmapProcessor beatmapProcessor;

        private readonly Dictionary<HitObject, Bindable<double>> startTimeBindables = new Dictionary<HitObject, Bindable<double>>();

        public EditorBeatmap(IBeatmap playableBeatmap, ISkin beatmapSkin = null, BeatmapInfo beatmapInfo = null)
        {
            PlayableBeatmap = playableBeatmap;
            PlayableBeatmap.ControlPointInfo = ConvertControlPoints(PlayableBeatmap.ControlPointInfo);

            this.beatmapInfo = beatmapInfo ?? playableBeatmap.BeatmapInfo;

            if (beatmapSkin is Skin skin)
            {
                BeatmapSkin = new EditorBeatmapSkin(skin);
                BeatmapSkin.BeatmapSkinChanged += SaveState;
            }

            beatmapProcessor = playableBeatmap.BeatmapInfo.Ruleset.CreateInstance().CreateBeatmapProcessor(PlayableBeatmap);

            foreach (var obj in HitObjects)
                trackStartTime(obj);

            PreviewTime = new BindableInt(BeatmapInfo.Metadata.PreviewTime);
            PreviewTime.BindValueChanged(s =>
            {
                BeginChange();
                BeatmapInfo.Metadata.PreviewTime = s.NewValue;
                EndChange();
            });
        }

        /// <summary>
        /// Converts a <see cref="ControlPointInfo"/> such that the resultant <see cref="ControlPointInfo"/> is non-legacy.
        /// </summary>
        /// <param name="incoming">The <see cref="ControlPointInfo"/> to convert.</param>
        /// <returns>The non-legacy <see cref="ControlPointInfo"/>. <paramref name="incoming"/> is returned if already non-legacy.</returns>
        public static ControlPointInfo ConvertControlPoints(ControlPointInfo incoming)
        {
            // ensure we are not working with legacy control points.
            // if we leave the legacy points around they will be applied over any local changes on
            // ApplyDefaults calls. this should eventually be removed once the default logic is moved to the decoder/converter.
            if (!(incoming is LegacyControlPointInfo))
                return incoming;

            var newControlPoints = new ControlPointInfo();

            foreach (var controlPoint in incoming.AllControlPoints)
            {
                switch (controlPoint)
                {
                    case DifficultyControlPoint:
                    case SampleControlPoint:
                        // skip legacy types.
                        continue;

                    default:
                        newControlPoints.Add(controlPoint.Time, controlPoint);
                        break;
                }
            }

            return newControlPoints;
        }

        public BeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set => throw new InvalidOperationException($"Can't set {nameof(BeatmapInfo)} on {nameof(EditorBeatmap)}");
        }

        public BeatmapMetadata Metadata => beatmapInfo.Metadata;

        public BeatmapDifficulty Difficulty
        {
            get => PlayableBeatmap.Difficulty;
            set => PlayableBeatmap.Difficulty = value;
        }

        public ControlPointInfo ControlPointInfo
        {
            get => PlayableBeatmap.ControlPointInfo;
            set => PlayableBeatmap.ControlPointInfo = value;
        }

        public List<BreakPeriod> Breaks => PlayableBeatmap.Breaks;

        public double TotalBreakTime => PlayableBeatmap.TotalBreakTime;

        public IReadOnlyList<HitObject> HitObjects => PlayableBeatmap.HitObjects;

        public IEnumerable<BeatmapStatistic> GetStatistics() => PlayableBeatmap.GetStatistics();

        public double GetMostCommonBeatLength() => PlayableBeatmap.GetMostCommonBeatLength();

        public IBeatmap Clone() => (EditorBeatmap)MemberwiseClone();

        private IList mutableHitObjects => (IList)PlayableBeatmap.HitObjects;

        private readonly List<HitObject> batchPendingInserts = new List<HitObject>();

        private readonly List<HitObject> batchPendingDeletes = new List<HitObject>();

        private readonly HashSet<HitObject> batchPendingUpdates = new HashSet<HitObject>();

        /// <summary>
        /// Perform the provided action on every selected hitobject.
        /// Changes will be grouped as one history action.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public void PerformOnSelection(Action<HitObject> action)
        {
            if (SelectedHitObjects.Count == 0)
                return;

            BeginChange();
            foreach (var h in SelectedHitObjects)
                action(h);
            EndChange();
        }

        /// <summary>
        /// Adds a collection of <see cref="HitObject"/>s to this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObjects">The <see cref="HitObject"/>s to add.</param>
        public void AddRange(IEnumerable<HitObject> hitObjects)
        {
            BeginChange();
            foreach (var h in hitObjects)
                Add(h);
            EndChange();
        }

        /// <summary>
        /// Adds a <see cref="HitObject"/> to this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to add.</param>
        public void Add(HitObject hitObject)
        {
            // Preserve existing sorting order in the beatmap
            int insertionIndex = findInsertionIndex(PlayableBeatmap.HitObjects, hitObject.StartTime);
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

            BeginChange();
            batchPendingInserts.Add(hitObject);
            EndChange();
        }

        /// <summary>
        /// Updates a <see cref="HitObject"/>, invoking <see cref="HitObject.ApplyDefaults"/> and re-processing the beatmap.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to update.</param>
        public void Update([NotNull] HitObject hitObject)
        {
            // updates are debounced regardless of whether a batch is active.
            batchPendingUpdates.Add(hitObject);

            updateInProgress.Value = true;
        }

        /// <summary>
        /// Update all hit objects with potentially changed difficulty or control point data.
        /// </summary>
        public void UpdateAllHitObjects()
        {
            foreach (var h in HitObjects)
                batchPendingUpdates.Add(h);

            updateInProgress.Value = true;
        }

        /// <summary>
        /// Removes a <see cref="HitObject"/> from this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> to remove.</param>
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
        /// Removes a collection of <see cref="HitObject"/>s to this <see cref="EditorBeatmap"/>.
        /// </summary>
        /// <param name="hitObjects">The <see cref="HitObject"/>s to remove.</param>
        public void RemoveRange(IEnumerable<HitObject> hitObjects)
        {
            BeginChange();
            foreach (var h in hitObjects)
                Remove(h);
            EndChange();
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
            HitObject hitObject = (HitObject)mutableHitObjects[index]!;

            mutableHitObjects.RemoveAt(index);

            var bindable = startTimeBindables[hitObject];
            bindable.UnbindAll();
            startTimeBindables.Remove(hitObject);

            BeginChange();
            batchPendingDeletes.Add(hitObject);
            EndChange();
        }

        protected override void Update()
        {
            base.Update();

            if (batchPendingUpdates.Count > 0)
                UpdateState();

            hasTiming.Value = !ReferenceEquals(ControlPointInfo.TimingPointAt(editorClock.CurrentTime), TimingControlPoint.DEFAULT);
        }

        protected override void UpdateState()
        {
            if (batchPendingUpdates.Count == 0 && batchPendingDeletes.Count == 0 && batchPendingInserts.Count == 0)
                return;

            beatmapProcessor?.PreProcess();

            foreach (var h in batchPendingDeletes) processHitObject(h);
            foreach (var h in batchPendingInserts) processHitObject(h);
            foreach (var h in batchPendingUpdates) processHitObject(h);

            beatmapProcessor?.PostProcess();

            BeatmapReprocessed?.Invoke();

            // callbacks may modify the lists so let's be safe about it
            var deletes = batchPendingDeletes.ToArray();
            batchPendingDeletes.Clear();

            var inserts = batchPendingInserts.ToArray();
            batchPendingInserts.Clear();

            var updates = batchPendingUpdates.ToArray();
            batchPendingUpdates.Clear();

            foreach (var h in deletes) SelectedHitObjects.Remove(h);

            foreach (var h in deletes) HitObjectRemoved?.Invoke(h);
            foreach (var h in inserts) HitObjectAdded?.Invoke(h);
            foreach (var h in updates) HitObjectUpdated?.Invoke(h);

            updateInProgress.Value = false;
        }

        /// <summary>
        /// Clears all <see cref="HitObjects"/> from this <see cref="EditorBeatmap"/>.
        /// </summary>
        public void Clear() => RemoveRange(HitObjects.ToArray());

        private void processHitObject(HitObject hitObject) => hitObject.ApplyDefaults(ControlPointInfo, PlayableBeatmap.Difficulty);

        private void trackStartTime(HitObject hitObject)
        {
            startTimeBindables[hitObject] = hitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindables[hitObject].ValueChanged += _ =>
            {
                // For now we'll remove and re-add the hitobject. This is not optimal and can be improved if required.
                mutableHitObjects.Remove(hitObject);

                int insertionIndex = findInsertionIndex(PlayableBeatmap.HitObjects, hitObject.StartTime);
                mutableHitObjects.Insert(insertionIndex + 1, hitObject);

                Update(hitObject);
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

        public double SnapTime(double time, double? referenceTime) => ControlPointInfo.GetClosestSnappedTime(time, BeatDivisor, referenceTime);

        public double GetBeatLengthAtTime(double referenceTime) => ControlPointInfo.TimingPointAt(referenceTime).BeatLength / BeatDivisor;

        IBeatmap IDeepCloneable<IBeatmap>.DeepClone(IDictionary<object, object> referenceLookup)
        {
            throw new NotSupportedException($"Cloning an {nameof(EditorBeatmap)} is not supported.");
        }

        public int BeatDivisor => beatDivisor?.Value ?? 1;
    }
}
