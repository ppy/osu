// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;
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
using osu.Game.Configuration;
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
        where T : OsuHitObject, IHasPath, IHasSliderVelocity
    {
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true; // allow context menu to appear outside the playfield.

        internal readonly Container<PathControlPointPiece<T>> Pieces;

        private readonly IBindableList<PathControlPoint> controlPoints = new BindableList<PathControlPoint>();
        private readonly T hitObject;
        private readonly bool allowSelection;

        private InputManager inputManager;

        public Action<List<PathControlPoint>> RemoveControlPointsRequested;
        public Action<List<PathControlPoint>> SplitControlPointsRequested;

        [Resolved(CanBeNull = true)]
        [CanBeNull]
        private OsuHitObjectComposer positionSnapProvider { get; set; }

        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider distanceSnapProvider { get; set; }

        private Bindable<bool> limitedDistanceSnap { get; set; } = null!;

        public PathControlPointVisualiser(T hitObject, bool allowSelection)
        {
            this.hitObject = hitObject;
            this.allowSelection = allowSelection;

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new PathControlPointConnection<T>(hitObject),
                Pieces = new Container<PathControlPointPiece<T>> { RelativeSizeAxes = Axes.Both }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            limitedDistanceSnap = config.GetBindable<bool>(OsuSetting.EditorLimitedDistanceSnap);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            controlPoints.CollectionChanged += onControlPointsChanged;
            controlPoints.BindTo(hitObject.Path.ControlPoints);
        }

        // Generally all the control points are within the visible area all the time.
        public override bool UpdateSubTreeMasking() => true;

        /// <summary>
        /// Handles correction of invalid path types.
        /// </summary>
        public void EnsureValidPathTypes()
        {
            List<PathControlPoint> pointsInCurrentSegment = new List<PathControlPoint>();

            foreach (var controlPoint in controlPoints)
            {
                if (controlPoint.Type != null)
                {
                    pointsInCurrentSegment.Add(controlPoint);
                    ensureValidPathType(pointsInCurrentSegment);
                    pointsInCurrentSegment.Clear();
                }

                pointsInCurrentSegment.Add(controlPoint);
            }

            ensureValidPathType(pointsInCurrentSegment);
        }

        private void ensureValidPathType(IReadOnlyList<PathControlPoint> segment)
        {
            if (segment.Count == 0)
                return;

            PathControlPoint first = segment[0];

            if (first.Type != PathType.PERFECT_CURVE)
                return;

            if (segment.Count > 3)
                first.Type = PathType.BEZIER;

            if (segment.Count != 3)
                return;

            ReadOnlySpan<Vector2> points = segment.Select(p => p.Position).ToArray();
            RectangleF boundingBox = PathApproximator.CircularArcBoundingBox(points);
            if (boundingBox.Width >= 640 || boundingBox.Height >= 480)
                first.Type = PathType.BEZIER;
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
        /// <returns>Whether any change actually took place.</returns>
        public bool DeleteSelected()
        {
            List<PathControlPoint> toRemove = Pieces.Where(p => p.IsSelected.Value).Select(p => p.ControlPoint).ToList();

            if (!Delete(toRemove))
                return false;

            // Since pieces are re-used, they will not point to the deleted control points while remaining selected
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;

            return true;
        }

        /// <summary>
        /// Delete the specified <see cref="PathControlPoint"/>s.
        /// </summary>
        /// <returns>Whether any change actually took place.</returns>
        public bool Delete(List<PathControlPoint> toRemove)
        {
            // Ensure that there are any points to be deleted
            if (toRemove.Count == 0)
                return false;

            changeHandler?.BeginChange();
            RemoveControlPointsRequested?.Invoke(toRemove);
            changeHandler?.EndChange();
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
            p.ControlPoint.Type.HasValue && p.ControlPoint != controlPoints.FirstOrDefault() && p.ControlPoint != controlPoints.LastOrDefault();

        private void onControlPointsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    for (int i = 0; i < e.NewItems.Count; i++)
                    {
                        var point = (PathControlPoint)e.NewItems[i];

                        Pieces.Add(new PathControlPointPiece<T>(hitObject, point).With(d =>
                        {
                            if (allowSelection)
                                d.RequestSelection = selectionRequested;

                            d.ControlPoint.Changed += controlPointChanged;
                            d.DragStarted = DragStarted;
                            d.DragInProgress = DragInProgress;
                            d.DragEnded = DragEnded;
                        }));
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    Debug.Assert(e.OldItems != null);

                    foreach (var point in e.OldItems.Cast<PathControlPoint>())
                    {
                        point.Changed -= controlPointChanged;

                        foreach (var piece in Pieces.Where(p => p.ControlPoint == point).ToArray())
                            piece.RemoveAndDisposeImmediately();
                    }

                    break;
            }
        }

        private void controlPointChanged() => updateCurveMenuItems();

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

        // ReSharper disable once StaticMemberInGenericType
        private static readonly PathType?[] path_types =
        [
            PathType.LINEAR,
            PathType.BEZIER,
            PathType.PERFECT_CURVE,
            PathType.BSpline(4),
            null,
        ];

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat)
                return false;

            switch (e.Key)
            {
                case Key.Tab:
                {
                    var selectedPieces = Pieces.Where(p => p.IsSelected.Value).ToArray();
                    if (selectedPieces.Length != 1)
                        return false;

                    PathControlPointPiece<T> selectedPiece = selectedPieces.Single();
                    PathControlPoint selectedPoint = selectedPiece.ControlPoint;

                    PathType?[] validTypes = path_types;

                    if (selectedPoint == controlPoints[0])
                        validTypes = validTypes.Where(t => t != null).ToArray();

                    int currentTypeIndex = Array.IndexOf(validTypes, selectedPoint.Type);

                    if (currentTypeIndex < 0 && e.ShiftPressed)
                        currentTypeIndex = 0;

                    changeHandler?.BeginChange();

                    do
                    {
                        currentTypeIndex = (validTypes.Length + currentTypeIndex + (e.ShiftPressed ? -1 : 1)) % validTypes.Length;

                        updatePathTypeOfSelectedPieces(validTypes[currentTypeIndex]);
                    } while (selectedPoint.Type != validTypes[currentTypeIndex]);

                    changeHandler?.EndChange();

                    return true;
                }

                case Key.Number1:
                case Key.Number2:
                case Key.Number3:
                case Key.Number4:
                case Key.Number5:
                {
                    if (!e.AltPressed)
                        return false;

                    // If no pieces are selected, we can't change the path type.
                    if (Pieces.All(p => !p.IsSelected.Value))
                        return false;

                    PathType? type = path_types[e.Key - Key.Number1];

                    // The first control point can never be inherit type
                    if (Pieces[0].IsSelected.Value && type == null)
                        return false;

                    updatePathTypeOfSelectedPieces(type);
                    return true;
                }

                default:
                    return false;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            foreach (var p in Pieces)
                p.ControlPoint.Changed -= controlPointChanged;

            if (draggedControlPointIndex >= 0)
                DragEnded();
        }

        private void selectionRequested(PathControlPointPiece<T> piece, MouseButtonEvent e)
        {
            if (e.Button == MouseButton.Left && inputManager.CurrentState.Keyboard.ControlPressed)
                piece.IsSelected.Toggle();
            else
                SetSelectionTo(piece.ControlPoint);
        }

        /// <summary>
        /// Attempts to set all selected control point pieces to the given path type.
        /// If that fails, try to change the path such that it instead succeeds
        /// in a UX-friendly way.
        /// </summary>
        /// <param name="type">The path type we want to assign to the given control point piece.</param>
        private void updatePathTypeOfSelectedPieces(PathType? type)
        {
            changeHandler?.BeginChange();

            double originalDistance = hitObject.Path.Distance;

            foreach (var p in Pieces.Where(p => p.IsSelected.Value))
            {
                List<PathControlPoint> pointsInSegment = hitObject.Path.PointsInSegment(p.ControlPoint);
                int indexInSegment = pointsInSegment.IndexOf(p.ControlPoint);

                if (type?.Type == SplineType.PerfectCurve)
                {
                    // Can't always create a circular arc out of 4 or more points,
                    // so we split the segment into one 3-point circular arc segment
                    // and one segment of the previous type.
                    int thirdPointIndex = indexInSegment + 2;

                    if (pointsInSegment.Count > thirdPointIndex + 1)
                        pointsInSegment[thirdPointIndex].Type = pointsInSegment[0].Type;
                }

                hitObject.Path.ExpectedDistance.Value = null;
                p.ControlPoint.Type = type;
            }

            EnsureValidPathTypes();

            if (hitObject.Path.Distance < originalDistance)
                hitObject.SnapTo(distanceSnapProvider);
            else
                hitObject.Path.ExpectedDistance.Value = originalDistance;

            changeHandler?.EndChange();
        }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        #region Drag handling

        private Vector2[] dragStartPositions;
        private PathType?[] dragPathTypes;
        private int draggedControlPointIndex = -1;
        private HashSet<PathControlPoint> selectedControlPoints;

        private List<MenuItem> curveTypeItems;

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
            Vector2 oldPosition = hitObject.Position;
            double oldStartTime = hitObject.StartTime;

            if (selectedControlPoints.Contains(hitObject.Path.ControlPoints[0]))
            {
                // Special handling for selections containing head control point - the position of the hit object changes which means the snapped position and time have to be taken into account
                Vector2 newHeadPosition = Parent!.ToScreenSpace(e.MousePosition + (dragStartPositions[0] - dragStartPositions[draggedControlPointIndex]));

                var result = positionSnapProvider?.TrySnapToNearbyObjects(newHeadPosition, oldStartTime);
                result ??= positionSnapProvider?.TrySnapToDistanceGrid(newHeadPosition, limitedDistanceSnap.Value ? oldStartTime : null);
                if (positionSnapProvider?.TrySnapToPositionGrid(result?.ScreenSpacePosition ?? newHeadPosition, result?.Time ?? oldStartTime) is SnapResult gridSnapResult)
                    result = gridSnapResult;
                result ??= new SnapResult(newHeadPosition, oldStartTime);

                Vector2 movementDelta = Parent!.ToLocalSpace(result.ScreenSpacePosition) - hitObject.Position;

                hitObject.Position += movementDelta;
                hitObject.StartTime = result.Time ?? hitObject.StartTime;

                for (int i = 1; i < hitObject.Path.ControlPoints.Count; i++)
                {
                    PathControlPoint controlPoint = hitObject.Path.ControlPoints[i];
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
                SnapResult result = positionSnapProvider?.TrySnapToPositionGrid(Parent!.ToScreenSpace(e.MousePosition));

                Vector2 movementDelta = Parent!.ToLocalSpace(result?.ScreenSpacePosition ?? Parent!.ToScreenSpace(e.MousePosition)) - dragStartPositions[draggedControlPointIndex] - hitObject.Position;

                for (int i = 0; i < controlPoints.Count; ++i)
                {
                    PathControlPoint controlPoint = controlPoints[i];
                    if (selectedControlPoints.Contains(controlPoint))
                        controlPoint.Position = dragStartPositions[i] + movementDelta;
                }
            }

            // Snap the path to the current beat divisor before checking length validity.
            hitObject.SnapTo(distanceSnapProvider);

            if (!hitObject.Path.HasValidLengthForPlacement)
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

            EnsureValidPathTypes();
        }

        public void DragEnded()
        {
            changeHandler?.EndChange();
            draggedControlPointIndex = -1;
        }

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

                curveTypeItems = new List<MenuItem>();

                for (int i = 0; i < path_types.Length; ++i)
                {
                    PathType? type = path_types[i];

                    // special inherit case
                    if (type == null)
                    {
                        if (selectedPieces.Contains(Pieces[0]))
                            continue;

                        curveTypeItems.Add(new OsuMenuItemSpacer());
                    }

                    curveTypeItems.Add(createMenuItemForPathType(type, InputKey.Number1 + i));
                }

                if (selectedPieces.Any(piece => piece.ControlPoint.Type?.Type == SplineType.Catmull))
                {
                    curveTypeItems.Add(new OsuMenuItemSpacer());
                    curveTypeItems.Add(createMenuItemForPathType(PathType.CATMULL));
                }

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

                updateCurveMenuItems();

                return menuItems.ToArray();

                CurveTypeMenuItem createMenuItemForPathType(PathType? type, InputKey? key = null)
                {
                    Hotkey hotkey = default;

                    if (key != null)
                        hotkey = new Hotkey(new KeyCombination(InputKey.Alt, key.Value));

                    return new CurveTypeMenuItem(type, _ => updatePathTypeOfSelectedPieces(type)) { Hotkey = hotkey };
                }
            }
        }

        private void updateCurveMenuItems()
        {
            if (curveTypeItems == null)
                return;

            foreach (var item in curveTypeItems.OfType<CurveTypeMenuItem>())
            {
                int totalCount = Pieces.Count(p => p.IsSelected.Value);
                int countOfState = Pieces.Where(p => p.IsSelected.Value).Count(p => p.ControlPoint.Type == item.PathType);

                if (countOfState == totalCount)
                    item.State.Value = TernaryState.True;
                else if (countOfState > 0)
                    item.State.Value = TernaryState.Indeterminate;
                else
                    item.State.Value = TernaryState.False;
            }
        }

        private class CurveTypeMenuItem : TernaryStateRadioMenuItem
        {
            public readonly PathType? PathType;

            public CurveTypeMenuItem(PathType? pathType, Action<TernaryState> action)
                : base(pathType?.Description ?? "Inherit", MenuItemType.Standard, action)
            {
                PathType = pathType;
            }
        }
    }
}
