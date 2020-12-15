// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A component which outlines <see cref="DrawableHitObject"/>s and handles movement of selections.
    /// </summary>
    public class SelectionHandler : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
    {
        public IEnumerable<SelectionBlueprint> SelectedBlueprints => selectedBlueprints;
        private readonly List<SelectionBlueprint> selectedBlueprints;

        public int SelectedCount => selectedBlueprints.Count;

        private Drawable content;

        private OsuSpriteText selectionDetailsText;

        protected SelectionBox SelectionBox { get; private set; }

        [Resolved]
        protected EditorBeatmap EditorBeatmap { get; private set; }

        [Resolved(CanBeNull = true)]
        protected IEditorChangeHandler ChangeHandler { get; private set; }

        public SelectionHandler()
        {
            selectedBlueprints = new List<SelectionBlueprint>();

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            createStateBindables();

            InternalChild = content = new Container
            {
                Children = new Drawable[]
                {
                    // todo: should maybe be inside the SelectionBox?
                    new Container
                    {
                        Name = "info text",
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                Colour = colours.YellowDark,
                                RelativeSizeAxes = Axes.Both,
                            },
                            selectionDetailsText = new OsuSpriteText
                            {
                                Padding = new MarginPadding(2),
                                Colour = colours.Gray0,
                                Font = OsuFont.Default.With(size: 11)
                            }
                        }
                    },
                    SelectionBox = CreateSelectionBox(),
                }
            };
        }

        public SelectionBox CreateSelectionBox()
            => new SelectionBox
            {
                OperationStarted = OnOperationBegan,
                OperationEnded = OnOperationEnded,

                OnRotation = HandleRotation,
                OnScale = HandleScale,
                OnFlip = HandleFlip,
                OnReverse = HandleReverse,
            };

        /// <summary>
        /// Fired when a drag operation ends from the selection box.
        /// </summary>
        protected virtual void OnOperationBegan()
        {
            ChangeHandler?.BeginChange();
        }

        /// <summary>
        /// Fired when a drag operation begins from the selection box.
        /// </summary>
        protected virtual void OnOperationEnded()
        {
            ChangeHandler?.EndChange();
        }

        #region User Input Handling

        /// <summary>
        /// Handles the selected <see cref="DrawableHitObject"/>s being moved.
        /// </summary>
        /// <remarks>
        /// Just returning true is enough to allow <see cref="HitObject.StartTime"/> updates to take place.
        /// Custom implementation is only required if other attributes are to be considered, like changing columns.
        /// </remarks>
        /// <param name="moveEvent">The move event.</param>
        /// <returns>
        /// Whether any <see cref="DrawableHitObject"/>s could be moved.
        /// Returning true will also propagate StartTime changes provided by the closest <see cref="IPositionSnapProvider.SnapScreenSpacePositionToValidTime"/>.
        /// </returns>
        public virtual bool HandleMovement(MoveSelectionEvent moveEvent) => false;

        /// <summary>
        /// Handles the selected <see cref="DrawableHitObject"/>s being rotated.
        /// </summary>
        /// <param name="angle">The delta angle to apply to the selection.</param>
        /// <returns>Whether any <see cref="DrawableHitObject"/>s could be rotated.</returns>
        public virtual bool HandleRotation(float angle) => false;

        /// <summary>
        /// Handles the selected <see cref="DrawableHitObject"/>s being scaled.
        /// </summary>
        /// <param name="scale">The delta scale to apply, in playfield local coordinates.</param>
        /// <param name="anchor">The point of reference where the scale is originating from.</param>
        /// <returns>Whether any <see cref="DrawableHitObject"/>s could be scaled.</returns>
        public virtual bool HandleScale(Vector2 scale, Anchor anchor) => false;

        /// <summary>
        /// Handles the selected <see cref="DrawableHitObject"/>s being flipped.
        /// </summary>
        /// <param name="direction">The direction to flip</param>
        /// <returns>Whether any <see cref="DrawableHitObject"/>s could be flipped.</returns>
        public virtual bool HandleFlip(Direction direction) => false;

        /// <summary>
        /// Handles the selected <see cref="DrawableHitObject"/>s being reversed pattern-wise.
        /// </summary>
        /// <returns>Whether any <see cref="DrawableHitObject"/>s could be reversed.</returns>
        public virtual bool HandleReverse() => false;

        public bool OnPressed(PlatformAction action)
        {
            switch (action.ActionMethod)
            {
                case PlatformActionMethod.Delete:
                    deleteSelected();
                    return true;
            }

            return false;
        }

        public void OnReleased(PlatformAction action)
        {
        }

        #endregion

        #region Selection Handling

        /// <summary>
        /// Bind an action to deselect all selected blueprints.
        /// </summary>
        internal Action DeselectAll { private get; set; }

        /// <summary>
        /// Handle a blueprint becoming selected.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        internal void HandleSelected(SelectionBlueprint blueprint)
        {
            selectedBlueprints.Add(blueprint);

            // there are potentially multiple SelectionHandlers active, but we only want to add hitobjects to the selected list once.
            if (!EditorBeatmap.SelectedHitObjects.Contains(blueprint.HitObject))
                EditorBeatmap.SelectedHitObjects.Add(blueprint.HitObject);
        }

        /// <summary>
        /// Handle a blueprint becoming deselected.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        internal void HandleDeselected(SelectionBlueprint blueprint)
        {
            selectedBlueprints.Remove(blueprint);

            EditorBeatmap.SelectedHitObjects.Remove(blueprint.HitObject);
        }

        /// <summary>
        /// Handle a blueprint requesting selection.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        /// <param name="e">The mouse event responsible for selection.</param>
        /// <returns>Whether a selection was performed.</returns>
        internal bool HandleSelectionRequested(SelectionBlueprint blueprint, MouseButtonEvent e)
        {
            if (e.ShiftPressed && e.Button == MouseButton.Right)
            {
                handleQuickDeletion(blueprint);
                return false;
            }

            if (e.ControlPressed && e.Button == MouseButton.Left)
                blueprint.ToggleSelection();
            else
                ensureSelected(blueprint);

            return true;
        }

        private void handleQuickDeletion(SelectionBlueprint blueprint)
        {
            if (blueprint.HandleQuickDeletion())
                return;

            if (!blueprint.IsSelected)
                EditorBeatmap.Remove(blueprint.HitObject);
            else
                deleteSelected();
        }

        private void ensureSelected(SelectionBlueprint blueprint)
        {
            if (blueprint.IsSelected)
                return;

            DeselectAll?.Invoke();
            blueprint.Select();
        }

        private void deleteSelected()
        {
            EditorBeatmap.RemoveRange(selectedBlueprints.Select(b => b.HitObject));
        }

        #endregion

        #region Outline Display

        /// <summary>
        /// Updates whether this <see cref="SelectionHandler"/> is visible.
        /// </summary>
        private void updateVisibility()
        {
            int count = selectedBlueprints.Count;

            selectionDetailsText.Text = count > 0 ? count.ToString() : string.Empty;

            this.FadeTo(count > 0 ? 1 : 0);
            OnSelectionChanged();
        }

        /// <summary>
        /// Triggered whenever the set of selected objects changes.
        /// Should update the selection box's state to match supported operations.
        /// </summary>
        protected virtual void OnSelectionChanged()
        {
        }

        protected override void Update()
        {
            base.Update();

            if (selectedBlueprints.Count == 0)
                return;

            // Move the rectangle to cover the hitobjects
            var topLeft = new Vector2(float.MaxValue, float.MaxValue);
            var bottomRight = new Vector2(float.MinValue, float.MinValue);

            foreach (var blueprint in selectedBlueprints)
            {
                topLeft = Vector2.ComponentMin(topLeft, ToLocalSpace(blueprint.SelectionQuad.TopLeft));
                bottomRight = Vector2.ComponentMax(bottomRight, ToLocalSpace(blueprint.SelectionQuad.BottomRight));
            }

            topLeft -= new Vector2(5);
            bottomRight += new Vector2(5);

            content.Size = bottomRight - topLeft;
            content.Position = topLeft;
        }

        #endregion

        #region Sample Changes

        /// <summary>
        /// Adds a hit sample to all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void AddHitSample(string sampleName)
        {
            EditorBeatmap.BeginChange();

            foreach (var h in EditorBeatmap.SelectedHitObjects)
            {
                // Make sure there isn't already an existing sample
                if (h.Samples.Any(s => s.Name == sampleName))
                    continue;

                h.Samples.Add(new HitSampleInfo(sampleName));
            }

            EditorBeatmap.EndChange();
        }

        /// <summary>
        /// Set the new combo state of all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="state">Whether to set or unset.</param>
        /// <exception cref="InvalidOperationException">Throws if any selected object doesn't implement <see cref="IHasComboInformation"/></exception>
        public void SetNewCombo(bool state)
        {
            EditorBeatmap.BeginChange();

            foreach (var h in EditorBeatmap.SelectedHitObjects)
            {
                var comboInfo = h as IHasComboInformation;

                if (comboInfo == null || comboInfo.NewCombo == state) continue;

                comboInfo.NewCombo = state;
                EditorBeatmap.Update(h);
            }

            EditorBeatmap.EndChange();
        }

        /// <summary>
        /// Removes a hit sample from all selected <see cref="HitObject"/>s.
        /// </summary>
        /// <param name="sampleName">The name of the hit sample.</param>
        public void RemoveHitSample(string sampleName)
        {
            EditorBeatmap.BeginChange();

            foreach (var h in EditorBeatmap.SelectedHitObjects)
                h.SamplesBindable.RemoveAll(s => s.Name == sampleName);

            EditorBeatmap.EndChange();
        }

        #endregion

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
            foreach (var sampleName in HitSampleInfo.AllAdditions)
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

            // bring in updates from selection changes
            EditorBeatmap.HitObjectUpdated += _ => Scheduler.AddOnce(UpdateTernaryStates);
            EditorBeatmap.SelectedHitObjects.CollectionChanged += (sender, args) =>
            {
                Scheduler.AddOnce(updateVisibility);
                Scheduler.AddOnce(UpdateTernaryStates);
            };
        }

        /// <summary>
        /// Called when context menu ternary states may need to be recalculated (selection changed or hitobject updated).
        /// </summary>
        protected virtual void UpdateTernaryStates()
        {
            SelectionNewComboState.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects.OfType<IHasComboInformation>(), h => h.NewCombo);

            foreach (var (sampleName, bindable) in SelectionSampleStates)
            {
                bindable.Value = GetStateFromSelection(EditorBeatmap.SelectedHitObjects, h => h.Samples.Any(s => s.Name == sampleName));
            }
        }

        /// <summary>
        /// Given a selection target and a function of truth, retrieve the correct ternary state for display.
        /// </summary>
        protected TernaryState GetStateFromSelection<T>(IEnumerable<T> selection, Func<T, bool> func)
        {
            if (selection.Any(func))
                return selection.All(func) ? TernaryState.True : TernaryState.Indeterminate;

            return TernaryState.False;
        }

        #endregion

        #region Context Menu

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!selectedBlueprints.Any(b => b.IsHovered))
                    return Array.Empty<MenuItem>();

                var items = new List<MenuItem>();

                items.AddRange(GetContextMenuItemsForSelection(selectedBlueprints));

                if (selectedBlueprints.All(b => b.HitObject is IHasComboInformation))
                {
                    items.Add(new TernaryStateMenuItem("New combo") { State = { BindTarget = SelectionNewComboState } });
                }

                if (selectedBlueprints.Count == 1)
                    items.AddRange(selectedBlueprints[0].ContextMenuItems);

                items.AddRange(new[]
                {
                    new OsuMenuItem("Sound")
                    {
                        Items = SelectionSampleStates.Select(kvp =>
                            new TernaryStateMenuItem(kvp.Value.Description) { State = { BindTarget = kvp.Value } }).ToArray()
                    },
                    new OsuMenuItem("Delete", MenuItemType.Destructive, deleteSelected),
                });

                return items.ToArray();
            }
        }

        /// <summary>
        /// Provide context menu items relevant to current selection. Calling base is not required.
        /// </summary>
        /// <param name="selection">The current selection.</param>
        /// <returns>The relevant menu items.</returns>
        protected virtual IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint> selection)
            => Enumerable.Empty<MenuItem>();

        #endregion
    }
}
