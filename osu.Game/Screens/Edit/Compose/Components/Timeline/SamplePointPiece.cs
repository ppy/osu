// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Screens.Edit.Timing;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class SamplePointPiece : HitObjectPointPiece, IHasPopover
    {
        public readonly HitObject HitObject;

        [Resolved]
        private EditorClock? editorClock { get; set; }

        [Resolved]
        private Editor? editor { get; set; }

        [Resolved]
        private TimelineBlueprintContainer? timelineBlueprintContainer { get; set; }

        public SamplePointPiece(HitObject hitObject)
        {
            HitObject = hitObject;
            Y = 2.5f;
        }

        public bool AlternativeColor { get; init; }

        protected override Color4 GetRepresentingColour(OsuColour colours) => AlternativeColor ? colours.Pink2 : colours.Pink1;

        protected virtual double GetTime() => HitObject is IHasRepeats r ? HitObject.StartTime + r.Duration / r.SpanCount() / 2 : HitObject.StartTime;

        [BackgroundDependencyLoader]
        private void load()
        {
            Label.AllowMultiline = false;
            LabelContainer.AutoSizeAxes = Axes.None;
            updateText();

            if (editor != null)
                editor.ShowSampleEditPopoverRequested += onShowSampleEditPopoverRequested;
        }

        private readonly Bindable<bool> contracted = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            HitObject.DefaultsApplied += onDefaultsApplied;

            if (timelineBlueprintContainer != null)
                contracted.BindTo(timelineBlueprintContainer.SamplePointContracted);

            contracted.BindValueChanged(v =>
            {
                if (v.NewValue)
                {
                    Label.FadeOut(200, Easing.OutQuint);
                    LabelContainer.ResizeTo(new Vector2(12), 200, Easing.OutQuint);
                    LabelContainer.CornerRadius = 6;
                }
                else
                {
                    Label.FadeIn(200, Easing.OutQuint);
                    LabelContainer.ResizeTo(new Vector2(Label.Width, 16), 200, Easing.OutQuint);
                    LabelContainer.CornerRadius = 8;
                }
            }, true);

            FinishTransforms();
        }

        private void onDefaultsApplied(HitObject hitObject)
        {
            updateText();
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (editor != null)
                editor.ShowSampleEditPopoverRequested -= onShowSampleEditPopoverRequested;

            HitObject.DefaultsApplied -= onDefaultsApplied;
        }

        private void onShowSampleEditPopoverRequested(double time)
        {
            if (!Precision.AlmostEquals(time, GetTime())) return;

            editorClock?.SeekSmoothlyTo(GetTime());
            this.ShowPopover();
        }

        protected override bool OnClick(ClickEvent e)
        {
            this.ShowPopover();
            return true;
        }

        private void updateText()
        {
            Label.Text = $"{abbreviateBank(GetBankValue(GetSamples()))} {GetVolumeValue(GetSamples())}";

            if (!contracted.Value)
                LabelContainer.ResizeWidthTo(Label.Width, 200, Easing.OutQuint);
        }

        private static string? abbreviateBank(string? bank)
        {
            return bank switch
            {
                HitSampleInfo.BANK_NORMAL => @"N",
                HitSampleInfo.BANK_SOFT => @"S",
                HitSampleInfo.BANK_DRUM => @"D",
                _ => bank
            };
        }

        public static string? GetBankValue(IEnumerable<HitSampleInfo> samples)
        {
            return samples.FirstOrDefault(o => o.Name == HitSampleInfo.HIT_NORMAL)?.Bank;
        }

        public static string? GetAdditionBankValue(IEnumerable<HitSampleInfo> samples)
        {
            var firstAddition = samples.FirstOrDefault(o => o.Name != HitSampleInfo.HIT_NORMAL);
            if (firstAddition == null)
                return null;

            return firstAddition.EditorAutoBank ? EditorSelectionHandler.HIT_BANK_AUTO : firstAddition.Bank;
        }

        public static int GetVolumeValue(ICollection<HitSampleInfo> samples)
        {
            return samples.Count == 0 ? 0 : samples.Max(o => o.Volume);
        }

        /// <summary>
        /// Gets the samples to be edited by this sample point piece.
        /// This could be the samples of the hit object itself, or of one of the nested hit objects. For example a slider repeat.
        /// </summary>
        /// <returns>The samples to be edited.</returns>
        protected virtual IList<HitSampleInfo> GetSamples() => HitObject.Samples;

        public virtual Popover GetPopover() => new SampleEditPopover(HitObject);

        public partial class SampleEditPopover : OsuPopover
        {
            private readonly HitObject hitObject;

            private LabelledTextBox bank = null!;
            private LabelledTextBox additionBank = null!;
            private IndeterminateSliderWithTextBoxInput<int> volume = null!;

            private FillFlowContainer togglesCollection = null!;

            private HitObject[] relevantObjects = null!;
            private (HitObject hitObject, IList<HitSampleInfo> samples)[] allRelevantSamples = null!;

            /// <summary>
            /// Gets the sub-set of samples relevant to this sample point piece.
            /// For example, to edit node samples this should return the samples at the index of the node.
            /// </summary>
            /// <param name="hitObjects">The hit objects to get the relevant samples from.</param>
            /// <returns>The relevant list of samples.</returns>
            protected virtual IEnumerable<(HitObject hitObject, IList<HitSampleInfo> samples)> GetRelevantSamples(HitObject[] hitObjects)
            {
                if (hitObjects.Length == 1)
                {
                    yield return (hitObjects[0], hitObjects[0].Samples);

                    yield break;
                }

                foreach (var ho in hitObjects)
                {
                    yield return (ho, ho.Samples);

                    if (ho is IHasRepeats hasRepeats)
                    {
                        foreach (var node in hasRepeats.NodeSamples)
                            yield return (ho, node);
                    }
                }
            }

            [Resolved(canBeNull: true)]
            private EditorBeatmap beatmap { get; set; } = null!;

            public SampleEditPopover(HitObject hitObject)
            {
                this.hitObject = hitObject;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                FillFlowContainer flow;

                Children = new Drawable[]
                {
                    flow = new FillFlowContainer
                    {
                        Width = 200,
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 10),
                        Children = new Drawable[]
                        {
                            togglesCollection = new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(5, 5),
                            },
                            bank = new LabelledTextBox
                            {
                                Label = "Bank Name",
                                SelectAllOnFocus = true,
                            },
                            additionBank = new LabelledTextBox
                            {
                                Label = "Addition Bank",
                                SelectAllOnFocus = true,
                            },
                            volume = new IndeterminateSliderWithTextBoxInput<int>("Volume", new BindableInt(100)
                            {
                                MinValue = DrawableHitObject.MINIMUM_SAMPLE_VOLUME,
                                MaxValue = 100,
                            })
                        }
                    }
                };

                bank.TabbableContentContainer = flow;
                additionBank.TabbableContentContainer = flow;
                volume.TabbableContentContainer = flow;

                // if the piece belongs to a currently selected object, assume that the user wants to change all selected objects.
                // if the piece belongs to an unselected object, operate on that object alone, independently of the selection.
                relevantObjects = (beatmap.SelectedHitObjects.Contains(hitObject) ? beatmap.SelectedHitObjects : hitObject.Yield()).ToArray();
                allRelevantSamples = GetRelevantSamples(relevantObjects).ToArray();

                // even if there are multiple objects selected, we can still display sample volume or bank if they all have the same value.
                int? commonVolume = getCommonVolume();
                if (commonVolume != null)
                    volume.Current.Value = commonVolume.Value;

                updatePrimaryBankState();
                bank.Current.BindValueChanged(val =>
                {
                    if (string.IsNullOrEmpty(val.NewValue))
                        return;

                    setBank(val.NewValue);
                    updatePrimaryBankState();
                });
                // on commit, ensure that the value is correct by sourcing it from the objects' samples again.
                // this ensures that committing empty text causes a revert to the previous value.
                bank.OnCommit += (_, _) => updatePrimaryBankState();

                updateAdditionBankState();
                additionBank.Current.BindValueChanged(val =>
                {
                    if (string.IsNullOrEmpty(val.NewValue))
                        return;

                    setAdditionBank(val.NewValue);
                    updateAdditionBankState();
                });
                additionBank.OnCommit += (_, _) => updateAdditionBankState();

                volume.Current.BindValueChanged(val =>
                {
                    if (val.NewValue != null)
                        setVolume(val.NewValue.Value);
                });

                createStateBindables();
                updateTernaryStates();
                togglesCollection.AddRange(createTernaryButtons());
            }

            private string? getCommonBank() => allRelevantSamples.Select(h => GetBankValue(h.samples)).Distinct().Count() == 1
                ? GetBankValue(allRelevantSamples.First().samples)
                : null;

            private string? getCommonAdditionBank()
            {
                string[] additionBanks = allRelevantSamples.Select(h => GetAdditionBankValue(h.samples)).Where(o => o is not null).Cast<string>().Distinct().ToArray();
                return additionBanks.Length == 1 ? additionBanks[0] : null;
            }

            private int? getCommonVolume() => allRelevantSamples.Select(h => GetVolumeValue(h.samples)).Distinct().Count() == 1
                ? GetVolumeValue(allRelevantSamples.First().samples)
                : null;

            private void updatePrimaryBankState()
            {
                string? commonBank = getCommonBank();
                bank.Current.Value = commonBank;
                bank.PlaceholderText = string.IsNullOrEmpty(commonBank) ? "(multiple)" : string.Empty;
            }

            private void updateAdditionBankState()
            {
                string? commonAdditionBank = getCommonAdditionBank();
                additionBank.PlaceholderText = string.IsNullOrEmpty(commonAdditionBank) ? "(multiple)" : string.Empty;
                additionBank.Current.Value = commonAdditionBank;

                bool anyAdditions = allRelevantSamples.Any(o => o.samples.Any(s => s.Name != HitSampleInfo.HIT_NORMAL));
                if (anyAdditions)
                    additionBank.Show();
                else
                    additionBank.Hide();
            }

            /// <summary>
            /// Applies the given update action on all samples of <see cref="allRelevantSamples"/>
            /// and invokes the necessary update notifiers for the beatmap and hit objects.
            /// </summary>
            /// <param name="updateAction">The action to perform on each element of <see cref="allRelevantSamples"/>.</param>
            private void updateAllRelevantSamples(Action<HitObject, IList<HitSampleInfo>> updateAction)
            {
                beatmap.BeginChange();

                foreach (var (relevantHitObject, relevantSamples) in GetRelevantSamples(relevantObjects))
                {
                    updateAction(relevantHitObject, relevantSamples);
                    beatmap.Update(relevantHitObject);
                }

                beatmap.EndChange();
            }

            private void setBank(string newBank)
            {
                updateAllRelevantSamples((_, relevantSamples) =>
                {
                    for (int i = 0; i < relevantSamples.Count; i++)
                    {
                        if (relevantSamples[i].Name != HitSampleInfo.HIT_NORMAL && !relevantSamples[i].EditorAutoBank) continue;

                        relevantSamples[i] = relevantSamples[i].With(newBank: newBank);
                    }
                });
            }

            private void setAdditionBank(string newBank)
            {
                updateAllRelevantSamples((_, relevantSamples) =>
                {
                    string normalBank = relevantSamples.FirstOrDefault(s => s.Name == HitSampleInfo.HIT_NORMAL)?.Bank ?? HitSampleInfo.BANK_SOFT;

                    for (int i = 0; i < relevantSamples.Count; i++)
                    {
                        if (relevantSamples[i].Name == HitSampleInfo.HIT_NORMAL)
                            continue;

                        // Addition samples with bank set to auto should inherit the bank of the normal sample
                        if (newBank == EditorSelectionHandler.HIT_BANK_AUTO)
                        {
                            relevantSamples[i] = relevantSamples[i].With(newBank: normalBank, newEditorAutoBank: true);
                        }
                        else
                            relevantSamples[i] = relevantSamples[i].With(newBank: newBank, newEditorAutoBank: false);
                    }
                });
            }

            private void setVolume(int newVolume)
            {
                updateAllRelevantSamples((_, relevantSamples) =>
                {
                    for (int i = 0; i < relevantSamples.Count; i++)
                    {
                        relevantSamples[i] = relevantSamples[i].With(newVolume: newVolume);
                    }
                });
            }

            #region hitsound toggles

            private readonly Dictionary<string, Bindable<TernaryState>> selectionSampleStates = new Dictionary<string, Bindable<TernaryState>>();

            private readonly List<string> banks = new List<string>();

            private void createStateBindables()
            {
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
                                removeHitSample(sampleName);
                                break;

                            case TernaryState.True:
                                addHitSample(sampleName);
                                break;
                        }
                    };

                    selectionSampleStates[sampleName] = bindable;
                }

                banks.AddRange(HitSampleInfo.ALL_BANKS.Prepend(EditorSelectionHandler.HIT_BANK_AUTO));
            }

            private void updateTernaryStates()
            {
                foreach ((string sampleName, var bindable) in selectionSampleStates)
                {
                    bindable.Value = SelectionHandler<HitObject>.GetStateFromSelection(GetRelevantSamples(relevantObjects), h => h.samples.Any(s => s.Name == sampleName));
                }
            }

            private IEnumerable<DrawableTernaryButton> createTernaryButtons()
            {
                foreach ((string sampleName, var bindable) in selectionSampleStates)
                {
                    yield return new DrawableTernaryButton
                    {
                        Current = bindable,
                        Description = string.Empty,
                        CreateIcon = () => ComposeBlueprintContainer.GetIconForSample(sampleName),
                        RelativeSizeAxes = Axes.None,
                        Size = new Vector2(40, 40),
                    };
                }
            }

            private void addHitSample(string sampleName)
            {
                if (string.IsNullOrEmpty(sampleName))
                    return;

                updateAllRelevantSamples((h, relevantSamples) =>
                {
                    // Make sure there isn't already an existing sample
                    if (relevantSamples.Any(s => s.Name == sampleName))
                        return;

                    // First try inheriting the sample info from the node samples instead of the samples of the hitobject
                    var relevantSample = relevantSamples.FirstOrDefault(s => s.Name != HitSampleInfo.HIT_NORMAL) ?? relevantSamples.FirstOrDefault();
                    relevantSamples.Add(relevantSample?.With(sampleName) ?? h.CreateHitSampleInfo(sampleName));
                });

                updateAdditionBankState();
            }

            private void removeHitSample(string sampleName)
            {
                if (string.IsNullOrEmpty(sampleName))
                    return;

                updateAllRelevantSamples((_, relevantSamples) =>
                {
                    for (int i = 0; i < relevantSamples.Count; i++)
                    {
                        if (relevantSamples[i].Name == sampleName)
                            relevantSamples.RemoveAt(i--);
                    }
                });

                updateAdditionBankState();
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (e.ControlPressed || e.SuperPressed || !checkRightToggleFromKey(e.Key, out int rightIndex))
                    return base.OnKeyDown(e);

                if (e.ShiftPressed || e.AltPressed)
                {
                    string? newBank = banks.ElementAtOrDefault(rightIndex);

                    if (string.IsNullOrEmpty(newBank))
                        return true;

                    if (e.ShiftPressed && newBank != EditorSelectionHandler.HIT_BANK_AUTO)
                    {
                        setBank(newBank);
                        updatePrimaryBankState();
                    }

                    if (e.AltPressed)
                    {
                        setAdditionBank(newBank);
                        updateAdditionBankState();
                    }
                }
                else
                {
                    var item = togglesCollection.ElementAtOrDefault(rightIndex - 1);

                    if (item is not DrawableTernaryButton button) return base.OnKeyDown(e);

                    button.Toggle();
                }

                return true;
            }

            private bool checkRightToggleFromKey(Key key, out int index)
            {
                switch (key)
                {
                    case Key.Q:
                        index = 0;
                        break;

                    case Key.W:
                        index = 1;
                        break;

                    case Key.E:
                        index = 2;
                        break;

                    case Key.R:
                        index = 3;
                        break;

                    default:
                        index = -1;
                        break;
                }

                return index >= 0;
            }

            #endregion
        }
    }
}
