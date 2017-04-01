﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Modes.Mods;
using osu.Game.Modes.Osu.Mods;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;
using osu.Game.Screens.Play;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Modes.Osu.Scoring;
using osu.Game.Modes.Scoring;

namespace osu.Game.Modes.Osu
{
    public class OsuRuleset : Ruleset
    {
        public override HitRenderer CreateHitRendererWith(WorkingBeatmap beatmap) => new OsuHitRenderer(beatmap);

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

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new OsuModEasy(),
                        new OsuModNoFail(),
                        new OsuModHalfTime(),
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new OsuModHardRock(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new OsuModSuddenDeath(),
                                new OsuModPerfect(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new OsuModDoubleTime(),
                                new OsuModNightcore(),
                            },
                        },
                        new OsuModHidden(),
                        new OsuModFlashlight(),
                    };

                case ModType.Special:
                    return new Mod[]
                    {
                        new OsuModRelax(),
                        new OsuModAutopilot(),
                        new OsuModSpunOut(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new OsuModAutoplay(),
                                new ModCinema(),
                            },
                        },
                        new OsuModTarget(),
                    };

                default:
                    return new Mod[] { };
            }
        }

        public override FontAwesome Icon => FontAwesome.fa_osu_osu_o;

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new OsuDifficultyCalculator(beatmap);

        protected override PlayMode PlayMode => PlayMode.Osu;

        public override string Description => "osu!";

        public override IEnumerable<KeyCounter> CreateGameplayKeys() => new KeyCounter[]
        {
            new KeyCounterKeyboard(Key.Z),
            new KeyCounterKeyboard(Key.X),
            new KeyCounterMouse(MouseButton.Left),
            new KeyCounterMouse(MouseButton.Right)
        };

        public override ScoreProcessor CreateScoreProcessor() => new OsuScoreProcessor();
    }
}
