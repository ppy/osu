// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A blueprint which governs the creation of a new <see cref="HitObject"/> to actualisation.
    /// </summary>
    public abstract partial class HitObjectPlacementBlueprint : PlacementBlueprint
    {
        /// <summary>
        /// Whether the sample bank should be taken from the previous hit object.
        /// </summary>
        public bool AutomaticBankAssignment { get; set; }

        /// <summary>
        /// Whether the sample addition bank should be taken from the previous hit objects.
        /// </summary>
        public bool AutomaticAdditionBankAssignment { get; set; }

        /// <summary>
        /// The <see cref="HitObject"/> that is being placed.
        /// </summary>
        public readonly HitObject HitObject;

        [Resolved]
        protected EditorClock EditorClock { get; private set; } = null!;

        [Resolved]
        private EditorBeatmap beatmap { get; set; } = null!;

        private Bindable<double> startTimeBindable = null!;

        private HitObject? getPreviousHitObject() => beatmap.HitObjects.TakeWhile(h => h.StartTime <= startTimeBindable.Value).LastOrDefault();

        [Resolved]
        private IPlacementHandler placementHandler { get; set; } = null!;

        protected HitObjectPlacementBlueprint(HitObject hitObject)
        {
            HitObject = hitObject;

            // adding the default hit sample should be the case regardless of the ruleset.
            HitObject.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_NORMAL));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            startTimeBindable = HitObject.StartTimeBindable.GetBoundCopy();
            startTimeBindable.BindValueChanged(_ => ApplyDefaultsToHitObject(), true);
        }

        protected override void BeginPlacement(bool commitStart = false)
        {
            base.BeginPlacement(commitStart);

            placementHandler.BeginPlacement(HitObject);
        }

        public override void EndPlacement(bool commit)
        {
            base.EndPlacement(commit);

            placementHandler.EndPlacement(HitObject, IsValidForPlacement && commit);
        }

        /// <summary>
        /// Updates the time and position of this <see cref="PlacementBlueprint"/> based on the provided snap information.
        /// </summary>
        /// <param name="result">The snap result information.</param>
        public override void UpdateTimeAndPosition(SnapResult result)
        {
            if (PlacementActive == PlacementState.Waiting)
            {
                HitObject.StartTime = result.Time ?? EditorClock.CurrentTime;

                if (HitObject is IHasComboInformation comboInformation)
                    comboInformation.UpdateComboInformation(getPreviousHitObject() as IHasComboInformation);
            }

            var lastHitObject = getPreviousHitObject();
            var lastHitNormal = lastHitObject?.Samples?.FirstOrDefault(o => o.Name == HitSampleInfo.HIT_NORMAL);

            if (AutomaticAdditionBankAssignment)
            {
                // Inherit the addition bank from the previous hit object
                // If there is no previous addition, inherit from the normal sample
                var lastAddition = lastHitObject?.Samples?.FirstOrDefault(o => o.Name != HitSampleInfo.HIT_NORMAL) ?? lastHitNormal;

                if (lastAddition != null)
                    HitObject.Samples = HitObject.Samples.Select(s => s.Name != HitSampleInfo.HIT_NORMAL ? s.With(newBank: lastAddition.Bank) : s).ToList();
            }

            if (lastHitNormal != null)
            {
                if (AutomaticBankAssignment)
                    // Inherit the bank from the previous hit object
                    HitObject.Samples = HitObject.Samples.Select(s => s.Name == HitSampleInfo.HIT_NORMAL ? s.With(newBank: lastHitNormal.Bank) : s).ToList();

                // Inherit the volume from the previous hit object
                HitObject.Samples = HitObject.Samples.Select(s => s.With(newVolume: lastHitNormal.Volume)).ToList();
            }

            if (HitObject is IHasRepeats hasRepeats)
            {
                // Make sure all the node samples are identical to the hit object's samples
                for (int i = 0; i < hasRepeats.NodeSamples.Count; i++)
                    hasRepeats.NodeSamples[i] = HitObject.Samples.Select(o => o.With()).ToList();
            }
        }

        /// <summary>
        /// Invokes <see cref="Objects.HitObject.ApplyDefaults(ControlPointInfo,IBeatmapDifficultyInfo,CancellationToken)"/>,
        /// refreshing <see cref="Objects.HitObject.NestedHitObjects"/> and parameters for the <see cref="HitObject"/>.
        /// </summary>
        protected void ApplyDefaultsToHitObject() => HitObject.ApplyDefaults(beatmap.ControlPointInfo, beatmap.Difficulty);
    }
}
