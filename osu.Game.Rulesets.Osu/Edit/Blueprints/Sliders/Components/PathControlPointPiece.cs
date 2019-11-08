// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    public class PathControlPointPiece : BlueprintPiece<Slider>
    {
        public Action<int> RequestSelection;
        public Action<Vector2[]> ControlPointsChanged;

        public readonly BindableBool IsSelected = new BindableBool();
        public readonly int Index;

        private readonly Slider slider;
        private readonly Path path;
        private readonly Container marker;
        private readonly Drawable markerRing;

        [Resolved(CanBeNull = true)]
        private IDistanceSnapProvider snapProvider { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        public PathControlPointPiece(Slider slider, int index)
        {
            this.slider = slider;
            Index = index;

            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                path = new SmoothPath
                {
                    Anchor = Anchor.Centre,
                    PathRadius = 1
                },
                marker = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(10),
                        },
                        markerRing = new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(14),
                            Masking = true,
                            BorderThickness = 2,
                            BorderColour = Color4.White,
                            Alpha = 0,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0,
                                AlwaysPresent = true
                            }
                        }
                    }
                }
            };
        }

        protected override void Update()
        {
            base.Update();

            Position = slider.StackedPosition + slider.Path.ControlPoints[Index];

            updateMarkerDisplay();
            updateConnectingPath();
        }

        /// <summary>
        /// Updates the state of the circular control point marker.
        /// </summary>
        private void updateMarkerDisplay()
        {
            markerRing.Alpha = IsSelected.Value ? 1 : 0;

            Color4 colour = isSegmentSeparator ? colours.Red : colours.Yellow;
            if (IsHovered || IsSelected.Value)
                colour = Color4.White;
            marker.Colour = colour;
        }

        /// <summary>
        /// Updates the path connecting this control point to the previous one.
        /// </summary>
        private void updateConnectingPath()
        {
            path.ClearVertices();

            if (Index != slider.Path.ControlPoints.Length - 1)
            {
                path.AddVertex(Vector2.Zero);
                path.AddVertex(slider.Path.ControlPoints[Index + 1] - slider.Path.ControlPoints[Index]);
            }

            path.OriginPosition = path.PositionInBoundingBox(Vector2.Zero);
        }

        // The connecting path is excluded from positional input
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => marker.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (RequestSelection != null)
            {
                RequestSelection.Invoke(Index);
                return true;
            }

            return false;
        }

        protected override bool OnMouseUp(MouseUpEvent e) => RequestSelection != null;

        protected override bool OnClick(ClickEvent e) => RequestSelection != null;

        protected override bool OnDragStart(DragStartEvent e) => true;

        protected override bool OnDrag(DragEvent e)
        {
            var newControlPoints = slider.Path.ControlPoints.ToArray();

            if (Index == 0)
            {
                // Special handling for the head control point - the position of the slider changes which means the snapped position and time have to be taken into account
                (Vector2 snappedPosition, double snappedTime) = snapProvider?.GetSnappedPosition(e.MousePosition, slider.StartTime) ?? (e.MousePosition, slider.StartTime);
                Vector2 movementDelta = snappedPosition - slider.Position;

                slider.Position += movementDelta;
                slider.StartTime = snappedTime;

                // Since control points are relative to the position of the slider, they all need to be offset backwards by the delta
                for (int i = 1; i < newControlPoints.Length; i++)
                    newControlPoints[i] -= movementDelta;
            }
            else
                newControlPoints[Index] += e.Delta;

            if (isSegmentSeparatorWithNext)
                newControlPoints[Index + 1] = newControlPoints[Index];

            if (isSegmentSeparatorWithPrevious)
                newControlPoints[Index - 1] = newControlPoints[Index];

            ControlPointsChanged?.Invoke(newControlPoints);

            return true;
        }

        protected override bool OnDragEnd(DragEndEvent e) => true;

        private bool isSegmentSeparator => isSegmentSeparatorWithNext || isSegmentSeparatorWithPrevious;

        private bool isSegmentSeparatorWithNext => Index < slider.Path.ControlPoints.Length - 1 && slider.Path.ControlPoints[Index + 1] == slider.Path.ControlPoints[Index];

        private bool isSegmentSeparatorWithPrevious => Index > 0 && slider.Path.ControlPoints[Index - 1] == slider.Path.ControlPoints[Index];
    }
}
