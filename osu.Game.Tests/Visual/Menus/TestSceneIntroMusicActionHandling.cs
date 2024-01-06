// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    public partial class TestSceneIntroMusicActionHandling : OsuTestScene
    {
        private OsuGameTestScene.TestOsuGame? game;

        private GlobalActionContainer globalActionContainer => game.ChildrenOfType<GlobalActionContainer>().First();

        [Test]
        public void TestPauseDuringIntro()
        {
            AddStep("Create new game instance", () =>
            {
                if (game?.Parent != null)
                    Remove(game, true);

                RecycleLocalStorage(false);

                AddGame(game = new OsuGameTestScene.TestOsuGame(LocalStorage, API));
            });

            AddUntilStep("Wait for load", () => game?.IsLoaded ?? false);
            AddUntilStep("Wait for intro", () => game?.ScreenStack.CurrentScreen is IntroScreen);
            AddUntilStep("Wait for music", () => game?.MusicController.IsPlaying == true);

            // Check that pause dosesn't work during intro sequence.
            AddStep("Toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddAssert("Still playing before menu", () => game?.MusicController.IsPlaying == true);
            AddUntilStep("Wait for main menu", () => game?.ScreenStack.CurrentScreen is MainMenu menu && menu.IsLoaded);

            // Check that toggling after intro still works.
            AddStep("Toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddUntilStep("Music paused", () => game?.MusicController.IsPlaying == false && game?.MusicController.UserPauseRequested == true);
            AddStep("Toggle playback", () => globalActionContainer.TriggerPressed(GlobalAction.MusicPlay));
            AddUntilStep("Music resumed", () => game?.MusicController.IsPlaying == true && game?.MusicController.UserPauseRequested == false);
        }
    }
}
