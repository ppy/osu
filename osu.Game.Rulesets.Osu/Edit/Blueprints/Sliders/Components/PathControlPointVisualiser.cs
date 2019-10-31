// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable
    {
        public Action<Vector2[]> ControlPointsChanged;

        internal readonly Container<PathControlPointPiece> Pieces;
        private readonly Slider slider;

        private InputManager inputManager;

        [Resolved(CanBeNull = true)]
        private IPlacementHandler placementHandler { get; set; }

        public PathControlPointVisualiser(Slider slider)
        {
            this.slider = slider;

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
                Pieces.Add(new PathControlPointPiece(slider, Pieces.Count)
                {
                    ControlPointsChanged = c => ControlPointsChanged?.Invoke(c),
                    RequestSelection = selectPiece
                });
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

        private void selectPiece(int index)
        {
            if (inputManager.CurrentState.Keyboard.ControlPressed)
                Pieces[index].IsSelected.Value = true;
            else
            {
                foreach (var piece in Pieces)
                    piece.IsSelected.Value = piece.Index == index;
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    var newControlPoints = new List<Vector2>();

                    foreach (var piece in pieces)
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
                    slider.Position = slider.Position + first;
                    ControlPointsChanged?.Invoke(newControlPoints.ToArray());

                    return true;
            }

            return false;
        }
    }
}
