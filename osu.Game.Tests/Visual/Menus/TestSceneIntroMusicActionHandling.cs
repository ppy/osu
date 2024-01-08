// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneIntroMusicActionHandling : OsuGameTestScene
    {
        private GlobalActionContainer globalActionContainer => Game.ChildrenOfType<GlobalActionContainer>().First();

        public override void SetUpSteps()
        {
            CreateNewGame();
            // we do not want to progress to main menu immediately, hence the override and lack of `ConfirmAtMainMenu()` call here.
        }

        [Test]
        public void TestPauseDuringIntro()
        {
            AddUntilStep("Wait for music", () => Game?.MusicController.IsPlaying == true);

            // Check that pause dosesn't work during intro sequence.
            AddStep("Toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddAssert("Still playing before menu", () => Game?.MusicController.IsPlaying == true);
            AddUntilStep("Wait for main menu", () => Game?.ScreenStack.CurrentScreen is MainMenu menu && menu.IsLoaded);

            // Check that toggling after intro still works.
            AddStep("Toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddUntilStep("Music paused", () => Game?.MusicController.IsPlaying == false && Game?.MusicController.UserPauseRequested == true);
            AddStep("Toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddUntilStep("Music resumed", () => Game?.MusicController.IsPlaying == true && Game?.MusicController.UserPauseRequested == false);
        }
    }
}
