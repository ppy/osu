// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.Taiko.UI;
using osu.Game.Modes.UI;
using osu.Game.Beatmaps;

namespace osu.Game.Modes.Taiko
{
    public class TaikoRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new OsuScoreOverlay();

        public override HitRenderer CreateHitRendererWith(Beatmap beatmap) => new TaikoHitRenderer { Beatmap = beatmap };

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new TaikoModEasy(),
                        new TaikoModNoFail(),
                        new TaikoModHalfTime(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new TaikoModHardRock(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new TaikoModSuddenDeath(),
                                new TaikoModPerfect(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new TaikoModDoubleTime(),
                                new TaikoModNightcore(),
                            },
                        },
                        new TaikoModHidden(),
                        new TaikoModFlashlight(),
                    };

                case ModType.Special:
                    return new Mod[]
                    {
                        new TaikoModRelax(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ModAutoplay(),
                                new ModCinema(),
                            },
                        },
                    };

                default:
                    return new Mod[] { };
            }
        }

        protected override PlayMode PlayMode => PlayMode.Taiko;

        public override FontAwesome Icon => FontAwesome.fa_osu_taiko_o;

        public override ScoreProcessor CreateScoreProcessor(int hitObjectCount) => null;

        public override HitObjectParser CreateHitObjectParser() => new NullHitObjectParser();

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new TaikoDifficultyCalculator(beatmap);
    }
}
