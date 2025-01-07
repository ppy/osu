// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public partial class RectangularPositionSnapGrid : LinedPositionSnapGrid
    {
        /// <summary>
        /// The spacing between grid lines of this <see cref="RectangularPositionSnapGrid"/>.
        /// </summary>
        public Bindable<Vector2> Spacing { get; } = new Bindable<Vector2>(Vector2.One);

        /// <summary>
        /// The rotation in degrees of the grid lines of this <see cref="RectangularPositionSnapGrid"/>.
        /// </summary>
        public BindableFloat GridLineRotation { get; } = new BindableFloat();

        public RectangularPositionSnapGrid()
        {
            Spacing.BindValueChanged(_ => GridCache.Invalidate());
            GridLineRotation.BindValueChanged(_ => GridCache.Invalidate());
        }

        protected override void CreateContent()
        {
            var drawSize = DrawSize;
            var rot = Quaternion.FromAxisAngle(Vector3.UnitZ, MathHelper.DegreesToRadians(GridLineRotation.Value));

            GenerateGridLines(Vector2.Transform(new Vector2(0, -Spacing.Value.Y), rot), drawSize);
            GenerateGridLines(Vector2.Transform(new Vector2(0, Spacing.Value.Y), rot), drawSize);

            GenerateGridLines(Vector2.Transform(new Vector2(-Spacing.Value.X, 0), rot), drawSize);
            GenerateGridLines(Vector2.Transform(new Vector2(Spacing.Value.X, 0), rot), drawSize);

            GenerateOutline(drawSize);
        }

        public override Vector2 GetSnappedPosition(Vector2 original)
        {
            Vector2 relativeToStart = GeometryUtils.RotateVector(original - StartPosition.Value, GridLineRotation.Value);
            Vector2 offset = Vector2.Divide(relativeToStart, Spacing.Value);
            Vector2 roundedOffset = new Vector2(MathF.Round(offset.X), MathF.Round(offset.Y));

            return StartPosition.Value + GeometryUtils.RotateVector(Vector2.Multiply(roundedOffset, Spacing.Value), -GridLineRotation.Value);
        }
    }
}
