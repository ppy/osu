// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Catch.Objects;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit.Blueprints.Components
{
    public abstract partial class EditablePath : CompositeDrawable
    {
        public int PathId => path.InvalidationID;

        public IReadOnlyList<JuiceStreamPathVertex> Vertices => path.Vertices;

        public int VertexCount => path.Vertices.Count;

        protected readonly Func<float, double> PositionToTime;

        protected IReadOnlyList<VertexState> VertexStates => vertexStates;

        private readonly JuiceStreamPath path = new JuiceStreamPath();

        // Invariant: `path.Vertices.Count == vertexStates.Count`
        private readonly List<VertexState> vertexStates = new List<VertexState>
        {
            new VertexState { IsFixed = true }
        };

        private readonly List<VertexState> previousVertexStates = new List<VertexState>();

        [Resolved]
        private IBeatSnapProvider? beatSnapProvider { get; set; }

        protected EditablePath(Func<float, double> positionToTime)
        {
            PositionToTime = positionToTime;

            Anchor = Anchor.BottomLeft;
        }

        public void UpdateFrom(ScrollingHitObjectContainer hitObjectContainer, JuiceStream hitObject)
        {
            while (path.Vertices.Count < InternalChildren.Count)
                RemoveInternal(InternalChildren[^1], true);

            while (InternalChildren.Count < path.Vertices.Count)
                AddInternal(new VertexPiece());

            double timeToYFactor = -hitObjectContainer.LengthAtTime(hitObject.StartTime, hitObject.StartTime + 1);

            for (int i = 0; i < VertexCount; i++)
            {
                var piece = (VertexPiece)InternalChildren[i];
                var vertex = path.Vertices[i];
                piece.Position = new Vector2(vertex.X, (float)(vertex.Time * timeToYFactor));
                piece.UpdateFrom(vertexStates[i]);
            }
        }

        public void InitializeFromHitObject(JuiceStream hitObject)
        {
            var sliderPath = hitObject.Path;
            path.ConvertFromSliderPath(sliderPath, hitObject.Velocity);

            // If the original slider path has non-linear type segments, resample the vertices at nested hit object times to reduce the number of vertices.
            if (sliderPath.ControlPoints.Any(p => p.Type != null && p.Type != PathType.LINEAR))
            {
                path.ResampleVertices(hitObject.NestedHitObjects
                                               .Skip(1).TakeWhile(h => !(h is Fruit)) // Only droplets in the first span are used.
                                               .Select(h => h.StartTime - hitObject.StartTime));
            }

            vertexStates.Clear();
            vertexStates.AddRange(path.Vertices.Select((_, i) => new VertexState
            {
                IsFixed = i == 0
            }));
        }

        public void UpdateHitObjectFromPath(JuiceStream hitObject)
        {
            // The SV setting may need to be changed for the current path.
            var svBindable = hitObject.SliderVelocityMultiplierBindable;
            double svToVelocityFactor = hitObject.Velocity / svBindable.Value;
            double requiredVelocity = path.ComputeRequiredVelocity();

            // The value is pre-rounded here because setting it to the bindable will rounded to the nearest value
            // but it should be always rounded up to satisfy the required minimum velocity condition.
            //
            // This is rounded to integers instead of using the precision of the bindable
            // because it results in a smaller number of non-redundant control points.
            //
            // The value is clamped here by the bindable min and max values.
            // In case the required velocity is too large, the path is not preserved.
            svBindable.Value = Math.Ceiling(requiredVelocity / svToVelocityFactor);

            path.ConvertToSliderPath(hitObject.Path, hitObject.LegacyConvertedY, hitObject.Velocity);

            if (beatSnapProvider == null) return;

            double endTime = hitObject.StartTime + path.Duration;
            double snappedEndTime = beatSnapProvider.SnapTime(endTime, hitObject.StartTime);
            hitObject.Path.ExpectedDistance.Value = (snappedEndTime - hitObject.StartTime) * hitObject.Velocity;
        }

        public Vector2 ToRelativePosition(Vector2 screenSpacePosition)
        {
            return ToLocalSpace(screenSpacePosition) - new Vector2(0, DrawHeight);
        }

        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;

        protected int AddVertex(double time, float x)
        {
            int index = path.InsertVertex(time);
            path.SetVertexPosition(index, x);
            vertexStates.Insert(index, new VertexState());

            correctFixedVertexPositions();

            Debug.Assert(vertexStates.Count == VertexCount);
            return index;
        }

        protected bool RemoveVertex(int index)
        {
            if (index < 0 || index >= path.Vertices.Count)
                return false;

            if (vertexStates[index].IsFixed)
                return false;

            path.RemoveVertices((_, i) => i == index);

            vertexStates.RemoveAt(index);
            if (vertexStates.Count == 0)
                vertexStates.Add(new VertexState());

            Debug.Assert(vertexStates.Count == VertexCount);
            return true;
        }

        protected void MoveSelectedVertices(double timeDelta, float xDelta)
        {
            // Because the vertex list may be reordered due to time change, the state list must be reordered as well.
            previousVertexStates.Clear();
            previousVertexStates.AddRange(vertexStates);

            // We will recreate the path from scratch. Note that `Clear` leaves the first vertex.
            int vertexCount = VertexCount;
            path.Clear();
            vertexStates.RemoveRange(1, vertexCount - 1);

            for (int i = 1; i < vertexCount; i++)
            {
                var state = previousVertexStates[i];
                double time = state.VertexBeforeChange.Time;
                if (state.IsSelected)
                    time += timeDelta;

                int newIndex = path.InsertVertex(Math.Max(0, time));
                vertexStates.Insert(newIndex, state);
            }

            // First, restore positions of the non-selected vertices.
            for (int i = 0; i < vertexCount; i++)
            {
                if (!vertexStates[i].IsSelected && !vertexStates[i].IsFixed)
                    path.SetVertexPosition(i, vertexStates[i].VertexBeforeChange.X);
            }

            // Then, move the selected vertices.
            for (int i = 0; i < vertexCount; i++)
            {
                if (vertexStates[i].IsSelected && !vertexStates[i].IsFixed)
                    path.SetVertexPosition(i, vertexStates[i].VertexBeforeChange.X + xDelta);
            }

            // Finally, correct the position of fixed vertices.
            correctFixedVertexPositions();
        }

        private void correctFixedVertexPositions()
        {
            for (int i = 0; i < VertexCount; i++)
            {
                if (vertexStates[i].IsFixed)
                    path.SetVertexPosition(i, vertexStates[i].VertexBeforeChange.X);
            }
        }
    }
}
