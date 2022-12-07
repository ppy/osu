// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A container which provides a "blueprint" display of items.
    /// Includes selection and manipulation support via a <see cref="Components.SelectionHandler{T}"/>.
    /// </summary>
    public abstract partial class BlueprintContainer<T> : CompositeDrawable, IKeyBindingHandler<PlatformAction>
        where T : class
    {
        protected DragBox DragBox { get; private set; }

        public Container<SelectionBlueprint<T>> SelectionBlueprints { get; private set; }

        protected SelectionHandler<T> SelectionHandler { get; private set; }

        private readonly Dictionary<T, SelectionBlueprint<T>> blueprintMap = new Dictionary<T, SelectionBlueprint<T>>();

        [Resolved(canBeNull: true)]
        private IPositionSnapProvider snapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        protected readonly BindableList<T> SelectedItems = new BindableList<T>();

        protected BlueprintContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SelectedItems.CollectionChanged += (_, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (object o in args.NewItems)
                        {
                            if (blueprintMap.TryGetValue((T)o, out var blueprint))
                                blueprint.Select();
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (object o in args.OldItems)
                        {
                            if (blueprintMap.TryGetValue((T)o, out var blueprint))
                                blueprint.Deselect();
                        }

                        break;
                }
            };

            SelectionHandler = CreateSelectionHandler();
            SelectionHandler.DeselectAll = DeselectAll;
            SelectionHandler.SelectedItems.BindTo(SelectedItems);

            AddRangeInternal(new[]
            {
                DragBox = CreateDragBox(),
                SelectionHandler,
                SelectionBlueprints = CreateSelectionBlueprintContainer(),
                SelectionHandler.CreateProxy(),
                DragBox.CreateProxy().With(p => p.Depth = float.MinValue)
            });
        }

        protected virtual Container<SelectionBlueprint<T>> CreateSelectionBlueprintContainer() => new Container<SelectionBlueprint<T>> { RelativeSizeAxes = Axes.Both };

        /// <summary>
        /// Creates a <see cref="Components.SelectionHandler{T}"/> which outlines items and handles movement of selections.
        /// </summary>
        protected abstract SelectionHandler<T> CreateSelectionHandler();

        /// <summary>
        /// Creates a <see cref="SelectionBlueprint{T}"/> for a specific item.
        /// </summary>
        /// <param name="item">The item to create the overlay for.</param>
        [CanBeNull]
        protected virtual SelectionBlueprint<T> CreateBlueprintFor(T item) => null;

        protected virtual DragBox CreateDragBox() => new DragBox();

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            bool selectionPerformed = performMouseDownActions(e);
            bool movementPossible = prepareSelectionMovement();

            // check if selection has occurred
            if (selectionPerformed)
            {
                // only unmodified right click should show context menu
                bool shouldShowContextMenu = e.Button == MouseButton.Right && !e.ShiftPressed && !e.AltPressed && !e.SuperPressed;

                // stop propagation if not showing context menu
                return !shouldShowContextMenu;
            }

            // even if a selection didn't occur, a drag event may still move the selection.
            return e.Button == MouseButton.Left && movementPossible;
        }

        protected SelectionBlueprint<T> ClickedBlueprint { get; private set; }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            // store for double-click handling
            ClickedBlueprint = SelectionHandler.SelectedBlueprints.FirstOrDefault(b => b.IsHovered);

            // Deselection should only occur if no selected blueprints are hovered
            // A special case for when a blueprint was selected via this click is added since OnClick() may occur outside the item and should not trigger deselection
            if (endClickSelection(e) || ClickedBlueprint != null)
                return true;

            DeselectAll();
            return true;
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            // ensure the blueprint which was hovered for the first click is still the hovered blueprint.
            if (ClickedBlueprint == null || SelectionHandler.SelectedBlueprints.FirstOrDefault(b => b.IsHovered) != ClickedBlueprint)
                return false;

            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            // Special case for when a drag happened instead of a click
            Schedule(() =>
            {
                endClickSelection(e);
                clickSelectionBegan = false;
                isDraggingBlueprint = false;
            });

            finishSelectionMovement();
        }

        private MouseButtonEvent lastDragEvent;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            lastDragEvent = e;

            if (movementBlueprints != null)
            {
                isDraggingBlueprint = true;
                changeHandler?.BeginChange();
                return true;
            }

            DragBox.HandleDrag(e);
            DragBox.Show();
            return true;
        }

        protected override void OnDrag(DragEvent e)
        {
            lastDragEvent = e;

            moveCurrentSelection(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            lastDragEvent = null;

            if (isDraggingBlueprint)
            {
                DragOperationCompleted();
                changeHandler?.EndChange();
            }

            DragBox.Hide();
        }

        protected override void Update()
        {
            base.Update();

            if (lastDragEvent != null && DragBox.State == Visibility.Visible)
            {
                lastDragEvent.Target = this;
                DragBox.HandleDrag(lastDragEvent);
                UpdateSelectionFromDragBox();
            }
        }

        /// <summary>
        /// Called whenever a drag operation completes, before any change transaction is committed.
        /// </summary>
        protected virtual void DragOperationCompleted()
        {
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (!SelectionHandler.SelectedBlueprints.Any())
                        return false;

                    DeselectAll();
                    return true;
            }

            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case PlatformAction.SelectAll:
                    SelectAll();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        #region Blueprint Addition/Removal

        protected virtual void AddBlueprintFor(T item)
        {
            if (blueprintMap.ContainsKey(item))
                return;

            var blueprint = CreateBlueprintFor(item);
            if (blueprint == null)
                return;

            blueprintMap[item] = blueprint;

            blueprint.Selected += OnBlueprintSelected;
            blueprint.Deselected += OnBlueprintDeselected;

            SelectionBlueprints.Add(blueprint);

            if (SelectionHandler.SelectedItems.Contains(item))
                blueprint.Select();

            OnBlueprintAdded(blueprint.Item);
        }

        protected void RemoveBlueprintFor(T item)
        {
            if (!blueprintMap.Remove(item, out var blueprint))
                return;

            blueprint.Deselect();
            blueprint.Selected -= OnBlueprintSelected;
            blueprint.Deselected -= OnBlueprintDeselected;

            SelectionBlueprints.Remove(blueprint, true);

            if (movementBlueprints?.Contains(blueprint) == true)
                finishSelectionMovement();

            OnBlueprintRemoved(blueprint.Item);
        }

        /// <summary>
        /// Called after an item's blueprint has been added.
        /// </summary>
        /// <param name="item">The item for which the blueprint has been added.</param>
        protected virtual void OnBlueprintAdded(T item)
        {
        }

        /// <summary>
        /// Called after an item's blueprint has been removed.
        /// </summary>
        /// <param name="item">The item for which the blueprint has been removed.</param>
        protected virtual void OnBlueprintRemoved(T item)
        {
        }

        /// <summary>
        /// Retrieves an item's blueprint.
        /// </summary>
        /// <param name="item">The item to retrieve the blueprint of.</param>
        /// <returns>The blueprint.</returns>
        protected SelectionBlueprint<T> GetBlueprintFor(T item) => blueprintMap[item];

        #endregion

        #region Selection

        /// <summary>
        /// Whether a blueprint was selected by a previous click event.
        /// </summary>
        private bool clickSelectionBegan;

        /// <summary>
        /// Attempts to select any hovered blueprints.
        /// </summary>
        /// <param name="e">The input event that triggered this selection.</param>
        /// <returns>Whether a selection was performed.</returns>
        private bool performMouseDownActions(MouseButtonEvent e)
        {
            // Iterate from the top of the input stack (blueprints closest to the front of the screen first).
            // Priority is given to already-selected blueprints.
            foreach (SelectionBlueprint<T> blueprint in SelectionBlueprints.AliveChildren.Reverse().OrderByDescending(b => b.IsSelected))
            {
                if (!blueprint.IsHovered) continue;

                return clickSelectionBegan = SelectionHandler.MouseDownSelectionRequested(blueprint, e);
            }

            return false;
        }

        /// <summary>
        /// Finishes the current blueprint selection.
        /// </summary>
        /// <param name="e">The mouse event which triggered end of selection.</param>
        /// <returns>Whether a click selection was active.</returns>
        private bool endClickSelection(MouseButtonEvent e)
        {
            if (!clickSelectionBegan && !isDraggingBlueprint)
            {
                // if a selection didn't occur, we may want to trigger a deselection.
                if (e.ControlPressed && e.Button == MouseButton.Left)
                {
                    // Iterate from the top of the input stack (blueprints closest to the front of the screen first).
                    // Priority is given to already-selected blueprints.
                    foreach (SelectionBlueprint<T> blueprint in SelectionBlueprints.AliveChildren.Reverse().OrderByDescending(b => b.IsSelected))
                    {
                        if (!blueprint.IsHovered) continue;

                        return clickSelectionBegan = SelectionHandler.MouseUpSelectionRequested(blueprint, e);
                    }
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Select all blueprints in a selection area specified by <see cref="DragBox"/>.
        /// </summary>
        protected virtual void UpdateSelectionFromDragBox()
        {
            var quad = DragBox.Box.ScreenSpaceDrawQuad;

            foreach (var blueprint in SelectionBlueprints)
            {
                switch (blueprint.State)
                {
                    case SelectionState.Selected:
                        // Selection is preserved even after blueprint becomes dead.
                        if (!quad.Contains(blueprint.ScreenSpaceSelectionPoint))
                            blueprint.Deselect();
                        break;

                    case SelectionState.NotSelected:
                        if (blueprint.IsAlive && blueprint.IsPresent && quad.Contains(blueprint.ScreenSpaceSelectionPoint))
                            blueprint.Select();
                        break;
                }
            }
        }

        /// <summary>
        /// Select all currently-present items.
        /// </summary>
        protected abstract void SelectAll();

        /// <summary>
        /// Deselect all selected items.
        /// </summary>
        protected void DeselectAll() => SelectedItems.Clear();

        protected virtual void OnBlueprintSelected(SelectionBlueprint<T> blueprint)
        {
            SelectionHandler.HandleSelected(blueprint);
            SelectionBlueprints.ChangeChildDepth(blueprint, 1);
        }

        protected virtual void OnBlueprintDeselected(SelectionBlueprint<T> blueprint)
        {
            SelectionBlueprints.ChangeChildDepth(blueprint, 0);
            SelectionHandler.HandleDeselected(blueprint);
        }

        #endregion

        #region Selection Movement

        private Vector2[] movementBlueprintOriginalPositions;
        private SelectionBlueprint<T>[] movementBlueprints;
        private bool isDraggingBlueprint;

        /// <summary>
        /// Attempts to begin the movement of any selected blueprints.
        /// </summary>
        /// <returns>Whether a movement is possible.</returns>
        private bool prepareSelectionMovement()
        {
            if (!SelectionHandler.SelectedBlueprints.Any())
                return false;

            // Any selected blueprint that is hovered can begin the movement of the group, however only the first item (according to SortForMovement) is used for movement.
            // A special case is added for when a click selection occurred before the drag
            if (!clickSelectionBegan && !SelectionHandler.SelectedBlueprints.Any(b => b.IsHovered))
                return false;

            // Movement is tracked from the blueprint of the earliest item, since it only makes sense to distance snap from that item
            movementBlueprints = SortForMovement(SelectionHandler.SelectedBlueprints).ToArray();
            movementBlueprintOriginalPositions = movementBlueprints.Select(m => m.ScreenSpaceSelectionPoint).ToArray();
            return true;
        }

        /// <summary>
        /// Apply sorting of selected blueprints before performing movement. Generally used to surface the "main" item to the beginning of the collection.
        /// </summary>
        /// <param name="blueprints">The blueprints to be moved.</param>
        /// <returns>Sorted blueprints.</returns>
        protected virtual IEnumerable<SelectionBlueprint<T>> SortForMovement(IReadOnlyList<SelectionBlueprint<T>> blueprints) => blueprints;

        /// <summary>
        /// Moves the current selected blueprints.
        /// </summary>
        /// <param name="e">The <see cref="DragEvent"/> defining the movement event.</param>
        /// <returns>Whether a movement was active.</returns>
        private bool moveCurrentSelection(DragEvent e)
        {
            if (movementBlueprints == null)
                return false;

            Debug.Assert(movementBlueprintOriginalPositions != null);

            Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            if (snapProvider != null)
            {
                // check for positional snap for every object in selection (for things like object-object snapping)
                for (int i = 0; i < movementBlueprintOriginalPositions.Length; i++)
                {
                    Vector2 originalPosition = movementBlueprintOriginalPositions[i];
                    var testPosition = originalPosition + distanceTravelled;

                    var positionalResult = snapProvider.FindSnappedPositionAndTime(testPosition, SnapType.NearbyObjects);

                    if (positionalResult.ScreenSpacePosition == testPosition) continue;

                    var delta = positionalResult.ScreenSpacePosition - movementBlueprints[i].ScreenSpaceSelectionPoint;

                    // attempt to move the objects, and abort any time based snapping if we can.
                    if (SelectionHandler.HandleMovement(new MoveSelectionEvent<T>(movementBlueprints[i], delta)))
                        return true;
                }
            }

            // if no positional snapping could be performed, try unrestricted snapping from the earliest
            // item in the selection.

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = movementBlueprintOriginalPositions.First() + distanceTravelled;

            // Retrieve a snapped position.
            var result = snapProvider?.FindSnappedPositionAndTime(movePosition, ~SnapType.NearbyObjects);

            if (result == null)
            {
                return SelectionHandler.HandleMovement(new MoveSelectionEvent<T>(movementBlueprints.First(), movePosition - movementBlueprints.First().ScreenSpaceSelectionPoint));
            }

            return ApplySnapResult(movementBlueprints, result);
        }

        protected virtual bool ApplySnapResult(SelectionBlueprint<T>[] blueprints, SnapResult result) =>
            SelectionHandler.HandleMovement(new MoveSelectionEvent<T>(blueprints.First(), result.ScreenSpacePosition - blueprints.First().ScreenSpaceSelectionPoint));

        /// <summary>
        /// Finishes the current movement of selected blueprints.
        /// </summary>
        /// <returns>Whether a movement was active.</returns>
        private bool finishSelectionMovement()
        {
            if (movementBlueprints == null)
                return false;

            movementBlueprintOriginalPositions = null;
            movementBlueprints = null;

            return true;
        }

        #endregion
    }
}
