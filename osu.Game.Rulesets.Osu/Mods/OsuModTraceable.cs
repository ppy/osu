// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;
using System.Collections.Generic;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables.Pieces;

namespace osu.Game.Rulesets.Osu.Mods
{
    internal class OsuModTraceable : Mod, IReadFromConfig, IApplicableToDrawableHitObjects
    {
        public override string Name => "Traceable";
        public override string Acronym => "TC";
        public override ModType Type => ModType.Fun;
        public override string Description => "Put your faith in the approach circles...";
        public override double ScoreMultiplier => 1;

        public override Type[] IncompatibleMods => new[] { typeof(OsuModHidden), typeof(OsuModSpinIn), typeof(OsuModObjectScaleTween) };
        private Bindable<bool> increaseFirstObjectVisibility = new Bindable<bool>();

        public void ReadFromConfig(OsuConfigManager config)
        {
            increaseFirstObjectVisibility = config.GetBindable<bool>(OsuSetting.IncreaseFirstObjectVisibility);
        }

        public void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            foreach (var drawable in drawables.Skip(increaseFirstObjectVisibility.Value ? 1 : 0))
                drawable.ApplyCustomUpdateState += ApplyTraceableState;
        }

        protected void ApplyTraceableState(DrawableHitObject drawable, ArmedState state)
        {
            if (!(drawable is DrawableOsuHitObject))
                return;

            //todo: expose and hide spinner background somehow

            switch (drawable)
            {
                case DrawableHitCircle circle:
                    // we only want to see the approach circle
                    applyCirclePieceState(circle, circle.CirclePiece);
                    break;

                case DrawableSliderTail sliderTail:
                    applyCirclePieceState(sliderTail);
                    break;

                case DrawableSliderRepeat sliderRepeat:
                    // show only the repeat arrow
                    applyCirclePieceState(sliderRepeat, sliderRepeat.CirclePiece);
                    break;

                case DrawableSlider slider:
                    slider.Body.OnSkinChanged += () => applySliderState(slider);
                    applySliderState(slider);
                    break;
            }
        }

        private void applyCirclePieceState(DrawableOsuHitObject hitObject, IDrawable hitCircle = null)
        {
            var h = hitObject.HitObject;
            using (hitObject.BeginAbsoluteSequence(h.StartTime - h.TimePreempt, true))
                (hitCircle ?? hitObject).Hide();
        }

        private void applySliderState(DrawableSlider slider)
        {
            ((PlaySliderBody)slider.Body.Drawable).AccentColour = slider.AccentColour.Value.Opacity(0);
            ((PlaySliderBody)slider.Body.Drawable).BorderColour = slider.AccentColour.Value;
        }
    }
}
