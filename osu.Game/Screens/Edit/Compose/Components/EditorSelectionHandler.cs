// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Audio;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class EditorSelectionHandler : SelectionHandler<HitObject>
    {
        /// <summary>
        /// A special bank name that is only used in the editor UI.
        /// When selected and in placement mode, the bank of the last hit object will always be used.
        /// </summary>
        public const string HIT_BANK_AUTO = "auto";

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            createStateBindables();

            // bring in updates from selection changes
            EditorBeatmap.HitObjectUpdated += _ => Scheduler.AddOnce(UpdateTernaryStates);

            SelectedItems.CollectionChanged += (_, _) => Scheduler.AddOnce(UpdateTernaryStates);
        }

        protected override void DeleteItems(IEnumerable<HitObject> items) => EditorBeatmap.RemoveRange(items);

        #region Selection State

        /// <summary>
        /// The state of "new combo" for all selected hitobjects.
        /// </summary>
        public readonly Bindable<TernaryState> SelectionNewComboState = new Bindable<TernaryState>();

        /// <summary>
        /// The state of each sample type for all selected hitobjects. Keys match with <see cref="HitSampleInfo"/> constant specifications.
        /// </summary>
        public readonly Dictionary<string, Bindable<TernaryState>> SelectionSampleStates = new Dictionary<string, Bindable<TernaryState>>();

        /// <summary>
        /// The state of each sample bank type for all selected hitobjects.
        /// </summary>
        public readonly Dictionary<string, Bindable<TernaryState>> SelectionBankStates = new Dictionary<string, Bindable<TernaryState>>();

        /// <summary>
        /// Set up ternary state bindables and bind them to selection/hitobject changes (in both directions)
        /// </summary>
        private void createStateBindables()
        {
            foreach (string bankName in HitSampleInfo.AllBanks.Prepend(HIT_BANK_AUTO))
            {
                var bindable = new Bindable<TernaryState>
                {
                    Description = bankName.Titleize()
                };

                bindable.ValueChanged += state =>
                {
                    switch (state.NewValue)
                    {
                        case TernaryState.False:
                            if (SelectedItems.Count == 0)
                            {
                                // Ensure that if this is the last selected bank, it should remain selected.
                                if (SelectionBankStates.Values.All(b => b.Value == TernaryState.False))
                                    bindable.Value = TernaryState.True;
                            }
                            else
                            {
                                // Auto should never apply when there is a selection made.
                                // This is also required to stop a bindable feedback loop when a HitObject has zero samples (and LINQ `All` below becomes true).
                                if (bankName == HIT_BANK_AUTO)
                                    break;

                                // Never remove a sample bank.
                                // These are basically radio buttons, not toggles.
                                if (SelectedItems.All(h => h.Samples.All(s => s.Bank == bankName)))
                                    bindable.Value = TernaryState.True;
                            }

                            break;

                        case TernaryState.True:
                            if (SelectedItems.Count == 0)
                            {
                                // Ensure the user can't stack multiple bank selections when there's no hitobject selection.
                                // Note that in normal scenarios this is sorted out by the feedback from applying the bank to the selected objects.
                                foreach (var other in SelectionBankStates.Values)
                                {
                                    if (other != bindable)
                                        other.Value = TernaryState.False;
                                }
                            }
                            else
                            {
                                // Auto should just not apply if there's a selection already made.
                                // Maybe we could make it a disabled button in the future, but right now the editor buttons don't support disabled state.
                                if (bankName == HIT_BANK_AUTO)
                                {
                                    bindable.Value = TernaryState.False;
                                    break;
                                }

                                AddSampleBank(bankName);
                            }

                            break;
                    }
                };

                SelectionBankStates[bankName] = bindable;
            }

            // start with normal selected.
            SelectionBankStates[SampleControlPoint.DEFAULT_BANK].Value = TernaryState.True;

            foreach (string sampleName in HitSampleInfo.AllAdditions)
            {
                var bindable = new Bindable<TernaryState>
                {
                    Description = sampleName.Replace("hit", string.Empty).Titleize()
                };

                bindable.ValueChanged += state =>
                {
                    switch (state.NewValue)
                    {
                        case TernaryState.False:
                            RemoveHitSample(sampleName);
                            break;

                        case TernaryState.True:
                            AddHitSample(sampleName);
                            break;
                    }
                };

                SelectionSampleStates[sampleName] = bindable;
            }

            // new combo
            SelectionNewComboState.ValueChanged += state =>
            {
                switch (state.NewValue)
                {
                    case TernaryState.False:
                        SetNewCombo(false);
                        break;

                    case TernaryState.True:
                        SetNewCombo(true);
                        break;
                }
            };
        }

        /// <summary>
        /// Called when context menu ternary states may need to be recalculated (selection changed or hitobject updated).
        /// </summary>
        protected virtual void UpdateTernaryStates()
        {
            SelectionNewComboState.Value = GetStateFromSelection(SelectedItems.OfType<IHasComboInformation>(), h => h.NewCombo);

            foreach ((string sampleName, var bindable) in SelectionSampleStates)
            {
                bindable.Value = GetStateFromSelection(SelectedItems, h => h.Samples.Any(s => s.Name == sampleName));
            }

            foreach ((string bankName, var bindable) in SelectionBankStates)
            {
                bindable.Value = GetStateFromSelection(SelectedItems, h => h.Samples.All(s => s.Bank == bankName));
            }
        }

        #endregion

        #region Ternary state changes

        /// <summary>
        /// Adds a sample bank to all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="bankName">The name of the sample bank.</param>
        public void AddSampleBank(string bankName)
        {
            EditorBeatmap.PerformOnSelection(h =>
            {
                if (h.Samples.All(s => s.Bank == bankName))
                    return;

                h.Samples = h.Samples.Select(s => s.With(newBank: bankName)).ToList();
                EditorBeatmap.Update(h);
            });
        }

        /// <summary>
        /// Adds a hit sample to all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void AddHitSample(string sampleName)
        {
            EditorBeatmap.PerformOnSelection(h =>
            {
                // Make sure there isn't already an existing sample
                if (h.Samples.Any(s => s.Name == sampleName))
                    return;

                h.Samples.Add(h.CreateHitSampleInfo(sampleName));
                EditorBeatmap.Update(h);
            });
        }

        /// <summary>
        /// Removes a hit sample from all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void RemoveHitSample(string sampleName)
        {
            EditorBeatmap.PerformOnSelection(h =>
            {
                h.SamplesBindable.RemoveAll(s => s.Name == sampleName);
                EditorBeatmap.Update(h);
            });
        }

        /// <summary>
        /// Set the new combo state of all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="state">Whether to set or unset.</param>
        /// <exception cref="InvalidOperationException">Throws if any selected object doesn't implement <see cref="IHasComboInformation"/></exception>
        public void SetNewCombo(bool state)
        {
            EditorBeatmap.PerformOnSelection(h =>
            {
                var comboInfo = h as IHasComboInformation;
                if (comboInfo == null || comboInfo.NewCombo == state) return;

                comboInfo.NewCombo = state;
                EditorBeatmap.Update(h);
            });
        }

        #endregion

        #region Context Menu

        /// <summary>
        /// Provide context menu items relevant to current selection. Calling base is not required.
        /// </summary>
        /// <param name="selection">The current selection.</param>
        /// <returns>The relevant menu items.</returns>
        protected override IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<HitObject>> selection)
        {
            if (SelectedBlueprints.All(b => b.Item is IHasComboInformation))
            {
                yield return new TernaryStateToggleMenuItem("New combo") { State = { BindTarget = SelectionNewComboState } };
            }

            yield return new OsuMenuItem("Sample")
            {
                Items = SelectionSampleStates.Select(kvp =>
                    new TernaryStateToggleMenuItem(kvp.Value.Description) { State = { BindTarget = kvp.Value } }).ToArray()
            };

            yield return new OsuMenuItem("Bank")
            {
                Items = SelectionBankStates.Select(kvp =>
                    new TernaryStateToggleMenuItem(kvp.Value.Description) { State = { BindTarget = kvp.Value } }).ToArray()
            };
        }

        #endregion
    }
}
