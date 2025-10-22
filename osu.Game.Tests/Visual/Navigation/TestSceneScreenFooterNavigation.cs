// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Screens.Footer;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestSceneScreenFooterNavigation : OsuGameTestScene
    {
        private ScreenFooter screenFooter => this.ChildrenOfType<ScreenFooter>().Single();

        [Test]
        public void TestFooterButtonsOnScreenTransitions()
        {
            PushAndConfirm(() => new TestScreenOne());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));

            PushAndConfirm(() => new TestScreenTwo());
            AddUntilStep("button two shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button Two"));

            AddStep("exit screen", () => Game.ScreenStack.Exit());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));
        }

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

        /// <summary>
        /// Tests pushing and exiting subscreens that have footers.
        /// </summary>
        [Test]
        public void TestPushAndExitSubScreens()
        {
            TestScreenWithSubScreen screen = null!;

            PushAndConfirm(() => screen = new TestScreenWithSubScreen());
            AddAssert("footer hidden", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("old back button shown", () => Game.BackButton.State.Value, () => Is.EqualTo(Visibility.Visible));

            pushSubScreenAndConfirm(() => screen, () => new TestScreenOne());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));

            pushSubScreenAndConfirm(() => screen, () => new TestScreenTwo());
            AddUntilStep("button two shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button Two"));

            AddStep("exit sub screen", () => screen.ExitSubScreen());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));

            AddStep("exit sub screen", () => screen.ExitSubScreen());
            AddAssert("footer hidden", () => screenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("old back button shown", () => Game.BackButton.State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        /// <summary>
        /// Tests pushing a new parenting screen while the footer is displayed from a subscreen.
        /// </summary>
        [Test]
        public void TestPushParentScreenDuringSubScreen()
        {
            TestScreenWithSubScreen screen = null!;

            PushAndConfirm(() => screen = new TestScreenWithSubScreen());
            pushSubScreenAndConfirm(() => screen, () => new TestScreenOne());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));

            PushAndConfirm(() => new TestScreenTwo());
            AddUntilStep("button two shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button Two"));

            AddStep("exit parent screen", () => Game.ScreenStack.Exit());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));
        }

        /// <summary>
        /// Tests pushing a new subscreen after a new parenting screen has been pushed.
        /// </summary>
        [Test]
        public void TestPushSubScreenWhileNotCurrent()
        {
            TestScreenWithSubScreen screen = null!;

            PushAndConfirm(() => screen = new TestScreenWithSubScreen());
            pushSubScreenAndConfirm(() => screen, () => new TestScreenOne());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));

            PushAndConfirm(() => new TestScreenOne());
            AddUntilStep("button one shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));

            // Can't use the helper method because the screen never loads
            AddStep("Push new sub screen", () => screen.PushSubScreen(new TestScreenTwo()));
            AddWaitStep("wait for potential screen load", 5);
            AddUntilStep("button one still shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button One"));

            AddStep("exit parent screen", () => Game.ScreenStack.Exit());
            AddUntilStep("button two shown", () => screenFooter.ChildrenOfType<ScreenFooterButton>().First().Text.ToString(), () => Is.EqualTo("Button Two"));
        }

        private void pushSubScreenAndConfirm(Func<TestScreenWithSubScreen> target, Func<Screen> newScreen)
        {
            Screen screen = null!;
            IScreen? previousScreen = null;

            AddStep("Push new sub screen", () =>
            {
                previousScreen = target().CurrentSubScreen;
                target().PushSubScreen(screen = newScreen());
            });

            AddUntilStep("Wait for new screen", () => screen.IsLoaded
                                                      && target().CurrentSubScreen != previousScreen
                                                      && (previousScreen == null || previousScreen.GetChildScreen() == screen));
        }

        private partial class TestScreenOne : OsuScreen
        {
            public override bool ShowFooter => true;

            [Cached]
            private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new[]
            {
                new ScreenFooterButton { Text = "Button One" },
            };
        }

        private partial class TestScreenTwo : OsuScreen
        {
            public override bool ShowFooter => true;

            [Cached]
            private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new[]
            {
                new ScreenFooterButton { Text = "Button Two" },
            };
        }

        private partial class TestScreen : OsuScreen
        {
            public override bool ShowFooter { get; }

            public TestScreen(bool footer)
            {
                ShowFooter = footer;
            }
        }

        private partial class TestScreenWithSubScreen : OsuScreen, IHasSubScreenStack
        {
            public ScreenStack SubScreenStack { get; }

            public TestScreenWithSubScreen()
            {
                InternalChild = SubScreenStack = new ScreenStack
                {
                    RelativeSizeAxes = Axes.Both
                };
            }

            public IScreen? CurrentSubScreen => SubScreenStack.CurrentScreen;

            public void PushSubScreen(IScreen screen) => SubScreenStack.Push(screen);

            public void ExitSubScreen() => SubScreenStack.Exit();
        }
    }
}
