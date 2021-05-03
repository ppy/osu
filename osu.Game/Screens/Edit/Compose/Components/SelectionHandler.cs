﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A component which outlines items and handles movement of selections.
    /// </summary>
    public abstract class SelectionHandler<T> : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
    {
        /// <summary>
        /// The currently selected blueprints.
        /// Should be used when operations are dealing directly with the visible blueprints.
        /// For more general selection operations, use <see cref="SelectedItems"/> instead.
        /// </summary>
        public IReadOnlyList<SelectionBlueprint<T>> SelectedBlueprints => selectedBlueprints;

        /// <summary>
        /// The currently selected items.
        /// </summary>
        public readonly BindableList<T> SelectedItems = new BindableList<T>();

        private readonly List<SelectionBlueprint<T>> selectedBlueprints;

        private Drawable content;

        private OsuSpriteText selectionDetailsText;

        protected SelectionBox SelectionBox { get; private set; }

        [Resolved(CanBeNull = true)]
        protected IEditorChangeHandler ChangeHandler { get; private set; }

        protected SelectionHandler()
        {
            selectedBlueprints = new List<SelectionBlueprint<T>>();

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
            Alpha = 0;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
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

            SelectedItems.CollectionChanged += (sender, args) =>
            {
                Scheduler.AddOnce(updateVisibility);
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
        /// Handles the selected items being moved.
        /// </summary>
        /// <remarks>
        /// Just returning true is enough to allow default movement to take place.
        /// Custom implementation is only required if other attributes are to be considered, like changing columns.
        /// </remarks>
        /// <param name="moveEvent">The move event.</param>
        /// <returns>
        /// Whether any items could be moved.
        /// </returns>
        public virtual bool HandleMovement(MoveSelectionEvent<T> moveEvent) => false;

        /// <summary>
        /// Handles the selected items being rotated.
        /// </summary>
        /// <param name="angle">The delta angle to apply to the selection.</param>
        /// <returns>Whether any items could be rotated.</returns>
        public virtual bool HandleRotation(float angle) => false;

        /// <summary>
        /// Handles the selected items being scaled.
        /// </summary>
        /// <param name="scale">The delta scale to apply, in local coordinates.</param>
        /// <param name="anchor">The point of reference where the scale is originating from.</param>
        /// <returns>Whether any items could be scaled.</returns>
        public virtual bool HandleScale(Vector2 scale, Anchor anchor) => false;

        /// <summary>
        /// Handles the selected items being flipped.
        /// </summary>
        /// <param name="direction">The direction to flip</param>
        /// <returns>Whether any items could be flipped.</returns>
        public virtual bool HandleFlip(Direction direction) => false;

        /// <summary>
        /// Handles the selected items being reversed pattern-wise.
        /// </summary>
        /// <returns>Whether any items could be reversed.</returns>
        public virtual bool HandleReverse() => false;

        public bool OnPressed(PlatformAction action)
        {
            switch (action.ActionMethod)
            {
                case PlatformActionMethod.Delete:
                    DeleteSelected();
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
        internal virtual void HandleSelected(SelectionBlueprint<T> blueprint)
        {
            // there are potentially multiple SelectionHandlers active, but we only want to add items to the selected list once.
            if (!SelectedItems.Contains(blueprint.Item))
                SelectedItems.Add(blueprint.Item);

            selectedBlueprints.Add(blueprint);
        }

        /// <summary>
        /// Handle a blueprint becoming deselected.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        internal virtual void HandleDeselected(SelectionBlueprint<T> blueprint)
        {
            SelectedItems.Remove(blueprint.Item);
            selectedBlueprints.Remove(blueprint);
        }

        /// <summary>
        /// Handle a blueprint requesting selection.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        /// <param name="e">The mouse event responsible for selection.</param>
        /// <returns>Whether a selection was performed.</returns>
        internal bool MouseDownSelectionRequested(SelectionBlueprint<T> blueprint, MouseButtonEvent e)
        {
            if (e.ShiftPressed && e.Button == MouseButton.Right)
            {
                handleQuickDeletion(blueprint);
                return true;
            }

            // while holding control, we only want to add to selection, not replace an existing selection.
            if (e.ControlPressed && e.Button == MouseButton.Left && !blueprint.IsSelected)
            {
                blueprint.ToggleSelection();
                return true;
            }

            return ensureSelected(blueprint);
        }

        /// <summary>
        /// Handle a blueprint requesting selection.
        /// </summary>
        /// <param name="blueprint">The blueprint.</param>
        /// <param name="e">The mouse event responsible for deselection.</param>
        /// <returns>Whether a deselection was performed.</returns>
        internal bool MouseUpSelectionRequested(SelectionBlueprint<T> blueprint, MouseButtonEvent e)
        {
            if (blueprint.IsSelected)
            {
                blueprint.ToggleSelection();
                return true;
            }

            return false;
        }

        private void handleQuickDeletion(SelectionBlueprint<T> blueprint)
        {
            if (blueprint.HandleQuickDeletion())
                return;

            if (!blueprint.IsSelected)
                DeleteItems(new[] { blueprint.Item });
            else
                DeleteSelected();
        }

        /// <summary>
        /// Called whenever the deletion of items has been requested.
        /// </summary>
        /// <param name="items">The items to be deleted.</param>
        protected abstract void DeleteItems(IEnumerable<T> items);

        /// <summary>
        /// Ensure the blueprint is in a selected state.
        /// </summary>
        /// <param name="blueprint">The blueprint to select.</param>
        /// <returns>Whether selection state was changed.</returns>
        private bool ensureSelected(SelectionBlueprint<T> blueprint)
        {
            if (blueprint.IsSelected)
                return false;

            DeselectAll?.Invoke();
            blueprint.Select();
            return true;
        }

        protected void DeleteSelected()
        {
            DeleteItems(selectedBlueprints.Select(b => b.Item));
        }

        #endregion

        #region Outline Display

        /// <summary>
        /// Updates whether this <see cref="SelectionHandler{T}"/> is visible.
        /// </summary>
        private void updateVisibility()
        {
            int count = SelectedItems.Count;

            selectionDetailsText.Text = count > 0 ? count.ToString() : string.Empty;

            this.FadeTo(count > 0 ? 1 : 0);
            OnSelectionChanged();
        }

        /// <summary>
        /// Triggered whenever the set of selected items changes.
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

            // Move the rectangle to cover the items
            RectangleF selectionRect = ToLocalSpace(selectedBlueprints[0].SelectionQuad).AABBFloat;

            for (int i = 1; i < selectedBlueprints.Count; i++)
                selectionRect = RectangleF.Union(selectionRect, ToLocalSpace(selectedBlueprints[i].SelectionQuad).AABBFloat);

            selectionRect = selectionRect.Inflate(5f);

            content.Position = selectionRect.Location;
            content.Size = selectionRect.Size;
        }

        #endregion

        #region Context Menu

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!SelectedBlueprints.Any(b => b.IsHovered))
                    return Array.Empty<MenuItem>();

                var items = new List<MenuItem>();

                items.AddRange(GetContextMenuItemsForSelection(SelectedBlueprints));

                if (SelectedBlueprints.Count == 1)
                    items.AddRange(SelectedBlueprints[0].ContextMenuItems);

                items.Add(new OsuMenuItem("Delete", MenuItemType.Destructive, DeleteSelected));

                return items.ToArray();
            }
        }

        /// <summary>
        /// Provide context menu items relevant to current selection. Calling base is not required.
        /// </summary>
        /// <param name="selection">The current selection.</param>
        /// <returns>The relevant menu items.</returns>
        protected virtual IEnumerable<MenuItem> GetContextMenuItemsForSelection(IEnumerable<SelectionBlueprint<T>> selection)
            => Enumerable.Empty<MenuItem>();

        #endregion
    }
}
