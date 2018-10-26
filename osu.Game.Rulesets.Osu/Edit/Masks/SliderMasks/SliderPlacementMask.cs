// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Edit.Masks.SliderMasks.Components;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SliderMasks
{
    public class SliderPlacementMask : PlacementMask
    {
        public new Objects.Slider HitObject => (Objects.Slider)base.HitObject;

        private Container<SliderControlPoint> controlPointContainer;

        private readonly List<Segment> segments = new List<Segment>();
        private Vector2 cursor;

        private PlacementState state;

        public SliderPlacementMask()
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
                controlPointContainer = new Container<SliderControlPoint> { RelativeSizeAxes = Axes.Both }
            };

            setState(PlacementState.Initial);
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
                    controlPointContainer.Last().NextPoint = e.MousePosition;
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

            controlPointContainer.Add(new SliderControlPoint { Position = e.MousePosition });

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
            controlPointContainer.Last().SegmentSeparator = true;
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
            HitObject.ControlPoints = segments.SelectMany(s => s.ControlPoints).Concat(cursor.Yield()).ToArray();
            HitObject.CurveType = HitObject.ControlPoints.Length > 2 ? CurveType.Bezier : CurveType.Linear;
            HitObject.Distance = segments.Sum(s => s.Distance);

            EndPlacement();
        }

        protected override void Update()
        {
            base.Update();

            for (int i = 0; i < segments.Count; i++)
                segments[i].Calculate(i == segments.Count - 1 ? (Vector2?)cursor : null);

            HitObject.ControlPoints = segments.SelectMany(s => s.ControlPoints).Concat(cursor.Yield()).ToArray();
            HitObject.CurveType = HitObject.ControlPoints.Length > 2 ? CurveType.Bezier : CurveType.Linear;
            HitObject.Distance = segments.Sum(s => s.Distance);
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
            public float Distance { get; private set; }

            public readonly List<Vector2> ControlPoints = new List<Vector2>();

            public Segment(Vector2 offset)
            {
                ControlPoints.Add(offset);
            }

            public void Calculate(Vector2? cursor = null)
            {
                Span<Vector2> allControlPoints = stackalloc Vector2[ControlPoints.Count + (cursor.HasValue ? 1 : 0)];

                for (int i = 0; i < ControlPoints.Count; i++)
                    allControlPoints[i] = ControlPoints[i];
                if (cursor.HasValue)
                    allControlPoints[allControlPoints.Length - 1] = cursor.Value;

                List<Vector2> result;

                switch (allControlPoints.Length)
                {
                    case 1:
                    case 2:
                        result = new LinearApproximator(allControlPoints).CreateLinear();
                        break;
                    default:
                        result = new BezierApproximator(allControlPoints).CreateBezier();
                        break;
                }

                Distance = 0;
                for (int i = 0; i < result.Count - 1; i++)
                    Distance += Vector2.Distance(result[i], result[i + 1]);
            }
        }
    }
}
