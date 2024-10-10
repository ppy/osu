// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuDropdown : ThemeComparisonTestScene
    {
        protected override Drawable CreateContent() => new OsuEnumDropdown<TestEnum>
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.TopCentre,
            Width = 150
        };

        [Test]
        public void TestBackAction()
        {
            AddStep("open", () => dropdownMenu.Open());
            AddStep("press back", () => InputManager.Key(Key.Escape));
            AddAssert("closed", () => dropdownMenu.State == MenuState.Closed);

            AddStep("open", () => dropdownMenu.Open());
            AddStep("type something", () => dropdownSearchBar.SearchTerm.Value = "something");
            AddAssert("search bar visible", () => dropdownSearchBar.State.Value == Visibility.Visible);
            AddStep("press back", () => InputManager.Key(Key.Escape));
            AddAssert("text clear", () => dropdownSearchBar.SearchTerm.Value == string.Empty);
            AddAssert("search bar hidden", () => dropdownSearchBar.State.Value == Visibility.Hidden);
            AddAssert("still open", () => dropdownMenu.State == MenuState.Open);
            AddStep("press back", () => InputManager.Key(Key.Escape));
            AddAssert("closed", () => dropdownMenu.State == MenuState.Closed);
        }

        [Test]
        public void TestSelectAction()
        {
            AddStep("open", () => dropdownMenu.Open());
            AddStep("press down", () => InputManager.Key(Key.Down));
            AddStep("press enter", () => InputManager.Key(Key.Enter));
            AddAssert("second selected", () => dropdown.Current.Value == TestEnum.ReallyLongOption);
        }

        private OsuEnumDropdown<TestEnum> dropdown => this.ChildrenOfType<OsuEnumDropdown<TestEnum>>().Last();
        private Menu dropdownMenu => dropdown.ChildrenOfType<Menu>().Single();
        private DropdownSearchBar dropdownSearchBar => dropdown.ChildrenOfType<DropdownSearchBar>().Single();

        private enum TestEnum
        {
            [System.ComponentModel.Description("Option")]
            Option,

            [System.ComponentModel.Description("Really lonnnnnnng option")]
            ReallyLongOption,
        }
    }
}
