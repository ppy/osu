// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.UI.Scrolling;
using osuTK;

namespace osu.Game.Rulesets.Catch.Edit
{
    /// <summary>
    /// The guide lines used in the osu!catch editor to compose patterns that can be caught with constant speed.
    /// Currently, only forward placement (an object is snapped based on the previous object, not the opposite) is supported.
    /// </summary>
    public class CatchDistanceSnapGrid : CompositeDrawable
    {
        public double StartTime { get; set; }

        public float StartX { get; set; }

        private readonly double[] velocities;

        private readonly List<Path> verticalPaths = new List<Path>();

        private readonly List<Vector2[]> verticalLineVertices = new List<Vector2[]>();

        [Resolved]
        private Playfield playfield { get; set; }

        private ScrollingHitObjectContainer hitObjectContainer => (ScrollingHitObjectContainer)playfield.HitObjectContainer;

        public CatchDistanceSnapGrid(double[] velocities)
        {
            RelativeSizeAxes = Axes.Both;
            Anchor = Anchor.BottomLeft;

            this.velocities = velocities;

            for (int i = 0; i < velocities.Length; i++)
            {
                verticalPaths.Add(new SmoothPath
                {
                    PathRadius = 2,
                    Alpha = 0.5f,
                });

                verticalLineVertices.Add(new[] { Vector2.Zero, Vector2.Zero });
            }

            AddRangeInternal(verticalPaths);
        }

        protected override void Update()
        {
            base.Update();

            float startY = hitObjectContainer.PositionAtTime(StartTime);

            for (int i = 0; i < velocities.Length; i++)
            {
                double velocity = velocities[i];
                var verticalPath = verticalPaths[i];

                // The line ends at the top of the screen.
                double topScreenTime = hitObjectContainer.TimeAtPosition(-hitObjectContainer.DrawHeight, hitObjectContainer.Time.Current);
                double endTime = Math.Max(StartTime, topScreenTime);

                float x = (float)((endTime - StartTime) * velocity);
                float y = hitObjectContainer.PositionAtTime(endTime, StartTime);

                Vector2[] lineVertices = verticalLineVertices[i];
                lineVertices[0] = new Vector2(StartX, startY);
                lineVertices[1] = lineVertices[0] + new Vector2(x, y);

                verticalPath.Vertices = verticalLineVertices[i];
                verticalPath.OriginPosition = verticalPath.PositionInBoundingBox(Vector2.Zero);
            }
        }

        [CanBeNull]
        public SnapResult GetSnappedPosition(Vector2 screenSpacePosition)
        {
            double time = hitObjectContainer.TimeAtScreenSpacePosition(screenSpacePosition);

            // If the cursor is below the distance snap grid, snap to the origin.
            // Not returning `null` to retain the continuous snapping behavior when the cursor is slightly below the origin.
            // This behavior is not currently visible in the editor because editor chooses the snap start time based on the mouse position.
            if (time <= StartTime)
            {
                float y = hitObjectContainer.PositionAtTime(StartTime);
                Vector2 originPosition = hitObjectContainer.ToScreenSpace(new Vector2(StartX, y));
                return new SnapResult(originPosition, StartTime);
            }

            return enumerateSnappingCandidates(time)
                   .OrderBy(pos => Vector2.DistanceSquared(screenSpacePosition, pos.ScreenSpacePosition))
                   .FirstOrDefault();
        }

        private IEnumerable<SnapResult> enumerateSnappingCandidates(double time)
        {
            float y = hitObjectContainer.PositionAtTime(time);

            foreach (double velocity in velocities)
            {
                float x = (float)(StartX + (time - StartTime) * velocity);
                Vector2 screenSpacePosition = hitObjectContainer.ToScreenSpace(new Vector2(x, y + hitObjectContainer.DrawHeight));
                yield return new SnapResult(screenSpacePosition, time);
            }
        }

        protected override bool ComputeIsMaskedAway(RectangleF maskingBounds) => false;
    }
}
