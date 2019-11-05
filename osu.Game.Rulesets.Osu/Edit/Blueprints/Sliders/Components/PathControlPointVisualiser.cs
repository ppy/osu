// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable
    {
        public Action<Vector2[]> ControlPointsChanged;

        internal readonly Container<PathControlPointPiece> Pieces;
        private readonly Slider slider;
        private readonly bool allowSelection;

        private InputManager inputManager;

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

        private void selectPiece(int index)
        {
            if (inputManager.CurrentState.Keyboard.ControlPressed)
                Pieces[index].IsSelected.Toggle();
            else
            {
                foreach (var piece in Pieces)
                    piece.IsSelected.Value = piece.Index == index;
            }
        }
    }
}
