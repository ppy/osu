// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Humanizer;
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
using osu.Game.Graphics.UserInterface;
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
                                d.RequestSelection = selectPiece;
                        }));

                        Connections.Add(new PathControlPointConnectionPiece(slider, e.NewStartingIndex + i));

                        point.Changed += updatePathTypes;
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (var point in e.OldItems.Cast<PathControlPoint>())
                    {
                        Pieces.RemoveAll(p => p.ControlPoint == point);
                        Connections.RemoveAll(c => c.ControlPoint == point);

                        point.Changed -= updatePathTypes;
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
            foreach (var piece in Pieces)
            {
                piece.IsSelected.Value = false;
            }

            return false;
        }

        public bool OnPressed(PlatformAction action)
        {
            switch (action.ActionMethod)
            {
                case PlatformActionMethod.Delete:
                    return DeleteSelected();
            }

            return false;
        }

        public void OnReleased(PlatformAction action)
        {
        }

        /// <summary>
        /// Handles correction of invalid path types.
        /// </summary>
        private void updatePathTypes()
        {
            foreach (PathControlPoint segmentStartPoint in slider.Path.ControlPoints.Where(p => p.Type.Value != null))
            {
                if (segmentStartPoint.Type.Value != PathType.PerfectCurve)
                    continue;

                Vector2[] points = slider.Path.PointsInSegment(segmentStartPoint).Select(p => p.Position.Value).ToArray();
                if (points.Length == 3 && circularArcBoundingBox(points).Area >= 640 * 480)
                    segmentStartPoint.Type.Value = PathType.Bezier;
            }
        }

        /// <summary>
        /// Computes the bounding box of the circular arc segment defined by the given points.
        /// </summary>
        /// <param name="points">The three points defining the circular arc.</param>
        /// <returns></returns>
        private RectangleF circularArcBoundingBox(IReadOnlyList<Vector2> points)
        {
            Vector2 a = points[0];
            Vector2 b = points[1];
            Vector2 c = points[2];

            // See: https://en.wikipedia.org/wiki/Circumscribed_circle#Cartesian_coordinates_2
            float d = 2 * (a.X * (b - c).Y + b.X * (c - a).Y + c.X * (a - b).Y);
            float aSq = a.LengthSquared;
            float bSq = b.LengthSquared;
            float cSq = c.LengthSquared;

            Vector2 center = new Vector2(
                aSq * (b - c).Y + bSq * (c - a).Y + cSq * (a - b).Y,
                aSq * (c - b).X + bSq * (a - c).X + cSq * (b - a).X) / d;

            Vector2 dA = a - center;
            Vector2 dC = c - center;

            float r = dA.Length;

            double thetaStart = Math.Atan2(dA.Y, dA.X);
            double thetaEnd = Math.Atan2(dC.Y, dC.X);

            while (thetaEnd < thetaStart)
                thetaEnd += 2 * Math.PI;

            // Decide in which direction to draw the circle, depending on which side of
            // AC B lies.
            Vector2 orthoAtoC = c - a;
            orthoAtoC = new Vector2(orthoAtoC.Y, -orthoAtoC.X);
            bool clockwise = Vector2.Dot(orthoAtoC, b - a) >= 0;

            if (clockwise)
            {
                if (thetaEnd < thetaStart)
                    thetaEnd += Math.PI * 2;
            }
            else
            {
                if (thetaStart < thetaEnd)
                    thetaStart += Math.PI * 2;
            }

            List<Vector2> boundingBoxPoints = new List<Vector2>(points);

            bool includes0Degrees = 0 > thetaStart && 0 < thetaEnd;
            bool includes90Degrees = Math.PI / 2 > thetaStart && Math.PI / 2 < thetaEnd;
            bool includes180Degrees = Math.PI > thetaStart && Math.PI < thetaEnd;
            bool includes270Degrees = Math.PI * 1.5f > thetaStart && Math.PI * 1.5f < thetaEnd;

            if (!clockwise)
            {
                includes0Degrees = 0 < thetaStart && 0 > thetaEnd;
                includes90Degrees = Math.PI / 2 < thetaStart && Math.PI / 2 > thetaEnd;
                includes180Degrees = Math.PI < thetaStart && Math.PI > thetaEnd;
                includes270Degrees = Math.PI * 1.5f < thetaStart && Math.PI * 1.5f > thetaEnd;
            }

            if (includes0Degrees) boundingBoxPoints.Add(center + new Vector2(1, 0) * r);
            if (includes90Degrees) boundingBoxPoints.Add(center + new Vector2(0, 1) * r);
            if (includes180Degrees) boundingBoxPoints.Add(center + new Vector2(-1, 0) * r);
            if (includes270Degrees) boundingBoxPoints.Add(center + new Vector2(0, -1) * r);

            float width = Math.Abs(boundingBoxPoints.Max(p => p.X) - boundingBoxPoints.Min(p => p.X));
            float height = Math.Abs(boundingBoxPoints.Max(p => p.Y) - boundingBoxPoints.Min(p => p.Y));

            return new RectangleF(slider.Position, new Vector2(width, height));
        }

        private void selectPiece(PathControlPointPiece piece, MouseButtonEvent e)
        {
            if (e.Button == MouseButton.Left && inputManager.CurrentState.Keyboard.ControlPressed)
                piece.IsSelected.Toggle();
            else
            {
                foreach (var p in Pieces)
                    p.IsSelected.Value = p == piece;
            }
        }

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

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
            int countOfState = Pieces.Where(p => p.IsSelected.Value).Count(p => p.ControlPoint.Type.Value == type);

            var item = new PathTypeMenuItem(type, () =>
            {
                foreach (var p in Pieces.Where(p => p.IsSelected.Value))
                    p.ControlPoint.Type.Value = type;
            });

            if (countOfState == totalCount)
                item.State.Value = TernaryState.True;
            else if (countOfState > 0)
                item.State.Value = TernaryState.Indeterminate;
            else
                item.State.Value = TernaryState.False;

            return item;
        }

        private class PathTypeMenuItem : TernaryStateMenuItem
        {
            public PathTypeMenuItem(PathType? type, Action action)
                : base(type == null ? "Inherit" : type.ToString().Humanize(), changeState, MenuItemType.Standard, _ => action?.Invoke())
            {
            }

            private static TernaryState changeState(TernaryState state) => TernaryState.True;
        }
    }
}
