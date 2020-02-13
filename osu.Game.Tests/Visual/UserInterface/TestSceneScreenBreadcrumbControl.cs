// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using osuTK;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public class TestSceneScreenBreadcrumbControl : OsuTestScene
    {
        private readonly ScreenBreadcrumbControl breadcrumbs;
        private readonly OsuScreenStack screenStack;

        public TestSceneScreenBreadcrumbControl()
        {
            OsuSpriteText titleText;

            IScreen startScreen = new TestScreenOne();

            screenStack = new OsuScreenStack { RelativeSizeAxes = Axes.Both };
            screenStack.Push(startScreen);

            Children = new Drawable[]
            {
                screenStack,
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        breadcrumbs = new ScreenBreadcrumbControl(screenStack)
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                        titleText = new OsuSpriteText(),
                    },
                },
            };

            breadcrumbs.Current.ValueChanged += screen => titleText.Text = $"Changed to {screen.NewValue}";
            breadcrumbs.Current.TriggerChange();

            waitForCurrent();
            pushNext();
            waitForCurrent();
            pushNext();
            waitForCurrent();

            AddStep(@"make start current", () => startScreen.MakeCurrent());

            waitForCurrent();
            pushNext();
            waitForCurrent();
            AddAssert(@"only 2 items", () => breadcrumbs.Items.Count == 2);
            AddStep(@"exit current", () => screenStack.CurrentScreen.Exit());
            AddAssert(@"current screen is first", () => startScreen == screenStack.CurrentScreen);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            breadcrumbs.StripColour = colours.Blue;
        }

        private void pushNext() => AddStep(@"push next screen", () => ((TestScreen)screenStack.CurrentScreen).PushNext());
        private void waitForCurrent() => AddUntilStep("current screen", () => screenStack.CurrentScreen.IsCurrentScreen());

        private abstract class TestScreen : OsuScreen
        {
            protected abstract string NextTitle { get; }
            protected abstract TestScreen CreateNextScreen();

            public TestScreen PushNext()
            {
                TestScreen screen = CreateNextScreen();
                this.Push(screen);

                return screen;
            }

            protected TestScreen()
            {
                InternalChild = new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Text = Title,
                        },
                        new TriangleButton
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Width = 100,
                            Text = $"Push {NextTitle}",
                            Action = () => PushNext(),
                        },
                    },
                };
            }
        }

        private class TestScreenOne : TestScreen
        {
            public override string Title => @"Screen One";
            protected override string NextTitle => @"Two";
            protected override TestScreen CreateNextScreen() => new TestScreenTwo();
        }

        private class TestScreenTwo : TestScreen
        {
            public override string Title => @"Screen Two";
            protected override string NextTitle => @"One";
            protected override TestScreen CreateNextScreen() => new TestScreenOne();
        }
    }
}
