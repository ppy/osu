// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Gameplay
{
    [HeadlessTest]
    public partial class TestSceneKeyBindings : OsuManualInputManagerTestScene
    {
        private readonly ActionReceiver receiver;

        public TestSceneKeyBindings()
        {
            Add(new TestKeyBindingContainer
            {
                Child = receiver = new ActionReceiver()
            });
        }

        [Test]
        public void TestDefaultsWhenNotDatabased()
        {
            AddStep("fire key", () => InputManager.Key(Key.A));

            AddAssert("received key", () => receiver.ReceivedAction);
        }

        private class TestRuleset : Ruleset
        {
            public override IEnumerable<Mod> GetModsFor(ModType type) =>
                throw new System.NotImplementedException();

            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod> mods = null) =>
                throw new System.NotImplementedException();

            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
                throw new System.NotImplementedException();

            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) =>
                throw new System.NotImplementedException();

            public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0)
            {
                return new[]
                {
                    new KeyBinding(InputKey.A, TestAction.Down),
                };
            }

            public override string Description => "test";
            public override string ShortName => "test";
        }

        private enum TestAction
        {
            Down,
        }

        private partial class TestKeyBindingContainer : DatabasedKeyBindingContainer<TestAction>
        {
            public TestKeyBindingContainer()
                : base(new TestRuleset().RulesetInfo, 0)
            {
            }
        }

        private partial class ActionReceiver : CompositeDrawable, IKeyBindingHandler<TestAction>
        {
            public bool ReceivedAction;

            public bool OnPressed(KeyBindingPressEvent<TestAction> e)
            {
                if (e.Repeat)
                    return false;

                ReceivedAction = e.Action == TestAction.Down;
                return true;
            }

            public void OnReleased(KeyBindingReleaseEvent<TestAction> e)
            {
            }
        }
    }
}
