// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;
using OpenTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Masks
{
    public partial class SliderPlacementMask : PlacementMask
    {
        public new Slider HitObject => (Slider)base.HitObject;

        private readonly CirclePlacementMask headMask;
        private readonly CirclePlacementMask tailMask;
        private readonly Path path;

        private readonly List<Segment> segments = new List<Segment>();
        private readonly Container<SliderControlPoint> controlPointContainer;
        private Vector2 cursor;

        private PlacementState state;

        public SliderPlacementMask()
            : base(new Slider())
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                path = new SmoothPath { PathWidth = 3 },
                headMask = new CirclePlacementMask(),
                tailMask = new CirclePlacementMask(),
                controlPointContainer = new Container<SliderControlPoint> { RelativeSizeAxes = Axes.Both }
            };

            segments.Add(new Segment(Vector2.Zero));

            setState(PlacementState.Initial);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            path.Colour = colours.YellowDark;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            switch (state)
            {
                case PlacementState.Initial:
                    headMask.Position = e.MousePosition;
                    return true;
                case PlacementState.Body:
                    tailMask.Position = e.MousePosition;
                    cursor = tailMask.Position - headMask.Position;
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
            setState(PlacementState.Body);
        }

        private void endCurve()
        {
            HitObject.Position = headMask.Position;
            HitObject.ControlPoints = segments.SelectMany(s => s.ControlPoints).Concat(cursor.Yield()).ToList();
            HitObject.CurveType = HitObject.ControlPoints.Count > 2 ? CurveType.Bezier : CurveType.Linear;
            HitObject.Distance = segments.Sum(s => s.Distance);

            Finish();
        }

        protected override void Update()
        {
            base.Update();

            segments.ForEach(s => s.Calculate());

            switch (state)
            {
                case PlacementState.Body:
                    path.Position = headMask.Position;
                    path.ClearVertices();

                    for (int i = 0; i < segments.Count; i++)
                    {
                        var segmentPath = segments[i].Calculate(i == segments.Count - 1 ? (Vector2?)cursor : null);
                        segmentPath.ForEach(v => path.AddVertex(v));
                    }

                    path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
                    break;
            }
        }

        private void setState(PlacementState newState)
        {
            switch (newState)
            {
                case PlacementState.Initial:
                    tailMask.Alpha = 0;
                    break;
                case PlacementState.Body:
                    tailMask.Alpha = 1;
                    break;
            }

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

            public List<Vector2> Calculate(Vector2? cursor = null)
            {
                var allControlPoints = ControlPoints.ToList();
                if (cursor.HasValue)
                    allControlPoints.Add(cursor.Value);

                IApproximator approximator;

                switch (allControlPoints.Count)
                {
                    case 1:
                    case 2:
                        approximator = new LinearApproximator();
                        break;
                    default:
                        approximator = new BezierApproximator();
                        break;
                }

                Distance = 0;

                var points = approximator.Approximate(allControlPoints);
                for (int i = 0; i < points.Count - 1; i++)
                    Distance += Vector2.Distance(points[i], points[i + 1]);

                return points;
            }
        }
    }
}
