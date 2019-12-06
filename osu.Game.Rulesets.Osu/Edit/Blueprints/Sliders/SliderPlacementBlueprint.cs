// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
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

        private InputManager inputManager;

        private PlacementState state;

        [Resolved(CanBeNull = true)]
        private HitObjectComposer composer { get; set; }

        public SliderPlacementBlueprint()
            : base(new Objects.Slider())
        {
            RelativeSizeAxes = Axes.Both;
            HitObject.Path.ControlPoints.Add(new PathControlPoint { Position = { Value = Vector2.Zero } });
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                bodyPiece = new SliderBodyPiece(),
                headCirclePiece = new HitCirclePiece(),
                tailCirclePiece = new HitCirclePiece(),
                new PathControlPointVisualiser(HitObject, false)
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
                    HitObject.Position = ToLocalSpace(screenSpacePosition);
                    break;

                case PlacementState.Body:
                    // The given screen-space position may have been externally snapped, but the unsnapped position from the input manager
                    // is used instead since snapping control points doesn't make much sense
                    HitObject.Path.ControlPoints.Last().Position.Value = ToLocalSpace(inputManager.CurrentState.Mouse.Position) - HitObject.Position;
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
                    switch (e.Button)
                    {
                        case MouseButton.Left:
                            HitObject.Path.ControlPoints.Add(new PathControlPoint { Position = { Value = HitObject.Path.ControlPoints.Last().Position.Value } });
                            break;
                    }

                    break;
            }

            return true;
        }

        protected override bool OnMouseUp(MouseUpEvent e)
        {
            if (state == PlacementState.Body && e.Button == MouseButton.Right)
                endCurve();
            return base.OnMouseUp(e);
        }

        protected override bool OnDoubleClick(DoubleClickEvent e)
        {
            // At the point of a double click, there's guaranteed to be at least two points - one from the click, and one from the cursor
            HitObject.Path.ControlPoints[HitObject.Path.ControlPoints.Count - 2].Type.Value = PathType.Bezier;
            return true;
        }

        private void beginCurve()
        {
            BeginPlacement();
            setState(PlacementState.Body);
        }

        private void endCurve()
        {
            updateSlider();
            EndPlacement();
        }

        protected override void Update()
        {
            base.Update();
            updateSlider();
        }

        private void updateSlider()
        {
            HitObject.Path.ExpectedDistance.Value = null;
            HitObject.Path.ExpectedDistance.Value = composer?.GetSnappedDistanceFromDistance(HitObject.StartTime, (float)HitObject.Path.Distance) ?? (float)HitObject.Path.Distance;

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

        private class Segment
        {
            public readonly List<Vector2> ControlPoints = new List<Vector2>();

            public Segment(Vector2 offset)
            {
                ControlPoints.Add(offset);
            }
        }
    }
}
