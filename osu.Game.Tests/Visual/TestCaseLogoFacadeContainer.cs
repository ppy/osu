// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Screens;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseLogoFacadeContainer : ScreenTestCase
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(PlayerLoader),
            typeof(Player),
            typeof(LogoFacadeContainer),
            typeof(ButtonSystem),
            typeof(ButtonSystemState),
            typeof(Menu),
            typeof(MainMenu)
        };

        [Cached]
        private OsuLogo logo;

        private readonly Bindable<float> uiScale = new Bindable<float>();

        public TestCaseLogoFacadeContainer()
        {
            Add(logo = new OsuLogo());
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            config.BindWith(OsuSetting.UIScale, uiScale);
            AddSliderStep("Adjust scale", 0.8f, 1.5f, 1f, v => uiScale.Value = v);
        }

        /// <summary>
        /// Move the facade to 0,0, then move it to a random new location while the logo is still transforming to it.
        /// Check if the logo is still tracking the facade.
        /// </summary>
        [Test]
        public void MoveFacadeTest()
        {
            TestScreen screen = null;
            bool randomPositions = false;
            AddToggleStep("Toggle move continuously", b => randomPositions = b);
            AddStep("Move facade to random position", () => LoadScreen(screen = new TestScreen(randomPositions)));
            AddUntilStep("Screen is current", () => screen.IsCurrentScreen());
            waitForMove();
            AddAssert("Logo is tracking", () => screen.IsLogoTracking);
        }

        /// <summary>
        /// Check if the facade is removed from the container, the logo stops tracking.
        /// </summary>
        [Test]
        public void RemoveFacadeTest()
        {
            TestScreen screen = null;
            AddStep("Move facade to random position", () => LoadScreen(screen = new TestScreen()));
            AddUntilStep("Screen is current", () => screen.IsCurrentScreen());
            AddStep("Remove facade from FacadeContainer", () => screen.RemoveFacade());
            waitForMove();
            AddAssert("Logo is not tracking", () => !screen.IsLogoTracking);
        }

        /// <summary>
        /// Check if the facade gets added to a new container, tracking starts on the new facade.
        /// </summary>
        [Test]
        public void TransferFacadeTest()
        {
            TestScreen screen = null;
            AddStep("Move facade to random position", () => LoadScreen(screen = new TestScreen()));
            AddUntilStep("Screen is current", () => screen.IsCurrentScreen());
            AddStep("Remove facade from FacadeContainer", () => screen.RemoveFacade());
            AddStep("Transfer facade to a new container", () => screen.TransferFacade());
            waitForMove();
            AddAssert("Logo is tracking", () => screen.IsLogoTracking);
        }

        private void waitForMove() => AddWaitStep("Wait for transforms to finish", 5);

        private class TestScreen : OsuScreen
        {
            private LogoFacadeContainer logoFacadeContainer;
            private Container transferContainer;
            private Container logoFacade;
            private readonly bool randomPositions;
            private OsuLogo logo;
            private Box visualBox;
            private Box transferContainerBox;

            public TestScreen(bool randomPositions = false)
            {
                this.randomPositions = randomPositions;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChildren = new Drawable[]
                {
                    logoFacadeContainer = new LogoFacadeContainer
                    {
                        Alpha = 0.35f,
                        RelativeSizeAxes = Axes.None,
                        Size = new Vector2(72),
                        Child = visualBox = new Box
                        {
                            Colour = Color4.Tomato,
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                    transferContainer = new Container
                    {
                        Alpha = 0.35f,
                        RelativeSizeAxes = Axes.None,
                        Size = new Vector2(72),
                        Child = transferContainerBox = new Box
                        {
                            Colour = Color4.White,
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                };

                logoFacadeContainer.Add(logoFacade = logoFacadeContainer.LogoFacade);
            }

            private Vector2 logoTrackingPosition => logo.Parent.ToLocalSpace(logoFacade.ScreenSpaceDrawQuad.Centre);

            /// <summary>
            /// Check that the logo is tracking the position of the facade, with an acceptable precision lenience.
            /// </summary>
            public bool IsLogoTracking => Math.Abs(logo.Position.X - logoTrackingPosition.X) < 0.001f && Math.Abs(logo.Position.Y - logoTrackingPosition.Y) < 0.001f;

            public void RemoveFacade()
            {
                logoFacadeContainer.Remove(logoFacade);
                visualBox.Colour = Color4.White;
                moveLogoFacade();
            }

            public void TransferFacade()
            {
                transferContainer.Add(logoFacade);
                transferContainerBox.Colour = Color4.Tomato;
                moveLogoFacade();
            }

            protected override void LogoArriving(OsuLogo logo, bool resuming)
            {
                base.LogoArriving(logo, resuming);
                this.logo = logo;
                logo.FadeIn(350);
                logo.ScaleTo(new Vector2(0.15f), 350, Easing.In);
                logoFacadeContainer.SetLogo(logo, 1.0f, 1000, Easing.InOutQuint);
                logoFacadeContainer.Tracking = true;
                moveLogoFacade();
            }

            protected override void LogoExiting(OsuLogo logo)
            {
                base.LogoExiting(logo);
                logoFacadeContainer.Tracking = false;
            }

            private void moveLogoFacade()
            {
                Random random = new Random();
                if (logoFacade.Transforms.Count == 0 && transferContainer.Transforms.Count == 0)
                {
                    logoFacadeContainer.Delay(500).MoveTo(new Vector2(random.Next(0, (int)DrawWidth), random.Next(0, (int)DrawHeight)), 300);
                    transferContainer.Delay(500).MoveTo(new Vector2(random.Next(0, (int)DrawWidth), random.Next(0, (int)DrawHeight)), 300);
                }

                if (randomPositions)
                    Schedule(moveLogoFacade);
            }
        }
    }
}
