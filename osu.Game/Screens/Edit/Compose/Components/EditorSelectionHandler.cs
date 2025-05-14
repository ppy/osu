// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class EditorSelectionHandler : SelectionHandler<HitObject>
    {
        /// <summary>
        /// Whether right click should delete even when shift is not held.
        /// </summary>
        public bool RightClickAlwaysQuickDeletes { get; set; }

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

            SelectedItems.CollectionChanged += onSelectedItemsChanged;
        }

        protected override bool ShouldQuickDelete(MouseButtonEvent e)
        {
            if (RightClickAlwaysQuickDeletes && e.Button == MouseButton.Right)
                return true;

            return base.ShouldQuickDelete(e);
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
        /// The state of each sample addition bank type for all selected hitobjects.
        /// </summary>
        public readonly Dictionary<string, Bindable<TernaryState>> SelectionAdditionBankStates = new Dictionary<string, Bindable<TernaryState>>();

        /// <summary>
        /// Whether there is no selection and the auto <see cref="SelectionBankStates"/> can be used.
        /// </summary>
        public readonly Bindable<bool> AutoSelectionBankEnabled = new Bindable<bool>();

        /// <summary>
        /// Whether the selection contains any addition samples and the <see cref="SelectionAdditionBankStates"/> can be used.
        /// </summary>
        public readonly Bindable<bool> SelectionAdditionBanksEnabled = new Bindable<bool>();

        /// <summary>
        /// Set up ternary state bindables and bind them to selection/hitobject changes (in both directions)
        /// </summary>
        private void createStateBindables()
        {
            foreach (string bankName in HitSampleInfo.ALL_BANKS.Prepend(HIT_BANK_AUTO))
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
                                if (SelectedItems.All(h => h.Samples.Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(s => s.Bank == bankName)))
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

                                SetSampleBank(bankName);
                            }

                            break;
                    }
                };

                SelectionBankStates[bankName] = bindable;
            }

            foreach (string bankName in HitSampleInfo.ALL_BANKS.Prepend(HIT_BANK_AUTO))
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
                                if (SelectionAdditionBankStates.Values.All(b => b.Value == TernaryState.False))
                                    bindable.Value = TernaryState.True;
                            }
                            else
                            {
                                // Completely empty selections should be allowed in the case that none of the selected objects have any addition samples.
                                // This is also required to stop a bindable feedback loop when a HitObject has zero addition samples (and LINQ `All` below becomes true).
                                if (SelectedItems.SelectMany(enumerateAllSamples).All(h => h.All(o => o.Name == HitSampleInfo.HIT_NORMAL)))
                                    break;

                                // Never remove a sample bank.
                                // These are basically radio buttons, not toggles.
                                if (bankName == HIT_BANK_AUTO)
                                {
                                    if (SelectedItems.SelectMany(enumerateAllSamples).All(h => h.Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(s => s.EditorAutoBank)))
                                        bindable.Value = TernaryState.True;
                                }
                                else
                                {
                                    if (SelectedItems.SelectMany(enumerateAllSamples).All(h => h.Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(s => s.Bank == bankName && !s.EditorAutoBank)))
                                        bindable.Value = TernaryState.True;
                                }
                            }

                            break;

                        case TernaryState.True:
                            if (SelectedItems.Count == 0)
                            {
                                // Ensure the user can't stack multiple bank selections when there's no hitobject selection.
                                // Note that in normal scenarios this is sorted out by the feedback from applying the bank to the selected objects.
                                foreach (var other in SelectionAdditionBankStates.Values)
                                {
                                    if (other != bindable)
                                        other.Value = TernaryState.False;
                                }
                            }
                            else
                            {
                                // If none of the selected objects have any addition samples, we should not apply the addition bank.
                                if (SelectedItems.SelectMany(enumerateAllSamples).All(h => h.All(o => o.Name == HitSampleInfo.HIT_NORMAL)))
                                {
                                    bindable.Value = TernaryState.False;
                                    break;
                                }

                                SetSampleAdditionBank(bankName);
                            }

                            break;
                    }
                };

                SelectionAdditionBankStates[bankName] = bindable;
            }

            resetTernaryStates();

            foreach (string sampleName in HitSampleInfo.ALL_ADDITIONS)
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

        private void resetTernaryStates()
        {
            if (SelectedItems.Count > 0)
                return;

            SelectionNewComboState.Value = TernaryState.False;
            AutoSelectionBankEnabled.Value = true;
            SelectionAdditionBanksEnabled.Value = true;
            SelectionBankStates[HIT_BANK_AUTO].Value = TernaryState.True;
            SelectionAdditionBankStates[HIT_BANK_AUTO].Value = TernaryState.True;
        }

        /// <summary>
        /// Called when context menu ternary states may need to be recalculated (selection changed or hitobject updated).
        /// </summary>
        protected virtual void UpdateTernaryStates()
        {
            if (SelectedItems.Any())
                SelectionNewComboState.Value = GetStateFromSelection(SelectedItems.OfType<IHasComboInformation>(), h => h.NewCombo);
            AutoSelectionBankEnabled.Value = SelectedItems.Count == 0;

            var samplesInSelection = SelectedItems.SelectMany(enumerateAllSamples).ToArray();

            foreach ((string sampleName, var bindable) in SelectionSampleStates)
            {
                bindable.Value = GetStateFromSelection(samplesInSelection, h => h.Any(s => s.Name == sampleName));
            }

            foreach ((string bankName, var bindable) in SelectionBankStates)
            {
                bindable.Value = GetStateFromSelection(samplesInSelection.SelectMany(s => s).Where(o => o.Name == HitSampleInfo.HIT_NORMAL), h => h.Bank == bankName);
            }

            SelectionAdditionBanksEnabled.Value = samplesInSelection.SelectMany(s => s).Any(o => o.Name != HitSampleInfo.HIT_NORMAL);

            foreach ((string bankName, var bindable) in SelectionAdditionBankStates)
            {
                bindable.Value = GetStateFromSelection(samplesInSelection.SelectMany(s => s).Where(o => o.Name != HitSampleInfo.HIT_NORMAL),
                    h => (bankName != HIT_BANK_AUTO && h.Bank == bankName && !h.EditorAutoBank) || (bankName == HIT_BANK_AUTO && h.EditorAutoBank));
            }
        }

        private void onSelectedItemsChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // Reset the ternary states when the selection is cleared.
            if (e.OldStartingIndex >= 0 && e.NewStartingIndex < 0)
                Scheduler.AddOnce(resetTernaryStates);
            else
                Scheduler.AddOnce(UpdateTernaryStates);
        }

        private IEnumerable<IList<HitSampleInfo>> enumerateAllSamples(HitObject hitObject)
        {
            yield return hitObject.Samples;

            if (hitObject is IHasRepeats withRepeats)
            {
                foreach (var node in withRepeats.NodeSamples)
                    yield return node;
            }
        }

        #endregion

        #region Ternary state changes

        /// <summary>
        /// Sets the sample bank for all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="bankName">The name of the sample bank.</param>
        public void SetSampleBank(string bankName)
        {
            bool hasRelevantBank(HitObject hitObject)
            {
                bool result = hitObject.Samples.Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(s => s.Bank == bankName);

                if (hitObject is IHasRepeats hasRepeats)
                {
                    foreach (var node in hasRepeats.NodeSamples)
                        result &= node.Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(s => s.Bank == bankName);
                }

                return result;
            }

            if (SelectedItems.All(hasRelevantBank))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (hasRelevantBank(h))
                    return;

                h.Samples = h.Samples.Select(s => s.Name == HitSampleInfo.HIT_NORMAL ? s.With(newBank: bankName) : s).ToList();

                if (h is IHasRepeats hasRepeats)
                {
                    for (int i = 0; i < hasRepeats.NodeSamples.Count; ++i)
                        hasRepeats.NodeSamples[i] = hasRepeats.NodeSamples[i].Select(s => s.Name == HitSampleInfo.HIT_NORMAL ? s.With(newBank: bankName) : s).ToList();
                }
            });
        }

        /// <summary>
        /// Sets the sample addition bank for all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="bankName">The name of the sample bank.</param>
        public void SetSampleAdditionBank(string bankName)
        {
            bool hasRelevantBank(HitObject hitObject) =>
                bankName == HIT_BANK_AUTO
                    ? enumerateAllSamples(hitObject).SelectMany(o => o).Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(s => s.EditorAutoBank)
                    : enumerateAllSamples(hitObject).SelectMany(o => o).Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(s => s.Bank == bankName && !s.EditorAutoBank);

            if (SelectedItems.All(hasRelevantBank))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (hasRelevantBank(h))
                    return;

                string normalBank = h.Samples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)?.Bank ?? HitSampleInfo.BANK_SOFT;
                h.Samples = h.Samples.Select(s =>
                                 s.Name != HitSampleInfo.HIT_NORMAL
                                     ? bankName == HIT_BANK_AUTO ? s.With(newBank: normalBank, newEditorAutoBank: true) : s.With(newBank: bankName, newEditorAutoBank: false)
                                     : s)
                             .ToList();

                if (h is IHasRepeats hasRepeats)
                {
                    for (int i = 0; i < hasRepeats.NodeSamples.Count; ++i)
                    {
                        normalBank = hasRepeats.NodeSamples[i].FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)?.Bank ?? HitSampleInfo.BANK_SOFT;
                        hasRepeats.NodeSamples[i] = hasRepeats.NodeSamples[i].Select(s =>
                            s.Name != HitSampleInfo.HIT_NORMAL
                                ? bankName == HIT_BANK_AUTO ? s.With(newBank: normalBank, newEditorAutoBank: true) : s.With(newBank: bankName, newEditorAutoBank: false)
                                : s).ToList();
                    }
                }
            });
        }

        private bool hasRelevantSample(HitObject hitObject, string sampleName)
        {
            bool result = hitObject.Samples.Any(s => s.Name == sampleName);

            if (hitObject is IHasRepeats hasRepeats)
            {
                foreach (var node in hasRepeats.NodeSamples)
                    result &= node.Any(s => s.Name == sampleName);
            }

            return result;
        }

        /// <summary>
        /// Adds a hit sample to all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void AddHitSample(string sampleName)
        {
            if (SelectedItems.All(h => hasRelevantSample(h, sampleName)))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                // Make sure there isn't already an existing sample
                if (h.Samples.All(s => s.Name != sampleName))
                    h.Samples.Add(h.CreateHitSampleInfo(sampleName));

                if (h is IHasRepeats hasRepeats)
                {
                    foreach (var node in hasRepeats.NodeSamples)
                    {
                        if (node.Any(s => s.Name == sampleName))
                            continue;

                        var hitSample = h.CreateHitSampleInfo(sampleName);

                        HitSampleInfo? existingAddition = node.FirstOrDefault(s => s.Name != HitSampleInfo.HIT_NORMAL);
                        if (existingAddition != null)
                            hitSample = hitSample.With(newBank: existingAddition.Bank, newEditorAutoBank: existingAddition.EditorAutoBank);

                        node.Add(hitSample);
                    }
                }
            });
        }

        /// <summary>
        /// Removes a hit sample from all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void RemoveHitSample(string sampleName)
        {
            if (SelectedItems.All(h => !hasRelevantSample(h, sampleName)))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                h.SamplesBindable.RemoveAll(s => s.Name == sampleName);

                if (h is IHasRepeats hasRepeats)
                {
                    for (int i = 0; i < hasRepeats.NodeSamples.Count; ++i)
                        hasRepeats.NodeSamples[i] = hasRepeats.NodeSamples[i].Where(s => s.Name != sampleName).ToList();
                }
            });
        }

        /// <summary>
        /// Set the new combo state of all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="state">Whether to set or unset.</param>
        /// <exception cref="InvalidOperationException">Throws if any selected object doesn't implement <see cref="IHasComboInformation"/></exception>
        public void SetNewCombo(bool state)
        {
            if (SelectedItems.OfType<IHasComboInformation>().All(h => h.NewCombo == state))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                var comboInfo = h as IHasComboInformation;

                if (comboInfo == null || comboInfo.NewCombo == state) return;

                comboInfo.NewCombo = state;
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
                yield return new TernaryStateToggleMenuItem("New combo")
                {
                    State = { BindTarget = SelectionNewComboState },
                    Hotkey = new Hotkey(new KeyCombination(InputKey.Q))
                };
            }

            yield return new OsuMenuItem("Sample") { Items = getSampleSubmenuItems().ToArray(), };
            yield return new OsuMenuItem("Bank") { Items = getBankSubmenuItems().ToArray(), };
        }

        private IEnumerable<MenuItem> getSampleSubmenuItems()
        {
            var whistle = SelectionSampleStates[HitSampleInfo.HIT_WHISTLE];
            yield return new TernaryStateToggleMenuItem(whistle.Description)
            {
                State = { BindTarget = whistle },
                Hotkey = new Hotkey(new KeyCombination(InputKey.W))
            };

            var finish = SelectionSampleStates[HitSampleInfo.HIT_FINISH];
            yield return new TernaryStateToggleMenuItem(finish.Description)
            {
                State = { BindTarget = finish },
                Hotkey = new Hotkey(new KeyCombination(InputKey.E))
            };

            var clap = SelectionSampleStates[HitSampleInfo.HIT_CLAP];
            yield return new TernaryStateToggleMenuItem(clap.Description)
            {
                State = { BindTarget = clap },
                Hotkey = new Hotkey(new KeyCombination(InputKey.R))
            };
        }

        private IEnumerable<MenuItem> getBankSubmenuItems()
        {
            var auto = SelectionBankStates[HIT_BANK_AUTO];
            yield return new TernaryStateToggleMenuItem(auto.Description)
            {
                State = { BindTarget = auto },
                Hotkey = new Hotkey(new KeyCombination(InputKey.Shift, InputKey.Q))
            };

            var normal = SelectionBankStates[HitSampleInfo.BANK_NORMAL];
            yield return new TernaryStateToggleMenuItem(normal.Description)
            {
                State = { BindTarget = normal },
                Hotkey = new Hotkey(new KeyCombination(InputKey.Shift, InputKey.W))
            };

            var soft = SelectionBankStates[HitSampleInfo.BANK_SOFT];
            yield return new TernaryStateToggleMenuItem(soft.Description)
            {
                State = { BindTarget = soft },
                Hotkey = new Hotkey(new KeyCombination(InputKey.Shift, InputKey.E))
            };

            var drum = SelectionBankStates[HitSampleInfo.BANK_DRUM];
            yield return new TernaryStateToggleMenuItem(drum.Description)
            {
                State = { BindTarget = drum },
                Hotkey = new Hotkey(new KeyCombination(InputKey.Shift, InputKey.R))
            };

            yield return new OsuMenuItem("Addition bank")
            {
                Items = SelectionAdditionBankStates.Select(kvp =>
                    new TernaryStateToggleMenuItem(kvp.Value.Description) { State = { BindTarget = kvp.Value } }).ToArray()
            };
        }

        #endregion
    }
}
