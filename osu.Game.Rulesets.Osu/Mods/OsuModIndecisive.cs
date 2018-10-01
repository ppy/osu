// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModIndecisive : OsuModHidden
    {
        public override string Name => "Indecisive";
        public override string ShortenedName => "ID";
        public override FontAwesome Icon => FontAwesome.fa_question_circle;
        public override ModType Type => ModType.Fun;
        public override string Description => "Nomod-Hidden alternation.";
        public override double ScoreMultiplier => 1;

        private const double fade_in_duration_multiplier = 0.4;
        private const double fade_out_duration_multiplier = 0.3;

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            void adjustFadeIn(OsuHitObject h) => h.TimeFadeIn = h.TimePreempt * fade_in_duration_multiplier;

            foreach (var d in drawables.Where((x, i) => i % 2 == 1).OfType<DrawableOsuHitObject>())
            {
                adjustFadeIn(d.HitObject);
                foreach (var h in d.HitObject.NestedHitObjects.OfType<OsuHitObject>())
                    adjustFadeIn(h);
            }

            base.ApplyToDrawableHitObjects(drawables.Where((x, i) => i % 2 == 1));
        }
    }
}
