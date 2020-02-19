// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Input.Bindings;
using osu.Framework.Testing;
using osu.Framework.Utils;
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
        private readonly LocalInputManager inputManager;

        public TestSceneTaikoEnforceAlternate()
            : base(new TaikoRuleset())
        {
            base.Content.Add(inputManager = new LocalInputManager());
        }

        [SetUpSteps]
        public override void SetUpSteps()
        {
        }

        [Test]
        public void TestFailOnRepeat()
        {
            AddStep("turn on fail on repeat", () =>
            {
                mod = new TaikoModEnforceAlternate();
                mod.CauseFail.Value = true;
            });

            base.SetUpSteps();

            bool judged = false;
            AddStep("setup judgements", () =>
            {
                judged = false;
                ((ScoreAccessiblePlayer)Player).ScoreProcessor.NewJudgement += b => judged = true;
            });

            Key bind = Key.F;
            AddStep("get random bind", () =>
            {
                var actions = Enum.GetValues(typeof(TaikoAction));
                var randomAction = (TaikoAction)actions.GetValue(RNG.Next() % actions.Length);
                bind = (Key)inputManager.KeyBindings.ToList().Find(b => b.GetAction<TaikoAction>() == randomAction).KeyCombination.Keys.First();
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

        private class LocalInputManager : TaikoInputManager
        {
            public IEnumerable<KeyBinding> KeyBindings => ((LocalKeyBindingContainer)KeyBindingContainer).KeyBindings;

            public LocalInputManager()
                : base(new TaikoRuleset().RulesetInfo)
            {
            }

            protected override RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                => new LocalKeyBindingContainer(ruleset, variant, unique);

            private class LocalKeyBindingContainer : TaikoKeyBindingContainer
            {
                public new IEnumerable<KeyBinding> KeyBindings => base.KeyBindings;

                public LocalKeyBindingContainer(RulesetInfo info, int variant, SimultaneousBindingMode unique)
                    : base(info, variant, unique)
                {
                }
            }
        }
    }
}
