// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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
                        new ManiaModKeyCoop(),
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

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap, Mod[] mods = null) => new ManiaDifficultyCalculator(beatmap);

        public override int LegacyID => 3;

        public ManiaRuleset(RulesetInfo rulesetInfo = null)
            : base(rulesetInfo)
        {
        }

        public override IEnumerable<int> AvailableVariants
        {
            get
            {
                for (int i = 1; i <= 9; i++)
                    yield return (int)ManiaKeyBindingVariantType.Single + i;
                for (int i = 2; i <= 18; i++)
                    yield return (int)ManiaKeyBindingVariantType.Dual + i;
            }
        }

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
        {
            switch (getVariantType(variant))
            {
                case ManiaKeyBindingVariantType.Single:
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
                case ManiaKeyBindingVariantType.Dual:
                    getDualStageKeyCounts(variant, out int s1K, out int s2K);

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
                    }.GenerateKeyBindingsFor(s1K, out var nextNormal);

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
                    }.GenerateKeyBindingsFor(s2K, out _);

                    return stage1Bindings.Concat(stage2Bindings);
            }

            return new KeyBinding[0];
        }

        public override string GetVariantName(int variant)
        {
            switch (getVariantType(variant))
            {
                default:
                    return $"{variant}K";
                case ManiaKeyBindingVariantType.Dual:
                {
                    getDualStageKeyCounts(variant, out int s1K, out int s2K);
                    return $"{s1K}K + {s2K}K";
                }
            }
        }

        /// <summary>
        /// Finds the number of keys for each stage in a <see cref="ManiaKeyBindingVariantType.Dual"/> variant.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <param name="stage1">The number of keys for the first stage.</param>
        /// <param name="stage2">The number of keys for the second stage.</param>
        private void getDualStageKeyCounts(int variant, out int stage1, out int stage2)
        {
            int totalKeys = variant - (int)ManiaKeyBindingVariantType.Dual;
            stage1 = (int)Math.Ceiling(totalKeys / 2f);
            stage2 = (int)Math.Floor(totalKeys / 2f);
        }

        /// <summary>
        /// Finds the <see cref="ManiaKeyBindingVariantType"/> that corresponds to a variant value.
        /// </summary>
        /// <param name="variant">The variant value.</param>
        /// <returns>The <see cref="ManiaKeyBindingVariantType"/> that corresponds to <paramref name="variant"/>.</returns>
        private ManiaKeyBindingVariantType getVariantType(int variant)
        {
            return (ManiaKeyBindingVariantType)Enum.GetValues(typeof(ManiaKeyBindingVariantType)).Cast<int>().OrderByDescending(i => i).First(v => variant >= v);
        }
    }

    public class VariantMappingGenerator
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

    public enum ManiaKeyBindingVariantType
    {
        /// <summary>
        /// A single stage.
        /// Number of columns in this stage lies at (item - Single).
        /// </summary>
        Single = 0,
        /// <summary>
        /// A split stage.
        /// Overall number of columns lies at (item - Dual), further computation is required for
        /// number of columns in each individual stage.
        /// </summary>
        Dual = 1000,
    }
}
