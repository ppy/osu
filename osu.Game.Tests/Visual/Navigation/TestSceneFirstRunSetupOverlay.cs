// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Overlays.FirstRunSetup;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestSceneFirstRunSetupOverlay : OsuTestScene
    {
        private FirstRunSetupOverlay overlay;

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("add overlay", () =>
            {
                Child = overlay = new FirstRunSetupOverlay();
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
    }
}
