// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Utils;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneLogoTrackingContainer : OsuTestScene
    {
        private OsuLogo logo;
        private TestLogoTrackingContainer trackingContainer;
        private Container transferContainer;
        private Box visualBox;
        private Box transferContainerBox;
        private Drawable logoFacade;
        private bool randomPositions;

        private const float visual_box_size = 72;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Clear facades", () =>
            {
                Clear();
                Add(logo = new OsuLogo { Scale = new Vector2(0.15f), RelativePositionAxes = Axes.Both });
                trackingContainer = null;
                transferContainer = null;
            });
        }

        /// <summary>
        /// Move the facade to 0,0, then move it to a random new location while the logo is still transforming to it.
        /// Check if the logo is still tracking the facade.
        /// </summary>
        [Test]
        public void TestMoveFacade()
        {
            AddToggleStep("Toggle move continuously", b => randomPositions = b);
            AddStep("Add tracking containers", addFacadeContainers);
            AddStep("Move facade to random position", moveLogoFacade);
            waitForMove();
            AddAssert("Logo is tracking", () => trackingContainer.IsLogoTracking);
        }

        /// <summary>
        /// Check if the facade is removed from the container, the logo stops tracking.
        /// </summary>
        [Test]
        public void TestRemoveFacade()
        {
            AddStep("Add tracking containers", addFacadeContainers);
            AddStep("Move facade to random position", moveLogoFacade);
            AddStep("Remove facade from FacadeContainer", removeFacade);
            waitForMove();
            AddAssert("Logo is not tracking", () => !trackingContainer.IsLogoTracking);
        }

        /// <summary>
        /// Check if the facade gets added to a new container, tracking starts on the new facade.
        /// </summary>
        [Test]
        public void TestTransferFacade()
        {
            AddStep("Add tracking containers", addFacadeContainers);
            AddStep("Move facade to random position", moveLogoFacade);
            AddStep("Remove facade from FacadeContainer", removeFacade);
            AddStep("Transfer facade to a new container", () =>
            {
                transferContainer.Add(logoFacade);
                transferContainerBox.Colour = Color4.Tomato;
                moveLogoFacade();
            });

            waitForMove();
            AddAssert("Logo is tracking", () => trackingContainer.IsLogoTracking);
        }

        /// <summary>
        /// Add a facade to a flow container, move the logo to the center of the screen, then start tracking on the facade.
        /// </summary>
        [Test]
        public void TestFlowContainer()
        {
            FillFlowContainer flowContainer;

            AddStep("Create new flow container with facade", () =>
            {
                Add(trackingContainer = new TestLogoTrackingContainer
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
                        Size = new Vector2(visual_box_size)
                    },
                    new Container
                    {
                        Alpha = 0.35f,
                        RelativeSizeAxes = Axes.None,
                        Size = new Vector2(visual_box_size),
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Children = new Drawable[]
                        {
                            visualBox = new Box
                            {
                                Colour = Color4.White,
                                RelativeSizeAxes = Axes.Both,
                            },
                            trackingContainer.LogoFacade,
                        }
                    },
                    new Box
                    {
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Colour = Color4.Azure,
                        Size = new Vector2(visual_box_size)
                    },
                };
            });

            AddStep("Perform logo movements", () =>
            {
                trackingContainer.StopTracking();
                logo.MoveTo(new Vector2(0.5f), 500, Easing.InOutExpo);

                visualBox.Colour = Color4.White;

                Scheduler.AddDelayed(() =>
                {
                    trackingContainer.StartTracking(logo, 1000, Easing.InOutExpo);
                    visualBox.Colour = Color4.Tomato;
                }, 700);
            });

            waitForMove(8);
            AddAssert("Logo is tracking", () => trackingContainer.IsLogoTracking);
        }

        [Test]
        public void TestSetFacadeSize()
        {
            bool failed = false;

            AddStep("Set up scenario", () =>
            {
                failed = false;
                addFacadeContainers();
            });

            AddStep("Try setting facade size", () =>
            {
                try
                {
                    logoFacade.Size = new Vector2(0, 0);
                }
                catch (Exception e)
                {
                    if (e is InvalidOperationException)
                        failed = true;
                }
            });

            AddAssert("Exception thrown", () => failed);
        }

        [Test]
        public void TestSetMultipleContainers()
        {
            bool failed = false;
            LogoTrackingContainer newContainer = new LogoTrackingContainer();

            AddStep("Set up scenario", () =>
            {
                failed = false;
                newContainer = new LogoTrackingContainer();
                addFacadeContainers();
                moveLogoFacade();
            });

            AddStep("Try tracking new container", () =>
            {
                try
                {
                    newContainer.StartTracking(logo);
                }
                catch (Exception e)
                {
                    if (e is InvalidOperationException)
                        failed = true;
                }
            });

            AddAssert("Exception thrown", () => failed);
        }

        private void addFacadeContainers()
        {
            AddRange(new Drawable[]
            {
                trackingContainer = new TestLogoTrackingContainer
                {
                    Alpha = 0.35f,
                    RelativeSizeAxes = Axes.None,
                    Size = new Vector2(visual_box_size),
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
                    Size = new Vector2(visual_box_size),
                    Child = transferContainerBox = new Box
                    {
                        Colour = Color4.White,
                        RelativeSizeAxes = Axes.Both,
                    }
                },
            });

            trackingContainer.Add(logoFacade = trackingContainer.LogoFacade);
            trackingContainer.StartTracking(logo, 1000);
        }

        private void waitForMove(int count = 5) => AddWaitStep("Wait for transforms to finish", count);

        private void removeFacade()
        {
            trackingContainer.Remove(logoFacade, false);
            visualBox.Colour = Color4.White;
            moveLogoFacade();
        }

        private void moveLogoFacade()
        {
            if (!logoFacade.Transforms.Any() && !transferContainer.Transforms.Any())
            {
                Random random = new Random();
                trackingContainer.Delay(500).MoveTo(new Vector2(random.Next(0, (int)logo.Parent!.DrawWidth), random.Next(0, (int)logo.Parent!.DrawHeight)), 300);
                transferContainer.Delay(500).MoveTo(new Vector2(random.Next(0, (int)logo.Parent!.DrawWidth), random.Next(0, (int)logo.Parent!.DrawHeight)), 300);
            }

            if (randomPositions)
                Schedule(moveLogoFacade);
        }

        private partial class TestLogoTrackingContainer : LogoTrackingContainer
        {
            /// <summary>
            /// Check that the logo is tracking the position of the facade, with an acceptable precision lenience.
            /// </summary>
            public bool IsLogoTracking => Precision.AlmostEquals(Logo.Position, ComputeLogoTrackingPosition());
        }
    }
}
