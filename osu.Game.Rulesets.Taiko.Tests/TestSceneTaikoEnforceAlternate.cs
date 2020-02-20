// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Utils;
using osu.Game.Input;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Mods;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Tests
{
    public class TestSceneTaikoEnforceAlternate : PlayerTestScene
    {
        protected override bool AllowFail => true;

        private TaikoModEnforceAlternate mod;

        private List<DatabasedKeyBinding> bindings;

        public TestSceneTaikoEnforceAlternate()
            : base(new TaikoRuleset())
        {
        }

        [BackgroundDependencyLoader]
        private void load(KeyBindingStore keyBindings)
        {
            var taikoBindings = keyBindings.Query(Ruleset.Value.ID, 0);

            bindings = taikoBindings.Where(b => Enum.IsDefined(typeof(Key), (Key)b.KeyCombination.Keys.First())).ToList();
        }

        [Test]
        public void TestFailOnRepeat()
        {
            bool judged = false;
            AddStep("setup judgements", () =>
            {
                judged = false;
                ((ScoreAccessiblePlayer)Player).ScoreProcessor.NewJudgement += b => judged = true;
            });

            var bind = Key.F;
            AddStep("get random bind", () =>
            {
                var actions = Enum.GetValues(typeof(TaikoAction));
                var randomAction = (TaikoAction)actions.GetValue(RNG.Next() % actions.Length);
                bind = (Key)bindings.Find(b => b.GetAction<TaikoAction>() == randomAction).KeyCombination.Keys.First();
            });

            AddStep("double press", () =>
            {
                InputManager.PressKey(bind);
                InputManager.ReleaseKey(bind);
                InputManager.PressKey(bind);
                InputManager.ReleaseKey(bind);
            });

            AddUntilStep("judged", () => judged);

            AddAssert("failed", () => Player.HasFailed);
        }

        protected override Player CreatePlayer(Ruleset ruleset)
        {
            mod = new TaikoModEnforceAlternate();
            mod.CauseFail.Value = true;
            SelectedMods.Value = new Mod[] { mod };

            return new ScoreAccessiblePlayer();
        }

        private class ScoreAccessiblePlayer : TestPlayer
        {
            public ScoreAccessiblePlayer()
                : base(false, false)
            {
            }

            public new ScoreProcessor ScoreProcessor => base.ScoreProcessor;
        }
    }
}
