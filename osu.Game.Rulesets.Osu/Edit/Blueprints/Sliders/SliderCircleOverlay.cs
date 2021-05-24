// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderCircleOverlay : CompositeDrawable
    {
        protected readonly HitCirclePiece CirclePiece;

        private readonly Slider slider;
        private readonly SliderPosition position;

        public SliderCircleOverlay(Slider slider, SliderPosition position)
        {
            this.slider = slider;
            this.position = position;

            InternalChild = CirclePiece = new HitCirclePiece();
        }

        protected override void Update()
        {
            base.Update();

            CirclePiece.UpdateFrom(position == SliderPosition.Start ? (HitCircle)slider.HeadCircle : slider.TailCircle);
        }
    }
}
