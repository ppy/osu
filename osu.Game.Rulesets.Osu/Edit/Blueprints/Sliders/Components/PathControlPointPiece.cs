// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
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
    public class PathControlPointPiece : BlueprintPiece<Slider>
    {
        public Action<PathControlPointPiece, MouseButtonEvent> RequestSelection;

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
        private IBindable<Vector2> controlPointPosition;

        public PathControlPointPiece(Slider slider, PathControlPoint controlPoint)
        {
            this.slider = slider;
            ControlPoint = controlPoint;

            controlPoint.Type.BindValueChanged(_ => updateMarkerDisplay());

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

            controlPointPosition = ControlPoint.Position.GetBoundCopy();
            controlPointPosition.BindValueChanged(_ => updateMarkerDisplay());

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

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (RequestSelection == null)
                return false;

            if (e.Button == MouseButton.Left)
            {
                dragStartPosition = ControlPoint.Position.Value;
                changeHandler?.BeginChange();
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e)
        {
            if (ControlPoint == slider.Path.ControlPoints[0])
            {
                // Special handling for the head control point - the position of the slider changes which means the snapped position and time have to be taken into account
                var result = snapProvider?.SnapScreenSpacePositionToValidTime(e.ScreenSpaceMousePosition);

                Vector2 movementDelta = Parent.ToLocalSpace(result?.ScreenSpacePosition ?? e.ScreenSpaceMousePosition) - slider.Position;

                slider.Position += movementDelta;
                slider.StartTime = result?.Time ?? slider.StartTime;

                // Since control points are relative to the position of the slider, they all need to be offset backwards by the delta
                for (int i = 1; i < slider.Path.ControlPoints.Count; i++)
                    slider.Path.ControlPoints[i].Position.Value -= movementDelta;
            }
            else
                ControlPoint.Position.Value = dragStartPosition + (e.MousePosition - e.MouseDownPosition);
        }

        protected override void OnDragEnd(DragEndEvent e) => changeHandler?.EndChange();

        /// <summary>
        /// Updates the state of the circular control point marker.
        /// </summary>
        private void updateMarkerDisplay()
        {
            Position = slider.StackedPosition + ControlPoint.Position.Value;

            markerRing.Alpha = IsSelected.Value ? 1 : 0;

            Color4 colour = ControlPoint.Type.Value != null ? colours.Red : colours.Yellow;

            if (IsHovered || IsSelected.Value)
                colour = colour.Lighten(1);

            marker.Colour = colour;
            marker.Scale = new Vector2(slider.Scale);
        }
    }
}
