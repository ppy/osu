// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
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

        /// <summary>
        /// Tests clicking the back button while an overlay is open.
        /// </summary>
        [Test]
        public void TestBackButtonWhenOverlayOpen()
        {
            TestScreenWithOverlay screen = null!;

            PushAndConfirm(() => screen = new TestScreenWithOverlay());

            AddStep("show overlay", () => screen.Overlay.Show());
            AddAssert("overlay shown", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("press back", () => screenFooter.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay hidden", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("screen still shown", () => screen.IsCurrentScreen(), () => Is.True);
        }

        /// <summary>
        /// Tests clicking the back button on an overlay with `BackButtonPressed` being overridden.
        /// </summary>
        [Test]
        public void TestBackButtonWithCustomBackButtonPressed()
        {
            TestScreenWithOverlay screen = null!;

            PushAndConfirm(() => screen = new TestScreenWithOverlay());

            AddStep("show overlay", () => screen.Overlay.Show());
            AddAssert("overlay shown", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddStep("set block count", () => screen.Overlay.BackButtonCount = 1);

            AddStep("press back", () => screenFooter.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay still shown", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("press back again", () => screenFooter.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay hidden", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("screen still shown", () => screen.IsCurrentScreen(), () => Is.True);
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

        private partial class TestScreenWithOverlay : OsuScreen
        {
            public override bool ShowFooter => true;

            private IDisposable? overlayRegistration;

            public TestShearedOverlayContainer Overlay { get; private set; } = null!;

            [Resolved]
            private IOverlayManager? overlayManager { get; set; }

            [Cached]
            private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponent(Overlay = new TestShearedOverlayContainer());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                overlayRegistration = overlayManager?.RegisterBlockingOverlay(Overlay);
            }

            public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new[]
            {
                new ScreenFooterButton(Overlay)
                {
                    AccentColour = Dependencies.Get<OsuColour>().Orange1,
                    Icon = FontAwesome.Solid.Toolbox,
                    Text = "One",
                },
                new ScreenFooterButton { Text = "Two", Action = () => { } },
                new ScreenFooterButton { Text = "Three", Action = () => { } },
            };

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                overlayRegistration?.Dispose();
            }
        }

        private partial class TestShearedOverlayContainer : ShearedOverlayContainer
        {
            public TestShearedOverlayContainer()
                : base(OverlayColourScheme.Orange)
            {
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Header.Title = "Test overlay";
                Header.Description = "An overlay that is made purely for testing purposes.";
            }

            public int BackButtonCount;

            public override bool OnBackButton()
            {
                if (BackButtonCount > 0)
                {
                    BackButtonCount--;
                    return true;
                }

                return false;
            }

            public override VisibilityContainer CreateFooterContent() => new TestFooterContent();

            public partial class TestFooterContent : VisibilityContainer
            {
                [BackgroundDependencyLoader]
                private void load()
                {
                    AutoSizeAxes = Axes.Both;

                    InternalChild = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new ShearedButton(200) { Text = "Action #1", Action = () => { } },
                            new ShearedButton(140) { Text = "Action #2", Action = () => { } },
                        }
                    };
                }

                protected override void PopIn()
                {
                    this.MoveToY(0, 400, Easing.OutQuint)
                        .FadeIn(400, Easing.OutQuint);
                }

                protected override void PopOut()
                {
                    this.MoveToY(-20f, 200, Easing.OutQuint)
                        .FadeOut(200, Easing.OutQuint);
                }
            }
        }
    }
}
