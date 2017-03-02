// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Game.Modes.Mania.UI;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Mania
{
    public class ManiaRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new OsuScoreOverlay();

        public override HitRenderer CreateHitRendererWith(Beatmap beatmap) => new ManiaHitRenderer { Beatmap = beatmap };

        public override IEnumerable<Mod> AvailableMods => new Mod[]
        {
            new ManiaModNoFail(),
            new ManiaModEasy(),
            new ManiaModHidden(),
            new ManiaModHardRock(),
            new ManiaModSuddenDeath(),
            new ManiaModDoubleTime(),
            new ManiaModHalfTime(),
            new ManiaModNightcore(),
            new ManiaModFlashlight(),
            new ManiaModFadeIn(),
            new ManiaModRandom(),
            new ManiaModKey1(),
            new ManiaModKey2(),
            new ManiaModKey3(),
            new ManiaModKey4(),
            new ManiaModKey5(),
            new ManiaModKey6(),
            new ManiaModKey7(),
            new ManiaModKey8(),
            new ManiaModKey9(),
            new ManiaModKeyCoop(),
        };

        protected override PlayMode PlayMode => PlayMode.Mania;

        public override FontAwesome Icon => FontAwesome.fa_osu_mania_o;

        public override ScoreProcessor CreateScoreProcessor(int hitObjectCount) => null;

        public override HitObjectParser CreateHitObjectParser() => new NullHitObjectParser();

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new ManiaDifficultyCalculator(beatmap);
    }
}
