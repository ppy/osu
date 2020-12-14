// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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

        protected readonly HitObjectComposer Composer;

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        [Resolved]
        protected EditorClock EditorClock { get; private set; }

        [Resolved]
        protected EditorBeatmap Beatmap { get; private set; }

        private readonly BindableList<HitObject> selectedHitObjects = new BindableList<HitObject>();
        private readonly Dictionary<HitObject, SelectionBlueprint> blueprintMap = new Dictionary<HitObject, SelectionBlueprint>();

        [Resolved(canBeNull: true)]
        private IPositionSnapProvider snapProvider { get; set; }

        protected BlueprintContainer(HitObjectComposer composer)
        {
            Composer = composer;

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

            // For non-pooled rulesets, hitobjects are already present in the playfield which allows the blueprints to be loaded in the async context.
            if (Composer != null)
            {
                foreach (var obj in Composer.HitObjects)
                    addBlueprintFor(obj.HitObject);
            }

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

            Beatmap.HitObjectAdded += addBlueprintFor;
            Beatmap.HitObjectRemoved += removeBlueprintFor;

            if (Composer != null)
            {
                // For pooled rulesets, blueprints must be added for hitobjects already "current" as they would've not been "current" during the async load addition process above.
                foreach (var obj in Composer.HitObjects)
                    addBlueprintFor(obj.HitObject);

                Composer.Playfield.HitObjectUsageBegan += addBlueprintFor;
                Composer.Playfield.HitObjectUsageFinished += removeBlueprintFor;
            }
        }

        protected virtual Container<SelectionBlueprint> CreateSelectionBlueprintContainer() => new HitObjectOrderedSelectionContainer { RelativeSizeAxes = Axes.Both };

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

            EditorClock?.SeekTo(clickedBlueprint.HitObject.StartTime);
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

            if (movementBlueprints != null)
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

        private void addBlueprintFor(HitObject hitObject)
        {
            if (blueprintMap.ContainsKey(hitObject))
                return;

            var blueprint = CreateBlueprintFor(hitObject);
            if (blueprint == null)
                return;

            blueprintMap[hitObject] = blueprint;

            blueprint.Selected += onBlueprintSelected;
            blueprint.Deselected += onBlueprintDeselected;

            if (Beatmap.SelectedHitObjects.Contains(hitObject))
                blueprint.Select();

            SelectionBlueprints.Add(blueprint);

            OnBlueprintAdded(hitObject);
        }

        private void removeBlueprintFor(HitObject hitObject)
        {
            if (!blueprintMap.Remove(hitObject, out var blueprint))
                return;

            blueprint.Deselect();
            blueprint.Selected -= onBlueprintSelected;
            blueprint.Deselected -= onBlueprintDeselected;

            SelectionBlueprints.Remove(blueprint);

            if (movementBlueprints?.Contains(blueprint) == true)
                finishSelectionMovement();

            OnBlueprintRemoved(hitObject);
        }

        /// <summary>
        /// Called after a <see cref="HitObject"/> blueprint has been added.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> for which the blueprint has been added.</param>
        protected virtual void OnBlueprintAdded(HitObject hitObject)
        {
        }

        /// <summary>
        /// Called after a <see cref="HitObject"/> blueprint has been removed.
        /// </summary>
        /// <param name="hitObject">The <see cref="HitObject"/> for which the blueprint has been removed.</param>
        protected virtual void OnBlueprintRemoved(HitObject hitObject)
        {
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
            // Iterate from the top of the input stack (blueprints closest to the front of the screen first).
            foreach (SelectionBlueprint blueprint in SelectionBlueprints.AliveChildren.Reverse())
            {
                if (!blueprint.IsHovered) continue;

                return clickSelectionBegan = SelectionHandler.HandleSelectionRequested(blueprint, e);
            }

            return false;
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
                        if (!EditorClock.IsRunning && !isValidForSelection())
                            blueprint.Deselect();
                        break;
                }
            }
        }

        /// <summary>
        /// Selects all <see cref="SelectionBlueprint"/>s.
        /// </summary>
        private void selectAll()
        {
            Composer.Playfield.KeepAllAlive();

            // Scheduled to allow the change in lifetime to take place.
            Schedule(() => SelectionBlueprints.ToList().ForEach(m => m.Select()));
        }

        /// <summary>
        /// Deselects all selected <see cref="SelectionBlueprint"/>s.
        /// </summary>
        private void deselectAll() => SelectionHandler.SelectedBlueprints.ToList().ForEach(m => m.Deselect());

        private void onBlueprintSelected(SelectionBlueprint blueprint)
        {
            SelectionHandler.HandleSelected(blueprint);
            SelectionBlueprints.ChangeChildDepth(blueprint, 1);

            Composer.Playfield.SetKeepAlive(blueprint.HitObject, true);
        }

        private void onBlueprintDeselected(SelectionBlueprint blueprint)
        {
            SelectionHandler.HandleDeselected(blueprint);
            SelectionBlueprints.ChangeChildDepth(blueprint, 0);

            Composer.Playfield.SetKeepAlive(blueprint.HitObject, false);
        }

        #endregion

        #region Selection Movement

        private Vector2[] movementBlueprintOriginalPositions;
        private SelectionBlueprint[] movementBlueprints;
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
            movementBlueprints = SelectionHandler.SelectedBlueprints.OrderBy(b => b.HitObject.StartTime).ToArray();
            movementBlueprintOriginalPositions = movementBlueprints.Select(m => m.ScreenSpaceSelectionPoint).ToArray();
        }

        /// <summary>
        /// Moves the current selected blueprints.
        /// </summary>
        /// <param name="e">The <see cref="DragEvent"/> defining the movement event.</param>
        /// <returns>Whether a movement was active.</returns>
        private bool moveCurrentSelection(DragEvent e)
        {
            if (movementBlueprints == null)
                return false;

            if (snapProvider == null)
                return true;

            Debug.Assert(movementBlueprintOriginalPositions != null);

            Vector2 distanceTravelled = e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            // check for positional snap for every object in selection (for things like object-object snapping)
            for (var i = 0; i < movementBlueprintOriginalPositions.Length; i++)
            {
                var testPosition = movementBlueprintOriginalPositions[i] + distanceTravelled;

                var positionalResult = snapProvider.SnapScreenSpacePositionToValidPosition(testPosition);

                if (positionalResult.ScreenSpacePosition == testPosition) continue;

                // attempt to move the objects, and abort any time based snapping if we can.
                if (SelectionHandler.HandleMovement(new MoveSelectionEvent(movementBlueprints[i], positionalResult.ScreenSpacePosition)))
                    return true;
            }

            // if no positional snapping could be performed, try unrestricted snapping from the earliest
            // hitobject in the selection.

            // The final movement position, relative to movementBlueprintOriginalPosition.
            Vector2 movePosition = movementBlueprintOriginalPositions.First() + distanceTravelled;

            // Retrieve a snapped position.
            var result = snapProvider.SnapScreenSpacePositionToValidTime(movePosition);

            // Move the hitobjects.
            if (!SelectionHandler.HandleMovement(new MoveSelectionEvent(movementBlueprints.First(), result.ScreenSpacePosition)))
                return true;

            if (result.Time.HasValue)
            {
                // Apply the start time at the newly snapped-to position
                double offset = result.Time.Value - movementBlueprints.First().HitObject.StartTime;

                foreach (HitObject obj in Beatmap.SelectedHitObjects)
                    obj.StartTime += offset;
            }

            return true;
        }

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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (Beatmap != null)
            {
                Beatmap.HitObjectAdded -= addBlueprintFor;
                Beatmap.HitObjectRemoved -= removeBlueprintFor;
            }

            if (Composer != null)
            {
                Composer.Playfield.HitObjectUsageBegan -= addBlueprintFor;
                Composer.Playfield.HitObjectUsageFinished -= removeBlueprintFor;
            }
        }
    }
}
