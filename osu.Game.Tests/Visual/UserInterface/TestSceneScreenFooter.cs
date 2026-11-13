// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Select;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneScreenFooter : ScreenTestScene
    {
        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Test]
        public void TestButtonsIn()
        {
            AddStep("push empty screen", () => LoadScreen(new TestScreen()));
            AddStep("push screen", () => LoadScreen(new TestScreen
            {
                FooterContent =
                {
                    Value = ScreenFooterContent.Argon(
                        leftButtons: () => new[]
                        {
                            new ScreenFooterButton { Text = "Button 1", Action = () => { } },
                            new ScreenFooterButton { Text = "Button 2", Action = () => { } },
                            new ScreenFooterButton { Text = "Button 3", Action = () => { } },
                        },
                        rightButtons: () => new[]
                        {
                            new ShearedButton
                            {
                                Text = "Main action 1",
                                Width = 150,
                                LighterColour = colours.Green1,
                                DarkerColour = colours.Green3,
                                Action = () => { }
                            },
                            new ShearedButton
                            {
                                Text = "Main action 4",
                                Width = 150,
                                LighterColour = colours.Purple1,
                                DarkerColour = colours.Purple3,
                                Action = () => { }
                            },
                        }
                    ),
                },
            }));
        }

        [Test]
        public void TestButtonsOut()
        {
            AddStep("push empty screen", () => LoadScreen(new TestScreen()));
            AddStep("push screen", () => LoadScreen(new TestScreen
            {
                FooterContent =
                {
                    Value = ScreenFooterContent.Argon(
                        leftButtons: () => new[]
                        {
                            new ScreenFooterButton { Text = "Button 1", Action = () => { } },
                            new ScreenFooterButton { Text = "Button 2", Action = () => { } },
                            new ScreenFooterButton { Text = "Button 3", Action = () => { } },
                        },
                        rightButtons: () => new[]
                        {
                            new ShearedButton
                            {
                                Text = "Main action 1",
                                Width = 150,
                                LighterColour = colours.Green1,
                                DarkerColour = colours.Green3,
                                Action = () => { }
                            },
                            new ShearedButton
                            {
                                Text = "Main action 2",
                                Width = 150,
                                LighterColour = colours.Pink1,
                                DarkerColour = colours.Pink3,
                                Action = () => { }
                            },
                        }
                    ),
                },
            }));
            AddStep("exit screen", () => Stack.Exit());
        }

        [Test]
        public void TestReplaceButtons()
        {
            AddStep("push first screen", () => LoadScreen(new TestScreen
            {
                FooterContent =
                {
                    Value = ScreenFooterContent.Argon(
                        leftButtons: () => new[]
                        {
                            new ScreenFooterButton { Text = "Button 1", Action = () => { } },
                            new ScreenFooterButton { Text = "Button 2", Action = () => { } },
                            new ScreenFooterButton { Text = "Button 3", Action = () => { } },
                        },
                        rightButtons: () => new[]
                        {
                            new ShearedButton
                            {
                                Text = "Main action 1",
                                Width = 150,
                                LighterColour = colours.Green1,
                                DarkerColour = colours.Green3,
                                Action = () => { }
                            },
                            new ShearedButton
                            {
                                Text = "Main action 2",
                                Width = 150,
                                LighterColour = colours.Pink1,
                                DarkerColour = colours.Pink3,
                                Action = () => { }
                            },
                        }
                    ),
                },
            }));
            AddStep("push second screen", () => LoadScreen(new TestScreen
            {
                FooterContent =
                {
                    Value = ScreenFooterContent.Argon(
                        leftButtons: () => new[]
                        {
                            new ScreenFooterButton { Text = "Button 4", Action = () => { } },
                            new ScreenFooterButton { Text = "Button 5", Action = () => { } },
                            new ScreenFooterButton { Text = "Button 6", Action = () => { } },
                        },
                        rightButtons: () => new[]
                        {
                            new ShearedButton
                            {
                                Text = "Main action 3",
                                Width = 150,
                                LighterColour = colours.Blue1,
                                DarkerColour = colours.Blue3,
                                Action = () => { }
                            },
                            new ShearedButton
                            {
                                Text = "Main action 4",
                                Width = 150,
                                LighterColour = colours.Purple1,
                                DarkerColour = colours.Purple3,
                                Action = () => { }
                            },
                        }
                    ),
                },
            }));
        }

        [Test]
        public void TestFooterVisibility()
        {
            TestScreen screen = null!;
            TestScreen screenWithoutFooter = null!;

            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("push screen", () => LoadScreen(screen = new TestScreen
            {
                FooterContent =
                {
                    Value = ScreenFooterContent.Argon(() => new[]
                    {
                        new ScreenFooterButton { Text = "Button 1", Action = () => { } },
                        new ScreenFooterButton { Text = "Button 2", Action = () => { } },
                        new ScreenFooterButton { Text = "Button 3", Action = () => { } },
                    }),
                },
            }));
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);
            AddAssert("footer shown", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));

            AddStep("push screen with no footer", () => LoadScreen(screenWithoutFooter = new TestScreen
            {
                FooterContent = { Value = ScreenFooterContent.None }
            }));
            AddUntilStep("wait until screen is loaded", () => screenWithoutFooter.IsCurrentScreen(), () => Is.True);
            AddAssert("footer hidden", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("exit screen", () => Stack.Exit());
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);
            AddAssert("footer shown", () => ScreenFooter.State.Value, () => Is.EqualTo(Visibility.Visible));
        }

        [Test]
        public void TestExternalOverlayContent()
        {
            TestScreen screen = null!;

            AddStep("push screen", () =>
            {
                ShearedOverlayContainer overlay = new TestShearedOverlayContainer();

                LoadScreen(screen = new TestScreen
                {
                    Overlay = overlay,
                    FooterContent =
                    {
                        Value = ScreenFooterContent.Argon(
                            leftButtons: () => new[]
                            {
                                new ScreenFooterButton(overlay)
                                {
                                    AccentColour = Dependencies.Get<OsuColour>().Orange1,
                                    Icon = FontAwesome.Solid.Toolbox,
                                    Text = "One",
                                },
                                new ScreenFooterButton { Text = "Two", Action = () => { } },
                                new ScreenFooterButton { Text = "Three", Action = () => { } },
                            },
                            rightButtons: () => new[]
                            {
                                new ShearedButton
                                {
                                    Text = "Main action 1",
                                    Width = 150,
                                    LighterColour = colours.Blue1,
                                    DarkerColour = colours.Blue3,
                                    Action = () => { }
                                }
                            }
                        ),
                    },
                });
            });
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);

            AddStep("show overlay", () => screen.Overlay.Show());
            contentDisplayed();
            AddAssert("left buttons hidden", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.Child.Parent!.Y > 0));
            AddAssert("right buttons hidden", () => ScreenFooter.ChildrenOfType<ShearedButton>().Any(b => b.ChildrenOfType<Container>().ElementAt(1).Y > 0));

            AddStep("hide overlay", () => screen.Overlay.Hide());
            contentHidden();
            AddAssert("left buttons returned", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.ChildrenOfType<Container>().First().Y == 0));
            AddAssert("right buttons returned", () => ScreenFooter.ChildrenOfType<ShearedButton>().All(b => b.ChildrenOfType<Container>().ElementAt(1).Y == 0));
        }

        [Test]
        public void TestButtonsHiddenByExternalOverlayContentCannotBeTriggered()
        {
            TestScreen screen = null!;
            bool buttonTriggered = false;

            AddStep("push screen", () =>
            {
                ShearedOverlayContainer overlay = new TestShearedOverlayContainer();

                LoadScreen(screen = new TestScreen
                {
                    Overlay = overlay,
                    FooterContent =
                    {
                        Value = ScreenFooterContent.Argon(
                            leftButtons: () => new[]
                            {
                                new ScreenFooterButton(overlay)
                                {
                                    AccentColour = Dependencies.Get<OsuColour>().Orange1,
                                    Icon = FontAwesome.Solid.Toolbox,
                                    Text = "One",
                                },
                                new ScreenFooterButton { Text = "Two", Hotkey = GlobalAction.Select, Action = () => buttonTriggered = true },
                                new ScreenFooterButton { Text = "Three", Action = () => { } },
                            },
                            rightButtons: () => new[]
                            {
                                new ShearedButton
                                {
                                    Text = "Main action 1",
                                    Width = 150,
                                    LighterColour = colours.Blue1,
                                    DarkerColour = colours.Blue3,
                                    Action = () => buttonTriggered = true,
                                }
                            }
                        ),
                    },
                });
            });
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);

            AddStep("show overlay", () => screen.Overlay.Show());
            contentDisplayed();

            AddStep("try direct click on left button", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().ElementAt(1).TriggerClick());
            AddAssert("action not triggered", () => buttonTriggered, () => Is.False);

            AddStep("try left button hotkey", () => InputManager.Key(Key.Enter));
            AddAssert("action not triggered", () => buttonTriggered, () => Is.False);

            AddStep("try direct click on right button", () => ScreenFooter.ChildrenOfType<ShearedButton>().Single(b => b.Text == "Main action 1").TriggerClick());
            AddAssert("action not triggered", () => buttonTriggered, () => Is.False);
        }

        [Test]
        public void TestTemporarilyShowFooter()
        {
            TestScreen screen = null!;

            AddStep("push screen", () => LoadScreen(screen = new TestScreen
            {
                FooterContent = { Value = ScreenFooterContent.None },
            }));
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
        public void TestShowOverlayHidesOtherOverlays()
        {
            TestScreen screen = null!;

            AddStep("push screen", () =>
            {
                ShearedOverlayContainer overlay = new TestShearedOverlayContainer();
                ModSelectOverlay secondOverlay = new ModSelectOverlay();

                LoadScreen(screen = new TestScreen
                {
                    Overlay = overlay,
                    SecondOverlay = secondOverlay,
                    FooterContent =
                    {
                        Value = ScreenFooterContent.Argon(() => new[]
                        {
                            new ScreenFooterButton(overlay)
                            {
                                AccentColour = Dependencies.Get<OsuColour>().Orange1,
                                Icon = FontAwesome.Solid.Toolbox,
                                Text = "One",
                            },
                            new FooterButtonMods(secondOverlay),
                            new ScreenFooterButton { Text = "Two", Action = () => { } },
                            new ScreenFooterButton { Text = "Three", Action = () => { } },
                        }),
                    },
                });
            });
            AddUntilStep("wait until screen is loaded", () => screen.IsCurrentScreen(), () => Is.True);

            AddStep("show mods overlay", () => ScreenFooter.ChildrenOfType<FooterButtonMods>().First().TriggerClick());
            AddUntilStep("wait until overlay is shown", () => screen.SecondOverlay.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("first button still visible", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().First(b => b.Text == "One").Y, () => Is.EqualTo(0));

            AddStep("show test overlay", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().First(b => b.Text == "One").TriggerClick());
            AddUntilStep("wait until overlay is shown", () => screen.Overlay.State.Value, () => Is.EqualTo(Visibility.Visible));
            AddAssert("mod overlay is hidden", () => screen.SecondOverlay.State.Value, () => Is.EqualTo(Visibility.Hidden));

            AddStep("hide test overlay", () => screen.Overlay.Hide());
            contentHidden();
            AddAssert("other buttons returned", () => ScreenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.ChildrenOfType<Container>().First().Y == 0));
        }

        [Test]
        public void TestButtonResizedAfterFooterIsDisplayed()
        {
            TestScreen screen = null!;

            const float initial_width = 116;
            const float width_increase = 124;

            float secondButtonX = 0;
            float overlayContentX = 0;

            AddStep("push screen", () =>
            {
                ShearedOverlayContainer overlay = new TestShearedOverlayContainer();

                LoadScreen(screen = new TestScreen
                {
                    Overlay = overlay,
                    FooterContent =
                    {
                        Value = ScreenFooterContent.Argon(() => new[]
                        {
                            new ScreenFooterButton(overlay)
                            {
                                AccentColour = Dependencies.Get<OsuColour>().Orange1,
                                Icon = FontAwesome.Solid.Toolbox,
                                Text = "One",
                            },
                            new ScreenFooterButton { Text = "Two", Action = () => { } },
                            new ScreenFooterButton { Text = "Three", Action = () => { } },
                        }),
                    },
                });
            });
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

        private partial class TestScreen : OsuScreen
        {
            public ShearedOverlayContainer Overlay = new TestShearedOverlayContainer();
            public ShearedOverlayContainer SecondOverlay = new TestShearedOverlayContainer();

            private IDisposable? overlayRegistration;
            private IDisposable? secondOverlayRegistration;

            [Cached]
            private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

            [Resolved]
            private IOverlayManager? overlayManager { get; set; }

            public TestScreen()
            {
                FooterContent.Value = ScreenFooterContent.Argon();
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                LoadComponent(Overlay);
                LoadComponent(SecondOverlay);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                overlayRegistration = overlayManager?.RegisterBlockingOverlay(Overlay);
                secondOverlayRegistration = overlayManager?.RegisterBlockingOverlay(SecondOverlay);
            }

            protected override void Dispose(bool isDisposing)
            {
                base.Dispose(isDisposing);
                overlayRegistration?.Dispose();
                secondOverlayRegistration?.Dispose();
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
                            new ShearedButton { Width = 200, Text = "Action #1", Action = () => { } },
                            new ShearedButton { Width = 140, Text = "Action #2", Action = () => { } },
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
