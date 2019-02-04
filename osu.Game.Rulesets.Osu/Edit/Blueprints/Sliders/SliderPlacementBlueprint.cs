// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderPlacementBlueprint : PlacementBlueprint
    {
        public new Objects.Slider HitObject => (Objects.Slider)base.HitObject;

        private readonly List<Segment> segments = new List<Segment>();
        private Vector2 cursor;

        private PlacementState state;

        public SliderPlacementBlueprint()
            : base(new Objects.Slider())
        {
            RelativeSizeAxes = Axes.Both;
            segments.Add(new Segment(Vector2.Zero));
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            InternalChildren = new Drawable[]
            {
                new SliderBodyPiece(HitObject),
                new SliderCirclePiece(HitObject, SliderPosition.Start),
                new SliderCirclePiece(HitObject, SliderPosition.End),
                new PathControlPointVisualiser(HitObject),
            };

            setState(PlacementState.Initial);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Fixes a 1-frame position discrepancy due to the first mouse move event happening in the next frame
            HitObject.Position = Parent?.ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position) ?? Vector2.Zero;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            switch (state)
            {
                case PlacementState.Initial:
                    HitObject.Position = e.MousePosition;
                    return true;
                case PlacementState.Body:
                    cursor = e.MousePosition - HitObject.Position;
                    return true;
            }

            return false;
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
                            segments.Last().ControlPoints.Add(cursor);
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
            segments.Add(new Segment(segments[segments.Count - 1].ControlPoints.Last()));
            return true;
        }

        private void beginCurve()
        {
            BeginPlacement();

            HitObject.StartTime = EditorClock.CurrentTime;
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
            var newControlPoints = segments.SelectMany(s => s.ControlPoints).Concat(cursor.Yield()).ToArray();
            HitObject.Path = new SliderPath(newControlPoints.Length > 2 ? PathType.Bezier : PathType.Linear, newControlPoints);
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
