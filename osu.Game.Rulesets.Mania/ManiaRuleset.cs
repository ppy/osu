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
                    yield return (int)ManiaKeyBindingVariantType.Solo + i;
                for (int i = 2; i <= 18; i++)
                    yield return (int)ManiaKeyBindingVariantType.Coop + i;
                // Todo: Versus mode
            }
        }

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
        {
            switch (getVariantType(variant))
            {
                case ManiaKeyBindingVariantType.Solo:
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
                case ManiaKeyBindingVariantType.Coop:
                case ManiaKeyBindingVariantType.Versus:
                    getMultiVariantKeyCounts(variant, out int p1K, out int p2K);

                    var player1Bindings = new VariantMappingGenerator
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
                    }.GenerateKeyBindingsFor(p1K, out var nextNormal);

                    var player2Bindings = new VariantMappingGenerator
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
                    }.GenerateKeyBindingsFor(p2K, out _);

                    return player1Bindings.Concat(player2Bindings);
            }

            return new KeyBinding[0];
        }

        public override string GetVariantName(int variant)
        {
            switch (getVariantType(variant))
            {
                default:
                case ManiaKeyBindingVariantType.Solo:
                    return $"{variant}K";
                case ManiaKeyBindingVariantType.Coop:
                {
                    getMultiVariantKeyCounts(variant, out int p1K, out int p2K);
                    return $"{p1K}K + {p2K}K";
                }
                case ManiaKeyBindingVariantType.Versus:
                {
                    getMultiVariantKeyCounts(variant, out int p1K, out int p2K);
                    return $"{p1K}K Vs. {p2K}K";
                }
            }
        }

        /// <summary>
        /// Finds the number of keys for each player in <see cref="ManiaKeyBindingVariantType.Coop"/> or <see cref="ManiaKeyBindingVariantType.Versus"/>.
        /// </summary>
        /// <param name="variant">The variant.</param>
        /// <param name="player1Keys">The number of keys for player 1.</param>
        /// <param name="player2Keys">The number of keys for player 2.</param>
        private void getMultiVariantKeyCounts(int variant, out int player1Keys, out int player2Keys)
        {
            player1Keys = 0;
            player2Keys = 0;

            switch (getVariantType(variant))
            {
                case ManiaKeyBindingVariantType.Coop:
                {
                    int totalKeys = variant - (int)ManiaKeyBindingVariantType.Coop;
                    player1Keys = (int)Math.Ceiling(totalKeys / 2f);
                    player2Keys = (int)Math.Floor(totalKeys / 2f);
                    break;
                }
                case ManiaKeyBindingVariantType.Versus:
                {
                    int totalKeys = variant - (int)ManiaKeyBindingVariantType.Versus;
                    player1Keys = totalKeys;
                    player2Keys = totalKeys;
                    break;
                }
            }
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
        /// Solo play keybinding variant (single stage).
        /// </summary>
        Solo = 0,
        /// <summary>
        /// Co-op play keybinding variant (multiple stages).
        /// </summary>
        Coop = 1000,
        /// <summary>
        /// Versus play keybinding variant (multiple stages).
        /// </summary>
        Versus = 10000
    }
}
