// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components
{
    /// <summary>
    /// A visualisation of a single <see cref="PathControlPoint"/> in an osu hit object with a path.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="OsuHitObject"/> which this <see cref="PathControlPointPiece{T}"/> visualises.</typeparam>
    public partial class PathControlPointPiece<T> : BlueprintPiece<T>, IHasTooltip
        where T : OsuHitObject, IHasPath
    {
        public Action<PathControlPointPiece<T>, MouseButtonEvent> RequestSelection;

        public Action<PathControlPoint> DragStarted;
        public Action<DragEvent> DragInProgress;
        public Action DragEnded;

        public readonly BindableBool IsSelected = new BindableBool();
        public readonly PathControlPoint ControlPoint;

        private readonly T hitObject;
        private readonly Container marker;
        private readonly Drawable markerRing;

        [Resolved]
        private OsuColour colours { get; set; }

        private IBindable<Vector2> hitObjectPosition;
        private IBindable<float> hitObjectScale;

        public PathControlPointPiece(T hitObject, PathControlPoint controlPoint)
        {
            this.hitObject = hitObject;
            ControlPoint = controlPoint;

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

            hitObjectPosition = hitObject.PositionBindable.GetBoundCopy();
            hitObjectPosition.BindValueChanged(_ => updateMarkerDisplay());

            hitObjectScale = hitObject.ScaleBindable.GetBoundCopy();
            hitObjectScale.BindValueChanged(_ => updateMarkerDisplay());

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

        // Used to pair up mouse down/drag events with their corresponding mouse up events,
        // to avoid deselecting the piece by accident when the mouse up corresponding to the mouse down/drag fires.
        private bool keepSelection;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (RequestSelection == null)
                return false;

            switch (e.Button)
            {
                case MouseButton.Left:
                    // if control is pressed, do not do anything as the user may be adding to current selection
                    // or dragging all currently selected control points.
                    // if it isn't and the user's intent is to deselect, deselection will happen on mouse up.
                    if (e.ControlPressed && IsSelected.Value)
                        return true;

                    RequestSelection.Invoke(this, e);
                    keepSelection = true;

                    return true;

                case MouseButton.Right:
                    if (!IsSelected.Value)
                        RequestSelection.Invoke(this, e);

                    keepSelection = true;
                    return false; // Allow context menu to show
            }

            return false;
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            base.OnMouseUp(e);

            // ctrl+click deselects this piece, but only if this event
            // wasn't immediately preceded by a matching mouse down or drag.
            if (IsSelected.Value && e.ControlPressed && !keepSelection)
                IsSelected.Value = false;

            keepSelection = false;
        }

        protected override bool OnClick(ClickEvent e) => RequestSelection != null;

        protected override bool OnDragStart(DragStartEvent e)
        {
            if (RequestSelection == null)
                return false;

            if (e.Button == MouseButton.Left)
            {
                DragStarted?.Invoke(ControlPoint);
                keepSelection = true;
                return true;
            }

            return false;
        }

        protected override void OnDrag(DragEvent e) => DragInProgress?.Invoke(e);

        protected override void OnDragEnd(DragEndEvent e) => DragEnded?.Invoke();

        /// <summary>
        /// Updates the state of the circular control point marker.
        /// </summary>
        private void updateMarkerDisplay()
        {
            Position = hitObject.StackedPosition + ControlPoint.Position;

            markerRing.Alpha = IsSelected.Value ? 1 : 0;

            Color4 colour = getColourFromNodeType();

            if (IsHovered || IsSelected.Value)
                colour = colour.Lighten(1);

            marker.Colour = colour;
            marker.Scale = new Vector2(hitObject.Scale);
        }

        private Color4 getColourFromNodeType()
        {
            if (ControlPoint.Type is not PathType pathType)
                return colours.Yellow;

            switch (pathType.Type)
            {
                case SplineType.Catmull:
                    return colours.SeaFoam;

                case SplineType.BSpline:
                    if (!pathType.Degree.HasValue)
                        return colours.PinkLighter;

                    int idx = Math.Clamp(pathType.Degree.Value, 0, 3);
                    return new[] { colours.PinkDarker, colours.PinkDark, colours.Pink, colours.PinkLight }[idx];

                case SplineType.PerfectCurve:
                    return colours.PurpleDark;

                default:
                    return colours.Red;
            }
        }

        public LocalisableString TooltipText => ControlPoint.Type?.Description ?? string.Empty;
    }
}
