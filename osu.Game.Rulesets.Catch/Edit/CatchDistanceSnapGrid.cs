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
using osu.Game.Rulesets.Catch.UI;
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

        private const double max_vertical_line_length_in_time = CatchPlayfield.WIDTH / Catcher.BASE_WALK_SPEED;

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

            double currentTime = hitObjectContainer.Time.Current;

            for (int i = 0; i < velocities.Length; i++)
            {
                double velocity = velocities[i];

                // The line ends at the top of the playfield.
                double endTime = hitObjectContainer.TimeAtPosition(-hitObjectContainer.DrawHeight, currentTime);

                // Non-vertical lines are cut at the sides of the playfield.
                // Vertical lines are cut at some reasonable length.
                if (velocity > 0)
                    endTime = Math.Min(endTime, StartTime + (CatchPlayfield.WIDTH - StartX) / velocity);
                else if (velocity < 0)
                    endTime = Math.Min(endTime, StartTime + StartX / -velocity);
                else
                    endTime = Math.Min(endTime, StartTime + max_vertical_line_length_in_time);

                Vector2[] lineVertices = verticalLineVertices[i];
                lineVertices[0] = calculatePosition(velocity, StartTime);
                lineVertices[1] = calculatePosition(velocity, endTime);

                var verticalPath = verticalPaths[i];
                verticalPath.Vertices = verticalLineVertices[i];
                verticalPath.OriginPosition = verticalPath.PositionInBoundingBox(Vector2.Zero);
            }

            Vector2 calculatePosition(double velocity, double time)
            {
                // Don't draw inverted lines.
                time = Math.Max(time, StartTime);

                float x = StartX + (float)((time - StartTime) * velocity);
                float y = hitObjectContainer.PositionAtTime(time, currentTime);
                return new Vector2(x, y);
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
