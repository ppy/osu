// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Lines;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Graphics;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderCircleOverlay : CompositeDrawable
    {
        public RectangleF VisibleQuad
        {
            get
            {
                var result = CirclePiece.ScreenSpaceDrawQuad.AABBFloat;

                if (endDragMarkerContainer == null) return result;

                var size = result.Size * 1.4f;
                var location = result.TopLeft - result.Size * 0.2f;
                return new RectangleF(location, size);
            }
        }

        protected readonly HitCirclePiece CirclePiece;

        private readonly Slider slider;
        private readonly SliderPosition position;
        private readonly HitCircleOverlapMarker marker;
        private readonly Container? endDragMarkerContainer;

        public SliderCircleOverlay(Slider slider, SliderPosition position)
        {
            this.slider = slider;
            this.position = position;

            InternalChildren = new Drawable[]
            {
                marker = new HitCircleOverlapMarker(),
                CirclePiece = new HitCirclePiece(),
            };

            if (position == SliderPosition.End)
            {
                AddInternal(endDragMarkerContainer = new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Padding = new MarginPadding(-2.5f),
                    Child = EndDragMarker = new SliderEndDragMarker()
                });
            }
        }

        public SliderEndDragMarker? EndDragMarker { get; }

        protected override void Update()
        {
            base.Update();

            var circle = position == SliderPosition.Start ? (HitCircle)slider.HeadCircle :
                slider.RepeatCount % 2 == 0 ? slider.TailCircle : slider.LastRepeat!;

            CirclePiece.UpdateFrom(circle);
            marker.UpdateFrom(circle);

            if (endDragMarkerContainer != null)
            {
                endDragMarkerContainer.Position = circle.Position;
                endDragMarkerContainer.Scale = CirclePiece.Scale * 1.2f;
                var diff = slider.Path.PositionAt(1) - slider.Path.PositionAt(0.99f);
                endDragMarkerContainer.Rotation = float.RadiansToDegrees(MathF.Atan2(diff.Y, diff.X));
            }
        }

        public override void Hide()
        {
            CirclePiece.Hide();
            endDragMarkerContainer?.Hide();
        }

        public override void Show()
        {
            CirclePiece.Show();
            endDragMarkerContainer?.Show();
        }

        public partial class SliderEndDragMarker : SmoothPath
        {
            public Action<DragStartEvent>? StartDrag { get; set; }
            public Action<DragEvent>? Drag { get; set; }
            public Action? EndDrag { get; set; }

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            [BackgroundDependencyLoader]
            private void load()
            {
                var path = PathApproximator.CircularArcToPiecewiseLinear([
                    new Vector2(0, OsuHitObject.OBJECT_RADIUS),
                    new Vector2(OsuHitObject.OBJECT_RADIUS, 0),
                    new Vector2(0, -OsuHitObject.OBJECT_RADIUS)
                ]);

                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                PathRadius = 5;
                Vertices = path;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                updateState();
            }

            protected override bool OnHover(HoverEvent e)
            {
                updateState();
                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                updateState();
                base.OnHoverLost(e);
            }

            protected override bool OnDragStart(DragStartEvent e)
            {
                updateState();
                StartDrag?.Invoke(e);
                return true;
            }

            protected override void OnDrag(DragEvent e)
            {
                updateState();
                base.OnDrag(e);
                Drag?.Invoke(e);
            }

            protected override void OnDragEnd(DragEndEvent e)
            {
                updateState();
                EndDrag?.Invoke();
                base.OnDragEnd(e);
            }

            private void updateState()
            {
                Colour = IsHovered || IsDragged ? colours.Red : colours.Yellow;
            }
        }
    }
}
