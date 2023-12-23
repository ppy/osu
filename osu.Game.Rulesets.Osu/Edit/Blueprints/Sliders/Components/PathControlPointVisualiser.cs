// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public partial class PathControlPointVisualiser<T> : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
        where T : OsuHitObject, IHasPath
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true; // allow context menu to appear outside of the playfield.

        internal readonly Container<PathControlPointPiece<T>> Pieces;
        internal readonly Container<PathControlPointConnectionPiece<T>> Connections;

        private readonly IBindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly T hitObject;
        private readonly bool allowSelection;

        private InputManager inputManager;

        public Action<List<PathControlPoint>> RemoveControlPointsRequested;
        public Action<List<PathControlPoint>> SplitControlPointsRequested;

        [Resolved(CanBeNull = true)]
        private IPositionSnapProvider positionSnapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider distanceSnapProvider { get; set; }

        public PathControlPointVisualiser(T hitObject, bool allowSelection)
        {
            this.hitObject = hitObject;
            this.allowSelection = allowSelection;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Connections = new Container<PathControlPointConnectionPiece<T>> { RelativeSizeAxes = Axes.Both },
                Pieces = new Container<PathControlPointPiece<T>> { RelativeSizeAxes = Axes.Both }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            controlPoints.CollectionChanged += onControlPointsChanged;
            controlPoints.BindTo(hitObject.Path.ControlPoints);
        }

        /// <summary>
        /// Selects the <see cref="PathControlPointPiece{T}"/> corresponding to the given <paramref name="pathControlPoint"/>,
        /// and deselects all other <see cref="PathControlPointPiece{T}"/>s.
        /// </summary>
        public void SetSelectionTo(PathControlPoint pathControlPoint)
        {
            foreach (var p in Pieces)
                p.IsSelected.Value = p.ControlPoint == pathControlPoint;
        }

        /// <summary>
        /// Delete all visually selected <see cref="PathControlPoint"/>s.
        /// </summary>
        /// <returns></returns>
        public bool DeleteSelected()
        {
            List<PathControlPoint> toRemove = Pieces.Where(p => p.IsSelected.Value).Select(p => p.ControlPoint).ToList();

            // Ensure that there are any points to be deleted
            if (toRemove.Count == 0)
                return false;

            changeHandler?.BeginChange();
            RemoveControlPointsRequested?.Invoke(toRemove);
            changeHandler?.EndChange();

            // Since pieces are re-used, they will not point to the deleted control points while remaining selected
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;

            return true;
        }

        private bool splitSelected()
        {
            List<PathControlPoint> controlPointsToSplitAt = Pieces.Where(p => p.IsSelected.Value && isSplittable(p)).Select(p => p.ControlPoint).ToList();

            // Ensure that there are any points to be split
            if (controlPointsToSplitAt.Count == 0)
                return false;

            changeHandler?.BeginChange();
            SplitControlPointsRequested?.Invoke(controlPointsToSplitAt);
            changeHandler?.EndChange();

            // Since pieces are re-used, they will not point to the deleted control points while remaining selected
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;

            return true;
        }

        private bool isSplittable(PathControlPointPiece<T> p) =>
            // A hit object can only be split on control points which connect two different path segments.
            p.ControlPoint.Type.HasValue && p != Pieces.FirstOrDefault() && p != Pieces.LastOrDefault();

        private void onControlPointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    // If inserting in the path (not appending),
                    // update indices of existing connections after insert location
                    if (e.NewStartingIndex < Pieces.Count)
                    {
                        foreach (var connection in Connections)
                        {
                            if (connection.ControlPointIndex >= e.NewStartingIndex)
                                connection.ControlPointIndex += e.NewItems.Count;
                        }
                    }

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var point = (PathControlPoint)e.NewItems[i];

                        Pieces.Add(new PathControlPointPiece<T>(hitObject, point).With(d =>
                        {
                            if (allowSelection)
                                d.RequestSelection = selectionRequested;

                            d.DragStarted = DragStarted;
                            d.DragInProgress = DragInProgress;
                            d.DragEnded = DragEnded;
                        }));

                        Connections.Add(new PathControlPointConnectionPiece<T>(hitObject, e.NewStartingIndex + i));
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (var point in e.OldItems.Cast<PathControlPoint>())
                    {
                        foreach (var piece in Pieces.Where(p => p.ControlPoint == point).ToArray())
                            piece.RemoveAndDisposeImmediately();
                        foreach (var connection in Connections.Where(c => c.ControlPoint == point).ToArray())
                            connection.RemoveAndDisposeImmediately();
                    }

                    // If removing before the end of the path,
                    // update indices of connections after remove location
                    if (e.OldStartingIndex < Pieces.Count)
                    {
                        foreach (var connection in Connections)
                        {
                            if (connection.ControlPointIndex >= e.OldStartingIndex)
                                connection.ControlPointIndex -= e.OldItems.Count;
                        }
                    }

                    break;
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (Pieces.Any(piece => piece.IsHovered))
                return false;

            foreach (var piece in Pieces)
            {
                piece.IsSelected.Value = false;
            }

            return false;
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.Delete:
                    return DeleteSelected();
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        private void selectionRequested(PathControlPointPiece<T> piece, MouseButtonEvent e)
        {
            if (e.Button == MouseButton.Left && inputManager.CurrentState.Keyboard.ControlPressed)
                piece.IsSelected.Toggle();
            else
                SetSelectionTo(piece.ControlPoint);
        }

        /// <summary>
        /// Attempts to set the given control point piece to the given path type.
        /// If that would fail, try to change the path such that it instead succeeds
        /// in a UX-friendly way.
        /// </summary>
        /// <param name="piece">The control point piece that we want to change the path type of.</param>
        /// <param name="type">The path type we want to assign to the given control point piece.</param>
        private void updatePathType(PathControlPointPiece<T> piece, PathType? type)
        {
            int indexInSegment = piece.PointsInSegment.IndexOf(piece.ControlPoint);

            if (type?.Type == SplineType.PerfectCurve)
            {
                // Can't always create a circular arc out of 4 or more points,
                // so we split the segment into one 3-point circular arc segment
                // and one segment of the previous type.
                int thirdPointIndex = indexInSegment + 2;

                if (piece.PointsInSegment.Count > thirdPointIndex + 1)
                    piece.PointsInSegment[thirdPointIndex].Type = piece.PointsInSegment[0].Type;
            }

            hitObject.Path.ExpectedDistance.Value = null;
            piece.ControlPoint.Type = type;
        }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        #region Drag handling

        private Vector2[] dragStartPositions;
        private PathType?[] dragPathTypes;
        private int draggedControlPointIndex;
        private HashSet<PathControlPoint> selectedControlPoints;

        public void DragStarted(PathControlPoint controlPoint)
        {
            dragStartPositions = hitObject.Path.ControlPoints.Select(point => point.Position).ToArray();
            dragPathTypes = hitObject.Path.ControlPoints.Select(point => point.Type).ToArray();
            draggedControlPointIndex = hitObject.Path.ControlPoints.IndexOf(controlPoint);
            selectedControlPoints = new HashSet<PathControlPoint>(Pieces.Where(piece => piece.IsSelected.Value).Select(piece => piece.ControlPoint));

            Debug.Assert(draggedControlPointIndex >= 0);

            changeHandler?.BeginChange();
        }

        public void DragInProgress(DragEvent e)
        {
            Vector2[] oldControlPoints = hitObject.Path.ControlPoints.Select(cp => cp.Position).ToArray();
            var oldPosition = hitObject.Position;
            double oldStartTime = hitObject.StartTime;

            if (selectedControlPoints.Contains(hitObject.Path.ControlPoints[0]))
            {
                // Special handling for selections containing head control point - the position of the hit object changes which means the snapped position and time have to be taken into account
                Vector2 newHeadPosition = Parent!.ToScreenSpace(e.MousePosition + (dragStartPositions[0] - dragStartPositions[draggedControlPointIndex]));
                var result = positionSnapProvider?.FindSnappedPositionAndTime(newHeadPosition);

                Vector2 movementDelta = Parent!.ToLocalSpace(result?.ScreenSpacePosition ?? newHeadPosition) - hitObject.Position;

                hitObject.Position += movementDelta;
                hitObject.StartTime = result?.Time ?? hitObject.StartTime;

                for (int i = 1; i < hitObject.Path.ControlPoints.Count; i++)
                {
                    var controlPoint = hitObject.Path.ControlPoints[i];
                    // Since control points are relative to the position of the hit object, all points that are _not_ selected
                    // need to be offset _back_ by the delta corresponding to the movement of the head point.
                    // All other selected control points (if any) will move together with the head point
                    // (and so they will not move at all, relative to each other).
                    if (!selectedControlPoints.Contains(controlPoint))
                        controlPoint.Position -= movementDelta;
                }
            }
            else
            {
                var result = positionSnapProvider?.FindSnappedPositionAndTime(Parent!.ToScreenSpace(e.MousePosition), SnapType.GlobalGrids);

                Vector2 movementDelta = Parent!.ToLocalSpace(result?.ScreenSpacePosition ?? Parent!.ToScreenSpace(e.MousePosition)) - dragStartPositions[draggedControlPointIndex] - hitObject.Position;

                for (int i = 0; i < controlPoints.Count; ++i)
                {
                    var controlPoint = controlPoints[i];
                    if (selectedControlPoints.Contains(controlPoint))
                        controlPoint.Position = dragStartPositions[i] + movementDelta;
                }
            }

            // Snap the path to the current beat divisor before checking length validity.
            hitObject.SnapTo(distanceSnapProvider);

            if (!hitObject.Path.HasValidLength)
            {
                for (int i = 0; i < hitObject.Path.ControlPoints.Count; i++)
                    hitObject.Path.ControlPoints[i].Position = oldControlPoints[i];

                hitObject.Position = oldPosition;
                hitObject.StartTime = oldStartTime;
                // Snap the path length again to undo the invalid length.
                hitObject.SnapTo(distanceSnapProvider);
                return;
            }

            // Maintain the path types in case they got defaulted to bezier at some point during the drag.
            for (int i = 0; i < hitObject.Path.ControlPoints.Count; i++)
                hitObject.Path.ControlPoints[i].Type = dragPathTypes[i];
        }

        public void DragEnded() => changeHandler?.EndChange();

        #endregion

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!Pieces.Any(p => p.IsHovered))
                    return null;

                var selectedPieces = Pieces.Where(p => p.IsSelected.Value).ToList();
                int count = selectedPieces.Count;

                if (count == 0)
                    return null;

                var splittablePieces = selectedPieces.Where(isSplittable).ToList();
                int splittableCount = splittablePieces.Count;

                List<MenuItem> curveTypeItems = new List<MenuItem>();

                if (!selectedPieces.Contains(Pieces[0]))
                {
                    curveTypeItems.Add(createMenuItemForPathType(null));
                    curveTypeItems.Add(new OsuMenuItemSpacer());
                }

                // todo: hide/disable items which aren't valid for selected points
                curveTypeItems.Add(createMenuItemForPathType(PathType.LINEAR));
                curveTypeItems.Add(createMenuItemForPathType(PathType.PERFECT_CURVE));
                curveTypeItems.Add(createMenuItemForPathType(PathType.BEZIER));
                curveTypeItems.Add(createMenuItemForPathType(PathType.BSpline(4)));

                if (selectedPieces.Any(piece => piece.ControlPoint.Type?.Type == SplineType.Catmull))
                    curveTypeItems.Add(createMenuItemForPathType(PathType.CATMULL));

                var menuItems = new List<MenuItem>
                {
                    new OsuMenuItem("Curve type")
                    {
                        Items = curveTypeItems
                    }
                };

                if (splittableCount > 0)
                {
                    menuItems.Add(new OsuMenuItem($"Split {"control point".ToQuantity(splittableCount, splittableCount > 1 ? ShowQuantityAs.Numeric : ShowQuantityAs.None)}",
                        MenuItemType.Destructive,
                        () => splitSelected()));
                }

                menuItems.Add(
                    new OsuMenuItem($"Delete {"control point".ToQuantity(count, count > 1 ? ShowQuantityAs.Numeric : ShowQuantityAs.None)}",
                        MenuItemType.Destructive,
                        () => DeleteSelected())
                );

                return menuItems.ToArray();
            }
        }

        private MenuItem createMenuItemForPathType(PathType? type)
        {
            int totalCount = Pieces.Count(p => p.IsSelected.Value);
            int countOfState = Pieces.Where(p => p.IsSelected.Value).Count(p => p.ControlPoint.Type == type);

            var item = new TernaryStateRadioMenuItem(type?.Description ?? "Inherit", MenuItemType.Standard, _ =>
            {
                foreach (var p in Pieces.Where(p => p.IsSelected.Value))
                    updatePathType(p, type);
            });

            if (countOfState == totalCount)
                item.State.Value = TernaryState.True;
            else if (countOfState > 0)
                item.State.Value = TernaryState.Indeterminate;
            else
                item.State.Value = TernaryState.False;

            return item;
        }
    }
}
