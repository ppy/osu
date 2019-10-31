// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable
    {
        public Action<Vector2[]> ControlPointsChanged;

        internal Container<PathControlPointPiece> Pieces { get; }

        private readonly Slider slider;

        public PathControlPointVisualiser(Slider slider)
        {
            this.slider = slider;

            RelativeSizeAxes = Axes.Both;

            InternalChild = Pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both };
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
            foreach (var piece in Pieces)
                piece.IsSelected.Value = piece.Index == index;
        }
    }
}
