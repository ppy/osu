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
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Screens.Edit.Timing;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components.Timeline
{
    public partial class SamplePointPiece : HitObjectPointPiece, IHasPopover
    {
        public readonly HitObject HitObject;

        public SamplePointPiece(HitObject hitObject)
        {
            HitObject = hitObject;
        }

        public bool AlternativeColor { get; init; }

        protected override Color4 GetRepresentingColour(OsuColour colours) => AlternativeColor ? colours.PinkDarker : colours.Pink;

        [BackgroundDependencyLoader]
        private void load()
        {
            HitObject.DefaultsApplied += _ => updateText();
            updateText();
        }

        protected override bool OnClick(ClickEvent e)
        {
            this.ShowPopover();
            return true;
        }

        private void updateText()
        {
            Label.Text = $"{abbreviateBank(GetBankValue(GetSamples()))} {GetVolumeValue(GetSamples())}";
        }

        private static string? abbreviateBank(string? bank)
        {
            return bank switch
            {
                "normal" => "N",
                "soft" => "S",
                "drum" => "D",
                _ => bank
            };
        }

        public static string? GetBankValue(IEnumerable<HitSampleInfo> samples)
        {
            return samples.FirstOrDefault(o => o.Name == HitSampleInfo.HIT_NORMAL)?.Bank;
        }

        public static string? GetAdditionBankValue(IEnumerable<HitSampleInfo> samples)
        {
            return samples.FirstOrDefault(o => o.Name != HitSampleInfo.HIT_NORMAL)?.Bank ?? GetBankValue(samples);
        }

        public static int GetVolumeValue(ICollection<HitSampleInfo> samples)
        {
            return samples.Count == 0 ? 0 : samples.Max(o => o.Volume);
        }

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
            private IList<HitSampleInfo>[] allRelevantSamples = null!;

            /// <summary>
            /// Gets the sub-set of samples relevant to this sample point piece.
            /// For example, to edit node samples this should return the samples at the index of the node.
            /// </summary>
            /// <param name="ho">The hit object to get the relevant samples from.</param>
            /// <returns>The relevant list of samples.</returns>
            protected virtual IList<HitSampleInfo> GetRelevantSamples(HitObject ho) => ho.Samples;

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
                            },
                            additionBank = new LabelledTextBox
                            {
                                Label = "Addition Bank",
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
                allRelevantSamples = relevantObjects.Select(GetRelevantSamples).ToArray();

                // even if there are multiple objects selected, we can still display sample volume or bank if they all have the same value.
                string? commonBank = getCommonBank();
                if (!string.IsNullOrEmpty(commonBank))
                    bank.Current.Value = commonBank;

                int? commonVolume = getCommonVolume();
                if (commonVolume != null)
                    volume.Current.Value = commonVolume.Value;

                updateBankPlaceholderText();
                bank.Current.BindValueChanged(val =>
                {
                    updateBank(val.NewValue);
                    updateBankPlaceholderText();
                });
                // on commit, ensure that the value is correct by sourcing it from the objects' samples again.
                // this ensures that committing empty text causes a revert to the previous value.
                bank.OnCommit += (_, _) => updateBankText();

                updateAdditionBankText();
                updateAdditionBankVisual();
                additionBank.Current.BindValueChanged(val =>
                {
                    updateAdditionBank(val.NewValue);
                    updateAdditionBankVisual();
                });
                additionBank.OnCommit += (_, _) => updateAdditionBankText();

                volume.Current.BindValueChanged(val => updateVolume(val.NewValue));

                createStateBindables();
                updateTernaryStates();
                togglesCollection.AddRange(createTernaryButtons().Select(b => new DrawableTernaryButton(b) { RelativeSizeAxes = Axes.None, Size = new Vector2(40, 40) }));
            }

            private string? getCommonBank() => allRelevantSamples.Select(GetBankValue).Distinct().Count() == 1 ? GetBankValue(allRelevantSamples.First()) : null;
            private string? getCommonAdditionBank() => allRelevantSamples.Select(GetAdditionBankValue).Distinct().Count() == 1 ? GetAdditionBankValue(allRelevantSamples.First()) : null;
            private int? getCommonVolume() => allRelevantSamples.Select(GetVolumeValue).Distinct().Count() == 1 ? GetVolumeValue(allRelevantSamples.First()) : null;

            /// <summary>
            /// Applies the given update action on all samples of <see cref="allRelevantSamples"/>
            /// and invokes the necessary update notifiers for the beatmap and hit objects.
            /// </summary>
            /// <param name="updateAction">The action to perform on each element of <see cref="allRelevantSamples"/>.</param>
            private void updateAllRelevantSamples(Action<HitObject, IList<HitSampleInfo>> updateAction)
            {
                beatmap.BeginChange();

                foreach (var relevantHitObject in relevantObjects)
                {
                    var relevantSamples = GetRelevantSamples(relevantHitObject);
                    updateAction(relevantHitObject, relevantSamples);
                    beatmap.Update(relevantHitObject);
                }

                beatmap.EndChange();
            }

            private void updateBank(string? newBank)
            {
                if (string.IsNullOrEmpty(newBank))
                    return;

                updateAllRelevantSamples((_, relevantSamples) =>
                {
                    for (int i = 0; i < relevantSamples.Count; i++)
                    {
                        if (relevantSamples[i].Name != HitSampleInfo.HIT_NORMAL) continue;

                        relevantSamples[i] = relevantSamples[i].With(newBank: newBank);
                    }
                });
            }

            private void updateAdditionBank(string? newBank)
            {
                if (string.IsNullOrEmpty(newBank))
                    return;

                updateAllRelevantSamples((_, relevantSamples) =>
                {
                    for (int i = 0; i < relevantSamples.Count; i++)
                    {
                        if (relevantSamples[i].Name == HitSampleInfo.HIT_NORMAL) continue;

                        relevantSamples[i] = relevantSamples[i].With(newBank: newBank);
                    }
                });
            }

            private void updateBankText()
            {
                bank.Current.Value = getCommonBank();
            }

            private void updateBankPlaceholderText()
            {
                string? commonBank = getCommonBank();
                bank.PlaceholderText = string.IsNullOrEmpty(commonBank) ? "(multiple)" : string.Empty;
            }

            private void updateAdditionBankVisual()
            {
                string? commonAdditionBank = getCommonAdditionBank();
                additionBank.PlaceholderText = string.IsNullOrEmpty(commonAdditionBank) ? "(multiple)" : string.Empty;

                bool anyAdditions = allRelevantSamples.Any(o => o.Any(s => s.Name != HitSampleInfo.HIT_NORMAL));
                if (anyAdditions)
                    additionBank.Show();
                else
                    additionBank.Hide();
            }

            private void updateAdditionBankText()
            {
                string? commonAdditionBank = getCommonAdditionBank();
                if (string.IsNullOrEmpty(commonAdditionBank)) return;

                additionBank.Current.Value = commonAdditionBank;
            }

            private void updateVolume(int? newVolume)
            {
                if (newVolume == null)
                    return;

                updateAllRelevantSamples((_, relevantSamples) =>
                {
                    for (int i = 0; i < relevantSamples.Count; i++)
                    {
                        relevantSamples[i] = relevantSamples[i].With(newVolume: newVolume.Value);
                    }
                });
            }

            #region hitsound toggles

            private readonly Dictionary<string, Bindable<TernaryState>> selectionSampleStates = new Dictionary<string, Bindable<TernaryState>>();

            private readonly List<string> banks = new List<string>();

            private void createStateBindables()
            {
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
                                removeHitSample(sampleName);
                                break;

                            case TernaryState.True:
                                addHitSample(sampleName);
                                break;
                        }
                    };

                    selectionSampleStates[sampleName] = bindable;
                }

                banks.AddRange(HitSampleInfo.AllBanks);
            }

            private void updateTernaryStates()
            {
                foreach ((string sampleName, var bindable) in selectionSampleStates)
                {
                    bindable.Value = SelectionHandler<HitObject>.GetStateFromSelection(relevantObjects, h => GetRelevantSamples(h).Any(s => s.Name == sampleName));
                }
            }

            private IEnumerable<TernaryButton> createTernaryButtons()
            {
                foreach ((string sampleName, var bindable) in selectionSampleStates)
                    yield return new TernaryButton(bindable, string.Empty, () => ComposeBlueprintContainer.GetIconForSample(sampleName));
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

                updateAdditionBankVisual();
                updateAdditionBankText();
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

                updateAdditionBankText();
                updateAdditionBankVisual();
            }

            protected override bool OnKeyDown(KeyDownEvent e)
            {
                if (e.ControlPressed || e.AltPressed || e.SuperPressed || !checkRightToggleFromKey(e.Key, out int rightIndex))
                    return base.OnKeyDown(e);

                if (e.ShiftPressed)
                {
                    string? newBank = banks.ElementAtOrDefault(rightIndex);
                    updateBank(newBank);
                    updateBankText();
                    updateAdditionBank(newBank);
                    updateAdditionBankText();
                }
                else
                {
                    var item = togglesCollection.ElementAtOrDefault(rightIndex);

                    if (item is not DrawableTernaryButton button) return base.OnKeyDown(e);

                    button.Button.Toggle();
                }

                return true;
            }

            private bool checkRightToggleFromKey(Key key, out int index)
            {
                switch (key)
                {
                    case Key.W:
                        index = 0;
                        break;

                    case Key.E:
                        index = 1;
                        break;

                    case Key.R:
                        index = 2;
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
