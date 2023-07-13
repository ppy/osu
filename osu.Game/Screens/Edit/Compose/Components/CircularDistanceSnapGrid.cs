// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Edit.Compose.Components
{
    public abstract partial class CircularDistanceSnapGrid : DistanceSnapGrid
    {
        [Resolved]
        private EditorClock editorClock { get; set; } = null!;

        protected CircularDistanceSnapGrid(HitObject referenceObject, Vector2 startPosition, double startTime, double? endTime = null)
            : base(referenceObject, startPosition, startTime, endTime)
        {
        }

        protected override void CreateContent()
        {
            const float crosshair_thickness = 1;
            const float crosshair_max_size = 10;

            AddRangeInternal(new[]
            {
                new Box
                {
                    Origin = Anchor.Centre,
                    Position = StartPosition,
                    Width = crosshair_thickness,
                    EdgeSmoothness = new Vector2(1),
                    Height = Math.Min(crosshair_max_size, DistanceBetweenTicks * 2),
                },
                new Box
                {
                    Origin = Anchor.Centre,
                    Position = StartPosition,
                    EdgeSmoothness = new Vector2(1),
                    Width = Math.Min(crosshair_max_size, DistanceBetweenTicks * 2),
                    Height = crosshair_thickness,
                }
            });

            float dx = Math.Max(StartPosition.X, DrawWidth - StartPosition.X);
            float dy = Math.Max(StartPosition.Y, DrawHeight - StartPosition.Y);
            float maxDistance = new Vector2(dx, dy).Length;
            int requiredCircles = Math.Min(MaxIntervals, (int)(maxDistance / DistanceBetweenTicks));

            // We need to offset the drawn lines to the next valid snap for the currently selected divisor.
            //
            // Picture the scenario where the user has just placed an object on a 1/2 snap, then changes to
            // 1/3 snap and expects to be able to place the next object on a valid 1/3 snap, regardless of the
            // fact that the 1/2 snap reference object is not valid for 1/3 snapping.
            float offset = SnapProvider.FindSnappedDistance(ReferenceObject, 0);

            for (int i = 0; i < requiredCircles; i++)
            {
                const float thickness = 4;
                float diameter = (offset + (i + 1) * DistanceBetweenTicks + thickness / 2) * 2;

                AddInternal(new Ring(ReferenceObject, GetColourForIndexFromPlacement(i))
                {
                    Position = StartPosition,
                    Origin = Anchor.Centre,
                    Size = new Vector2(diameter),
                    InnerRadius = thickness * 1f / diameter,
                });
            }
        }

        public override (Vector2 position, double time) GetSnappedPosition(Vector2 position)
        {
            if (MaxIntervals == 0)
                return (StartPosition, StartTime);

            // This grid implementation factors in the user's distance spacing specification,
            // which is usually not considered by an `IDistanceSnapProvider`.
            float distanceSpacingMultiplier = (float)DistanceSpacingMultiplier.Value;

            Vector2 travelVector = (position - StartPosition);

            // We need a non-zero travel vector in order to find a valid direction.
            if (travelVector == Vector2.Zero)
                travelVector = new Vector2(0, -1);

            float travelLength = travelVector.Length;

            // FindSnappedDistance will always round down, but we want to potentially round upwards.
            travelLength += DistanceBetweenTicks / 2;

            // We never want to snap towards zero.
            if (travelLength < DistanceBetweenTicks)
                travelLength = DistanceBetweenTicks;

            float snappedDistance = LimitedDistanceSnap.Value
                ? SnapProvider.DurationToDistance(ReferenceObject, editorClock.CurrentTime - ReferenceObject.GetEndTime())
                // When interacting with the resolved snap provider, the distance spacing multiplier should first be removed
                // to allow for snapping at a non-multiplied ratio.
                : SnapProvider.FindSnappedDistance(ReferenceObject, travelLength / distanceSpacingMultiplier);

            double snappedTime = StartTime + SnapProvider.DistanceToDuration(ReferenceObject, snappedDistance);

            if (snappedTime > LatestEndTime)
            {
                double tickLength = Beatmap.GetBeatLengthAtTime(StartTime);

                snappedDistance = SnapProvider.DurationToDistance(ReferenceObject, MaxIntervals * tickLength);
                snappedTime = StartTime + SnapProvider.DistanceToDuration(ReferenceObject, snappedDistance);
            }

            // The multiplier can then be reapplied to the final position.
            Vector2 snappedPosition = StartPosition + travelVector.Normalized() * snappedDistance * distanceSpacingMultiplier;

            return (snappedPosition, snappedTime);
        }

        private partial class Ring : CircularProgress
        {
            [Resolved]
            private IDistanceSnapProvider snapProvider { get; set; } = null!;

            [Resolved]
            private EditorClock? editorClock { get; set; }

            private readonly HitObject referenceObject;

            private readonly Color4 baseColour;

            public Ring(HitObject referenceObject, Color4 baseColour)
            {
                this.referenceObject = referenceObject;

                Colour = this.baseColour = baseColour;

                Current.Value = 1;
            }

            protected override void Update()
            {
                base.Update();

                if (editorClock == null)
                    return;

                float distanceSpacingMultiplier = (float)snapProvider.DistanceSpacingMultiplier.Value;
                double timeFromReferencePoint = editorClock.CurrentTime - referenceObject.GetEndTime();

                float distanceForCurrentTime = snapProvider.DurationToDistance(referenceObject, timeFromReferencePoint)
                                               * distanceSpacingMultiplier;

                float timeBasedAlpha = 1 - Math.Clamp(Math.Abs(distanceForCurrentTime - Size.X / 2) / 30, 0, 1);

                Colour = baseColour.Opacity(Math.Max(baseColour.A, timeBasedAlpha));
            }
        }
    }
}
