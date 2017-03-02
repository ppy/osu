// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Graphics;
using osu.Game.Modes.Mania.UI;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Mods;
using OpenTK.Input;

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
                            new ManiaModEasy(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.W,
                        Mods = new Mod[]
                        {
                            new ManiaModNoFail(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.E,
                        Mods = new Mod[]
                        {
                            new ManiaModHalfTime(),
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
                            new ManiaModHardRock(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.S,
                        Mods = new Mod[]
                        {
                            new ManiaModSuddenDeath(),
                            new ModPerfect(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.D,
                        Mods = new Mod[]
                        {
                            new ManiaModDoubleTime(),
                            new ManiaModNightcore(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.F,
                        Mods = new Mod[]
                        {
                            new ManiaModHidden(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.G,
                        Mods = new Mod[]
                        {
                            new ManiaModFlashlight(),
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
                            new ManiaModKey4(),
                            new ManiaModKey5(),
                            new ManiaModKey6(),
                            new ManiaModKey7(),
                            new ManiaModKey8(),
                            new ManiaModKey9(),
                            new ManiaModKey1(),
                            new ManiaModKey2(),
                            new ManiaModKey3(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.X,
                        Mods = new Mod[]
                        {
                            new ManiaModKeyCoop(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.C,
                        Mods = new Mod[]
                        {
                            new ManiaModRandom(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.V,
                        Mods = new Mod[]
                        {
                            new ModAutoplay(),
                            new ModCinema(),
                        },
                    },
                }
            },
        };

        protected override PlayMode PlayMode => PlayMode.Mania;

        public override FontAwesome Icon => FontAwesome.fa_osu_mania_o;

        public override ScoreProcessor CreateScoreProcessor(int hitObjectCount) => null;

        public override HitObjectParser CreateHitObjectParser() => new NullHitObjectParser();

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new ManiaDifficultyCalculator(beatmap);
    }
}
