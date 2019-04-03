// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Drawing;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual
{
    public class TestCaseLogoFacadeContainer : OsuTestCase
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

        private OsuLogo logo;

        private readonly Bindable<float> uiScale = new Bindable<float>();
        private LogoFacadeContainer logoFacadeContainer;
        private Container transferContainer;
        private Box visualBox;
        private Box transferContainerBox;
        private Container logoFacade;

        private bool randomPositions = false;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Add(logo = new OsuLogo { Scale = new Vector2(0.15f), RelativePositionAxes = Axes.None });

            config.BindWith(OsuSetting.UIScale, uiScale);
            AddSliderStep("Adjust scale", 0.8f, 1.5f, 1f, v => uiScale.Value = v);
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Clear facades", () =>
            {
                Clear();
                Add(logo = new OsuLogo { Scale = new Vector2(0.15f), RelativePositionAxes = Axes.None });
                logoFacadeContainer = null;
                transferContainer = null;
            });
        }

        /// <summary>
        /// Move the facade to 0,0, then move it to a random new location while the logo is still transforming to it.
        /// Check if the logo is still tracking the facade.
        /// </summary>
        [Test]
        public void MoveFacadeTest()
        {
            AddToggleStep("Toggle move continuously", b => randomPositions = b);
            AddStep("Add facade containers", addFacadeContainers);
            AddStep("Move facade to random position", StartTrackingRandom);
            waitForMove();
            AddAssert("Logo is tracking", () => IsLogoTracking);
        }

        /// <summary>
        /// Check if the facade is removed from the container, the logo stops tracking.
        /// </summary>
        [Test]
        public void RemoveFacadeTest()
        {
            AddStep("Add facade containers", addFacadeContainers);
            AddStep("Move facade to random position", StartTrackingRandom);
            AddStep("Remove facade from FacadeContainer", RemoveFacade);
            waitForMove();
            AddAssert("Logo is not tracking", () => !IsLogoTracking);
        }

        /// <summary>
        /// Check if the facade gets added to a new container, tracking starts on the new facade.
        /// </summary>
        [Test]
        public void TransferFacadeTest()
        {
            AddStep("Add facade containers", addFacadeContainers);
            AddStep("Move facade to random position", StartTrackingRandom);
            AddStep("Remove facade from FacadeContainer", RemoveFacade);
            AddStep("Transfer facade to a new container", TransferFacade);
            waitForMove();
            AddAssert("Logo is tracking", () => IsLogoTracking);
        }

        /// <summary>
        /// Add a facade to a flow container then move logo to facade.
        /// </summary>
        [Test]
        public void FlowContainerTest()
        {
            FillFlowContainer flowContainer;

            AddStep("Create new Logo Facade Container", () =>
            {
                Add(logoFacadeContainer = new LogoFacadeContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Child = flowContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Direction = FillDirection.Vertical,
                    }
                });
                flowContainer.Children = new Drawable[]
                {
                    new Box
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Colour = Color4.Azure,
                        Size = new Vector2(70)
                    },
                    new Container
                    {
                        Alpha = 0.35f,
                        RelativeSizeAxes = Axes.None,
                        Size = new Vector2(72),
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Children = new Drawable[]
                        {
                            visualBox = new Box
                            {
                                Colour = Color4.White,
                                RelativeSizeAxes = Axes.Both,
                            },
                            logoFacadeContainer.LogoFacade,
                        }
                    },
                    new Box
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Colour = Color4.Azure,
                        Size = new Vector2(70)
                    },
                };
            });

            AddStep("Perform logo movements", () =>
            {
                logoFacadeContainer.Tracking = false;
                logo.RelativePositionAxes = Axes.Both;
                logo.MoveTo(new Vector2(0.5f), 500, Easing.InOutExpo);
                logoFacadeContainer.SetLogo(logo, 1.0f, 1000, Easing.InOutExpo);
                visualBox.Colour = Color4.White;

                Scheduler.AddDelayed(() =>
                {
                    logoFacadeContainer.Tracking = true;
                    visualBox.Colour = Color4.Tomato;
                }, 700);
            });
        }

        private void addFacadeContainers()
        {
            AddRange(new Drawable[]
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
            });

            logoFacadeContainer.Add(logoFacade = logoFacadeContainer.LogoFacade);
            logoFacadeContainer.SetLogo(logo, 1.0f, 1000);
        }

        private void waitForMove() => AddWaitStep("Wait for transforms to finish", 5);

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

        public void StartTrackingRandom()
        {
            logoFacadeContainer.Tracking = true;
            moveLogoFacade();
        }

        private void moveLogoFacade()
        {
            Random random = new Random();
            if (logoFacade.Transforms.Count == 0 && transferContainer.Transforms.Count == 0)
            {
                logoFacadeContainer.Delay(500).MoveTo(new Vector2(random.Next(0, (int)logo.Parent.DrawWidth), random.Next(0, (int)logo.Parent.DrawHeight)), 300);
                transferContainer.Delay(500).MoveTo(new Vector2(random.Next(0, (int)logo.Parent.DrawWidth), random.Next(0, (int)logo.Parent.DrawHeight)), 300);
            }

            if (randomPositions)
                Schedule(moveLogoFacade);
        }
    }
}
