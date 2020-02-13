// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
using osu.Framework.Timing;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.Edit.Compose.Components
{
    /// <summary>
    /// A container which provides a "blueprint" display of hitobjects.
    /// Includes selection and manipulation support via a <see cref="SelectionHandler"/>.
    /// </summary>
    public abstract class BlueprintContainer : CompositeDrawable, IKeyBindingHandler<PlatformAction>
    {
        public event Action<IEnumerable<HitObject>> SelectionChanged;

        protected DragBox DragBox { get; private set; }

        protected Container<SelectionBlueprint> SelectionBlueprints { get; private set; }

        private SelectionHandler selectionHandler;

        [Resolved]
        private IAdjustableClock adjustableClock { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        private readonly BindableList<HitObject> selectedHitObjects = new BindableList<HitObject>();

        [Resolved(canBeNull: true)]
        private IDistanceSnapProvider snapProvider { get; set; }

        protected BlueprintContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            selectionHandler = CreateSelectionHandler();
            selectionHandler.DeselectAll = deselectAll;

            AddRangeInternal(new[]
            {
                DragBox = CreateDragBox(select),
                selectionHandler,
                SelectionBlueprints = CreateSelectionBlueprintContainer(),
                DragBox.CreateProxy().With(p => p.Depth = float.MinValue)
            });

            foreach (var obj in beatmap.HitObjects)
                AddBlueprintFor(obj);

            selectedHitObjects.BindTo(beatmap.SelectedHitObjects);
            selectedHitObjects.ItemsAdded += objects =>
            {
                foreach (var o in objects)
                    SelectionBlueprints.FirstOrDefault(b => b.HitObject == o)?.Select();

                SelectionChanged?.Invoke(selectedHitObjects);
            };

            selectedHitObjects.ItemsRemoved += objects =>
            {
                foreach (var o in objects)
                    SelectionBlueprints.FirstOrDefault(b => b.HitObject == o)?.Deselect();

                SelectionChanged?.Invoke(selectedHitObjects);
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.HitObjectAdded += AddBlueprintFor;
            beatmap.HitObjectRemoved += removeBlueprintFor;
        }

        protected virtual Container<SelectionBlueprint> CreateSelectionBlueprintContainer() =>
            new Container<SelectionBlueprint> { RelativeSizeAxes = Axes.Both };

        /// <summary>
        /// Creates a <see cref="SelectionHandler"/> which outlines <see cref="DrawableHitObject"/>s and handles movement of selections.
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
            beginClickSelection(e);
            prepareSelectionMovement();

            return e.Button == MouseButton.Left;
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            // Deselection should only occur if no selected blueprints are hovered
            // A special case for when a blueprint was selected via this click is added since OnClick() may occur outside the hitobject and should not trigger deselection
            if (endClickSelection() || selectionHandler.SelectedBlueprints.Any(b => b.IsHovered))
                return true;

            deselectAll();
            return true;
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            if (e.Button == MouseButton.Right)
                return false;

            SelectionBlueprint clickedBlueprint = selectionHandler.SelectedBlueprints.FirstOrDefault(b => b.IsHovered);

            if (clickedBlueprint == null)
                return false;

            adjustableClock?.Seek(clickedBlueprint.HitObject.StartTime);
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
                return true;

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

            if (DragBox.State == Visibility.Visible)
            {
                DragBox.Hide();
                selectionHandler.UpdateVisibility();
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (!selectionHandler.SelectedBlueprints.Any())
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
        }

        protected virtual void AddBlueprintFor(HitObject hitObject)
        {
            var blueprint = CreateBlueprintFor(hitObject);
            if (blueprint == null)
                return;

            blueprint.Selected += onBlueprintSelected;
            blueprint.Deselected += onBlueprintDeselected;

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
        private void beginClickSelection(MouseButtonEvent e)
        {
            Debug.Assert(!clickSelectionBegan);

            // Deselections are only allowed for control + left clicks
            bool allowDeselection = e.ControlPressed && e.Button == MouseButton.Left;

            // Todo: This is probably incorrectly disallowing multiple selections on stacked objects
            if (!allowDeselection && selectionHandler.SelectedBlueprints.Any(s => s.IsHovered))
                return;

            foreach (SelectionBlueprint blueprint in SelectionBlueprints.AliveChildren)
            {
                if (blueprint.IsHovered)
                {
                    selectionHandler.HandleSelectionRequested(blueprint, e.CurrentState);
                    clickSelectionBegan = true;
                    break;
                }
            }
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
        private void select(RectangleF rect)
        {
            foreach (var blueprint in SelectionBlueprints)
            {
                if (blueprint.IsAlive && blueprint.IsPresent && rect.Contains(blueprint.SelectionPoint))
                    blueprint.Select();
                else
                    blueprint.Deselect();
            }
        }

        /// <summary>
        /// Selects all <see cref="SelectionBlueprint"/>s.
        /// </summary>
        private void selectAll()
        {
            SelectionBlueprints.ToList().ForEach(m => m.Select());
            selectionHandler.UpdateVisibility();
        }

        /// <summary>
        /// Deselects all selected <see cref="SelectionBlueprint"/>s.
        /// </summary>
        private void deselectAll() => selectionHandler.SelectedBlueprints.ToList().ForEach(m => m.Deselect());

        private void onBlueprintSelected(SelectionBlueprint blueprint)
        {
            selectionHandler.HandleSelected(blueprint);
            SelectionBlueprints.ChangeChildDepth(blueprint, 1);
            beatmap.SelectedHitObjects.Add(blueprint.HitObject);
        }

        private void onBlueprintDeselected(SelectionBlueprint blueprint)
        {
            selectionHandler.HandleDeselected(blueprint);
            SelectionBlueprints.ChangeChildDepth(blueprint, 0);
            beatmap.SelectedHitObjects.Remove(blueprint.HitObject);
        }

        #endregion

        #region Selection Movement

        private Vector2? movementBlueprintOriginalPosition;
        private SelectionBlueprint movementBlueprint;

        /// <summary>
        /// Attempts to begin the movement of any selected blueprints.
        /// </summary>
        private void prepareSelectionMovement()
        {
            if (!selectionHandler.SelectedBlueprints.Any())
                return;

            // Any selected blueprint that is hovered can begin the movement of the group, however only the earliest hitobject is used for movement
            // A special case is added for when a click selection occurred before the drag
            if (!clickSelectionBegan && !selectionHandler.SelectedBlueprints.Any(b => b.IsHovered))
                return;

            // Movement is tracked from the blueprint of the earliest hitobject, since it only makes sense to distance snap from that hitobject
            movementBlueprint = selectionHandler.SelectedBlueprints.OrderBy(b => b.HitObject.StartTime).First();
            movementBlueprintOriginalPosition = movementBlueprint.SelectionPoint; // todo: unsure if correct
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

            // The final movement position, relative to screenSpaceMovementStartPosition
            Vector2 movePosition = movementBlueprintOriginalPosition.Value + e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;

            (Vector2 snappedPosition, double snappedTime) = snapProvider.GetSnappedPosition(ToLocalSpace(movePosition), draggedObject.StartTime);

            // Move the hitobjects
            if (!selectionHandler.HandleMovement(new MoveSelectionEvent(movementBlueprint, ToScreenSpace(snappedPosition))))
                return true;

            // Apply the start time at the newly snapped-to position
            double offset = snappedTime - draggedObject.StartTime;
            foreach (HitObject obj in selectionHandler.SelectedHitObjects)
                obj.StartTime += offset;

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

            if (beatmap != null)
            {
                beatmap.HitObjectAdded -= AddBlueprintFor;
                beatmap.HitObjectRemoved -= removeBlueprintFor;
            }
        }
    }
}
