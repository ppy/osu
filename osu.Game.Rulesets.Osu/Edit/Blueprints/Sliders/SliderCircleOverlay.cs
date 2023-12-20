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
        protected readonly Slider Slider;

        private readonly HitCircleOverlapMarker marker;
        private readonly SliderPosition position;

        public SliderCircleOverlay(Slider slider, SliderPosition position)
        {
            Slider = slider;
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

            var circle = position == SliderPosition.Start ? (HitCircle)Slider.HeadCircle :
                Slider.RepeatCount % 2 == 0 ? Slider.TailCircle : Slider.LastRepeat;

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
