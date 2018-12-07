// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Mods {
    public class ManiaModHidden : ModHidden {
        public override string Description => @"Keys fade out before you hit them!";
        public override double ScoreMultiplier => 1;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight<ManiaHitObject>) };
        private const double fading_duration = 150;
        private const double fading_offset_time = 300;

        protected override void ApplyHiddenState(DrawableHitObject drawableHitObject, ArmedState state) {
            var fadeOutStartTime = drawableHitObject.HitObject.StartTime - fading_duration - fading_offset_time;

            switch (drawableHitObject) {
                case DrawableHoldNoteTick holdNoteTick:
                case DrawableNote note:
                    using (drawableHitObject.BeginAbsoluteSequence(fadeOutStartTime, true))
                        drawableHitObject.FadeOut(fading_duration);

                    break;
                case DrawableHoldNote holdNote:
                    //TODO

                    break;
                default:
                    break;
            }
        }
    }
}
