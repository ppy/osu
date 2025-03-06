// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Screens.SelectV2.Footer;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneScreenFooter : OsuManualInputManagerTestScene
    {
        private DependencyProvidingContainer contentContainer = null!;
        private ScreenFooter screenFooter = null!;
        private UserModSelectOverlay modOverlay = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            screenFooter = new ScreenFooter();

            Child = contentContainer = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(ScreenFooter), screenFooter)
                },
                Children = new Drawable[]
                {
                    modOverlay = new UserModSelectOverlay { ShowPresets = true },
                    new PopoverContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Depth = float.MinValue,
                        Child = screenFooter,
                    },
                },
            };

            screenFooter.SetButtons(new ScreenFooterButton[]
            {
                new ScreenFooterButtonMods(modOverlay) { Current = SelectedMods },
                new ScreenFooterButtonRandom(),
                new ScreenFooterButtonOptions(),
            });
        });

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("show footer", () => screenFooter.Show());
        }

        /// <summary>
        /// Transition when moving from a screen with no buttons to a screen with buttons.
        /// </summary>
        [Test]
        public void TestButtonsIn()
        {
        }

        /// <summary>
        /// Transition when moving from a screen with buttons to a screen with no buttons.
        /// </summary>
        [Test]
        public void TestButtonsOut()
        {
            AddStep("clear buttons", () => screenFooter.SetButtons(Array.Empty<ScreenFooterButton>()));
        }

        /// <summary>
        /// Transition when moving from a screen with buttons to a screen with buttons.
        /// </summary>
        [Test]
        public void TestReplaceButtons()
        {
            AddStep("replace buttons", () => screenFooter.SetButtons(new[]
            {
                new ScreenFooterButton { Text = "One", Action = () => { } },
                new ScreenFooterButton { Text = "Two", Action = () => { } },
                new ScreenFooterButton { Text = "Three", Action = () => { } },
            }));
        }

        [Test]
        public void TestExternalOverlayContent()
        {
            TestShearedOverlayContainer externalOverlay = null!;

            AddStep("add overlay", () => contentContainer.Add(externalOverlay = new TestShearedOverlayContainer()));
            AddStep("set buttons", () => screenFooter.SetButtons(new[]
            {
                new ScreenFooterButton(externalOverlay)
                {
                    AccentColour = Dependencies.Get<OsuColour>().Orange1,
                    Icon = FontAwesome.Solid.Toolbox,
                    Text = "One",
                },
                new ScreenFooterButton { Text = "Two", Action = () => { } },
                new ScreenFooterButton { Text = "Three", Action = () => { } },
            }));
            AddWaitStep("wait for transition", 3);

            AddStep("show overlay", () => externalOverlay.Show());
            AddAssert("content displayed in footer", () => screenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().Single().IsPresent);
            AddUntilStep("other buttons hidden", () => screenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.Child.Parent!.Y > 0));

            AddStep("hide overlay", () => externalOverlay.Hide());
            AddUntilStep("content hidden from footer", () => screenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().SingleOrDefault()?.IsPresent != true);
            AddUntilStep("other buttons returned", () => screenFooter.ChildrenOfType<ScreenFooterButton>().Skip(1).All(b => b.ChildrenOfType<Container>().First().Y == 0));
        }

        [Test]
        public void TestTemporarilyShowFooter()
        {
            TestShearedOverlayContainer externalOverlay = null!;

            AddStep("hide footer", () => screenFooter.Hide());
            AddStep("remove buttons", () => screenFooter.SetButtons(Array.Empty<ScreenFooterButton>()));

            AddStep("add external overlay", () => contentContainer.Add(externalOverlay = new TestShearedOverlayContainer()));
            AddStep("show external overlay", () => externalOverlay.Show());
            AddAssert("footer shown", () => screenFooter.State.Value == Visibility.Visible);
            AddAssert("content displayed in footer", () => screenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().Single().IsPresent);

            AddStep("hide external overlay", () => externalOverlay.Hide());
            AddAssert("footer hidden", () => screenFooter.State.Value == Visibility.Hidden);
            AddUntilStep("content hidden from footer", () => screenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().SingleOrDefault()?.IsPresent != true);

            AddStep("show footer", () => screenFooter.Show());
            AddAssert("content still hidden from footer", () => screenFooter.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().SingleOrDefault()?.IsPresent != true);

            AddStep("show external overlay", () => externalOverlay.Show());
            AddAssert("footer still visible", () => screenFooter.State.Value == Visibility.Visible);

            AddStep("hide external overlay", () => externalOverlay.Hide());
            AddAssert("footer still visible", () => screenFooter.State.Value == Visibility.Visible);

            AddStep("hide footer", () => screenFooter.Hide());
            AddStep("show external overlay", () => externalOverlay.Show());
        }

        [Test]
        public void TestBackButton()
        {
            TestShearedOverlayContainer externalOverlay = null!;

            AddStep("hide footer", () => screenFooter.Hide());
            AddStep("remove buttons", () => screenFooter.SetButtons(Array.Empty<ScreenFooterButton>()));

            AddStep("add external overlay", () => contentContainer.Add(externalOverlay = new TestShearedOverlayContainer()));
            AddStep("show external overlay", () => externalOverlay.Show());
            AddAssert("footer shown", () => screenFooter.State.Value == Visibility.Visible);

            AddStep("press back", () => this.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay hidden", () => externalOverlay.State.Value == Visibility.Hidden);
            AddAssert("footer hidden", () => screenFooter.State.Value == Visibility.Hidden);

            AddStep("show external overlay", () => externalOverlay.Show());
            AddStep("set block count", () => externalOverlay.BackButtonCount = 1);
            AddStep("press back", () => this.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay still visible", () => externalOverlay.State.Value == Visibility.Visible);
            AddAssert("footer still shown", () => screenFooter.State.Value == Visibility.Visible);
            AddStep("press back again", () => this.ChildrenOfType<ScreenBackButton>().Single().TriggerClick());
            AddAssert("overlay hidden", () => externalOverlay.State.Value == Visibility.Hidden);
            AddAssert("footer hidden", () => screenFooter.State.Value == Visibility.Hidden);
        }

        [Test]
        public void TestLoadOverlayAfterFooterIsDisplayed()
        {
            TestShearedOverlayContainer externalOverlay = null!;

            AddStep("show mod overlay", () => modOverlay.Show());
            AddUntilStep("mod footer content shown", () => this.ChildrenOfType<ModSelectFooterContent>().SingleOrDefault()?.IsPresent, () => Is.True);

            AddStep("add external overlay", () => contentContainer.Add(externalOverlay = new TestShearedOverlayContainer()));
            AddUntilStep("wait for load", () => externalOverlay.IsLoaded);
            AddAssert("mod footer content still shown", () => this.ChildrenOfType<ModSelectFooterContent>().SingleOrDefault()?.IsPresent, () => Is.True);
            AddAssert("external overlay content not shown", () => this.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().SingleOrDefault()?.IsPresent, () => Is.Not.True);

            AddStep("hide mod overlay", () => modOverlay.Hide());
            AddUntilStep("mod footer content hidden", () => this.ChildrenOfType<ModSelectFooterContent>().SingleOrDefault()?.IsPresent, () => Is.Not.True);
            AddAssert("external overlay content still not shown", () => this.ChildrenOfType<TestShearedOverlayContainer.TestFooterContent>().SingleOrDefault()?.IsPresent, () => Is.Not.True);
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
                    RelativeSizeAxes = Axes.Both;

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
