// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Bindables;
using osu.Framework.Graphics.Lines;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    /// <summary>
    /// A visualisation of the lines between <see cref="PathControlPointPiece{T}"/>s.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="OsuHitObject"/> which this <see cref="PathControlPointConnection{T}"/> visualises.</typeparam>
    public partial class PathControlPointConnection<T> : SmoothPath where T : OsuHitObject, IHasPath
    {
        private readonly T hitObject;

        private IBindable<Vector2> hitObjectPosition;
        private IBindable<int> pathVersion;
        private IBindable<int> stackHeight;

        public PathControlPointConnection(T hitObject)
        {
            this.hitObject = hitObject;
            PathRadius = 1;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            hitObjectPosition = hitObject.PositionBindable.GetBoundCopy();
            hitObjectPosition.BindValueChanged(_ => Scheduler.AddOnce(updateConnectingPath));

            pathVersion = hitObject.Path.Version.GetBoundCopy();
            pathVersion.BindValueChanged(_ => Scheduler.AddOnce(updateConnectingPath));

            stackHeight = hitObject.StackHeightBindable.GetBoundCopy();
            stackHeight.BindValueChanged(_ => updateConnectingPath());

            updateConnectingPath();
        }

        /// <summary>
        /// Updates the path connecting this control point to the next one.
        /// </summary>
        private void updateConnectingPath()
        {
            Position = hitObject.StackedPosition;

            ClearVertices();

            foreach (var controlPoint in hitObject.Path.ControlPoints)
                AddVertex(controlPoint.Position);

            OriginPosition = PositionInBoundingBox(Vector2.Zero);
        }
    }
}
