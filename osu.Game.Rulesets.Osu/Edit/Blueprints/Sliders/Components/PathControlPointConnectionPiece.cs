// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    /// <summary>
    /// A visualisation of the line between two <see cref="PathControlPointPiece"/>s.
    /// </summary>
    public class PathControlPointConnectionPiece : CompositeDrawable
    {
        public readonly PathControlPoint ControlPoint;

        private readonly Path path;
        private readonly Slider slider;
        public int ControlPointIndex { get; set; }

        private IBindable<Vector2> sliderPosition;
        private IBindable<int> pathVersion;

        public PathControlPointConnectionPiece(Slider slider, int controlPointIndex)
        {
            this.slider = slider;
            ControlPointIndex = controlPointIndex;

            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            ControlPoint = slider.Path.ControlPoints[controlPointIndex];

            InternalChild = path = new SmoothPath
            {
                Anchor = Anchor.Centre,
                PathRadius = 1
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sliderPosition = slider.PositionBindable.GetBoundCopy();
            sliderPosition.BindValueChanged(_ => updateConnectingPath());

            pathVersion = slider.Path.Version.GetBoundCopy();
            pathVersion.BindValueChanged(_ => updateConnectingPath());

            updateConnectingPath();
        }

        /// <summary>
        /// Updates the path connecting this control point to the next one.
        /// </summary>
        private void updateConnectingPath()
        {
            Position = slider.StackedPosition + ControlPoint.Position.Value;

            path.ClearVertices();

            int nextIndex = ControlPointIndex + 1;
            if (nextIndex == 0 || nextIndex >= slider.Path.ControlPoints.Count)
                return;

            path.AddVertex(Vector2.Zero);
            path.AddVertex(slider.Path.ControlPoints[nextIndex].Position.Value - ControlPoint.Position.Value);

            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
        }
    }
}
