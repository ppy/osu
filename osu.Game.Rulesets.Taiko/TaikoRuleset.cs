﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Replays.Types;
using osu.Game.Rulesets.Taiko.Replays;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Rulesets.Taiko.Difficulty;

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoRuleset : Ruleset
    {
        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap) => new TaikoRulesetContainer(this, beatmap);
        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => new TaikoBeatmapConverter(beatmap);

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(InputKey.D, TaikoAction.LeftRim),
            new KeyBinding(InputKey.F, TaikoAction.LeftCentre),
            new KeyBinding(InputKey.J, TaikoAction.RightCentre),
            new KeyBinding(InputKey.K, TaikoAction.RightRim),
            new KeyBinding(InputKey.MouseLeft, TaikoAction.LeftCentre),
            new KeyBinding(InputKey.MouseLeft, TaikoAction.RightCentre),
            new KeyBinding(InputKey.MouseRight, TaikoAction.LeftRim),
            new KeyBinding(InputKey.MouseRight, TaikoAction.RightRim),
        };

        public override IEnumerable<Mod> ConvertLegacyMods(LegacyMods mods)
        {
            if (mods.HasFlag(LegacyMods.Nightcore))
                yield return new TaikoModNightcore();
            else if (mods.HasFlag(LegacyMods.DoubleTime))
                yield return new TaikoModDoubleTime();

            if (mods.HasFlag(LegacyMods.Autoplay))
                yield return new TaikoModAutoplay();

            if (mods.HasFlag(LegacyMods.Easy))
                yield return new TaikoModEasy();

            if (mods.HasFlag(LegacyMods.Flashlight))
                yield return new TaikoModFlashlight();

            if (mods.HasFlag(LegacyMods.HalfTime))
                yield return new TaikoModHalfTime();

            if (mods.HasFlag(LegacyMods.HardRock))
                yield return new TaikoModHardRock();

            if (mods.HasFlag(LegacyMods.Hidden))
                yield return new TaikoModHidden();

            if (mods.HasFlag(LegacyMods.NoFail))
                yield return new TaikoModNoFail();

            if (mods.HasFlag(LegacyMods.Perfect))
                yield return new TaikoModPerfect();

            if (mods.HasFlag(LegacyMods.Relax))
                yield return new TaikoModRelax();

            if (mods.HasFlag(LegacyMods.SuddenDeath))
                yield return new TaikoModSuddenDeath();
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new TaikoModEasy(),
                        new TaikoModNoFail(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new TaikoModHalfTime(),
                                new TaikoModDaycore(),
                            },
                        },
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
                                new TaikoModAutoplay(),
                                new ModCinema(),
                            },
                        },
                    };

                default:
                    return new Mod[] { };
            }
        }

        public override string Description => "osu!taiko";

        public override string ShortName => "taiko";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.fa_osu_taiko_o };

        public override DifficultyCalculator CreateDifficultyCalculator(IBeatmap beatmap, Mod[] mods = null) => new TaikoDifficultyCalculator(beatmap);

        public override int? LegacyID => 1;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new TaikoReplayFrame();

        public TaikoRuleset(RulesetInfo rulesetInfo = null)
            : base(rulesetInfo)
        {
        }
    }
}
