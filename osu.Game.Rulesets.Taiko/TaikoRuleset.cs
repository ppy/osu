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

namespace osu.Game.Rulesets.Taiko
{
    public class TaikoRuleset : Ruleset
    {
        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap, bool isForCurrentRuleset) => new TaikoRulesetContainer(this, beatmap, isForCurrentRuleset);

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

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap, Mod[] mods = null) => new TaikoDifficultyCalculator(beatmap);

        public override int? LegacyID => 1;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new TaikoReplayFrame();

        public TaikoRuleset(RulesetInfo rulesetInfo = null)
            : base(rulesetInfo)
        {
        }
    }
}
