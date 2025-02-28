// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Configuration.Tracking;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges.Events;
using osu.Framework.IO.Stores;
using osu.Framework.Lists;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Input.Handlers;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Tests.Resources;
using osu.Game.Tests.Visual;

namespace osu.Game.Tests.Input
{
    [HeadlessTest]
    public partial class ReplayInputHandlerTest : OsuTestScene
    {
        // There are commented out assertions that will always fail as Replay inputs don't go through the typical input flow
        // Related framework issue: https://github.com/ppy/osu-framework/issues/6037
        [Test]
        public void TestNoSimultaneousBindings()
        {
            Clear();

            TestRulesetInputManager rulesetInputManager = new TestRulesetInputManager(SimultaneousBindingMode.None);
            Add(rulesetInputManager);
            RulesetInputManagerInputState<TestAction> state = new RulesetInputManagerInputState<TestAction>(rulesetInputManager.CurrentState);

            List<TestAction> actions = new List<TestAction>();

            AddAssert("No actions are pressed.", () => rulesetInputManager.PressedActions.Count == 0);

            AddLabel("Test single action");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Remove action", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is not pressed anymore", () => rulesetInputManager.PressedActions.Count == 0);
            AddAssert("1 Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Reset actions", resetActions);

            AddLabel("Test multiple unique actions");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Add TestKey2", () => actions.Add(TestAction.TestKey2));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is not pressed anymore", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 0);
            AddAssert("TestKey2 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey2) == 1);
            AddAssert("1 Press event for TestKey2", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey2) == 1);

            // Pressing TestKey2 should've released TestKey1 in this mode
            // AddAssert("1 Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1);

            AddStep("Remove TestKey2", () => actions.Remove(TestAction.TestKey2));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey2 is not pressed anymore", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey2) == 0);
            AddAssert("TestKey1 is still not pressed anymore", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 0);
            AddAssert("1 Release event for TestKey2", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey2) == 1);
            AddStep("Reset actions", resetActions);

            AddLabel("Test multiple identical actions");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Add TestKey1 again", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);

            // Again, this normally shouldn't fail, but it does due to the same reason as above.
            // AddAssert("No Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 0);
            AddAssert("No Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 0);

            AddStep("Remove TestKey1", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);

            AddAssert("TestKey1 is not pressed anymore", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 0);
            AddAssert("1 Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1);

            AddStep("Remove TestKey1 again", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);

            // Normally removing the first binding would be a no-op, because that has already been done when the second binding was pressed
            // AddAssert("TestKey1 is not pressed anymore", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 0);
            // AddAssert("No Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 0);

            return;

            void applyActions() => new ReplayInputHandler.ReplayState<TestAction>()
            {
                PressedActions = actions.ToList(),
            }.Apply(state, rulesetInputManager);

            void resetActions()
            {
                actions.Clear();
                applyActions();
            }
        }

        [Test]
        public void TestUniqueSimultaneousBindings()
        {
            Clear();
            TestRulesetInputManager rulesetInputManager = new TestRulesetInputManager(SimultaneousBindingMode.Unique);
            Add(rulesetInputManager);

            List<TestAction> actions = new List<TestAction>();

            AddAssert("No actions are pressed.", () => rulesetInputManager.PressedActions.Count == 0);

            AddLabel("Test single action");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Remove action", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is not pressed anymore", () => rulesetInputManager.PressedActions.Count == 0);
            AddAssert("1 Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Reset actions", resetActions);

            AddLabel("Test multiple unique actions");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Add TestKey2", () => actions.Add(TestAction.TestKey2));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is still pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("No release events for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 0);
            AddAssert("TestKey2 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey2) == 1);
            AddAssert("1 Press event for TestKey2", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey2) == 1);
            AddStep("Remove both keys", () =>
            {
                actions.Remove(TestAction.TestKey1);
                actions.Remove(TestAction.TestKey2);
            });
            AddStep("Apply actions", applyActions);

            AddAssert("No keys are pressed", () => rulesetInputManager.PressedActions.Count == 0);
            AddAssert("1 release event for each key", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1 && rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey2) == 1);
            AddStep("Reset actions", resetActions);

            AddLabel("Test multiple identical actions");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Add TestKey1 again", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);

            // SimultaneousBindingMode.Unique will only fire the press event on the first press, subsequent presses of the same action will not trigger an event
            // AddAssert("No Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 0);
            AddAssert("No Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 0);

            AddStep("Remove TestKey1", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);

            // SimultaneousBindingMode.Unique will only release when all bindings to the same action is released
            // But TriggerReleased bypasses that check...
            // AddAssert("TestKey1 is still pressed", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            // AddAssert("No Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 0);

            AddStep("Remove TestKey1 again", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);

            AddAssert("TestKey1 is not pressed anymore", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 0);
            AddAssert("1 Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1);

            return;

            void applyActions() => new ReplayInputHandler.ReplayState<TestAction>()
            {
                PressedActions = actions.ToList(),
            }.Apply(rulesetInputManager.CurrentState, rulesetInputManager);

            void resetActions()
            {
                actions.Clear();
                applyActions();
            }
        }

        [Test]
        public void TestAllSimultaneousBindings()
        {
            Clear();
            TestRulesetInputManager rulesetInputManager = new TestRulesetInputManager(SimultaneousBindingMode.All);
            Add(rulesetInputManager);

            List<TestAction> actions = new List<TestAction>();

            AddAssert("No actions are pressed.", () => rulesetInputManager.PressedActions.Count == 0);

            AddLabel("Test single action");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Remove action", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is not pressed anymore", () => rulesetInputManager.PressedActions.Count == 0);
            AddAssert("1 Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Reset actions", resetActions);

            AddLabel("Test multiple unique actions");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Add TestKey2", () => actions.Add(TestAction.TestKey2));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is still pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("No release events for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 0);
            AddAssert("TestKey2 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey2) == 1);
            AddAssert("1 Press event for TestKey2", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey2) == 1);
            AddStep("Remove both keys", () =>
            {
                actions.Remove(TestAction.TestKey1);
                actions.Remove(TestAction.TestKey2);
            });
            AddStep("Apply actions", applyActions);
            AddAssert("No keys are pressed", () => rulesetInputManager.PressedActions.Count == 0);
            AddAssert("1 release event for each key", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1 && rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey2) == 1);
            AddStep("Reset actions", resetActions);

            AddLabel("Test multiple identical actions");
            AddStep("Add TestKey1", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Add TestKey1 again", () => actions.Add(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("TestKey1 is pressed twice", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 2);
            AddAssert("No Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 0);
            AddAssert("1 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddStep("Remove TestKey1", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("1 Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddAssert("0 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 0);
            AddAssert("TestKey1 is pressed once", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 1);
            AddStep("Remove TestKey1", () => actions.Remove(TestAction.TestKey1));
            AddStep("Apply actions", applyActions);
            AddAssert("1 Release event for TestKey1", () => rulesetInputManager.ReleaseEventsSinceLastObservation(TestAction.TestKey1) == 1);
            AddAssert("0 Press event for TestKey1", () => rulesetInputManager.PressEventsSinceLastObservation(TestAction.TestKey1) == 0);
            AddAssert("TestKey1 is not pressed", () => rulesetInputManager.PressedActions.Count(k => k == TestAction.TestKey1) == 0);

            return;

            void applyActions() => new ReplayInputHandler.ReplayState<TestAction>()
            {
                PressedActions = actions.ToList(),
            }.Apply(rulesetInputManager.CurrentState, rulesetInputManager);

            void resetActions()
            {
                actions.Clear();
                applyActions();
            }
        }

        private enum TestAction
        {
            TestKey1,
            TestKey2,
        }

        private partial class TestRulesetInputManager : RulesetInputManager<TestAction>
        {
            public TestRulesetInputManager(SimultaneousBindingMode mode)
                : base(new TestRuleset().RulesetInfo, 0, mode)
            {
                Add(new DummyInputConsumer
                {
                    PressedActionCounts = pressedActionCounts,
                    ReleasedActionCounts = releasedActionCounts,
                });
            }

            public SlimReadOnlyListWrapper<TestAction> PressedActions => KeyBindingContainer.PressedActions;

            private readonly Dictionary<TestAction, int> pressedActionCounts = new Dictionary<TestAction, int>
            {
                {
                    TestAction.TestKey1, 0
                },
                {
                    TestAction.TestKey2, 0
                },
            };

            private readonly Dictionary<TestAction, int> releasedActionCounts = new Dictionary<TestAction, int>
            {
                {
                    TestAction.TestKey1, 0
                },
                {
                    TestAction.TestKey2, 0
                },
            };

            public int PressEventsSinceLastObservation(TestAction action)
            {
                if (!pressedActionCounts.TryGetValue(action, out int count))
                    return 0;

                pressedActionCounts[action] = 0;
                return count;
            }
            public int ReleaseEventsSinceLastObservation(TestAction action)
            {
                if (!releasedActionCounts.TryGetValue(action, out int count))
                    return 0;

                releasedActionCounts[action] = 0;
                return count;
            }

            private partial class DummyInputConsumer : Drawable, IKeyBindingHandler<TestAction>
            {
                public required Dictionary<TestAction, int> ReleasedActionCounts;
                public required Dictionary<TestAction, int> PressedActionCounts;

                public bool OnPressed(KeyBindingPressEvent<TestAction> e)
                {
                    PressedActionCounts[e.Action]++;
                    return true;
                }

                public void OnReleased(KeyBindingReleaseEvent<TestAction> e)
                {
                    ReleasedActionCounts[e.Action]++;
                }
            }
        }

        public class TestRuleset : Ruleset
        {
            public override string Description => string.Empty;
            public override string ShortName => string.Empty;

            public TestRuleset()
            {
                // temporary ID to let RulesetConfigCache pass our
                // config manager to the ruleset dependencies.
                RulesetInfo.OnlineID = -1;
            }

            public override IResourceStore<byte[]> CreateResourceStore() => new NamespacedResourceStore<byte[]>(TestResources.GetStore(), @"Resources");
            public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new TestRulesetConfigManager();

            public override IEnumerable<Mod> GetModsFor(ModType type) => Array.Empty<Mod>();
            public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) => null!;
            public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) => null!;
            public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) => null!;
        }

        private class TestRulesetConfigManager : IRulesetConfigManager
        {
            public void Load()
            {
            }

            public bool Save() => true;

            public TrackedSettings CreateTrackedSettings() => new TrackedSettings();

            public void LoadInto(TrackedSettings settings)
            {
            }

            public void Dispose()
            {
            }
        }
    }
}
