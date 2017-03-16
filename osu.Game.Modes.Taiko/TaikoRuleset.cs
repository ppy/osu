// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Modes.Mods;
using osu.Game.Modes.Taiko.Mods;
using osu.Game.Modes.Taiko.UI;
using osu.Game.Modes.UI;
using osu.Game.Screens.Play;
using System.Collections.Generic;

namespace osu.Game.Modes.Taiko
{
    public class TaikoRuleset : Ruleset
    {
        public override HitRenderer CreateHitRendererWith(WorkingBeatmap beatmap) => new TaikoHitRenderer(beatmap);

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
                        null,
                        null,
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

        public override string Description => "osu!taiko";

        public override FontAwesome Icon => FontAwesome.fa_osu_taiko_o;

        public override IEnumerable<KeyCounter> CreateGameplayKeys() => new KeyCounter[]
        {
            new KeyCounterKeyboard(Key.D),
            new KeyCounterKeyboard(Key.F),
            new KeyCounterKeyboard(Key.J),
            new KeyCounterKeyboard(Key.K)
        };

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new TaikoDifficultyCalculator(beatmap);

        public override ScoreProcessor CreateScoreProcessor() => new TaikoScoreProcessor();
    }
}
