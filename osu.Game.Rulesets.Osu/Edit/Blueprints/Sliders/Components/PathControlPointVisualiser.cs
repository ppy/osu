// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable
    {
        public Action<Vector2[]> ControlPointsChanged;

        private readonly Slider slider;

        private readonly Container<PathControlPointPiece> pieces;

        public PathControlPointVisualiser(Slider slider)
        {
            this.slider = slider;

            InternalChild = pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both };
        }

        protected override void Update()
        {
            base.Update();

            while (slider.Path.ControlPoints.Length > pieces.Count)
                pieces.Add(new PathControlPointPiece(slider, pieces.Count) { ControlPointsChanged = c => ControlPointsChanged?.Invoke(c) });
            while (slider.Path.ControlPoints.Length < pieces.Count)
                pieces.Remove(pieces[pieces.Count - 1]);
        }
    }
}
