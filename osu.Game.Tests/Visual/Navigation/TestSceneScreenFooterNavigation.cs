// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Screens;
using osu.Game.Screens.Footer;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneScreenFooterNavigation : OsuGameTestScene
    {
        private ScreenFooter screenFooter => this.ChildrenOfType<ScreenFooter>().Single();

        [Test]
        public void TestFooterHidesOldBackButton()
        {
            PushAndConfirm(() => new TestScreen(false));
            AddAssert("footer hidden", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("old back button shown", () => Game.BackButton.State.Value, () => Is.EqualTo(Visibility.Visible));

            PushAndConfirm(() => new TestScreen(true));
            AddAssert("footer shown", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("old back button hidden", () => Game.BackButton.State.Value, () => Is.EqualTo(Visibility.Hidden));

            PushAndConfirm(() => new TestScreen(false));
            AddAssert("footer hidden", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("back button shown", () => Game.BackButton.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("exit screen", () => Game.ScreenStack.Exit());
            AddAssert("footer shown", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("old back button hidden", () => Game.BackButton.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("exit screen", () => Game.ScreenStack.Exit());
            AddAssert("footer hidden", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("old back button shown", () => Game.BackButton.State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        private partial class TestScreen : OsuScreen
        {
            public override bool ShowFooter { get; }

            public TestScreen(bool footer)
            {
                ShowFooter = footer;
            }
        }
    }
}
