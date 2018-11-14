// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointVisualiser : CompositeDrawable
    {
        private readonly Slider slider;

        private readonly Container<PathControlPointPiece> pieces;

        public PathControlPointVisualiser(Slider slider)
        {
            this.slider = slider;

            InternalChild = pieces = new Container<PathControlPointPiece> { RelativeSizeAxes = Axes.Both };

            slider.PathChanged += _ => updatePathControlPoints();
            updatePathControlPoints();
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
