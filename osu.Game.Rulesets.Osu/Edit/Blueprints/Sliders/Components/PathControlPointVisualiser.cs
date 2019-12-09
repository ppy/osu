// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Rulesets.Objects;
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

            while (slider.Path.ControlPoints.Count > Pieces.Count)
            {
                var piece = new PathControlPointPiece(slider, Pieces.Count);

                if (allowSelection)
                    piece.RequestSelection = selectPiece;

                Pieces.Add(piece);
            }

            while (slider.Path.ControlPoints.Count < Pieces.Count)
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
            List<PathControlPoint> toRemove = Pieces.Where(p => p.IsSelected.Value).Select(p => slider.Path.ControlPoints[p.Index]).ToList();

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

            // If there are 0 remaining control points, treat the slider as being deleted
            if (slider.Path.ControlPoints.Count == 0)
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
