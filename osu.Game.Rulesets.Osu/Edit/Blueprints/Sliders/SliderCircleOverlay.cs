// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public partial class SliderCircleOverlay : CompositeDrawable
    {
        protected readonly HitCirclePiece CirclePiece;

        private readonly Slider slider;
        private readonly SliderPosition position;
        private readonly HitCircleOverlapMarker marker;

        public SliderCircleOverlay(Slider slider, SliderPosition position)
        {
            this.slider = slider;
            this.position = position;

            InternalChildren = new Drawable[]
            {
                marker = new HitCircleOverlapMarker(),
                CirclePiece = new HitCirclePiece(),
            };
        }

        protected override void Update()
        {
            base.Update();

            var circle = position == SliderPosition.Start ? (HitCircle)slider.HeadCircle : slider.TailCircle;

            CirclePiece.UpdateFrom(circle);
            marker.UpdateFrom(circle);
        }

        public override void Hide()
        {
            CirclePiece.Hide();
        }

        public override void Show()
        {
            CirclePiece.Show();
        }
    }
}
