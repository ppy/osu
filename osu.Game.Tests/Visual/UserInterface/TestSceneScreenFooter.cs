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
using osu.Game.Screens.SelectV2;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneScreenFooter : ScreenTestScene
    {
        [Test]
        public void TestButtonsIn()
        {
            AddStep("push empty screen", () => LoadScreen(new BaseTestOsuScreen()));
            AddStep("push screen", () => LoadScreen(new TestScreenOne()));
        }

        [Test]
        public void TestButtonsOut()
        {
            AddStep("push empty screen", () => LoadScreen(new BaseTestOsuScreen()));
            AddStep("push screen", () => LoadScreen(new TestScreenOne()));
            AddStep("exit screen", () => Stack.Exit());
        }

        [Test]
        public void TestReplaceButtons()
        {
            AddStep("push first screen", () => LoadScreen(new TestScreenOne()));
            AddStep("push second screen", () => LoadScreen(new TestScreenTwo()));
        }

        [Test]
        public void TestFooterVisibility()
        {
            TestScreenOne screen = null!;
            TestScreenWithoutFooter screenWithoutFooter = null!;

            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("push screen", () => LoadScreen(screen = new TestScreenOne()));
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);
            AddAssert("footer shown", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("push screen with no footer", () => LoadScreen(screenWithoutFooter = new TestScreenWithoutFooter()));
            AddUntilStep("wait until screen is loaded", () => screenWithoutFooter.IsCurrentScreen(), () => Is.True);
            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("exit screen", () => Stack.Exit());
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);
            AddAssert("footer shown", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestExternalOverlayContent()
        {
            TestScreenWithOverlay screen = null!;

            AddStep("push screen", () => LoadScreen(screen = new TestScreenWithOverlay()));
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);

            AddStep("show overlay", () => screen.Overlay.Show());
            contentDisplayed();
            AddAssert("other buttons hidden", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.Child.Parent!.Y > 0));

            AddStep("hide overlay", () => screen.Overlay.Hide());
            contentHidden();
            AddAssert("other buttons returned", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.ChildrenOfType<Container>().First().Y == 0));
        }

        [Test]
        public void TestTemporarilyShowFooter()
        {
            TestScreenWithOverlayWithoutFooter screen = null!;

            AddStep("push screen", () => LoadScreen(screen = new TestScreenWithOverlayWithoutFooter()));
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);
            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("show overlay", () => screen.Overlay.Show());
            AddAssert("footer shown", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));
            contentDisplayed();

            AddStep("hide overlay", () => screen.Overlay.Hide());
            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
            contentHidden();
        }

        [Test]
        public void TestBackButton()
        {
            TestScreenWithOverlayWithoutFooter screen = null!;

            AddStep("push screen", () => LoadScreen(screen = new TestScreenWithOverlayWithoutFooter()));
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);
            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("show overlay", () => screen.Overlay.Show());
            AddAssert("footer shown", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("press back", () => this.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay hidden", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestCustomBackButtonPressedLogic()
        {
            TestScreenWithOverlayWithoutFooter screen = null!;

            AddStep("push screen", () => LoadScreen(screen = new TestScreenWithOverlayWithoutFooter()));
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);
            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("show overlay", () => screen.Overlay.Show());
            AddStep("set block count", () => screen.Overlay.BackButtonCount = 1);

            AddStep("press back", () => this.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay still visible", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("footer still visible", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("press back again", () => this.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay hidden", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Hidden));
            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));
        }

        [Test]
        public void TestShowOverlayHidesOtherOverlays()
        {
            TestScreenWithTwoOverlays screen = null!;

            AddStep("push screen", () => LoadScreen(screen = new TestScreenWithTwoOverlays()));
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);

            AddStep("show mods overlay", () => ScreenFooter.ChildrenOfType<FooterButtonMods>().First().TriggerClick());
            AddUntilStep("wait until overlay is shown", () => screen.ModSelectOverlay.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("first button still visible", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().First(b => b.Text == "One").Y, () => Is.EqualTo(0));

            AddStep("show test overlay", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().First(b => b.Text == "One").TriggerClick());
            AddUntilStep("wait until overlay is shown", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("mod overlay is hidden", () => screen.ModSelectOverlay.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("hide test overlay", () => screen.Overlay.Hide());
            contentHidden();
            AddAssert("other buttons returned", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.ChildrenOfType<Container>().First().Y == 0));
        }

        [Test]
        public void TestButtonResizedAfterFooterIsDisplayed()
        {
            TestScreenWithOverlay screen = null!;

            const float initial_width = 116;
            const float width_increase = 124;

            float secondButtonX = 0;
            float overlayContentX = 0;

            AddStep("push screen", () => LoadScreen(screen = new TestScreenWithOverlay()));
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);
            AddStep("save second button position", () => secondButtonX = ScreenFooter.ChildrenOfType<ScreenFooterButton>().ElementAt(1).X);

            AddStep("resize active button", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().First().ResizeWidthTo(initial_width + width_increase, 300, Easing.OutQuint));
            AddUntilStep("second button moved", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().ElementAt(1).X, () => Is.EqualTo(secondButtonX + width_increase).Within(0.001));
            AddStep("resize active button back", () => this.ChildrenOfType<ScreenFooterButton>().First().ResizeWidthTo(initial_width, 300, Easing.OutQuint));
            AddUntilStep("second button moved back", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().ElementAt(1).X, () => Is.EqualTo(secondButtonX).Within(0.001));

            AddStep("show overlay", () => screen.Overlay.Show());
            contentDisplayed();
            AddAssert("other buttons hidden", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.Child.Parent!.Y > 0));
            AddStep("save overlay content position", () => overlayContentX = ScreenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().First().Parent!.Parent!.X);

            AddStep("resize active button", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().First().ResizeWidthTo(initial_width + width_increase, 300, Easing.OutQuint));
            AddUntilStep("overlay content moved", () => ScreenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().First().Parent!.Parent!.X, () => Is.EqualTo(overlayContentX + width_increase).Within(0.001));
            AddStep("resize active button back", () => this.ChildrenOfType<ScreenFooterButton>().First().ResizeWidthTo(initial_width, 300, Easing.OutQuint));
            AddUntilStep("overlay content moved back", () => ScreenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().First().Parent!.Parent!.X, () => Is.EqualTo(overlayContentX).Within(0.001));

            AddStep("hide overlay", () => screen.Overlay.Hide());
            contentHidden();
            AddUntilStep("other buttons returned", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.ChildrenOfType<Container>().First().Y == 0));
        }

        private void contentHidden()
        {
            AddUntilStep("content hidden from footer", () => ScreenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().SingleOrDefault()?.IsPresent != true);
        }

        private void contentDisplayed()
        {
            AddUntilStep("content displayed in footer", () => ScreenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().Single().IsPresent);
        }

        private partial class BaseTestOsuScreen : OsuScreen
        {
            public override bool ShowFooter => true;

            [Cached]
            private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);
        }

        private partial class TestScreenWithoutFooter : BaseTestOsuScreen
        {
            public override bool ShowFooter => false;
        }

        private partial class TestScreenOne : BaseTestOsuScreen
        {
            public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new[]
            {
                new ScreenFooterButton { Text = "Button 1", Action = () => { } },
                new ScreenFooterButton { Text = "Button 2", Action = () => { } },
                new ScreenFooterButton { Text = "Button 3", Action = () => { } },
            };
        }

        private partial class TestScreenTwo : BaseTestOsuScreen
        {
            public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new[]
            {
                new ScreenFooterButton { Text = "Button 4", Action = () => { } },
                new ScreenFooterButton { Text = "Button 5", Action = () => { } },
                new ScreenFooterButton { Text = "Button 6", Action = () => { } },
            };
        }

        private partial class TestScreenWithOverlay : BaseTestOsuScreen
        {
            private IDisposable? overlayRegistration;

            public TestShearedOverlayContainer Overlay { get; private set; } = null!;

            [Resolved]
            private IOverlayManager? overlayManager { get; set; }

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

        private partial class TestScreenWithTwoOverlays : TestScreenWithOverlay
        {
            private IDisposable? overlayRegistration;

            public ModSelectOverlay ModSelectOverlay { get; private set; } = null!;

            [Resolved]
            private IOverlayManager? overlayManager { get; set; }

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponent(ModSelectOverlay = new ModSelectOverlay());
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                overlayRegistration = overlayManager?.RegisterBlockingOverlay(ModSelectOverlay);
            }

            public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons()
            {
                var buttons = base.CreateFooterButtons().ToList();

                buttons.Insert(1, new FooterButtonMods(ModSelectOverlay));

                return buttons;
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                overlayRegistration?.Dispose();
            }
        }

        private partial class TestScreenWithOverlayWithoutFooter : TestScreenWithOverlay
        {
            public override bool ShowFooter => false;
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
