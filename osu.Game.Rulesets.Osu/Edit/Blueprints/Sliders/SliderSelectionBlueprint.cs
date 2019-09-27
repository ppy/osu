// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osuTK;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderSelectionBlueprint : OsuSelectionBlueprint<Slider>
    {
        private readonly SliderBodyPiece bodyPiece;
        private readonly SliderCircleSelectionBlueprint headBlueprint;

        public SliderSelectionBlueprint(DrawableSlider slider)
            : base(slider)
        {
            var sliderObject = (Slider)slider.HitObject;

            InternalChildren = new Drawable[]
            {
                bodyPiece = new SliderBodyPiece(),
                headBlueprint = new SliderCircleSelectionBlueprint(slider, SliderPosition.Start),
                new SliderCircleSelectionBlueprint(slider, SliderPosition.End),
                new PathControlPointVisualiser(sliderObject),
            };
        }

        protected override void Update()
        {
            base.Update();

            bodyPiece.UpdateFrom(HitObject);
        }

        public override Vector2 SelectionPoint => headBlueprint.SelectionPoint;
    }
}
