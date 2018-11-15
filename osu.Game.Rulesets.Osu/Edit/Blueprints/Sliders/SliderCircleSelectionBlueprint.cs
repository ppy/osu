﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Input.Events;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders
{
    public class SliderCircleSelectionBlueprint : OsuSelectionBlueprint
    {
        private readonly Slider slider;

        public SliderCircleSelectionBlueprint(DrawableOsuHitObject hitObject, Slider slider, SliderPosition position)
            : base(hitObject)
        {
            this.slider = slider;

            InternalChild = new SliderCirclePiece(slider, position);

            Select();
        }

        // Todo: This is temporary, since the slider circle masks don't do anything special yet. In the future they will handle input.
        public override bool HandlePositionalInput => false;

        public override void AdjustPosition(DragEvent dragEvent) => slider.Position += dragEvent.Delta;
    }
}
