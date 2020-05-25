// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays.Toolbar;
using osu.Game.Rulesets;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public class TestSceneToolbar : OsuManualInputManagerTestScene
    {
        private Toolbar toolbar;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = toolbar = new Toolbar { State = { Value = Visibility.Visible } };
        });

        [Test]
        public void TestNotificationCounter()
        {
            ToolbarNotificationButton notificationButton = null;

            AddStep("retrieve notification button", () => notificationButton = toolbar.ChildrenOfType<ToolbarNotificationButton>().Single());

            setNotifications(1);
            setNotifications(2);
            setNotifications(3);
            setNotifications(0);
            setNotifications(144);

            void setNotifications(int count)
                => AddStep($"set notification count to {count}",
                    () => notificationButton.NotificationCount.Value = count);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestRulesetSwitchingShortcut(bool toolbarHidden)
        {
            ToolbarRulesetSelector rulesetSelector = null;

            if (toolbarHidden)
                AddStep("hide toolbar", () => toolbar.Hide());

            AddStep("retrieve ruleset selector", () => rulesetSelector = toolbar.ChildrenOfType<ToolbarRulesetSelector>().Single());

            for (int i = 0; i < 4; i++)
            {
                var expected = rulesets.AvailableRulesets.ElementAt(i);
                var numberKey = Key.Number1 + i;

                AddStep($"switch to ruleset {i} via shortcut", () =>
                {
                    InputManager.PressKey(Key.ControlLeft);
                    InputManager.PressKey(numberKey);

                    InputManager.ReleaseKey(Key.ControlLeft);
                    InputManager.ReleaseKey(numberKey);
                });

                AddUntilStep("ruleset switched", () => rulesetSelector.Current.Value.Equals(expected));
            }
        }
    }
}
