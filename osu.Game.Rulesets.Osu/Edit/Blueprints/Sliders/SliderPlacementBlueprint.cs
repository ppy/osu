// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderPlacementBlueprint : PlacementBlueprint
    {
        public new Objects.Slider HitObject => (Objects.Slider)base.HitObject;

        private SliderBodyPiece bodyPiece;
        private HitCirclePiece headCirclePiece;
        private HitCirclePiece tailCirclePiece;
        private PathControlPointVisualiser controlPointVisualiser;

        private InputManager inputManager;

        private PlacementState state;
        private PathControlPoint segmentStart;
        private PathControlPoint cursor;
        private int currentSegmentLength;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        public SliderPlacementBlueprint()
            : base(new Objects.Slider())
        {
            RelativeSizeAxes = Axes.Both;

            HitObject.Path.ControlPoints.Add(segmentStart = new PathControlPoint(Vector2.Zero, PathType.Linear));
            currentSegmentLength = 1;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                bodyPiece = new SliderBodyPiece(),
                headCirclePiece = new HitCirclePiece(),
                tailCirclePiece = new HitCirclePiece(),
                controlPointVisualiser = new PathControlPointVisualiser(HitObject, false)
            };

            setState(PlacementState.Initial);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            inputManager = GetContainingInputManager();
        }

        public override void UpdatePosition(Vector2 screenSpacePosition)
        {
            switch (state)
            {
                case PlacementState.Initial:
                    BeginPlacement();
                    HitObject.Position = ToLocalSpace(screenSpacePosition);
                    break;

                case PlacementState.Body:
                    ensureCursor();

                    // The given screen-space position may have been externally snapped, but the unsnapped position from the input manager
                    // is used instead since snapping control points doesn't make much sense
                    cursor.Position.Value = ToLocalSpace(inputManager.CurrentState.Mouse.Position) - HitObject.Position;
                    break;
            }
        }

        protected override bool OnClick(ClickEvent e)
        {
            switch (state)
            {
                case PlacementState.Initial:
                    beginCurve();
                    break;

                case PlacementState.Body:
                    if (e.Button != MouseButton.Left)
                        break;

                    // Find the last non-cursor control point and the respective drawable piece
                    var lastPoint = HitObject.Path.ControlPoints.LastOrDefault(p => p != cursor);
                    var lastPiece = controlPointVisualiser.Pieces.Single(p => p.ControlPoint == lastPoint);

                    if (lastPiece?.IsHovered == true)
                    {
                        Debug.Assert(lastPoint != null);

                        segmentStart = lastPoint;
                        segmentStart.Type.Value = PathType.Linear;

                        currentSegmentLength = 1;
                    }
                    else
                    {
                        ensureCursor();
                        cursor = null; // Detatch the cursor
                    }

                    return true;
            }

            return true;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            if (state == PlacementState.Body && e.Button == MouseButton.Right)
                endCurve();
            base.OnMouseUp(e);
        }

        private void beginCurve()
        {
            BeginPlacement(commitStart: true);
            setState(PlacementState.Body);
        }

        private void endCurve()
        {
            updateSlider();
            EndPlacement(true);
        }

        protected override void Update()
        {
            base.Update();
            updateSlider();
        }

        private void updatePathType()
        {
            switch (currentSegmentLength)
            {
                case 1:
                case 2:
                    segmentStart.Type.Value = PathType.Linear;
                    break;

                case 3:
                    segmentStart.Type.Value = PathType.PerfectCurve;
                    break;

                default:
                    segmentStart.Type.Value = PathType.Bezier;
                    break;
            }
        }

        private void ensureCursor()
        {
            if (cursor == null)
            {
                HitObject.Path.ControlPoints.Add(cursor = new PathControlPoint { Position = { Value = Vector2.Zero } });
                currentSegmentLength++;

                updatePathType();

                Logger.Log("Set cursor");
            }
        }

        private void updateSlider()
        {
            HitObject.Path.ExpectedDistance.Value = composer?.GetSnappedDistanceFromDistance(HitObject.StartTime, (float)HitObject.Path.CalculatedDistance) ?? (float)HitObject.Path.CalculatedDistance;

            bodyPiece.UpdateFrom(HitObject);
            headCirclePiece.UpdateFrom(HitObject.HeadCircle);
            tailCirclePiece.UpdateFrom(HitObject.TailCircle);
        }

        private void setState(PlacementState newState)
        {
            state = newState;
        }

        private enum PlacementState
        {
            Initial,
            Body,
        }
    }
}
