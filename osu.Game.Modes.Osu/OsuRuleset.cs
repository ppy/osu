// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using OpenTK.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;
using osu.Game.Overlays.Mods;

namespace osu.Game.Modes.Osu
{
    public class OsuRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new OsuScoreOverlay();

        public override HitRenderer CreateHitRendererWith(Beatmap beatmap) => new OsuHitRenderer { Beatmap = beatmap };

        public override IEnumerable<BeatmapStatistic> GetBeatmapStatistics(WorkingBeatmap beatmap) => new[]
        {
            new BeatmapStatistic
            {
                Name = @"Circle count",
                Content = beatmap.Beatmap.HitObjects.Count(h => h is HitCircle).ToString(),
                Icon = FontAwesome.fa_dot_circle_o
            },
            new BeatmapStatistic
            {
                Name = @"Slider count",
                Content = beatmap.Beatmap.HitObjects.Count(h => h is Slider).ToString(),
                Icon = FontAwesome.fa_circle_o
            }
        };

        public override IEnumerable<Mod> AvailableMods => new Mod[]
        {
            new OsuModNoFail(),
            new OsuModEasy(),
            new OsuModHidden(),
            new OsuModHardRock(),
            new OsuModSuddenDeath(),
            new OsuModDoubleTime(),
            new OsuModRelax(),
            new OsuModHalfTime(),
            new OsuModNightcore(),
            new OsuModFlashlight(),
            new OsuModSpunOut(),
            new OsuModAutopilot(),
            new OsuModTarget(),
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
                            new OsuModEasy(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.W,
                        Mods = new Mod[]
                        {
                            new OsuModNoFail(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.E,
                        Mods = new Mod[]
                        {
                            new OsuModHalfTime(),
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
                            new OsuModHardRock(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.S,
                        Mods = new Mod[]
                        {
                            new OsuModSuddenDeath(),
                            new ModPerfect(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.D,
                        Mods = new Mod[]
                        {
                            new OsuModDoubleTime(),
                            new OsuModNightcore(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.F,
                        Mods = new Mod[]
                        {
                            new OsuModHidden(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.G,
                        Mods = new Mod[]
                        {
                            new OsuModFlashlight(),
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
                            new OsuModRelax(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.X,
                        Mods = new Mod[]
                        {
                            new OsuModAutopilot(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.C,
                        Mods = new Mod[]
                        {
                            new OsuModTarget(),
                        },
                    },
                    new ModButton
                    {
                        ToggleKey = Key.V,
                        Mods = new Mod[]
                        {
                            new OsuModSpunOut(),
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

        public override FontAwesome Icon => FontAwesome.fa_osu_osu_o;

        public override HitObjectParser CreateHitObjectParser() => new OsuHitObjectParser();

        public override ScoreProcessor CreateScoreProcessor(int hitObjectCount) => new OsuScoreProcessor(hitObjectCount);

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new OsuDifficultyCalculator(beatmap);

        protected override PlayMode PlayMode => PlayMode.Osu;
    }
}
