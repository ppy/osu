// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
    {
        internal readonly Container<PathControlPointPiece> Pieces;

        private readonly Slider slider;

        private readonly bool allowSelection;

        private InputManager inputManager;

        [Resolved(CanBeNull = true)]
        private IPlacementHandler placementHandler { get; set; }

        private IBindableList<PathControlPoint> controlPoints;

        public PathControlPointVisualiser(Slider slider, bool allowSelection)
        {
            this.slider = slider;
            this.allowSelection = allowSelection;

            RelativeSizeAxes = Axes.Both;

            InternalChild = Pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager();

            controlPoints = slider.Path.ControlPoints.GetBoundCopy();
            controlPoints.ItemsAdded += addControlPoints;
            controlPoints.ItemsRemoved += removeControlPoints;

            addControlPoints(controlPoints);
        }

        private void addControlPoints(IEnumerable<PathControlPoint> controlPoints)
        {
            foreach (var point in controlPoints)
            {
                var piece = new PathControlPointPiece(slider, point);

                if (allowSelection)
                    piece.RequestSelection = selectPiece;

                Pieces.Add(piece);
            }
        }

        private void removeControlPoints(IEnumerable<PathControlPoint> controlPoints)
        {
            foreach (var point in controlPoints)
                Pieces.RemoveAll(p => p.ControlPoint == point);
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
                    return deleteSelected();
            }

            return false;
        }

        public bool OnReleased(PlatformAction action) => action.ActionMethod == PlatformActionMethod.Delete;

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

        private bool deleteSelected()
        {
            List<PathControlPoint> toRemove = Pieces.Where(p => p.IsSelected.Value).Select(p => p.ControlPoint).ToList();

            // Ensure that there are any points to be deleted
            if (toRemove.Count == 0)
                return false;

            foreach (var c in toRemove)
            {
                // The first control point in the slider must have a type, so take it from the previous "first" one
                // Todo: Should be handled within SliderPath itself
                if (c == slider.Path.ControlPoints[0] && slider.Path.ControlPoints.Count > 1 && slider.Path.ControlPoints[1].Type.Value == null)
                    slider.Path.ControlPoints[1].Type.Value = slider.Path.ControlPoints[0].Type.Value;

                slider.Path.ControlPoints.Remove(c);
            }

            // If there are 0 or 1 remaining control points, the slider is in a degenerate (single point) form and should be deleted
            if (slider.Path.ControlPoints.Count <= 1)
            {
                placementHandler?.Delete(slider);
                return true;
            }

            // The path will have a non-zero offset if the head is removed, but sliders don't support this behaviour since the head is positioned at the slider's position
            // So the slider needs to be offset by this amount instead, and all control points offset backwards such that the path is re-positioned at (0, 0)
            Vector2 first = slider.Path.ControlPoints[0].Position.Value;
            foreach (var c in slider.Path.ControlPoints)
                c.Position.Value -= first;
            slider.Position += first;

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
                    new OsuMenuItem($"Delete {"control point".ToQuantity(count, count > 1 ? ShowQuantityAs.Numeric : ShowQuantityAs.None)}", MenuItemType.Destructive, () => deleteSelected()),
                    new OsuMenuItem("Type")
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
