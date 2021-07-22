// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Catch.Objects;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public class PlacementEditablePath : EditablePath
    {
        private JuiceStreamPathVertex originalNewVertex;

        public PlacementEditablePath(Func<float, double> positionToDistance)
            : base(positionToDistance)
        {
        }

        public void AddNewVertex()
        {
            var endVertex = Vertices[^1];
            int index = AddVertex(endVertex.Distance, endVertex.X);

            for (int i = 0; i < VertexCount; i++)
            {
                VertexStates[i].IsSelected = i == index;
                VertexStates[i].VertexBeforeChange = Vertices[i];
            }

            originalNewVertex = Vertices[index];
        }

        /// <summary>
        /// Move the vertex added by <see cref="AddNewVertex"/> in the last time.
        /// </summary>
        public void MoveLastVertex(Vector2 screenSpacePosition)
        {
            Vector2 position = ToRelativePosition(screenSpacePosition);
            double distanceDelta = PositionToDistance(position.Y) - originalNewVertex.Distance;
            float xDelta = position.X - originalNewVertex.X;
            MoveSelectedVertices(distanceDelta, xDelta);
        }
    }
}
