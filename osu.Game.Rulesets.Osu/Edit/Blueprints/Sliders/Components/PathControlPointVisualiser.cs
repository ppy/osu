// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable, IKeyBindingHandler<PlatformAction>, IHasContextMenu
    {
        public Action<Vector2[]> ControlPointsChanged;

        internal readonly Container<PathControlPointPiece> Pieces;
        private readonly Slider slider;
        private readonly bool allowSelection;

        private InputManager inputManager;

        [Resolved(CanBeNull = true)]
        private IPlacementHandler placementHandler { get; set; }

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
        }

        protected override void Update()
        {
            base.Update();

            while (slider.Path.ControlPoints.Length > Pieces.Count)
            {
                var piece = new PathControlPointPiece(slider, Pieces.Count)
                {
                    ControlPointsChanged = c => ControlPointsChanged?.Invoke(c),
                };

                if (allowSelection)
                    piece.RequestSelection = selectPiece;

                Pieces.Add(piece);
            }

            while (slider.Path.ControlPoints.Length < Pieces.Count)
                Pieces.Remove(Pieces[Pieces.Count - 1]);
        }

        protected override bool OnClick(ClickEvent e)
        {
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;
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

        private void selectPiece(int index, MouseButtonEvent e)
        {
            if (e.Button == MouseButton.Left && inputManager.CurrentState.Keyboard.ControlPressed)
                Pieces[index].IsSelected.Toggle();
            else
            {
                foreach (var piece in Pieces)
                    piece.IsSelected.Value = piece.Index == index;
            }
        }

        private bool deleteSelected()
        {
            var newControlPoints = new List<Vector2>();

            foreach (var piece in Pieces)
            {
                if (!piece.IsSelected.Value)
                    newControlPoints.Add(slider.Path.ControlPoints[piece.Index]);
            }

            // Ensure that there are any points to be deleted
            if (newControlPoints.Count == slider.Path.ControlPoints.Length)
                return false;

            // If there are 0 remaining control points, treat the slider as being deleted
            if (newControlPoints.Count == 0)
            {
                placementHandler?.Delete(slider);
                return true;
            }

            // Make control points relative
            Vector2 first = newControlPoints[0];
            for (int i = 0; i < newControlPoints.Count; i++)
                newControlPoints[i] = newControlPoints[i] - first;

            // The slider's position defines the position of the first control point, and all further control points are relative to that point
            slider.Position += first;

            // Since pieces are re-used, they will not point to the deleted control points while remaining selected
            foreach (var piece in Pieces)
                piece.IsSelected.Value = false;

            ControlPointsChanged?.Invoke(newControlPoints.ToArray());
            return true;
        }

        public MenuItem[] ContextMenuItems
        {
            get
            {
                if (!Pieces.Any(p => p.IsHovered))
                    return null;

                int selectedPoints = Pieces.Count(p => p.IsSelected.Value);

                if (selectedPoints == 0)
                    return null;

                return new MenuItem[]
                {
                    new OsuMenuItem($"Delete {"control point".ToQuantity(selectedPoints)}", MenuItemType.Destructive, () => deleteSelected())
                };
            }
        }
    }
}
