// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    /// <summary>
    /// A visualisation of a single <see cref="PathControlPoint"/> in a <see cref="Slider"/>.
    /// </summary>
    public class PathControlPointPiece : BlueprintPiece<Slider>, IHasTooltip
    {
        public Action<PathControlPointPiece, MouseButtonEvent> RequestSelection;
        public List<PathControlPoint> PointsInSegment;

        public readonly BindableBool IsSelected = new BindableBool();
        public readonly PathControlPoint ControlPoint;

        private readonly Slider slider;
        private readonly Container marker;
        private readonly Drawable markerRing;

        [Resolved(CanBeNull = true)]
        private IEditorChangeHandler changeHandler { get; set; }

        [Resolved(CanBeNull = true)]
        private IPositionSnapProvider snapProvider { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private IBindable<Vector2> sliderPosition;
        private IBindable<float> sliderScale;

        public PathControlPointPiece(Slider slider, PathControlPoint controlPoint)
        {
            this.slider = slider;
            ControlPoint = controlPoint;

            // we don't want to run the path type update on construction as it may inadvertently change the slider.
            cachePoints(slider);

            slider.Path.Version.BindValueChanged(_ =>
            {
                cachePoints(slider);
                updatePathType();
            });

            controlPoint.Changed += updateMarkerDisplay;

            Origin = Anchor.Centre;
            AutoSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
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
                            Size = new Vector2(20),
                        },
                        markerRing = new CircularContainer
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(28),
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

        protected override void LoadComplete()
        {
            base.LoadComplete();

            sliderPosition = slider.PositionBindable.GetBoundCopy();
            sliderPosition.BindValueChanged(_ => updateMarkerDisplay());

            sliderScale = slider.ScaleBindable.GetBoundCopy();
            sliderScale.BindValueChanged(_ => updateMarkerDisplay());

            IsSelected.BindValueChanged(_ => updateMarkerDisplay());

            updateMarkerDisplay();
        }

        // The connecting path is excluded from positional input
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => marker.ReceivePositionalInputAt(screenSpacePos);

        protected override bool OnHover(HoverEvent e)
        {
            updateMarkerDisplay();
            return false;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateMarkerDisplay();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (RequestSelection == null)
                return false;

            switch (e.Button)
            {
                case MouseButton.Left:
                    RequestSelection.Invoke(this, e);
                    return true;

                case MouseButton.Right:
                    if (!IsSelected.Value)
                        RequestSelection.Invoke(this, e);
                    return false; // Allow context menu to show
            }

            return false;
        }

        protected override bool OnClick(ClickEvent e) => RequestSelection != null;

        private Vector2 dragStartPosition;
        private PathType? dragPathType;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (RequestSelection == null)
                return false;

            if (e.Button == MouseButton.Left)
            {
                dragStartPosition = ControlPoint.Position;
                dragPathType = PointsInSegment[0].Type;

                changeHandler?.BeginChange();
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e)
        {
            Vector2[] oldControlPoints = slider.Path.ControlPoints.Select(cp => cp.Position).ToArray();
            var oldPosition = slider.Position;
            double oldStartTime = slider.StartTime;

            if (ControlPoint == slider.Path.ControlPoints[0])
            {
                // Special handling for the head control point - the position of the slider changes which means the snapped position and time have to be taken into account
                var result = snapProvider?.SnapScreenSpacePositionToValidTime(e.ScreenSpaceMousePosition);

                Vector2 movementDelta = Parent.ToLocalSpace(result?.ScreenSpacePosition ?? e.ScreenSpaceMousePosition) - slider.Position;

                slider.Position += movementDelta;
                slider.StartTime = result?.Time ?? slider.StartTime;

                // Since control points are relative to the position of the slider, they all need to be offset backwards by the delta
                for (int i = 1; i < slider.Path.ControlPoints.Count; i++)
                    slider.Path.ControlPoints[i].Position -= movementDelta;
            }
            else
                ControlPoint.Position = dragStartPosition + (e.MousePosition - e.MouseDownPosition);

            if (!slider.Path.HasValidLength)
            {
                for (int i = 0; i < slider.Path.ControlPoints.Count; i++)
                    slider.Path.ControlPoints[i].Position = oldControlPoints[i];

                slider.Position = oldPosition;
                slider.StartTime = oldStartTime;
                return;
            }

            // Maintain the path type in case it got defaulted to bezier at some point during the drag.
            PointsInSegment[0].Type = dragPathType;
        }

        protected override void OnDragEnd(DragEndEvent e) => changeHandler?.EndChange();

        private void cachePoints(Slider slider) => PointsInSegment = slider.Path.PointsInSegment(ControlPoint);

        /// <summary>
        /// Handles correction of invalid path types.
        /// </summary>
        private void updatePathType()
        {
            if (ControlPoint.Type != PathType.PerfectCurve)
                return;

            if (PointsInSegment.Count > 3)
                ControlPoint.Type = PathType.Bezier;

            if (PointsInSegment.Count != 3)
                return;

            ReadOnlySpan<Vector2> points = PointsInSegment.Select(p => p.Position).ToArray();
            RectangleF boundingBox = PathApproximator.CircularArcBoundingBox(points);
            if (boundingBox.Width >= 640 || boundingBox.Height >= 480)
                ControlPoint.Type = PathType.Bezier;
        }

        /// <summary>
        /// Updates the state of the circular control point marker.
        /// </summary>
        private void updateMarkerDisplay()
        {
            Position = slider.StackedPosition + ControlPoint.Position;

            markerRing.Alpha = IsSelected.Value ? 1 : 0;

            Color4 colour = getColourFromNodeType();

            if (IsHovered || IsSelected.Value)
                colour = colour.Lighten(1);

            marker.Colour = colour;
            marker.Scale = new Vector2(slider.Scale);
        }

        private Color4 getColourFromNodeType()
        {
            if (!(ControlPoint.Type is PathType pathType))
                return colours.Yellow;

            switch (pathType)
            {
                case PathType.Catmull:
                    return colours.Seafoam;

                case PathType.Bezier:
                    return colours.Pink;

                case PathType.PerfectCurve:
                    return colours.PurpleDark;

                default:
                    return colours.Red;
            }
        }

        public LocalisableString TooltipText => ControlPoint.Type.ToString() ?? string.Empty;
    }
}
