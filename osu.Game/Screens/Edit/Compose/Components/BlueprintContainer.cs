// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A container which provides a "blueprint" display of hitobjects.
    /// Includes selection and manipulation support via a <see cref="Components.SelectionHandler"/>.
    /// </summary>
    public abstract class BlueprintContainer : CompositeDrawable, IKeyBindingHandler<PlatformAction>
    {
        protected DragBox DragBox { get; private set; }

        public Container<SelectionBlueprint> SelectionBlueprints { get; private set; }

        protected SelectionHandler SelectionHandler { get; private set; }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        [Resolved]
        private EditorClock editorClock { get; set; }

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; }

        private readonly BindableList<HitObject> selectedHitObjects = new BindableList<HitObject>();

        [Resolved(canBeNull: true)]
        private IPositionSnapProvider snapProvider { get; set; }

        protected BlueprintContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            SelectionHandler = CreateSelectionHandler();
            SelectionHandler.DeselectAll = deselectAll;

            AddRangeInternal(new[]
            {
                DragBox = CreateDragBox(selectBlueprintsFromDragRectangle),
                SelectionHandler,
                SelectionBlueprints = CreateSelectionBlueprintContainer(),
                SelectionHandler.CreateProxy(),
                DragBox.CreateProxy().With(p => p.Depth = float.MinValue)
            });

            foreach (var obj in Beatmap.HitObjects)
                AddBlueprintFor(obj);

            selectedHitObjects.BindTo(Beatmap.SelectedHitObjects);
            selectedHitObjects.CollectionChanged += (selectedObjects, args) =>
            {
                switch (args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var o in args.NewItems)
                            SelectionBlueprints.FirstOrDefault(b => b.HitObject == o)?.Select();
                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var o in args.OldItems)
                            SelectionBlueprints.FirstOrDefault(b => b.HitObject == o)?.Deselect();

                        break;
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.HitObjectAdded += AddBlueprintFor;
            Beatmap.HitObjectRemoved += removeBlueprintFor;
        }

        protected virtual Container<SelectionBlueprint> CreateSelectionBlueprintContainer() =>
            new Container<SelectionBlueprint> { RelativeSizeAxes = Axes.Both };

        /// <summary>
        /// Creates a <see cref="Components.SelectionHandler"/> which outlines <see cref="DrawableHitObject"/>s and handles movement of selections.
        /// </summary>
        protected virtual SelectionHandler CreateSelectionHandler() => new SelectionHandler();

        /// <summary>
        /// Creates a <see cref="SelectionBlueprint"/> for a specific <see cref="DrawableHitObject"/>.
        /// </summary>
        /// <param name="hitObject">The <see cref="DrawableHitObject"/> to create the overlay for.</param>
        protected virtual SelectionBlueprint CreateBlueprintFor(HitObject hitObject) => null;

        protected virtual DragBox CreateDragBox(Action<RectangleF> performSelect) => new DragBox(performSelect);

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!beginClickSelection(e)) return true;

            prepareSelectionMovement();

            return e.Button == MouseButton.Left;
        }

        private SelectionBlueprint clickedBlueprint;

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            // store for double-click handling
            clickedBlueprint = SelectionHandler.SelectedBlueprints.FirstOrDefault(b => b.IsHovered);

            // Deselection should only occur if no selected blueprints are hovered
            // A special case for when a blueprint was selected via this click is added since OnClick() may occur outside the hitobject and should not trigger deselection
            if (endClickSelection() || clickedBlueprint != null)
                return true;

            deselectAll();
            return true;
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            // ensure the blueprint which was hovered for the first click is still the hovered blueprint.
            if (clickedBlueprint == null || SelectionHandler.SelectedBlueprints.FirstOrDefault(b => b.IsHovered) != clickedBlueprint)
                return false;

            editorClock?.SeekTo(clickedBlueprint.HitObject.StartTime);
            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            // Special case for when a drag happened instead of a click
            Schedule(() => endClickSelection());

            finishSelectionMovement();
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            if (movementBlueprint != null)
            {
                isDraggingBlueprint = true;
                changeHandler?.BeginChange();
                return true;
            }

            if (DragBox.HandleDrag(e))
            {
                DragBox.Show();
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e)
        {
            if (e.Button == MouseButton.Right)
                return;

            if (DragBox.State == Visibility.Visible)
                DragBox.HandleDrag(e);

            moveCurrentSelection(e);
        }

        protected override void OnDragEnd(DragEndEvent e)
        {
            if (e.Button == MouseButton.Right)
                return;

            if (isDraggingBlueprint)
            {
                // handle positional change etc.
                foreach (var obj in selectedHitObjects)
                    Beatmap.Update(obj);

                changeHandler?.EndChange();
                isDraggingBlueprint = false;
            }

            if (DragBox.State == Visibility.Visible)
                DragBox.Hide();
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (!SelectionHandler.SelectedBlueprints.Any())
                        return false;

                    deselectAll();
                    return true;
            }

            return false;
        }

        public bool OnPressed(PlatformAction action)
        {
            switch (action.ActionType)
            {
                case PlatformActionType.SelectAll:
                    selectAll();
                    return true;
            }

            return false;
        }

        public void OnReleased(PlatformAction action)
        {
        }

        #region Blueprint Addition/Removal

        private void removeBlueprintFor(HitObject hitObject)
        {
            var blueprint = SelectionBlueprints.SingleOrDefault(m => m.HitObject == hitObject);
            if (blueprint == null)
                return;

            blueprint.Deselect();

            blueprint.Selected -= onBlueprintSelected;
            blueprint.Deselected -= onBlueprintDeselected;

            SelectionBlueprints.Remove(blueprint);

            if (movementBlueprint == blueprint)
                finishSelectionMovement();
        }

        protected virtual void AddBlueprintFor(HitObject hitObject)
        {
            var blueprint = CreateBlueprintFor(hitObject);
            if (blueprint == null)
                return;

            blueprint.Selected += onBlueprintSelected;
            blueprint.Deselected += onBlueprintDeselected;

            if (Beatmap.SelectedHitObjects.Contains(hitObject))
                blueprint.Select();

            SelectionBlueprints.Add(blueprint);
        }

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
        private bool beginClickSelection(MouseButtonEvent e)
        {
            Debug.Assert(!clickSelectionBegan);

            bool selectedPerformed = true;

            foreach (SelectionBlueprint blueprint in SelectionBlueprints.AliveChildren)
            {
                if (blueprint.IsHovered)
                {
                    selectedPerformed &= SelectionHandler.HandleSelectionRequested(blueprint, e.CurrentState);
                    clickSelectionBegan = true;
                    break;
                }
            }

            return selectedPerformed;
        }

        /// <summary>
        /// Finishes the current blueprint selection.
        /// </summary>
        /// <returns>Whether a click selection was active.</returns>
        private bool endClickSelection()
        {
            if (!clickSelectionBegan)
                return false;

            clickSelectionBegan = false;
            return true;
        }

        /// <summary>
        /// Select all masks in a given rectangle selection area.
        /// </summary>
        /// <param name="rect">The rectangle to perform a selection on in screen-space coordinates.</param>
        private void selectBlueprintsFromDragRectangle(RectangleF rect)
        {
            foreach (var blueprint in SelectionBlueprints)
            {
                // only run when utmost necessary to avoid unnecessary rect computations.
                bool isValidForSelection() => blueprint.IsAlive && blueprint.IsPresent && rect.Contains(blueprint.ScreenSpaceSelectionPoint);

                switch (blueprint.State)
                {
                    case SelectionState.NotSelected:
                        if (isValidForSelection())
                            blueprint.Select();
                        break;

                    case SelectionState.Selected:
                        // if the editor is playing, we generally don't want to deselect objects even if outside the selection area.
                        if (!editorClock.IsRunning && !isValidForSelection())
                            blueprint.Deselect();
                        break;
                }
            }
        }

        /// <summary>
        /// Selects all <see cref="SelectionBlueprint"/>s.
        /// </summary>
        private void selectAll() => SelectionBlueprints.ToList().ForEach(m => m.Select());

        /// <summary>
        /// Deselects all selected <see cref="SelectionBlueprint"/>s.
        /// </summary>
        private void deselectAll() => SelectionHandler.SelectedBlueprints.ToList().ForEach(m => m.Deselect());

        private void onBlueprintSelected(SelectionBlueprint blueprint)
        {
            SelectionHandler.HandleSelected(blueprint);
            SelectionBlueprints.ChangeChildDepth(blueprint, 1);
        }

        private void onBlueprintDeselected(SelectionBlueprint blueprint)
        {
            SelectionHandler.HandleDeselected(blueprint);
            SelectionBlueprints.ChangeChildDepth(blueprint, 0);
        }

        #endregion

        #region Selection Movement

        private Vector2? movementBlueprintOriginalPosition;
        private SelectionBlueprint movementBlueprint;
        private bool isDraggingBlueprint;

        /// <summary>
        /// Attempts to begin the movement of any selected blueprints.
        /// </summary>
        private void prepareSelectionMovement()
        {
            if (!SelectionHandler.SelectedBlueprints.Any())
                return;

            // Any selected blueprint that is hovered can begin the movement of the group, however only the earliest hitobject is used for movement
            // A special case is added for when a click selection occurred before the drag
            if (!clickSelectionBegan && !SelectionHandler.SelectedBlueprints.Any(b => b.IsHovered))
                return;

            // Movement is tracked from the blueprint of the earliest hitobject, since it only makes sense to distance snap from that hitobject
            movementBlueprint = SelectionHandler.SelectedBlueprints.OrderBy(b => b.HitObject.StartTime).First();
            movementBlueprintOriginalPosition = movementBlueprint.ScreenSpaceSelectionPoint; // todo: unsure if correct
        }

        /// <summary>
        /// Moves the current selected blueprints.
        /// </summary>
        /// <param name="e">The <see cref="DragEvent"/> defining the movement event.</param>
        /// <returns>Whether a movement was active.</returns>
        private bool moveCurrentSelection(DragEvent e)
        {
            if (movementBlueprint == null)
                return false;

            Debug.Assert(movementBlueprintOriginalPosition != null);

            HitObject draggedObject = movementBlueprint.HitObject;

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = movementBlueprintOriginalPosition.Value + e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            // Retrieve a snapped position.
            var result = snapProvider.SnapScreenSpacePositionToValidTime(movePosition);

            // Move the hitobjects.
            if (!SelectionHandler.HandleMovement(new MoveSelectionEvent(movementBlueprint, result.ScreenSpacePosition)))
                return true;

            if (result.Time.HasValue)
            {
                // Apply the start time at the newly snapped-to position
                double offset = result.Time.Value - draggedObject.StartTime;

                foreach (HitObject obj in Beatmap.SelectedHitObjects)
                {
                    obj.StartTime += offset;
                    Beatmap.Update(obj);
                }
            }

            return true;
        }

        /// <summary>
        /// Finishes the current movement of selected blueprints.
        /// </summary>
        /// <returns>Whether a movement was active.</returns>
        private bool finishSelectionMovement()
        {
            if (movementBlueprint == null)
                return false;

            movementBlueprintOriginalPosition = null;
            movementBlueprint = null;

            return true;
        }

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (Beatmap != null)
            {
                Beatmap.HitObjectAdded -= AddBlueprintFor;
                Beatmap.HitObjectRemoved -= removeBlueprintFor;
            }
        }
    }
}
