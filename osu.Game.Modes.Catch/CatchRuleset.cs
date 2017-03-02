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

        public override IEnumerable<ModSection> CreateModSections() => new ModSection[]
        {
            new DifficultyReductionSection
            {
                Buttons = new[]
                {
                    new ModButton
                    {
                        ToggleKey = Key.Q,
                        Mods = new Mod[]
                        {
                            new CatchModEasy(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.W,
                        Mods = new Mod[]
                        {
                            new CatchModNoFail(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.E,
                        Mods = new Mod[]
                        {
                            new CatchModHalfTime(),
                        },
                    },
                },
            },
            new DifficultyIncreaseSection
            {
                Buttons = new ModButton[]
                {
                    new ModButton
                    {
                        ToggleKey = Key.A,
                        Mods = new Mod[]
                        {
                            new CatchModHardRock(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.S,
                        Mods = new Mod[]
                        {
                            new CatchModSuddenDeath(),
                            new ModPerfect(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.D,
                        Mods = new Mod[]
                        {
                            new CatchModDoubleTime(),
                            new CatchModNightcore(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.F,
                        Mods = new Mod[]
                        {
                            new CatchModHidden(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.G,
                        Mods = new Mod[]
                        {
                            new CatchModFlashlight(),
                        },
                    },
                },
            },
            new AssistedSection
            {
                Buttons = new[]
                {
                    new ModButton
                    {
                        ToggleKey = Key.Z,
                        Mods = new Mod[]
                        {
                            new CatchModRelax(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.B,
                        Mods = new Mod[]
                        {
                            new ModAutoplay(),
                            new ModCinema(),
                        },
                    },
                }
            },
        };

        protected override PlayMode PlayMode => PlayMode.Catch;

        public override FontAwesome Icon => FontAwesome.fa_osu_fruits_o;

        public override ScoreProcessor CreateScoreProcessor(int hitObjectCount) => null;

        public override HitObjectParser CreateHitObjectParser() => new NullHitObjectParser();

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new CatchDifficultyCalculator(beatmap);
    }
}
