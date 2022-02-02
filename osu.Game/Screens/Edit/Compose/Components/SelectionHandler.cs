// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A component which outlines items and handles movement of selections.
    /// </summary>
    public abstract class SelectionHandler<T> : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IKeyBindingHandler<GlobalAction>, IHasContextMenu
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

        protected SelectionBox SelectionBox { get; private set; }

        [Resolved(CanBeNull = true)]
        protected IEditorChangeHandler ChangeHandler { get; private set; }

        protected SelectionHandler()
        {
            selectedBlueprints = new List<SelectionBlueprint<T>>();

            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = SelectionBox = CreateSelectionBox();

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
        /// <param name="direction">The direction to flip.</param>
        /// <param name="flipOverOrigin">Whether the flip operation should be global to the playfield's origin or local to the selected pattern.</param>
        /// <returns>Whether any items could be flipped.</returns>
        public virtual bool HandleFlip(Direction direction, bool flipOverOrigin) => false;

        /// <summary>
        /// Handles the selected items being reversed pattern-wise.
        /// </summary>
        /// <returns>Whether any items could be reversed.</returns>
        public virtual bool HandleReverse() => false;

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.EditorFlipHorizontally:
                    return HandleFlip(Direction.Horizontal, true);

                case GlobalAction.EditorFlipVertically:
                    return HandleFlip(Direction.Vertical, true);
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.Delete:
                    DeleteSelected();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
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
        internal virtual bool MouseDownSelectionRequested(SelectionBlueprint<T> blueprint, MouseButtonEvent e)
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
        /// Given a selection target and a function of truth, retrieve the correct ternary state for display.
        /// </summary>
        protected static TernaryState GetStateFromSelection<TObject>(IEnumerable<TObject> selection, Func<TObject, bool> func)
        {
            if (selection.Any(func))
                return selection.All(func) ? TernaryState.True : TernaryState.Indeterminate;

            return TernaryState.False;
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

            SelectionBox.Text = count > 0 ? count.ToString() : string.Empty;
            SelectionBox.FadeTo(count > 0 ? 1 : 0);

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

            SelectionBox.Position = selectionRect.Location;
            SelectionBox.Size = selectionRect.Size;
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

        #region Helper Methods

        /// <summary>
        /// Rotate a point around an arbitrary origin.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <param name="origin">The centre origin to rotate around.</param>
        /// <param name="angle">The angle to rotate (in degrees).</param>
        protected static Vector2 RotatePointAroundOrigin(Vector2 point, Vector2 origin, float angle)
        {
            angle = -angle;

            point.X -= origin.X;
            point.Y -= origin.Y;

            Vector2 ret;
            ret.X = point.X * MathF.Cos(MathUtils.DegreesToRadians(angle)) + point.Y * MathF.Sin(MathUtils.DegreesToRadians(angle));
            ret.Y = point.X * -MathF.Sin(MathUtils.DegreesToRadians(angle)) + point.Y * MathF.Cos(MathUtils.DegreesToRadians(angle));

            ret.X += origin.X;
            ret.Y += origin.Y;

            return ret;
        }

        /// <summary>
        /// Given a flip direction, a surrounding quad for all selected objects, and a position,
        /// will return the flipped position in screen space coordinates.
        /// </summary>
        protected static Vector2 GetFlippedPosition(Direction direction, Quad quad, Vector2 position)
        {
            var centre = quad.Centre;

            switch (direction)
            {
                case Direction.Horizontal:
                    position.X = centre.X - (position.X - centre.X);
                    break;

                case Direction.Vertical:
                    position.Y = centre.Y - (position.Y - centre.Y);
                    break;
            }

            return position;
        }

        /// <summary>
        /// Given a scale vector, a surrounding quad for all selected objects, and a position,
        /// will return the scaled position in screen space coordinates.
        /// </summary>
        protected static Vector2 GetScaledPosition(Anchor reference, Vector2 scale, Quad selectionQuad, Vector2 position)
        {
            // adjust the direction of scale depending on which side the user is dragging.
            float xOffset = ((reference & Anchor.x0) > 0) ? -scale.X : 0;
            float yOffset = ((reference & Anchor.y0) > 0) ? -scale.Y : 0;

            // guard against no-ops and NaN.
            if (scale.X != 0 && selectionQuad.Width > 0)
                position.X = selectionQuad.TopLeft.X + xOffset + (position.X - selectionQuad.TopLeft.X) / selectionQuad.Width * (selectionQuad.Width + scale.X);

            if (scale.Y != 0 && selectionQuad.Height > 0)
                position.Y = selectionQuad.TopLeft.Y + yOffset + (position.Y - selectionQuad.TopLeft.Y) / selectionQuad.Height * (selectionQuad.Height + scale.Y);

            return position;
        }

        /// <summary>
        /// Returns a quad surrounding the provided points.
        /// </summary>
        /// <param name="points">The points to calculate a quad for.</param>
        protected static Quad GetSurroundingQuad(IEnumerable<Vector2> points)
        {
            if (!points.Any())
                return new Quad();

            Vector2 minPosition = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 maxPosition = new Vector2(float.MinValue, float.MinValue);

            // Go through all hitobjects to make sure they would remain in the bounds of the editor after movement, before any movement is attempted
            foreach (var p in points)
            {
                minPosition = Vector2.ComponentMin(minPosition, p);
                maxPosition = Vector2.ComponentMax(maxPosition, p);
            }

            Vector2 size = maxPosition - minPosition;

            return new Quad(minPosition.X, minPosition.Y, size.X, size.Y);
        }

        #endregion
    }
}
