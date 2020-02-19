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
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests
{
    public class TestSceneOsuEnforceAlternate : TestSceneOsuPlayer
    {
        protected override bool AllowFail => true;

        private OsuModEnforceAlternate mod;
        private readonly LocalInputManager inputManager;

        public TestSceneOsuEnforceAlternate()
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
                mod = new OsuModEnforceAlternate();
                mod.CauseFail.Value = true;
            });

            base.SetUpSteps();

            bool judged = false;
            AddStep("setup judgements", () =>
            {
                judged = false;
                ((ScoreAccessiblePlayer)Player).ScoreProcessor.NewJudgement += b => judged = true;
            });

            var bind = Key.F;
            AddStep("get random bind", () =>
            {
                var actions = Enum.GetValues(typeof(OsuAction));
                var randomAction = (OsuAction)actions.GetValue(RNG.Next() % actions.Length);
                bind = (Key)inputManager.KeyBindings.ToList().Find(b => b.GetAction<OsuAction>() == randomAction).KeyCombination.Keys.First();
            });

            AddStep("double click", () =>
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

        private class LocalInputManager : OsuInputManager
        {
            public IEnumerable<KeyBinding> KeyBindings => ((LocalKeyBindingContainer)KeyBindingContainer).KeyBindings;

            public LocalInputManager()
                : base(new OsuRuleset().RulesetInfo)
            {
            }

            protected override RulesetKeyBindingContainer CreateKeyBindingContainer(RulesetInfo ruleset, int variant, SimultaneousBindingMode unique)
                => new LocalKeyBindingContainer(ruleset, variant, unique);

            private class LocalKeyBindingContainer : OsuKeyBindingContainer
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
