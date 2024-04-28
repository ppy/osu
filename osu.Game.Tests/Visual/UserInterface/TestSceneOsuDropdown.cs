// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Input.States;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuDropdown : ThemeComparisonTestScene
    {
        protected override Drawable CreateContent() =>
            new OsuEnumDropdown<TestEnum>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                Width = 150
            };

        private enum TestEnum
        {
            [System.ComponentModel.Description("Option")]
            Option,

            [System.ComponentModel.Description("Really lonnnnnnng option")]
            ReallyLongOption,
        }

        [Test]
        // todo: this can be written much better if ThemeComparisonTestScene has a manual input manager
        public void TestBackAction()
        {
            AddStep("open", () => dropdown().ChildrenOfType<Menu>().Single().Open());
            AddStep("press back", () => dropdown().OnPressed(new KeyBindingPressEvent<GlobalAction>(new InputState(), GlobalAction.Back)));
            AddAssert("closed", () => dropdown().ChildrenOfType<Menu>().Single().State == MenuState.Closed);

            AddStep("open", () => dropdown().ChildrenOfType<Menu>().Single().Open());
            AddStep("type something", () => dropdown().ChildrenOfType<DropdownSearchBar>().Single().SearchTerm.Value = "something");
            AddAssert("search bar visible", () => dropdown().ChildrenOfType<DropdownSearchBar>().Single().State.Value == Visibility.Visible);
            AddStep("press back", () => dropdown().OnPressed(new KeyBindingPressEvent<GlobalAction>(new InputState(), GlobalAction.Back)));
            AddAssert("text clear", () => dropdown().ChildrenOfType<DropdownSearchBar>().Single().SearchTerm.Value == string.Empty);
            AddAssert("search bar hidden", () => dropdown().ChildrenOfType<DropdownSearchBar>().Single().State.Value == Visibility.Hidden);
            AddAssert("still open", () => dropdown().ChildrenOfType<Menu>().Single().State == MenuState.Open);
            AddStep("press back", () => dropdown().OnPressed(new KeyBindingPressEvent<GlobalAction>(new InputState(), GlobalAction.Back)));
            AddAssert("closed", () => dropdown().ChildrenOfType<Menu>().Single().State == MenuState.Closed);

            OsuEnumDropdown<TestEnum> dropdown() => this.ChildrenOfType<OsuEnumDropdown<TestEnum>>().First();
        }
    }
}
