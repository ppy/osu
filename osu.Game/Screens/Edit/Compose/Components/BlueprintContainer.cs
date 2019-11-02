// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public class BlueprintContainer : CompositeDrawable
    {
        public event Action<IEnumerable<HitObject>> SelectionChanged;

        private DragBox dragBox;
        private SelectionBlueprintContainer selectionBlueprints;
        private Container<PlacementBlueprint> placementBlueprintContainer;
        private PlacementBlueprint currentPlacement;
        private SelectionHandler selectionHandler;
        private InputManager inputManager;

        [Resolved]
        private HitObjectComposer composer { get; set; }

        [Resolved]
        private IEditorBeatmap beatmap { get; set; }

        public BlueprintContainer()
        {
            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            selectionHandler = composer.CreateSelectionHandler();
            selectionHandler.DeselectAll = deselectAll;

            InternalChildren = new[]
            {
                dragBox = new DragBox(select),
                selectionHandler,
                selectionBlueprints = new SelectionBlueprintContainer { RelativeSizeAxes = Axes.Both },
                placementBlueprintContainer = new Container<PlacementBlueprint> { RelativeSizeAxes = Axes.Both },
                dragBox.CreateProxy()
            };

            foreach (var obj in composer.HitObjects)
                addBlueprintFor(obj);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.HitObjectAdded += addBlueprintFor;
            beatmap.HitObjectRemoved += removeBlueprintFor;

            inputManager = GetContainingInputManager();
        }

        private HitObjectCompositionTool currentTool;

        /// <summary>
        /// The current placement tool.
        /// </summary>
        public HitObjectCompositionTool CurrentTool
        {
            get => currentTool;
            set
            {
                if (currentTool == value)
                    return;

                currentTool = value;

                refreshTool();
            }
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            beginClickSelection(e);
            return true;
        }

        protected override bool OnClick(ClickEvent e)
        {
            // Deselection should only occur if no selected blueprints are hovered
            // A special case for when a blueprint was selected via this click is added since OnClick() may occur outside the hitobject and should not trigger deselection
            if (endClickSelection() || selectionHandler.SelectedBlueprints.Any(b => b.IsHovered))
                return true;

            deselectAll();
            return true;
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            // Special case for when a drag happened instead of a click
            Schedule(() => endClickSelection());
            return true;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            if (currentPlacement != null)
            {
                updatePlacementPosition(e.ScreenSpaceMousePosition);
                return true;
            }

            return base.OnMouseMove(e);
        }

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (!beginSelectionMovement())
            {
                dragBox.UpdateDrag(e);
                dragBox.FadeIn(250, Easing.OutQuint);
            }

            return true;
        }

        protected override bool OnDrag(DragEvent e)
        {
            if (!moveCurrentSelection(e))
                dragBox.UpdateDrag(e);

            return true;
        }

        protected override bool OnDragEnd(DragEndEvent e)
        {
            if (!finishSelectionMovement())
            {
                dragBox.FadeOut(250, Easing.OutQuint);
                selectionHandler.UpdateVisibility();
            }

            return true;
        }

        protected override void Update()
        {
            base.Update();

            if (currentPlacement != null)
            {
                if (composer.CursorInPlacementArea)
                    currentPlacement.State = PlacementState.Shown;
                else if (currentPlacement?.PlacementBegun == false)
                    currentPlacement.State = PlacementState.Hidden;
            }
        }

        #region Blueprint Addition/Removal

        private void addBlueprintFor(HitObject hitObject)
        {
            var drawable = composer.HitObjects.FirstOrDefault(d => d.HitObject == hitObject);
            if (drawable == null)
                return;

            addBlueprintFor(drawable);
        }

        private void removeBlueprintFor(HitObject hitObject)
        {
            var blueprint = selectionBlueprints.Single(m => m.DrawableObject.HitObject == hitObject);
            if (blueprint == null)
                return;

            blueprint.Deselect();

            blueprint.Selected -= onBlueprintSelected;
            blueprint.Deselected -= onBlueprintDeselected;

            selectionBlueprints.Remove(blueprint);
        }

        private void addBlueprintFor(DrawableHitObject hitObject)
        {
            refreshTool();

            var blueprint = composer.CreateBlueprintFor(hitObject);
            if (blueprint == null)
                return;

            blueprint.Selected += onBlueprintSelected;
            blueprint.Deselected += onBlueprintDeselected;

            selectionBlueprints.Add(blueprint);
        }

        #endregion

        #region Placement

        /// <summary>
        /// Refreshes the current placement tool.
        /// </summary>
        private void refreshTool()
        {
            placementBlueprintContainer.Clear();
            currentPlacement = null;

            var blueprint = CurrentTool?.CreatePlacementBlueprint();

            if (blueprint != null)
            {
                placementBlueprintContainer.Child = currentPlacement = blueprint;

                // Fixes a 1-frame position discrepancy due to the first mouse move event happening in the next frame
                updatePlacementPosition(inputManager.CurrentState.Mouse.Position);
            }
        }

        private void updatePlacementPosition(Vector2 screenSpacePosition)
        {
            Vector2 snappedGridPosition = composer.GetSnappedPosition(ToLocalSpace(screenSpacePosition), 0).position;
            Vector2 snappedScreenSpacePosition = ToScreenSpace(snappedGridPosition);

            currentPlacement.UpdatePosition(snappedScreenSpacePosition);
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
        private void beginClickSelection(UIEvent e)
        {
            Debug.Assert(!clickSelectionBegan);

            foreach (SelectionBlueprint blueprint in selectionBlueprints.AliveBlueprints)
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
            foreach (var blueprint in selectionBlueprints)
            {
                if (blueprint.IsAlive && blueprint.IsPresent && rect.Contains(blueprint.SelectionPoint))
                    blueprint.Select();
                else
                    blueprint.Deselect();
            }
        }

        /// <summary>
        /// Deselects all selected <see cref="SelectionBlueprint"/>s.
        /// </summary>
        private void deselectAll() => selectionHandler.SelectedBlueprints.ToList().ForEach(m => m.Deselect());

        private void onBlueprintSelected(SelectionBlueprint blueprint)
        {
            selectionHandler.HandleSelected(blueprint);
            selectionBlueprints.ChangeChildDepth(blueprint, 1);

            SelectionChanged?.Invoke(selectionHandler.SelectedHitObjects);
        }

        private void onBlueprintDeselected(SelectionBlueprint blueprint)
        {
            selectionHandler.HandleDeselected(blueprint);
            selectionBlueprints.ChangeChildDepth(blueprint, 0);

            SelectionChanged?.Invoke(selectionHandler.SelectedHitObjects);
        }

        #endregion

        #region Selection Movement

        private Vector2? screenSpaceMovementStartPosition;
        private SelectionBlueprint movementBlueprint;

        /// <summary>
        /// Attempts to begin the movement of any selected blueprints.
        /// </summary>
        /// <returns>Whether movement began.</returns>
        private bool beginSelectionMovement()
        {
            Debug.Assert(movementBlueprint == null);

            // Any selected blueprint that is hovered can begin the movement of the group, however only the earliest hitobject is used for movement
            // A special case is added for when a click selection occurred before the drag
            if (!clickSelectionBegan && !selectionHandler.SelectedBlueprints.Any(b => b.IsHovered))
                return false;

            // Movement is tracked from the blueprint of the earliest hitobject, since it only makes sense to distance snap from that hitobject
            movementBlueprint = selectionHandler.SelectedBlueprints.OrderBy(b => b.DrawableObject.HitObject.StartTime).First();
            screenSpaceMovementStartPosition = movementBlueprint.DrawableObject.ToScreenSpace(movementBlueprint.DrawableObject.OriginPosition);

            return true;
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

            Debug.Assert(screenSpaceMovementStartPosition != null);

            Vector2 startPosition = screenSpaceMovementStartPosition.Value;
            HitObject draggedObject = movementBlueprint.DrawableObject.HitObject;

            // The final movement position, relative to screenSpaceMovementStartPosition
            Vector2 movePosition = startPosition + e.ScreenSpaceMousePosition - e.ScreenSpaceMouseDownPosition;
            (Vector2 snappedPosition, double snappedTime) = composer.GetSnappedPosition(ToLocalSpace(movePosition), draggedObject.StartTime);

            // Move the hitobjects
            selectionHandler.HandleMovement(new MoveSelectionEvent(movementBlueprint, startPosition, ToScreenSpace(snappedPosition)));

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

            screenSpaceMovementStartPosition = null;
            movementBlueprint = null;

            return true;
        }

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (beatmap != null)
            {
                beatmap.HitObjectAdded -= addBlueprintFor;
                beatmap.HitObjectRemoved -= removeBlueprintFor;
            }
        }

        private class SelectionBlueprintContainer : Container<SelectionBlueprint>
        {
            public IEnumerable<SelectionBlueprint> AliveBlueprints => AliveInternalChildren.Cast<SelectionBlueprint>();

            protected override int Compare(Drawable x, Drawable y)
            {
                if (!(x is SelectionBlueprint xBlueprint) || !(y is SelectionBlueprint yBlueprint))
                    return base.Compare(x, y);

                return Compare(xBlueprint, yBlueprint);
            }

            public int Compare(SelectionBlueprint x, SelectionBlueprint y)
            {
                // dpeth is used to denote selected status (we always want selected blueprints to handle input first).
                int d = x.Depth.CompareTo(y.Depth);
                if (d != 0)
                    return d;

                // Put earlier hitobjects towards the end of the list, so they handle input first
                int i = y.DrawableObject.HitObject.StartTime.CompareTo(x.DrawableObject.HitObject.StartTime);
                return i == 0 ? CompareReverseChildID(x, y) : i;
            }
        }
    }
}
