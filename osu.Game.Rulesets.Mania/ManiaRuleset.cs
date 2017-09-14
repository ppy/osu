// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Rulesets.Mania.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using System.Collections.Generic;
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
                        new ManiaModGravity()
                    };

                default:
                    return new Mod[] { };
            }
        }

        public override string Description => "osu!mania";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.fa_osu_mania_o };

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new ManiaDifficultyCalculator(beatmap);

        public override int LegacyID => 3;

        public ManiaRuleset(RulesetInfo rulesetInfo)
            : base(rulesetInfo)
        {
        }

        public override IEnumerable<int> AvailableVariants => new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
        {
            var leftKeys = new[]
            {
                InputKey.A,
                InputKey.S,
                InputKey.D,
                InputKey.F
            };

            var rightKeys = new[]
            {
                InputKey.J,
                InputKey.K,
                InputKey.L,
                InputKey.Semicolon
            };

            ManiaAction currentKey = ManiaAction.Key1;

            var bindings = new List<KeyBinding>();

            for (int i = leftKeys.Length - variant / 2; i < leftKeys.Length; i++)
                bindings.Add(new KeyBinding(leftKeys[i], currentKey++));

            for (int i = 0; i < variant / 2; i++)
                bindings.Add(new KeyBinding(rightKeys[i], currentKey++));

            if (variant % 2 == 1)
                bindings.Add(new KeyBinding(InputKey.Space, ManiaAction.Special));

            return bindings;
        }

        public override string GetVariantName(int variant) => $"{variant}K";
    }
}
