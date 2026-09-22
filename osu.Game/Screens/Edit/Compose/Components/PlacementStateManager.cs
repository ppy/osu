// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Audio;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class PlacementStateManager : Component
    {
        public bool PerformBeatmapUpdates { get; init; }

        private readonly HitObject[] hitObjects;

        private bool automaticBankAssignment;
        private bool automaticAdditionBankAssignment;

        public PlacementStateManager(params HitObject[] hitObjects)
        {
            this.hitObjects = hitObjects;
        }

        [Resolved]
        private HitObjectComposer? composer { get; set; }

        [Resolved]
        private EditorBeatmap editorBeatmap { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            if (composer != null)
                composer.SelectionStateChanged += updatePlacementFromSelectionStateChange;

            updatePlacementFromSelectionStateChange();
            CopyStateFromPreviousObject();
        }

        private void updatePlacementFromSelectionStateChange()
        {
            updatePlacementNewCombo();
            updatePlacementSamples();
        }

        private void updatePlacementNewCombo()
        {
            if (composer == null)
                return;

            IHasComboInformation? comboStarter = null;

            for (int i = 0; i < hitObjects.Length; ++i)
            {
                var hitObject = hitObjects[i];

                if (hitObject is not IHasComboInformation withCombo)
                    continue;

                if (comboStarter == null)
                {
                    withCombo.NewCombo = composer.SelectionNewComboState?.Value == TernaryState.True;
                    comboStarter = withCombo;
                }
                else
                    withCombo.NewCombo = false;

                if (PerformBeatmapUpdates)
                    editorBeatmap.Update(hitObject);
            }
        }

        private void updatePlacementSamples()
        {
            if (composer == null)
                return;

            foreach (var hitObject in hitObjects)
            {
                foreach (var kvp in composer.SelectionSampleStates)
                    sampleChanged(hitObject, kvp.Key, kvp.Value.Value);

                foreach (var kvp in composer.SelectionBankStates)
                    bankChanged(hitObject, kvp.Key, kvp.Value.Value);

                foreach (var kvp in composer.SelectionAdditionBankStates)
                    additionBankChanged(hitObject, kvp.Key, kvp.Value.Value);
            }
        }

        private void sampleChanged(HitObject hitObject, string sampleName, TernaryState state)
        {
            var samples = hitObject.Samples;

            var existingSample = samples.FirstOrDefault(s => s.Name == sampleName);

            switch (state)
            {
                case TernaryState.False:
                    if (existingSample != null)
                        samples.Remove(existingSample);
                    break;

                case TernaryState.True:
                    if (existingSample == null)
                        samples.Add(hitObject.CreateHitSampleInfo(sampleName));
                    break;
            }

            if (PerformBeatmapUpdates)
                editorBeatmap.Update(hitObject);
        }

        private void bankChanged(HitObject hitObject, string bankName, TernaryState state)
        {
            if (bankName == HitObjectComposer.HIT_BANK_AUTO)
            {
                automaticBankAssignment = state == TernaryState.True;
                if (automaticBankAssignment)
                    CopyStateFromPreviousObject();
            }
            else if (state == TernaryState.True)
            {
                hitObject.Samples = hitObject.Samples.Select(s => s.Name == HitSampleInfo.HIT_NORMAL ? s.With(newBank: bankName, newEditorAutoBank: false) : s).ToList();

                if (PerformBeatmapUpdates)
                    editorBeatmap.Update(hitObject);
            }
        }

        private void additionBankChanged(HitObject hitObject, string bankName, TernaryState state)
        {
            if (bankName == HitObjectComposer.HIT_BANK_AUTO)
            {
                automaticAdditionBankAssignment = state == TernaryState.True;
                if (automaticAdditionBankAssignment)
                    CopyStateFromPreviousObject();
            }
            else if (state == TernaryState.True)
            {
                hitObject.Samples = hitObject.Samples.Select(s => s.Name != HitSampleInfo.HIT_NORMAL ? s.With(newBank: bankName, newEditorAutoBank: false) : s).ToList();

                if (PerformBeatmapUpdates)
                    editorBeatmap.Update(hitObject);
            }
        }

        private HitObject? getPreviousHitObject()
        {
            if (hitObjects.Length == 0)
                return null;

            return editorBeatmap.HitObjects.TakeWhile(h => h.StartTime < hitObjects.First().GetEndTime()).LastOrDefault();
        }

        /// <summary>
        /// Updates the state of combo and sample information to inherit the required pieces from the previous object.
        /// Should be manually called by the consumer whenever a placement-in-progress changes its temporal placement
        /// (as it can change which previous object the information should be inherited from).
        /// </summary>
        public void CopyStateFromPreviousObject()
        {
            var lastHitObject = getPreviousHitObject();
            var lastHitNormal = lastHitObject?.Samples?.FirstOrDefault(o => o.Name == HitSampleInfo.HIT_NORMAL);

            foreach (var hitObject in hitObjects)
            {
                if (hitObject is IHasComboInformation comboInformation)
                    comboInformation.UpdateComboInformation(lastHitObject as IHasComboInformation);

                if (automaticBankAssignment && lastHitNormal != null)
                    // Inherit the bank from the previous hit object
                    hitObject.Samples = hitObject.Samples.Select(s => s.Name == HitSampleInfo.HIT_NORMAL ? s.With(newBank: lastHitNormal.Bank, newEditorAutoBank: true) : s).ToList();
                else
                    // There is no previous object to derive from, so ensure that `EditorAutoBank` flag is definitively turned off for the normal samples
                    hitObject.Samples = hitObject.Samples.Select(s => s.Name == HitSampleInfo.HIT_NORMAL ? s.With(newEditorAutoBank: false) : s).ToList();

                if (lastHitNormal != null)
                {
                    // Inherit the volume and sample set info from the previous hit object
                    hitObject.Samples = hitObject.Samples.Select(s => s.With(
                        newVolume: lastHitNormal.Volume,
                        newSuffix: lastHitNormal.Suffix,
                        newUseBeatmapSamples: lastHitNormal.UseBeatmapSamples)).ToList();
                }

                if (automaticAdditionBankAssignment)
                {
                    string bank = hitObject.Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)?.Bank ?? HitSampleInfo.BANK_SOFT;
                    hitObject.Samples = hitObject.Samples.Select(s => s.Name != HitSampleInfo.HIT_NORMAL ? s.With(newBank: bank, newEditorAutoBank: true) : s).ToList();
                }

                if (hitObject is IHasRepeats hasRepeats)
                {
                    // Make sure all the node samples are identical to the hit object's samples
                    for (int i = 0; i < hasRepeats.NodeSamples.Count; i++)
                        hasRepeats.NodeSamples[i] = hitObject.Samples.Select(o => o.With()).ToList();
                }

                if (PerformBeatmapUpdates)
                    editorBeatmap.Update(hitObject);

                lastHitObject = hitObject;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (composer != null)
                composer.SelectionStateChanged -= updatePlacementFromSelectionStateChange;

            base.Dispose(isDisposing);
        }
    }
}
