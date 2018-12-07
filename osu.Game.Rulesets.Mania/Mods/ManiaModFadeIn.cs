// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Mania.Objects.Drawables;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Mods {
    public class ManiaModFadeIn : ModHidden {
        public override string Name => "Fade In";
        public override string Acronym => "FI";
        public override FontAwesome Icon => FontAwesome.fa_osu_mod_hidden;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => @"Keys appear out of nowhere!";
        public override double ScoreMultiplier => 1;
        public override bool Ranked => true;
        public override Type[] IncompatibleMods => new[] { typeof(ModFlashlight<ManiaHitObject>) };
        private const double fading_duration = 150;
        private const double fading_offset_time = 300;

        protected override void ApplyHiddenState(DrawableHitObject drawableHitObject, ArmedState state) {
            var fadingStartTime = drawableHitObject.HitObject.StartTime - fading_duration - fading_offset_time;

            switch (drawableHitObject) {
                case DrawableHoldNoteTick holdNoteTick:
                case DrawableNote note:
                    drawableHitObject.Hide();
                    using (drawableHitObject.BeginAbsoluteSequence(fadingStartTime, true))
                        drawableHitObject.FadeIn(fading_duration);

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
