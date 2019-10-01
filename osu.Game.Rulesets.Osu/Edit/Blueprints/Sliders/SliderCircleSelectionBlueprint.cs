// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderCircleSelectionBlueprint : OsuSelectionBlueprint<Slider>
    {
        protected readonly HitCirclePiece CirclePiece;

        private readonly SliderPosition position;

        public SliderCircleSelectionBlueprint(DrawableSlider slider, SliderPosition position)
            : base(slider)
        {
            this.position = position;
            InternalChild = CirclePiece = new HitCirclePiece();

            Select();
        }

        protected override void Update()
        {
            base.Update();

            CirclePiece.UpdateFrom(position == SliderPosition.Start ? HitObject.HeadCircle : HitObject.TailCircle);
        }

        // Todo: This is temporary, since the slider circle masks don't do anything special yet. In the future they will handle input.
        public override bool HandlePositionalInput => false;
    }
}
