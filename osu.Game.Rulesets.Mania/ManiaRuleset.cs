﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Graphics;
using osu.Game.Rulesets.Mania.Replays;
using osu.Game.Rulesets.Replays.Types;

namespace osu.Game.Rulesets.Mania
{
    public class ManiaRuleset : Ruleset
    {
        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap, bool isForCurrentRuleset) => new ManiaRulesetContainer(this, beatmap, isForCurrentRuleset);

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new ManiaModEasy(),
                        new ManiaModNoFail(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ManiaModHalfTime(),
                                new ManiaModDaycore(),
                            },
                        },
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new ManiaModHardRock(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ManiaModSuddenDeath(),
                                new ManiaModPerfect(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ManiaModDoubleTime(),
                                new ManiaModNightcore(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ManiaModFadeIn(),
                                new ManiaModHidden(),
                            }
                        },
                        new ManiaModFlashlight(),
                    };

                case ModType.Special:
                    return new Mod[]
                    {
                        new MultiMod
                        {
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
                        new ManiaModRandom(),
                        new ManiaModDualStages(),
                        new ManiaModMirror(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ManiaModAutoplay(),
                                new ModCinema(),
                            },
                        },
                    };

                default:
                    return new Mod[] { };
            }
        }

        public override string Description => "osu!mania";

        public override string ShortName => "mania";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.fa_osu_mania_o };

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap, Mod[] mods = null) => new ManiaDifficultyCalculator(beatmap, mods);

        public override int? LegacyID => 3;

        public override IConvertibleReplayFrame CreateConvertibleReplayFrame() => new ManiaReplayFrame();

        public ManiaRuleset(RulesetInfo rulesetInfo = null)
            : base(rulesetInfo)
        {
        }

        public override IEnumerable<int> AvailableVariants
        {
            get
            {
                for (int i = 1; i <= 9; i++)
                    yield return (int)PlayfieldType.Single + i;
                for (int i = 2; i <= 18; i += 2)
                    yield return (int)PlayfieldType.Dual + i;
            }
        }

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
        {
            switch (getPlayfieldType(variant))
            {
                case PlayfieldType.Single:
                    return new VariantMappingGenerator
                    {
                        LeftKeys = new[]
                        {
                            InputKey.A,
                            InputKey.S,
                            InputKey.D,
                            InputKey.F
                        },
                        RightKeys = new[]
                        {
                            InputKey.J,
                            InputKey.K,
                            InputKey.L,
                            InputKey.Semicolon
                        },
                        SpecialKey = InputKey.Space,
                        SpecialAction = ManiaAction.Special1,
                        NormalActionStart = ManiaAction.Key1,
                    }.GenerateKeyBindingsFor(variant, out _);
                case PlayfieldType.Dual:
                    int keys = getDualStageKeyCount(variant);

                    var stage1Bindings = new VariantMappingGenerator
                    {
                        LeftKeys = new[]
                        {
                            InputKey.Number1,
                            InputKey.Number2,
                            InputKey.Number3,
                            InputKey.Number4,
                        },
                        RightKeys = new[]
                        {
                            InputKey.Z,
                            InputKey.X,
                            InputKey.C,
                            InputKey.V
                        },
                        SpecialKey = InputKey.Tilde,
                        SpecialAction = ManiaAction.Special1,
                        NormalActionStart = ManiaAction.Key1
                    }.GenerateKeyBindingsFor(keys, out var nextNormal);

                    var stage2Bindings = new VariantMappingGenerator
                    {
                        LeftKeys = new[]
                        {
                            InputKey.Number7,
                            InputKey.Number8,
                            InputKey.Number9,
                            InputKey.Number0
                        },
                        RightKeys = new[]
                        {
                            InputKey.O,
                            InputKey.P,
                            InputKey.BracketLeft,
                            InputKey.BracketRight
                        },
                        SpecialKey = InputKey.BackSlash,
                        SpecialAction = ManiaAction.Special2,
                        NormalActionStart = nextNormal
                    }.GenerateKeyBindingsFor(keys, out _);

                    return stage1Bindings.Concat(stage2Bindings);
            }

            return new KeyBinding[0];
        }

        public override string GetVariantName(int variant)
        {
            switch (getPlayfieldType(variant))
            {
                default:
                    return $"{variant}K";
                case PlayfieldType.Dual:
                {
                    var keys = getDualStageKeyCount(variant);
                    return $"{keys}K + {keys}K";
                }
            }
        }

        /// <summary>
        /// Finds the number of keys for each stage in a <see cref="PlayfieldType.Dual"/> variant.
        /// </summary>
        /// <param name="variant">The variant.</param>
        private int getDualStageKeyCount(int variant) => (variant - (int)PlayfieldType.Dual) / 2;

        /// <summary>
        /// Finds the <see cref="PlayfieldType"/> that corresponds to a variant value.
        /// </summary>
        /// <param name="variant">The variant value.</param>
        /// <returns>The <see cref="PlayfieldType"/> that corresponds to <paramref name="variant"/>.</returns>
        private PlayfieldType getPlayfieldType(int variant)
        {
            return (PlayfieldType)Enum.GetValues(typeof(PlayfieldType)).Cast<int>().OrderByDescending(i => i).First(v => variant >= v);
        }

        private class VariantMappingGenerator
        {
            /// <summary>
            /// All the <see cref="InputKey"/>s available to the left hand.
            /// </summary>
            public InputKey[] LeftKeys;

            /// <summary>
            /// All the <see cref="InputKey"/>s available to the right hand.
            /// </summary>
            public InputKey[] RightKeys;

            /// <summary>
            /// The <see cref="InputKey"/> for the special key.
            /// </summary>
            public InputKey SpecialKey;

            /// <summary>
            /// The <see cref="ManiaAction"/> at which the normal columns should begin.
            /// </summary>
            public ManiaAction NormalActionStart;

            /// <summary>
            /// The <see cref="ManiaAction"/> for the special column.
            /// </summary>
            public ManiaAction SpecialAction;

            /// <summary>
            /// Generates a list of <see cref="KeyBinding"/>s for a specific number of columns.
            /// </summary>
            /// <param name="columns">The number of columns that need to be bound.</param>
            /// <param name="nextNormalAction">The next <see cref="ManiaAction"/> to use for normal columns.</param>
            /// <returns>The keybindings.</returns>
            public IEnumerable<KeyBinding> GenerateKeyBindingsFor(int columns, out ManiaAction nextNormalAction)
            {
                ManiaAction currentNormalAction = NormalActionStart;

                var bindings = new List<KeyBinding>();

                for (int i = LeftKeys.Length - columns / 2; i < LeftKeys.Length; i++)
                    bindings.Add(new KeyBinding(LeftKeys[i], currentNormalAction++));

                for (int i = 0; i < columns / 2; i++)
                    bindings.Add(new KeyBinding(RightKeys[i], currentNormalAction++));

                if (columns % 2 == 1)
                    bindings.Add(new KeyBinding(SpecialKey, SpecialAction));

                nextNormalAction = currentNormalAction;
                return bindings;
            }
        }
    }

    public enum PlayfieldType
    {
        /// <summary>
        /// Columns are grouped into a single stage.
        /// Number of columns in this stage lies at (item - Single).
        /// </summary>
        Single = 0,
        /// <summary>
        /// Columns are grouped into two stages.
        /// Overall number of columns lies at (item - Dual), further computation is required for
        /// number of columns in each individual stage.
        /// </summary>
        Dual = 1000,
    }
}
