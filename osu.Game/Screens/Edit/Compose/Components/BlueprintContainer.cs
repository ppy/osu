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
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A container which provides a "blueprint" display of items.
    /// Includes selection and manipulation support via a <see cref="Components.SelectionHandler{T}"/>.
    /// </summary>
    public abstract partial class BlueprintContainer<T> : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IKeyBindingHandler<GlobalAction>
        where T : class
    {
        protected DragBox DragBox { get; private set; }

        public Container<SelectionBlueprint<T>> SelectionBlueprints { get; private set; }

        public SelectionHandler<T> SelectionHandler { get; private set; }

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
                        Debug.Assert(args.NewItems != null);

                        foreach (object o in args.NewItems)
                        {
                            if (blueprintMap.TryGetValue((T)o, out var blueprint))
                                blueprint.Select();
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        Debug.Assert(args.OldItems != null);

                        foreach (object o in args.OldItems)
                        {
                            if (blueprintMap.TryGetValue((T)o, out var blueprint))
                                blueprint.Deselect();
                        }

                        break;
                }
            };

            SelectionHandler = CreateSelectionHandler();
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

            doubleClickHandled = true;
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            // Special case for when a drag happened instead of a click
            Schedule(() =>
            {
                endClickSelection(e);
                clickSelectionHandled = false;
                doubleClickHandled = false;
                isDraggingBlueprint = false;
                wasDragStarted = false;
            });

            finishSelectionMovement();
        }

        private MouseButtonEvent lastDragEvent;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            lastDragEvent = e;
            wasDragStarted = true;

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

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    if (SelectedItems.Count > 0)
                    {
                        DeselectAll();
                        return true;
                    }

                    break;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
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
        private bool clickSelectionHandled;

        /// <summary>
        /// Whether a blueprint was double-clicked since last mouse down.
        /// </summary>
        private bool doubleClickHandled;

        /// <summary>
        /// Whether the selected blueprint(s) were already selected on mouse down. Generally used to perform selection cycling on mouse up in such a case.
        /// </summary>
        private bool selectedBlueprintAlreadySelectedOnMouseDown;

        /// <summary>
        /// Sorts the supplied <paramref name="blueprints"/> by the order of preference when making a selection.
        /// Blueprints at the start of the list will be prioritised over later items if the selection requested is ambiguous due to spatial overlap.
        /// </summary>
        protected virtual IEnumerable<SelectionBlueprint<T>> ApplySelectionOrder(IEnumerable<SelectionBlueprint<T>> blueprints) => blueprints.Reverse();

        /// <summary>
        /// Attempts to select any hovered blueprints.
        /// </summary>
        /// <param name="e">The input event that triggered this selection.</param>
        /// <returns>Whether a selection was performed.</returns>
        private bool performMouseDownActions(MouseButtonEvent e)
        {
            // Iterate from the top of the input stack (blueprints closest to the front of the screen first).
            // Priority is given to already-selected blueprints.
            foreach (SelectionBlueprint<T> blueprint in SelectionBlueprints.AliveChildren.Where(b => b.IsSelected))
            {
                if (runForBlueprint(blueprint))
                    return true;
            }

            foreach (SelectionBlueprint<T> blueprint in ApplySelectionOrder(SelectionBlueprints.AliveChildren))
            {
                if (runForBlueprint(blueprint))
                    return true;
            }

            return false;

            bool runForBlueprint(SelectionBlueprint<T> blueprint)
            {
                if (!blueprint.IsHovered) return false;

                selectedBlueprintAlreadySelectedOnMouseDown = blueprint.State == SelectionState.Selected;
                clickSelectionHandled = SelectionHandler.MouseDownSelectionRequested(blueprint, e);
                return true;
            }
        }

        /// <summary>
        /// Finishes the current blueprint selection.
        /// </summary>
        /// <param name="e">The mouse event which triggered end of selection.</param>
        /// <returns>Whether a click selection was active.</returns>
        private bool endClickSelection(MouseButtonEvent e)
        {
            // If already handled a selection, double-click, or drag, we don't want to perform a mouse up / click action.
            if (clickSelectionHandled || doubleClickHandled || isDraggingBlueprint) return true;

            if (e.Button != MouseButton.Left) return false;

            if (e.ControlPressed)
            {
                // if a selection didn't occur, we may want to trigger a deselection.

                // Iterate from the top of the input stack (blueprints closest to the front of the screen first).
                // Priority is given to already-selected blueprints.
                foreach (SelectionBlueprint<T> blueprint in SelectionBlueprints.AliveChildren.Where(b => b.IsHovered).OrderByDescending(b => b.IsSelected))
                    return clickSelectionHandled = SelectionHandler.MouseUpSelectionRequested(blueprint, e);

                return false;
            }

            if (!wasDragStarted && selectedBlueprintAlreadySelectedOnMouseDown && SelectedItems.Count == 1)
            {
                // If a click occurred and was handled by the currently selected blueprint but didn't result in a drag,
                // cycle between other blueprints which are also under the cursor.

                // The depth of blueprints is constantly changing (see above where selected blueprints are brought to the front).
                // For this logic, we want a stable sort order so we can correctly cycle, thus using the blueprintMap instead.
                IEnumerable<SelectionBlueprint<T>> cyclingSelectionBlueprints = ApplySelectionOrder(blueprintMap.Values);

                // If there's already a selection, let's start from the blueprint after the selection.
                cyclingSelectionBlueprints = cyclingSelectionBlueprints.SkipWhile(b => !b.IsSelected).Skip(1);

                // Add the blueprints from before the selection to the end of the enumerable to allow for cyclic selection.
                cyclingSelectionBlueprints = cyclingSelectionBlueprints.Concat(ApplySelectionOrder(blueprintMap.Values).TakeWhile(b => !b.IsSelected));

                foreach (SelectionBlueprint<T> blueprint in cyclingSelectionBlueprints)
                {
                    if (!blueprint.IsHovered) continue;

                    // We are performing a mouse up, but selection handlers perform selection on mouse down, so we need to call that instead.
                    return clickSelectionHandled = SelectionHandler.MouseDownSelectionRequested(blueprint, e);
                }
            }

            return false;
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
                        if (blueprint.IsSelectable && quad.Contains(blueprint.ScreenSpaceSelectionPoint))
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

        private Vector2[][] movementBlueprintsOriginalPositions;
        private SelectionBlueprint<T>[] movementBlueprints;

        /// <summary>
        /// Whether a blueprint is currently being dragged.
        /// </summary>
        private bool isDraggingBlueprint;

        /// <summary>
        /// Whether a drag operation was started at all.
        /// </summary>
        private bool wasDragStarted;

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
            if (!clickSelectionHandled && !SelectionHandler.SelectedBlueprints.Any(b => b.IsHovered))
                return false;

            // Movement is tracked from the blueprint of the earliest item, since it only makes sense to distance snap from that item
            movementBlueprints = SortForMovement(SelectionHandler.SelectedBlueprints).ToArray();
            movementBlueprintsOriginalPositions = movementBlueprints.Select(m => m.ScreenSpaceSnapPoints).ToArray();
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

            Debug.Assert(movementBlueprintsOriginalPositions != null);

            Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            if (snapProvider != null)
            {
                for (int i = 0; i < movementBlueprints.Length; i++)
                {
                    if (checkSnappingBlueprintToNearbyObjects(movementBlueprints[i], distanceTravelled, movementBlueprintsOriginalPositions[i]))
                        return true;
                }
            }

            // if no positional snapping could be performed, try unrestricted snapping from the earliest
            // item in the selection.

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = movementBlueprintsOriginalPositions.First().First() + distanceTravelled;

            // Retrieve a snapped position.
            var result = snapProvider?.FindSnappedPositionAndTime(movePosition, ~SnapType.NearbyObjects);

            if (result == null)
            {
                return SelectionHandler.HandleMovement(new MoveSelectionEvent<T>(movementBlueprints.First(), movePosition - movementBlueprints.First().ScreenSpaceSelectionPoint));
            }

            return ApplySnapResult(movementBlueprints, result);
        }

        /// <summary>
        /// Check for positional snap for given blueprint.
        /// </summary>
        /// <param name="blueprint">The blueprint to check for snapping.</param>
        /// <param name="distanceTravelled">Distance travelled since start of dragging action.</param>
        /// <param name="originalPositions">The snap positions of blueprint before start of dragging action.</param>
        /// <returns>Whether an object to snap to was found.</returns>
        private bool checkSnappingBlueprintToNearbyObjects(SelectionBlueprint<T> blueprint, Vector2 distanceTravelled, Vector2[] originalPositions)
        {
            var currentPositions = blueprint.ScreenSpaceSnapPoints;

            for (int i = 0; i < originalPositions.Length; i++)
            {
                Vector2 originalPosition = originalPositions[i];
                var testPosition = originalPosition + distanceTravelled;

                var positionalResult = snapProvider.FindSnappedPositionAndTime(testPosition, SnapType.NearbyObjects);

                if (positionalResult.ScreenSpacePosition == testPosition) continue;

                var delta = positionalResult.ScreenSpacePosition - currentPositions[i];

                // attempt to move the objects, and abort any time based snapping if we can.
                if (SelectionHandler.HandleMovement(new MoveSelectionEvent<T>(blueprint, delta)))
                    return true;
            }

            return false;
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

            movementBlueprintsOriginalPositions = null;
            movementBlueprints = null;

            return true;
        }

        #endregion
    }
}
