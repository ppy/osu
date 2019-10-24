// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointPiece : BlueprintPiece<Slider>
    {
        public Action<Vector2[]> ControlPointsChanged;

        private readonly Slider slider;
        private readonly int index;

        private readonly Path path;
        private readonly CircularContainer marker;

        [Resolved]
        private OsuColour colours { get; set; }

        public PathControlPointPiece(Slider slider, int index)
        {
            this.slider = slider;
            this.index = index;

            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                path = new SmoothPath
                {
                    Anchor = Anchor.Centre,
                    PathRadius = 1
                },
                marker = new CircularContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(10),
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            Position = slider.StackedPosition + slider.Path.ControlPoints[index];

            marker.Colour = isSegmentSeparator ? colours.Red : colours.Yellow;

            path.ClearVertices();

            if (index != slider.Path.ControlPoints.Length - 1)
            {
                path.AddVertex(Vector2.Zero);
                path.AddVertex(slider.Path.ControlPoints[index + 1] - slider.Path.ControlPoints[index]);
            }

            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => marker.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override bool OnDrag(DragEvent e)
        {
            var newControlPoints = slider.Path.ControlPoints.ToArray();

            if (index == 0)
            {
                // Special handling for the head - only the position of the slider changes
                slider.Position += e.Delta;

                // Since control points are relative to the position of the slider, they all need to be offset backwards by the delta
                for (int i = 1; i < newControlPoints.Length; i++)
                    newControlPoints[i] -= e.Delta;
            }
            else
                newControlPoints[index] += e.Delta;

            if (isSegmentSeparatorWithNext)
                newControlPoints[index + 1] = newControlPoints[index];

            if (isSegmentSeparatorWithPrevious)
                newControlPoints[index - 1] = newControlPoints[index];

            ControlPointsChanged?.Invoke(newControlPoints);

            return true;
        }

        protected override bool OnDragEnd(DragEndEvent e) => true;

        private bool isSegmentSeparator => isSegmentSeparatorWithNext || isSegmentSeparatorWithPrevious;

        private bool isSegmentSeparatorWithNext => index < slider.Path.ControlPoints.Length - 1 && slider.Path.ControlPoints[index + 1] == slider.Path.ControlPoints[index];

        private bool isSegmentSeparatorWithPrevious => index > 0 && slider.Path.ControlPoints[index - 1] == slider.Path.ControlPoints[index];
    }
}
