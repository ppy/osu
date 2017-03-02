// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.Taiko.UI;
using osu.Game.Modes.UI;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Mods;
using OpenTK.Input;

namespace osu.Game.Modes.Taiko
{
    public class TaikoRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new OsuScoreOverlay();

        public override HitRenderer CreateHitRendererWith(Beatmap beatmap) => new TaikoHitRenderer { Beatmap = beatmap };

        public override IEnumerable<Mod> AvailableMods => new Mod[]
        {
            new TaikoModNoFail(),
            new TaikoModEasy(),
            new TaikoModHidden(),
            new TaikoModHardRock(),
            new TaikoModSuddenDeath(),
            new TaikoModDoubleTime(),
            new TaikoModRelax(),
            new TaikoModHalfTime(),
            new TaikoModNightcore(),
            new TaikoModFlashlight(),
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
                            new TaikoModEasy(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.W,
                        Mods = new Mod[]
                        {
                            new TaikoModNoFail(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.E,
                        Mods = new Mod[]
                        {
                            new TaikoModHalfTime(),
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
                            new TaikoModHardRock(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.S,
                        Mods = new Mod[]
                        {
                            new TaikoModSuddenDeath(),
                            new ModPerfect(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.D,
                        Mods = new Mod[]
                        {
                            new TaikoModDoubleTime(),
                            new TaikoModNightcore(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.F,
                        Mods = new Mod[]
                        {
                            new TaikoModHidden(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.G,
                        Mods = new Mod[]
                        {
                            new TaikoModFlashlight(),
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
                            new TaikoModRelax(),
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

        protected override PlayMode PlayMode => PlayMode.Taiko;

        public override FontAwesome Icon => FontAwesome.fa_osu_taiko_o;

        public override ScoreProcessor CreateScoreProcessor(int hitObjectCount) => null;

        public override HitObjectParser CreateHitObjectParser() => new NullHitObjectParser();

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new TaikoDifficultyCalculator(beatmap);
    }
}
