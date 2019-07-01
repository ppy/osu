// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : SliderPiece
    {
        private readonly Slider slider;

        private readonly Container<PathControlPointPiece> pieces;

        public PathControlPointVisualiser(Slider slider)
            : base(slider)
        {
            this.slider = slider;

            InternalChild = pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both };
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            PathBindable.BindValueChanged(_ => updatePathControlPoints(), true);
        }

        private void updatePathControlPoints()
        {
            while (slider.Path.ControlPoints.Length > pieces.Count)
                pieces.Add(new PathControlPointPiece(slider, pieces.Count));
            while (slider.Path.ControlPoints.Length < pieces.Count)
                pieces.Remove(pieces[pieces.Count - 1]);
        }
    }
}
