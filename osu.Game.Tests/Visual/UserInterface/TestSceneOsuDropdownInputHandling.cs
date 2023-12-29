// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneOsuDropdownInputHandling : OsuManualInputManagerTestScene
    {
        private OsuDropdown<BeatmapOnlineStatus> dropdown = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = dropdown = new OsuEnumDropdown<BeatmapOnlineStatus>
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.TopCentre,
                Width = 150
            };
        });

        [Test]
        public void TestBackAction()
        {
            AddStep("open", () => dropdown.ChildrenOfType<Menu>().Single().Open());
            AddStep("press back", () => InputManager.Key(Key.Escape));
            AddAssert("closed", () => dropdown.ChildrenOfType<Menu>().Single().State == MenuState.Closed);
            AddStep("open", () => dropdown.ChildrenOfType<Menu>().Single().Open());
            AddStep("type something", () => dropdown.ChildrenOfType<DropdownSearchBar>().Single().SearchTerm.Value = "something");
            AddAssert("search bar visible", () => dropdown.ChildrenOfType<DropdownSearchBar>().Single().State.Value == Visibility.Visible);
            AddStep("press back", () => InputManager.Key(Key.Escape));
            AddAssert("text clear", () => dropdown.ChildrenOfType<DropdownSearchBar>().Single().SearchTerm.Value == string.Empty);
            AddAssert("search bar hidden", () => dropdown.ChildrenOfType<DropdownSearchBar>().Single().State.Value == Visibility.Hidden);
            AddAssert("still open", () => dropdown.ChildrenOfType<Menu>().Single().State == MenuState.Open);
            AddStep("press back", () => InputManager.Key(Key.Escape));
            AddAssert("closed", () => dropdown.ChildrenOfType<Menu>().Single().State == MenuState.Closed);
        }
    }
}
