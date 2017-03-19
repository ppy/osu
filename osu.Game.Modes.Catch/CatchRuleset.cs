// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Modes.Catch.Mods;
using osu.Game.Modes.Catch.UI;
using osu.Game.Modes.Mods;
using osu.Game.Modes.UI;
using osu.Game.Screens.Play;
using System.Collections.Generic;

namespace osu.Game.Modes.Catch
{
    public class CatchRuleset : Ruleset
    {
        public override HitRenderer CreateHitRendererWith(WorkingBeatmap beatmap) => new CatchHitRenderer(beatmap);

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
                                new CatchModSuddenDeath(),
                                new CatchModPerfect(),
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

        protected override PlayMode PlayMode => PlayMode.Catch;

        public override string Description => "osu!catch";

        public override FontAwesome Icon => FontAwesome.fa_osu_fruits_o;

        public override IEnumerable<KeyCounter> CreateGameplayKeys() => new KeyCounter[]
        {
            new KeyCounterKeyboard(Key.ShiftLeft),
            new KeyCounterMouse(MouseButton.Left),
            new KeyCounterMouse(MouseButton.Right)
        };

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new CatchDifficultyCalculator(beatmap);

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor();
    }
}
