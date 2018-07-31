// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens;
using OpenTK;

namespace osu.Game.Tests.Visual
{
    [TestFixture]
    public class TestCaseScreenBreadcrumbControl : OsuTestCase
    {
        private readonly ScreenBreadcrumbControl breadcrumbs;
        private Screen currentScreen, changedScreen;

        public TestCaseScreenBreadcrumbControl()
        {
            TestScreen startScreen;
            OsuSpriteText titleText;

            Children = new Drawable[]
            {
                currentScreen = startScreen = new TestScreenOne(),
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(10),
                    Children = new Drawable[]
                    {
                        breadcrumbs = new ScreenBreadcrumbControl(startScreen)
                        {
                            RelativeSizeAxes = Axes.X,
                        },
                        titleText = new OsuSpriteText(),
                    },
                },
            };

            breadcrumbs.Current.ValueChanged += s =>
            {
                titleText.Text = $"Changed to {s.ToString()}";
                changedScreen = s;
            };

            breadcrumbs.Current.TriggerChange();

            waitForCurrent();
            pushNext();
            waitForCurrent();
            pushNext();
            waitForCurrent();

            AddStep(@"make start current", () =>
            {
                startScreen.MakeCurrent();
                currentScreen = startScreen;
            });

            waitForCurrent();
            pushNext();
            waitForCurrent();
            AddAssert(@"only 2 items", () => breadcrumbs.Items.Count() == 2);
            AddStep(@"exit current", () => changedScreen.Exit());
            AddAssert(@"current screen is first", () => startScreen == changedScreen);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            breadcrumbs.StripColour = colours.Blue;
        }

        private void pushNext() => AddStep(@"push next screen", () => currentScreen = ((TestScreen)currentScreen).PushNext());
        private void waitForCurrent() => AddUntilStep(() => currentScreen.IsCurrentScreen, "current screen");

        private abstract class TestScreen : OsuScreen
        {
            protected abstract string NextTitle { get; }
            protected abstract TestScreen CreateNextScreen();

            public TestScreen PushNext()
            {
                TestScreen screen = CreateNextScreen();
                Push(screen);

                return screen;
            }

            protected TestScreen()
            {
                Child = new FillFlowContainer
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
