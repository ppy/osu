// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    public abstract partial class HitObjectComposer<TObject, TAction>
        where TObject : HitObject
        where TAction : struct, Enum
    {
        #region Selection State

        public override IReadOnlyDictionary<string, Bindable<TernaryState>> SelectionSampleStates => selectionSampleStates;
        private readonly Dictionary<string, Bindable<TernaryState>> selectionSampleStates = new Dictionary<string, Bindable<TernaryState>>();

        public override IReadOnlyDictionary<string, Bindable<TernaryState>> SelectionBankStates => selectionBankStates;
        private readonly Dictionary<string, Bindable<TernaryState>> selectionBankStates = new Dictionary<string, Bindable<TernaryState>>();

        public override IReadOnlyDictionary<string, Bindable<TernaryState>> SelectionAdditionBankStates => selectionAdditionBankStates;
        private readonly Dictionary<string, Bindable<TernaryState>> selectionAdditionBankStates = new Dictionary<string, Bindable<TernaryState>>();

        public override Bindable<bool> AutoSelectionBankEnabled { get; } = new Bindable<bool>();

        /// <summary>
        /// Set up ternary state bindables and bind them to selection/hitobject changes (in both directions)
        /// </summary>
        private void createSelectionStateBindables()
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
                            if (EditorBeatmap.SelectedHitObjects.Count == 0)
                            {
                                // Ensure that if this is the last selected bank, it should remain selected.
                                if (selectionBankStates.Values.All(b => b.Value == TernaryState.False))
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
                                if (EditorBeatmap.SelectedHitObjects.All(h => h.Samples.Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(s => s.Bank == bankName)))
                                    bindable.Value = TernaryState.True;
                            }

                            break;

                        case TernaryState.True:
                            if (EditorBeatmap.SelectedHitObjects.Count == 0)
                            {
                                // Ensure the user can't stack multiple bank selections when there's no hitobject selection.
                                // Note that in normal scenarios this is sorted out by the feedback from applying the bank to the selected objects.
                                foreach (var other in selectionBankStates.Values)
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

                    InvokeSelectionStateChanged();
                };

                selectionBankStates[bankName] = bindable;
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
                            if (EditorBeatmap.SelectedHitObjects.Count == 0)
                            {
                                // Ensure that if this is the last selected bank, it should remain selected.
                                if (selectionAdditionBankStates.Values.All(b => b.Value == TernaryState.False))
                                    bindable.Value = TernaryState.True;
                            }
                            else
                            {
                                // Completely empty selections should be allowed in the case that none of the selected objects have any addition samples.
                                // This is also required to stop a bindable feedback loop when a HitObject has zero addition samples (and LINQ `All` below becomes true).
                                if (EditorBeatmap.SelectedHitObjects.SelectMany(enumerateAllSamples).All(h => h.All(o => o.Name == HitSampleInfo.HIT_NORMAL)))
                                    break;

                                // Never remove a sample bank.
                                // These are basically radio buttons, not toggles.
                                if (bankName == HIT_BANK_AUTO)
                                {
                                    if (EditorBeatmap.SelectedHitObjects.SelectMany(enumerateAllSamples).All(h => h.Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(s => s.EditorAutoBank)))
                                        bindable.Value = TernaryState.True;
                                }
                                else
                                {
                                    if (EditorBeatmap.SelectedHitObjects.SelectMany(enumerateAllSamples)
                                                     .All(h => h.Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(s => s.Bank == bankName && !s.EditorAutoBank)))
                                        bindable.Value = TernaryState.True;
                                }
                            }

                            break;

                        case TernaryState.True:
                            // If any of the selected objects have any addition samples, we should apply the addition bank.
                            if (EditorBeatmap.SelectedHitObjects.SelectMany(enumerateAllSamples).Any(h => h.Any(o => o.Name != HitSampleInfo.HIT_NORMAL)))
                                SetSampleAdditionBank(bankName);

                            // There are either no selected items, or none of the selected items have addition sounds.
                            // This state is basically the user pre-selecting an addition bank before actually adding an addition.
                            // Ensure the user can't stack multiple bank selections in this state.
                            // Note that in normal scenarios this is sorted out by the feedback from applying the bank to the selected objects.
                            foreach (var other in selectionAdditionBankStates.Values)
                            {
                                if (other != bindable)
                                    other.Value = TernaryState.False;
                            }

                            break;
                    }

                    InvokeSelectionStateChanged();
                };

                selectionAdditionBankStates[bankName] = bindable;
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

                    InvokeSelectionStateChanged();
                };

                selectionSampleStates[sampleName] = bindable;
            }

            // new combo
            if (SelectionNewComboState != null)
            {
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

                    InvokeSelectionStateChanged();
                };
            }
        }

        private void resetTernaryStates()
        {
            if (EditorBeatmap.SelectedHitObjects.Count > 0)
                return;

            if (SelectionNewComboState != null)
                SelectionNewComboState.Value = TernaryState.False;
            AutoSelectionBankEnabled.Value = true;
            selectionBankStates[HIT_BANK_AUTO].Value = TernaryState.True;
            selectionAdditionBankStates[HIT_BANK_AUTO].Value = TernaryState.True;
            foreach (var (_, sampleState) in selectionSampleStates)
                sampleState.Value = TernaryState.False;
        }

        /// <summary>
        /// Called when context menu ternary states may need to be recalculated (selection changed or hitobject updated).
        /// </summary>
        protected virtual void UpdateTernaryStates()
        {
            if (EditorBeatmap.SelectedHitObjects.Any() && SelectionNewComboState != null)
                SelectionNewComboState.Value = EditorBeatmap.SelectedHitObjects.OfType<IHasComboInformation>().GetTernaryState(h => h.NewCombo);
            AutoSelectionBankEnabled.Value = EditorBeatmap.SelectedHitObjects.Count == 0;

            var samplesInSelection = EditorBeatmap.SelectedHitObjects.SelectMany(enumerateAllSamples).ToArray();

            if (samplesInSelection.Length > 0)
            {
                foreach ((string sampleName, var bindable) in selectionSampleStates)
                {
                    bindable.Value = samplesInSelection.GetTernaryState(h => h.Any(s => s.Name == sampleName));
                }

                foreach ((string bankName, var bindable) in selectionBankStates)
                {
                    bindable.Value = samplesInSelection.SelectMany(s => s).Where(o => o.Name == HitSampleInfo.HIT_NORMAL).GetTernaryState(h => h.Bank == bankName);
                }

                // if there are no addition samples in the selection, do not touch the state of addition bank bindables.
                // this is to reduce annoyance from the bank resetting if the user wants to e.g. remove the only addition sound on an object, but then add another addition sound
                // while keeping the bank the same.
                // note that deselecting all objects will still reset the addition bank selection to auto via `ResetTernaryStates()`. this may need to be reconsidered later.
                if (samplesInSelection.SelectMany(s => s).Any(o => o.Name != HitSampleInfo.HIT_NORMAL))
                {
                    foreach ((string bankName, var bindable) in selectionAdditionBankStates)
                    {
                        bindable.Value = samplesInSelection.SelectMany(s => s).Where(o => o.Name != HitSampleInfo.HIT_NORMAL)
                                                           .GetTernaryState(h =>
                                                               (bankName != HIT_BANK_AUTO && h.Bank == bankName && !h.EditorAutoBank) || (bankName == HIT_BANK_AUTO && h.EditorAutoBank));
                    }
                }
            }
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

        #region Ternary button creation

        /// <summary>
        /// Create all ternary states required to be displayed to the user.
        /// </summary>
        protected virtual IEnumerable<Drawable> CreateTernaryButtons()
        {
            if (SelectionNewComboState != null && CompositionTools.Count > 0)
            {
                yield return new NewComboTernaryButton
                {
                    Current = SelectionNewComboState,
                    CreateIcon = () => new Container
                    {
                        Children = new[]
                        {
                            CompositionTools[0].CreateIcon()?.With(d =>
                            {
                                d.Anchor = Anchor.BottomLeft;
                                d.Origin = Anchor.BottomLeft;
                                d.Size = new Vector2(15);
                            }) ?? Empty(),
                            new SpriteIcon
                            {
                                Icon = OsuIcon.EditorNewComboSparkles,
                                Size = new Vector2(20),
                            }
                        },
                    },
                };
            }

            foreach (var kvp in SelectionSampleStates)
            {
                yield return new DrawableTernaryButton<GlobalAction>
                {
                    Current = kvp.Value,
                    Description = kvp.Key.Replace(@"hit", string.Empty).Titleize(),
                    CreateIcon = () => GetIconForSample(kvp.Key),
                    Action = GetActionForSample(kvp.Key),
                    Hotkey = new Hotkey(GetActionForSample(kvp.Key)),
                };
            }
        }

        private IEnumerable<SampleBankTernaryButton> createSampleBankTernaryButtons()
        {
            foreach (string bankName in HitSampleInfo.ALL_BANKS.Prepend(HIT_BANK_AUTO))
            {
                yield return new SampleBankTernaryButton(bankName)
                {
                    NormalState = { Current = SelectionBankStates[bankName], },
                    AdditionsState = { Current = SelectionAdditionBankStates[bankName], },
                    NormalHotkey = getHotkeyForBank(bankName, false),
                    AdditionsHotkey = getHotkeyForBank(bankName, true),
                    CreateIcon = () => getIconForBank(bankName),
                    CreateCompactIcon = () => getCompactIconForBank(bankName),
                };
            }

            AutoSelectionBankEnabled.BindValueChanged(_ => updateAutoBankTernaryButtonTooltip(), true);
        }

        private Drawable getIconForBank(string sampleName)
        {
            return new SpriteIcon
            {
                Size = new Vector2(20, 20),
                Icon = sampleName switch
                {
                    HIT_BANK_AUTO => OsuIcon.EditorBankAuto,
                    HitSampleInfo.BANK_NORMAL => OsuIcon.EditorBankNormal,
                    HitSampleInfo.BANK_SOFT => OsuIcon.EditorBankSoft,
                    HitSampleInfo.BANK_DRUM => OsuIcon.EditorBankDrum,
                    _ => throw new ArgumentOutOfRangeException(nameof(sampleName), sampleName, null)
                },
            };
        }

        private Drawable getCompactIconForBank(string sampleName)
        {
            return new SpriteIcon
            {
                Size = new Vector2(10, 20),
                Icon = sampleName switch
                {
                    HIT_BANK_AUTO => OsuIcon.EditorBankAutoCompact,
                    HitSampleInfo.BANK_NORMAL => OsuIcon.EditorBankNormalCompact,
                    HitSampleInfo.BANK_SOFT => OsuIcon.EditorBankSoftCompact,
                    HitSampleInfo.BANK_DRUM => OsuIcon.EditorBankDrumCompact,
                    _ => throw new ArgumentOutOfRangeException(nameof(sampleName), sampleName, null)
                },
            };
        }

        private GlobalAction getHotkeyForBank(string sampleName, bool addition) => (sampleName, addition) switch
        {
            (HIT_BANK_AUTO, false) => GlobalAction.EditorToggleNormalAutoBank,
            (HitSampleInfo.BANK_NORMAL, false) => GlobalAction.EditorToggleNormalNormalBank,
            (HitSampleInfo.BANK_SOFT, false) => GlobalAction.EditorToggleNormalSoftBank,
            (HitSampleInfo.BANK_DRUM, false) => GlobalAction.EditorToggleNormalDrumBank,

            (HIT_BANK_AUTO, true) => GlobalAction.EditorToggleAdditionAutoBank,
            (HitSampleInfo.BANK_NORMAL, true) => GlobalAction.EditorToggleAdditionNormalBank,
            (HitSampleInfo.BANK_SOFT, true) => GlobalAction.EditorToggleAdditionSoftBank,
            (HitSampleInfo.BANK_DRUM, true) => GlobalAction.EditorToggleAdditionDrumBank,

            _ => throw new ArgumentOutOfRangeException(nameof(sampleName), sampleName, null)
        };

        private void updateAutoBankTernaryButtonTooltip()
        {
            bool enabled = AutoSelectionBankEnabled.Value;

            var autoBankButton = sampleBankTogglesCollection.Single(t => t.BankName == HIT_BANK_AUTO);
            autoBankButton.NormalButton.Enabled.Value = enabled;
            autoBankButton.NormalButton.TooltipText = !enabled ? "Auto normal bank can only be used during hit object placement" : string.Empty;
        }

        #endregion

        #region Ternary state changes

        /// <summary>
        /// Sets the sample bank for all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <remarks>
        /// Should be kept in sync with <see cref="SamplePointPiece.SampleEditPopover.setBank"/>.
        /// </remarks>
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

            if (EditorBeatmap.SelectedHitObjects.All(hasRelevantBank))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                if (hasRelevantBank(h))
                    return;

                h.Samples = h.Samples.Select(s => s.Name == HitSampleInfo.HIT_NORMAL || s.EditorAutoBank ? s.With(newBank: bankName) : s).ToList();

                if (h is IHasRepeats hasRepeats)
                {
                    for (int i = 0; i < hasRepeats.NodeSamples.Count; ++i)
                        hasRepeats.NodeSamples[i] = hasRepeats.NodeSamples[i].Select(s => s.Name == HitSampleInfo.HIT_NORMAL || s.EditorAutoBank ? s.With(newBank: bankName) : s).ToList();
                }
            });
        }

        /// <summary>
        /// Sets the sample addition bank for all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <remarks>
        /// Should be kept in sync with <see cref="SamplePointPiece.SampleEditPopover.setAdditionBank"/>.
        /// </remarks>
        /// <param name="bankName">The name of the sample bank.</param>
        public void SetSampleAdditionBank(string bankName)
        {
            bool hasRelevantBank(HitObject hitObject) =>
                bankName == HIT_BANK_AUTO
                    ? enumerateAllSamples(hitObject).SelectMany(o => o).Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(s => s.EditorAutoBank)
                    : enumerateAllSamples(hitObject).SelectMany(o => o).Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(s => s.Bank == bankName && !s.EditorAutoBank);

            if (EditorBeatmap.SelectedHitObjects.All(hasRelevantBank))
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
        /// <remarks>
        /// Should be kept in sync with <see cref="SamplePointPiece.SampleEditPopover.addHitSample"/>.
        /// </remarks>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void AddHitSample(string sampleName)
        {
            if (EditorBeatmap.SelectedHitObjects.All(h => hasRelevantSample(h, sampleName)))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                string? forcedBank = null;
                // if the selected object(s) only have normal samples, check whether the user has preselected a singular non-auto bank using `SelectionAdditionBankStates`.
                // other scenarios are already handled by `CreateHitSampleInfo()`:
                // - if the selected object(s) already have addition samples, `CreateHitSampleInfo()` will copy the bank from said addition samples.
                // - if the selected object(s) do not have addition samples but the user has preselected auto bank, `CreateHitSampleInfo()` will use the auto bank anyway.
                if (h.Samples.All(s => s.Name == HitSampleInfo.HIT_NORMAL))
                    forcedBank = selectionAdditionBankStates.SingleOrDefault(kv => kv.Value.Value == TernaryState.True).Key;

                // Make sure there isn't already an existing sample
                if (h.Samples.All(s => s.Name != sampleName))
                {
                    var hitSample = h.CreateHitSampleInfo(sampleName);

                    if (forcedBank != null && forcedBank != HIT_BANK_AUTO)
                        hitSample = hitSample.With(newBank: forcedBank, newEditorAutoBank: false);

                    h.Samples.Add(hitSample);
                }

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
            if (EditorBeatmap.SelectedHitObjects.All(h => !hasRelevantSample(h, sampleName)))
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
            if (EditorBeatmap.SelectedHitObjects.OfType<IHasComboInformation>().All(h => h.NewCombo == state))
                return;

            EditorBeatmap.PerformOnSelection(h =>
            {
                var comboInfo = h as IHasComboInformation;

                if (comboInfo == null || comboInfo.NewCombo == state) return;

                comboInfo.NewCombo = state;
            });
        }

        #endregion
    }

    public abstract partial class HitObjectComposer
    {
        #region Selection State

        /// <summary>
        /// A special bank name that is only used in the editor UI.
        /// When selected and in placement mode, the bank of the last hit object will always be used.
        /// </summary>
        public const string HIT_BANK_AUTO = @"auto";

        public event Action? SelectionStateChanged;

        public void InvokeSelectionStateChanged() => SelectionStateChanged?.Invoke();

        /// <summary>
        /// The state of "new combo" for all selected hitobjects.
        /// </summary>
        public abstract Bindable<TernaryState>? SelectionNewComboState { get; }

        /// <summary>
        /// The state of each sample type for all selected hitobjects. Keys match with <see cref="HitSampleInfo"/> constant specifications.
        /// </summary>
        public abstract IReadOnlyDictionary<string, Bindable<TernaryState>> SelectionSampleStates { get; }

        /// <summary>
        /// The state of each sample bank type for all selected hitobjects.
        /// </summary>
        public abstract IReadOnlyDictionary<string, Bindable<TernaryState>> SelectionBankStates { get; }

        /// <summary>
        /// The state of each sample addition bank type for all selected hitobjects.
        /// </summary>
        public abstract IReadOnlyDictionary<string, Bindable<TernaryState>> SelectionAdditionBankStates { get; }

        /// <summary>
        /// Whether there is no selection and the auto <see cref="SelectionBankStates"/> can be used.
        /// </summary>
        public abstract Bindable<bool> AutoSelectionBankEnabled { get; }

        #endregion

        public static GlobalAction GetActionForSample(string sampleName) => sampleName switch
        {
            HitSampleInfo.HIT_CLAP => GlobalAction.EditorToggleClapSound,
            HitSampleInfo.HIT_WHISTLE => GlobalAction.EditorToggleWhistleSound,
            HitSampleInfo.HIT_FINISH => GlobalAction.EditorToggleFinishSound,
            _ => throw new ArgumentOutOfRangeException(nameof(sampleName), sampleName, null),
        };

        public static Drawable GetIconForSample(string sampleName)
        {
            switch (sampleName)
            {
                case HitSampleInfo.HIT_CLAP:
                    return new SpriteIcon { Icon = OsuIcon.EditorClap };

                case HitSampleInfo.HIT_WHISTLE:
                    return new SpriteIcon { Icon = OsuIcon.EditorWhistle };

                case HitSampleInfo.HIT_FINISH:
                    return new SpriteIcon { Icon = OsuIcon.EditorFinish };
            }

            throw new ArgumentOutOfRangeException(nameof(sampleName), sampleName, null);
        }
    }
}
