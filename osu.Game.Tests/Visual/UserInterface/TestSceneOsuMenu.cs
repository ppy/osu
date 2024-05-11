// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuMenu : OsuManualInputManagerTestScene
    {
        private OsuMenu menu;
        private bool actionPerformed;

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            actionPerformed = false;

            Child = menu = new OsuMenu(Direction.Vertical, true)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Items = new[]
                {
                    new OsuMenuItem("standard", MenuItemType.Standard, performAction),
                    new OsuMenuItem("highlighted", MenuItemType.Highlighted, performAction),
                    new OsuMenuItem("destructive", MenuItemType.Destructive, performAction),
                }
            };
        });

        [Test]
        public void TestClickEnabledMenuItem()
        {
            AddStep("move to first menu item", () => InputManager.MoveMouseTo(menu.ChildrenOfType<DrawableOsuMenuItem>().First()));
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("action performed", () => actionPerformed);
        }

        [Test]
        public void TestDisableMenuItemsAndClick()
        {
            AddStep("disable menu items", () =>
            {
                foreach (var item in menu.Items)
                    ((OsuMenuItem)item).Action.Disabled = true;
            });

            AddStep("move to first menu item", () => InputManager.MoveMouseTo(menu.ChildrenOfType<DrawableOsuMenuItem>().First()));
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("action not performed", () => !actionPerformed);
        }

        [Test]
        public void TestEnableMenuItemsAndClick()
        {
            AddStep("disable menu items", () =>
            {
                foreach (var item in menu.Items)
                    ((OsuMenuItem)item).Action.Disabled = true;
            });

            AddStep("enable menu items", () =>
            {
                foreach (var item in menu.Items)
                    ((OsuMenuItem)item).Action.Disabled = false;
            });

            AddStep("move to first menu item", () => InputManager.MoveMouseTo(menu.ChildrenOfType<DrawableOsuMenuItem>().First()));
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("action performed", () => actionPerformed);
        }

        private void performAction() => actionPerformed = true;
    }
}
