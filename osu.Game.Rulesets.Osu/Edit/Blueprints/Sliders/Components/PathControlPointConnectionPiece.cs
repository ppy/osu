// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    /// <summary>
    /// A visualisation of the line between two <see cref="PathControlPointPiece{T}"/>s.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="OsuHitObject"/> which this <see cref="PathControlPointConnectionPiece{T}"/> visualises.</typeparam>
    public partial class PathControlPointConnectionPiece<T> : CompositeDrawable where T : OsuHitObject, IHasPath
    {
        public readonly PathControlPoint ControlPoint;

        private readonly Path path;
        private readonly T hitObject;
        public int ControlPointIndex { get; set; }

        private IBindable<Vector2> hitObjectPosition;
        private IBindable<int> pathVersion;

        public PathControlPointConnectionPiece(T hitObject, int controlPointIndex)
        {
            this.hitObject = hitObject;
            ControlPointIndex = controlPointIndex;

            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            ControlPoint = hitObject.Path.ControlPoints[controlPointIndex];

            InternalChild = path = new SmoothPath
            {
                Anchor = Anchor.Centre,
                PathRadius = 1
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            hitObjectPosition = hitObject.PositionBindable.GetBoundCopy();
            hitObjectPosition.BindValueChanged(_ => Scheduler.AddOnce(updateConnectingPath));

            pathVersion = hitObject.Path.Version.GetBoundCopy();
            pathVersion.BindValueChanged(_ => Scheduler.AddOnce(updateConnectingPath));

            updateConnectingPath();
        }

        /// <summary>
        /// Updates the path connecting this control point to the next one.
        /// </summary>
        private void updateConnectingPath()
        {
            Position = hitObject.StackedPosition + ControlPoint.Position;

            path.ClearVertices();

            int nextIndex = ControlPointIndex + 1;
            if (nextIndex == 0 || nextIndex >= hitObject.Path.ControlPoints.Count)
                return;

            path.AddVertex(Vector2.Zero);
            path.AddVertex(hitObject.Path.ControlPoints[nextIndex].Position - ControlPoint.Position);

            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
        }
    }
}
