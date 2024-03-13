// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public partial class PlacementEditablePath : EditablePath
    {
        /// <summary>
        /// The original position of the last added vertex.
        /// This is not same as the last vertex of the current path because the vertex ordering can change.
        /// </summary>
        private JuiceStreamPathVertex lastVertex;

        public PlacementEditablePath(Func<float, double> positionToTime)
            : base(positionToTime)
        {
        }

        public void AddNewVertex()
        {
            var endVertex = Vertices[^1];
            int index = AddVertex(endVertex.Time, endVertex.X);

            for (int i = 0; i < VertexCount; i++)
            {
                VertexStates[i].IsSelected = i == index;
                VertexStates[i].IsFixed = i != index;
                VertexStates[i].VertexBeforeChange = Vertices[i];
            }

            lastVertex = Vertices[index];
        }

        /// <summary>
        /// Move the vertex added by <see cref="AddNewVertex"/> in the last time.
        /// </summary>
        public void MoveLastVertex(Vector2 screenSpacePosition)
        {
            Vector2 position = ToRelativePosition(screenSpacePosition);
            double timeDelta = PositionToTime(position.Y) - lastVertex.Time;
            float xDelta = position.X - lastVertex.X;
            MoveSelectedVertices(timeDelta, xDelta);
        }
    }
}
