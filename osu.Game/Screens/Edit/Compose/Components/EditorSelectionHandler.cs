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
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class EditorSelectionHandler : SelectionHandler<HitObject>
    {
        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            createStateBindables();

            // bring in updates from selection changes
            EditorBeatmap.HitObjectUpdated += _ => Scheduler.AddOnce(UpdateTernaryStates);

            SelectedItems.BindTo(EditorBeatmap.SelectedHitObjects);
            SelectedItems.CollectionChanged += (sender, args) =>
            {
                Scheduler.AddOnce(UpdateTernaryStates);
            };
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
        /// Set up ternary state bindables and bind them to selection/hitobject changes (in both directions)
        /// </summary>
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
        }

        #endregion

        #region Ternary state changes

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

                h.Samples.Add(new HitSampleInfo(sampleName));
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

            yield return new OsuMenuItem("Sound")
            {
                Items = SelectionSampleStates.Select(kvp =>
                    new TernaryStateToggleMenuItem(kvp.Value.Description) { State = { BindTarget = kvp.Value } }).ToArray()
            };
        }

        #endregion
    }
}
