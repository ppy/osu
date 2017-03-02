// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Game.Modes.Catch.UI;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Mods;
using OpenTK.Input;

namespace osu.Game.Modes.Catch
{
    public class CatchRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new OsuScoreOverlay();

        public override HitRenderer CreateHitRendererWith(Beatmap beatmap) => new CatchHitRenderer { Beatmap = beatmap };

        public override IEnumerable<Mod> AvailableMods => new Mod[]
        {
            new CatchModNoFail(),
            new CatchModEasy(),
            new CatchModHidden(),
            new CatchModHardRock(),
            new CatchModSuddenDeath(),
            new CatchModDoubleTime(),
            new CatchModRelax(),
            new CatchModHalfTime(),
            new CatchModNightcore(),
            new CatchModFlashlight(),
        };

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new CatchModEasy(),
                        new CatchModNoFail(),
                        new CatchModHalfTime(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new CatchModHardRock(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new CatchModPerfect(),
                                new CatchModSuddenDeath(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new CatchModDoubleTime(),
                                new CatchModNightcore(),
                            },
                        },
                        new CatchModHidden(),
                        new CatchModFlashlight(),
                    };

                case ModType.Special:
                    return new Mod[]
                    {
                        new CatchModRelax(),
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

        protected override PlayMode PlayMode => PlayMode.Catch;

        public override FontAwesome Icon => FontAwesome.fa_osu_fruits_o;

        public override ScoreProcessor CreateScoreProcessor(int hitObjectCount) => null;

        public override HitObjectParser CreateHitObjectParser() => new NullHitObjectParser();

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new CatchDifficultyCalculator(beatmap);
    }
}
