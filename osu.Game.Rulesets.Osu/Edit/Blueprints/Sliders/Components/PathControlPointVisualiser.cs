// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class PathControlPointVisualiser : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true; // allow context menu to appear outside of the playfield.

        internal readonly Container<PathControlPointPiece> Pieces;
        internal readonly Container<PathControlPointConnectionPiece> Connections;

        private readonly IBindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly Slider slider;
        private readonly bool allowSelection;

        private InputManager inputManager;

        public Action<List<PathControlPoint>> RemoveControlPointsRequested;

        [Resolved(CanBeNull = true)]
        private IPositionSnapProvider snapProvider { get; set; }

        public PathControlPointVisualiser(Slider slider, bool allowSelection)
        {
            this.slider = slider;
            this.allowSelection = allowSelection;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                Connections = new Container<PathControlPointConnectionPiece> { RelativeSizeAxes = Axes.Both },
                Pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            controlPoints.CollectionChanged += onControlPointsChanged;
            controlPoints.BindTo(slider.Path.ControlPoints);
        }

        /// <summary>
        /// Selects the <see cref="PathControlPointPiece"/> corresponding to the given <paramref name="pathControlPoint"/>,
        /// and deselects all other <see cref="PathControlPointPiece"/>s.
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

        private void onControlPointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
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

                        Pieces.Add(new PathControlPointPiece(slider, point).With(d =>
                        {
                            if (allowSelection)
                                d.RequestSelection = selectionRequested;

                            d.DragStarted = dragStarted;
                            d.DragInProgress = dragInProgress;
                            d.DragEnded = dragEnded;
                        }));

                        Connections.Add(new PathControlPointConnectionPiece(slider, e.NewStartingIndex + i));
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var point in e.OldItems.Cast<PathControlPoint>())
                    {
                        Pieces.RemoveAll(p => p.ControlPoint == point);
                        Connections.RemoveAll(c => c.ControlPoint == point);
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

        private void selectionRequested(PathControlPointPiece piece, MouseButtonEvent e)
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
        private void updatePathType(PathControlPointPiece piece, PathType? type)
        {
            int indexInSegment = piece.PointsInSegment.IndexOf(piece.ControlPoint);

            switch (type)
            {
                case PathType.PerfectCurve:
                    // Can't always create a circular arc out of 4 or more points,
                    // so we split the segment into one 3-point circular arc segment
                    // and one segment of the previous type.
                    int thirdPointIndex = indexInSegment + 2;

                    if (piece.PointsInSegment.Count > thirdPointIndex + 1)
                        piece.PointsInSegment[thirdPointIndex].Type = piece.PointsInSegment[0].Type;

                    break;
            }

            piece.ControlPoint.Type = type;
        }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        #region Drag handling

        private Vector2[] dragStartPositions;
        private PathType?[] dragPathTypes;
        private int draggedControlPointIndex;
        private HashSet<PathControlPoint> selectedControlPoints;

        private void dragStarted(PathControlPoint controlPoint)
        {
            dragStartPositions = slider.Path.ControlPoints.Select(point => point.Position).ToArray();
            dragPathTypes = slider.Path.ControlPoints.Select(point => point.Type).ToArray();
            draggedControlPointIndex = slider.Path.ControlPoints.IndexOf(controlPoint);
            selectedControlPoints = new HashSet<PathControlPoint>(Pieces.Where(piece => piece.IsSelected.Value).Select(piece => piece.ControlPoint));

            Debug.Assert(draggedControlPointIndex >= 0);

            changeHandler?.BeginChange();
        }

        private void dragInProgress(DragEvent e)
        {
            Vector2[] oldControlPoints = slider.Path.ControlPoints.Select(cp => cp.Position).ToArray();
            var oldPosition = slider.Position;
            double oldStartTime = slider.StartTime;

            if (selectedControlPoints.Contains(slider.Path.ControlPoints[0]))
            {
                // Special handling for selections containing head control point - the position of the slider changes which means the snapped position and time have to be taken into account
                Vector2 newHeadPosition = Parent.ToScreenSpace(e.MousePosition + (dragStartPositions[0] - dragStartPositions[draggedControlPointIndex]));
                var result = snapProvider?.SnapScreenSpacePositionToValidTime(newHeadPosition);

                Vector2 movementDelta = Parent.ToLocalSpace(result?.ScreenSpacePosition ?? newHeadPosition) - slider.Position;

                slider.Position += movementDelta;
                slider.StartTime = result?.Time ?? slider.StartTime;

                for (int i = 1; i < slider.Path.ControlPoints.Count; i++)
                {
                    var controlPoint = slider.Path.ControlPoints[i];
                    // Since control points are relative to the position of the slider, all points that are _not_ selected
                    // need to be offset _back_ by the delta corresponding to the movement of the head point.
                    // All other selected control points (if any) will move together with the head point
                    // (and so they will not move at all, relative to each other).
                    if (!selectedControlPoints.Contains(controlPoint))
                        controlPoint.Position -= movementDelta;
                }
            }
            else
            {
                for (int i = 0; i < controlPoints.Count; ++i)
                {
                    var controlPoint = controlPoints[i];
                    if (selectedControlPoints.Contains(controlPoint))
                        controlPoint.Position = dragStartPositions[i] + (e.MousePosition - e.MouseDownPosition);
                }
            }

            // Snap the path to the current beat divisor before checking length validity.
            slider.SnapTo(snapProvider);

            if (!slider.Path.HasValidLength)
            {
                for (int i = 0; i < slider.Path.ControlPoints.Count; i++)
                    slider.Path.ControlPoints[i].Position = oldControlPoints[i];

                slider.Position = oldPosition;
                slider.StartTime = oldStartTime;
                // Snap the path length again to undo the invalid length.
                slider.SnapTo(snapProvider);
                return;
            }

            // Maintain the path types in case they got defaulted to bezier at some point during the drag.
            for (int i = 0; i < slider.Path.ControlPoints.Count; i++)
                slider.Path.ControlPoints[i].Type = dragPathTypes[i];
        }

        private void dragEnded() => changeHandler?.EndChange();

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

                List<MenuItem> items = new List<MenuItem>();

                if (!selectedPieces.Contains(Pieces[0]))
                    items.Add(createMenuItemForPathType(null));

                // todo: hide/disable items which aren't valid for selected points
                items.Add(createMenuItemForPathType(PathType.Linear));
                items.Add(createMenuItemForPathType(PathType.PerfectCurve));
                items.Add(createMenuItemForPathType(PathType.Bezier));
                items.Add(createMenuItemForPathType(PathType.Catmull));

                return new MenuItem[]
                {
                    new OsuMenuItem($"Delete {"control point".ToQuantity(count, count > 1 ? ShowQuantityAs.Numeric : ShowQuantityAs.None)}", MenuItemType.Destructive, () => DeleteSelected()),
                    new OsuMenuItem("Curve type")
                    {
                        Items = items
                    }
                };
            }
        }

        private MenuItem createMenuItemForPathType(PathType? type)
        {
            int totalCount = Pieces.Count(p => p.IsSelected.Value);
            int countOfState = Pieces.Where(p => p.IsSelected.Value).Count(p => p.ControlPoint.Type == type);

            var item = new TernaryStateRadioMenuItem(type == null ? "Inherit" : type.ToString().Humanize(), MenuItemType.Standard, _ =>
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
