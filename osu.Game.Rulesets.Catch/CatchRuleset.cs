// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Input;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Rulesets.Catch.Mods;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Play;
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Catch.Scoring;
using osu.Game.Rulesets.Scoring;
using osu.Framework.Input.Bindings;

namespace osu.Game.Rulesets.Catch
{
    public class CatchRuleset : Ruleset
    {
        public override RulesetContainer CreateRulesetContainerWith(WorkingBeatmap beatmap, bool isForCurrentRuleset) => new CatchRulesetContainer(this, beatmap, isForCurrentRuleset);

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => new[]
        {
            new KeyBinding(Key.Z, CatchAction.MoveLeft),
            new KeyBinding(Key.Left, CatchAction.MoveLeft),
            new KeyBinding(Key.X, CatchAction.MoveRight),
            new KeyBinding(Key.Right, CatchAction.MoveRight),
            new KeyBinding(Key.LShift, CatchAction.Dash),
            new KeyBinding(Key.RShift, CatchAction.Dash),
        };

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return new Mod[]
                    {
                        new CatchModEasy(),
                        new CatchModNoFail(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new CatchModHalfTime(),
                                new CatchModDaycore(),
                            },
                        },
                    };

                case ModType.DifficultyIncrease:
                    return new Mod[]
                    {
                        new CatchModHardRock(),
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new CatchModSuddenDeath(),
                                new CatchModPerfect(),
                            },
                        },
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new CatchModDoubleTime(),
                                new CatchModNightcore(),
                            },
                        },
                        new CatchModHidden(),
                        new CatchModFlashlight(),
                    };

                case ModType.Special:
                    return new Mod[]
                    {
                        new CatchModRelax(),
                        null,
                        null,
                        new MultiMod
                        {
                            Mods = new Mod[]
                            {
                                new ModAutoplay(),
                                new ModCinema(),
                            },
                        },
                    };

                default:
                    return new Mod[] { };
            }
        }

        public override Mod GetAutoplayMod() => new ModAutoplay();

        public override string Description => "osu!catch";

        public override Drawable CreateIcon() => new SpriteIcon { Icon = FontAwesome.fa_osu_fruits_o };

        public override IEnumerable<KeyCounter> CreateGameplayKeys() => new KeyCounter[]
        {
            new KeyCounterKeyboard(Key.ShiftLeft),
            new KeyCounterMouse(MouseButton.Left),
            new KeyCounterMouse(MouseButton.Right)
        };

        public override DifficultyCalculator CreateDifficultyCalculator(Beatmap beatmap) => new CatchDifficultyCalculator(beatmap);

        public override ScoreProcessor CreateScoreProcessor() => new CatchScoreProcessor();

        public override int LegacyID => 2;

        public CatchRuleset(RulesetInfo rulesetInfo)
            : base(rulesetInfo)
        {
        }
    }
}
