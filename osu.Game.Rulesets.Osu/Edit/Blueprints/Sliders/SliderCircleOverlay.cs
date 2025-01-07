// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderCircleOverlay : CompositeDrawable
    {
        public SliderEndDragMarker? EndDragMarker { get; }

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
        private readonly HitCircleOverlapMarker? marker;
        private readonly Container? endDragMarkerContainer;

        public SliderCircleOverlay(Slider slider, SliderPosition position)
        {
            this.slider = slider;
            this.position = position;

            if (position == SliderPosition.Start)
                AddInternal(marker = new HitCircleOverlapMarker());

            AddInternal(CirclePiece = new HitCirclePiece());

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

        protected override void Update()
        {
            base.Update();

            var circle = position == SliderPosition.Start ? (HitCircle)slider.HeadCircle :
                slider.RepeatCount % 2 == 0 ? slider.TailCircle : slider.LastRepeat!;

            CirclePiece.UpdateFrom(circle);
            marker?.UpdateFrom(circle);

            if (endDragMarkerContainer != null)
            {
                endDragMarkerContainer.Position = circle.Position + slider.StackOffset;
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
    }
}
