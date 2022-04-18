// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.FirstRunSetup;
using osu.Game.Screens;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneFirstRunSetupOverlay : OsuManualInputManagerTestScene
    {
        private FirstRunSetupOverlay overlay;

        private readonly Mock<IPerformFromScreenRunner> perfomer = new Mock<IPerformFromScreenRunner>();

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.CacheAs(perfomer.Object);

            perfomer.Setup(g => g.PerformFromScreen(It.IsAny<Action<IScreen>>(), It.IsAny<IEnumerable<Type>>()))
                    .Callback((Action<IScreen> action, IEnumerable<Type> types) => action(null));
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("add overlay", () =>
            {
                Child = overlay = new FirstRunSetupOverlay
                {
                    State = { Value = Visibility.Visible }
                };
            });
        }

        [Test]
        public void TestOverlayRunsToFinish()
        {
            AddUntilStep("step through", () =>
            {
                if (overlay.CurrentScreen?.IsLoaded != false)
                    overlay.NextButton.TriggerClick();

                return overlay.State.Value == Visibility.Hidden;
            });

            AddUntilStep("wait for screens removed", () => !overlay.ChildrenOfType<Screen>().Any());

            AddStep("display again on demand", () => overlay.Show());

            AddUntilStep("back at start", () => overlay.CurrentScreen is ScreenWelcome);
        }

        [Test]
        public void TestBackButton()
        {
            AddAssert("back button disabled", () => !overlay.BackButton.Enabled.Value);

            AddUntilStep("step to last", () =>
            {
                var nextButton = overlay.NextButton;

                if (overlay.CurrentScreen?.IsLoaded != false)
                    nextButton.TriggerClick();

                return nextButton.Text.ToString() == "Finish";
            });

            AddUntilStep("step back to start", () =>
            {
                if (overlay.CurrentScreen?.IsLoaded != false)
                    overlay.BackButton.TriggerClick();

                return overlay.CurrentScreen is ScreenWelcome;
            });

            AddAssert("back button disabled", () => !overlay.BackButton.Enabled.Value);
        }

        [Test]
        public void TestClickAwayToExit()
        {
            AddStep("click inside content", () =>
            {
                InputManager.MoveMouseTo(overlay.ScreenSpaceDrawQuad.Centre);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("overlay not dismissed", () => overlay.State.Value == Visibility.Visible);

            AddStep("click outside content", () =>
            {
                InputManager.MoveMouseTo(overlay.ScreenSpaceDrawQuad.TopLeft - new Vector2(1));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("overlay dismissed", () => overlay.State.Value == Visibility.Hidden);
        }
    }
}
