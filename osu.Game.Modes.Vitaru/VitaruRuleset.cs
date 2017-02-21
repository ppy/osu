//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.UI;
using osu.Game.Modes.Vitaru.Objects;
using System;
using osu.Game.Modes.Vitaru.UI;
using osu.Game.Graphics;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Vitaru
{
    public class VitaruRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new VitaruScoreOverlay();

        public override HitObjectParser CreateHitObjectParser() => new VitaruObjectParser();

        public ScoreProcessor CreateScoreProcessor() => new VitaruScoreProcessor();

        public override ScoreProcessor CreateScoreProcessor(int hitObjectCount)
        {
            throw new NotImplementedException();
        }

        public override HitRenderer CreateHitRendererWith(Beatmap beatmap)
        {
            throw new NotImplementedException();
        }

        public override FontAwesome Icon => FontAwesome.fa_osu_vitaru_o;

        protected override PlayMode PlayMode => PlayMode.Vitaru;
    }
}
