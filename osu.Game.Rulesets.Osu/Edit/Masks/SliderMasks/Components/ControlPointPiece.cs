// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Osu.Edit.Masks.SliderMasks.Components
{
    public class ControlPointPiece : CompositeDrawable
    {
        private readonly Slider slider;
        private readonly int index;

        private readonly Path path;
        private readonly CircularContainer marker;

        [Resolved]
        private OsuColour colours { get; set; }

        public ControlPointPiece(Slider slider, int index)
        {
            this.slider = slider;
            this.index = index;

            Origin = Anchor.Centre;
            Size = new Vector2(10);

            InternalChildren = new Drawable[]
            {
                path = new SmoothPath
                {
                    BypassAutoSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    PathWidth = 1
                },
                marker = new CircularContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    Child = new Box { RelativeSizeAxes = Axes.Both }
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            Position = slider.StackedPosition + slider.ControlPoints[index];

            marker.Colour = segmentSeparator ? colours.Red : colours.Yellow;

            path.ClearVertices();

            if (index != slider.ControlPoints.Length - 1)
            {
                path.AddVertex(Vector2.Zero);
                path.AddVertex(slider.ControlPoints[index + 1] - slider.ControlPoints[index]);
            }

            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
        }

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override bool OnDrag(DragEvent e)
        {
            if (index == 0)
            {
                // Special handling for the head - only the position of the slider changes
                slider.Position += e.Delta;

                // Since control points are relative to the position of the slider, they all need to be offset backwards by the delta
                var newControlPoints = slider.ControlPoints.ToArray();
                for (int i = 1; i < newControlPoints.Length; i++)
                    newControlPoints[i] -= e.Delta;

                slider.ControlPoints = newControlPoints;
                slider.Curve.Calculate(true);
            }
            else
            {
                var newControlPoints = slider.ControlPoints.ToArray();
                newControlPoints[index] += e.Delta;

                slider.ControlPoints = newControlPoints;
                slider.Curve.Calculate(true);
            }

            return true;
        }

        protected override bool OnDragEnd(DragEndEvent e) => true;

        private bool segmentSeparator => index != 0 && index != slider.ControlPoints.Length - 1
                                                    && slider.ControlPoints[index - 1] != slider.ControlPoints[index]
                                                    && slider.ControlPoints[index + 1] != slider.ControlPoints[index];
    }
}
