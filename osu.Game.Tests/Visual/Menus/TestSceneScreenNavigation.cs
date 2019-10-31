// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Tests.Beatmaps.IO;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using IntroSequence = osu.Game.Configuration.IntroSequence;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneScreenNavigation : ManualInputManagerTestScene
    {
        private const float click_padding = 25;

        private GameHost host;
        private TestOsuGame game;

        private Vector2 backButtonPosition => game.ToScreenSpace(new Vector2(click_padding, game.LayoutRectangle.Bottom - click_padding));

        private Vector2 optionsButtonPosition => game.ToScreenSpace(new Vector2(click_padding, click_padding));

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            this.host = host;

            Child = new Box
            {
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
            };
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Create new game instance", () =>
            {
                if (game != null)
                {
                    Remove(game);
                    game.Dispose();
                }

                game = new TestOsuGame(LocalStorage, API);
                game.SetHost(host);

                // todo: this can be removed once we can run audio trakcs without a device present
                // see https://github.com/ppy/osu/issues/1302
                game.LocalConfig.Set(OsuSetting.IntroSequence, IntroSequence.Circles);

                Add(game);
            });
            AddUntilStep("Wait for load", () => game.IsLoaded);
            AddUntilStep("Wait for intro", () => game.ScreenStack.CurrentScreen is IntroScreen);
            confirmAtMainMenu();
        }

        [Test]
        public void TestExitSongSelectWithEscape()
        {
            TestSongSelect songSelect = null;

            pushAndConfirm(() => songSelect = new TestSongSelect());
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
            pushEscape();
            AddAssert("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
            exitViaEscapeAndConfirm();
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestSongContinuesAfterExitPlayer(bool withUserPause)
        {
            Player player = null;

            WorkingBeatmap beatmap() => game.Beatmap.Value;
            Track track() => beatmap().Track;

            pushAndConfirm(() => new TestSongSelect());

            AddStep("import beatmap", () => ImportBeatmapTest.LoadOszIntoOsu(game, virtualTrack: true).Wait());

            AddUntilStep("wait for selected", () => !game.Beatmap.IsDefault);

            if (withUserPause)
                AddStep("pause", () => game.Dependencies.Get<MusicController>().Stop());

            AddStep("press enter", () => pressAndRelease(Key.Enter));

            AddUntilStep("wait for player", () => (player = game.ScreenStack.CurrentScreen as Player) != null);
            AddUntilStep("wait for fail", () => player.HasFailed);

            AddUntilStep("wait for track stop", () => !track().IsRunning);
            AddAssert("Ensure time before preview point", () => track().CurrentTime < beatmap().Metadata.PreviewTime);

            pushEscape();

            AddUntilStep("wait for track playing", () => track().IsRunning);
            AddAssert("Ensure time wasn't reset to preview point", () => track().CurrentTime < beatmap().Metadata.PreviewTime);
        }

        [Test]
        public void TestExitSongSelectWithClick()
        {
            TestSongSelect songSelect = null;

            pushAndConfirm(() => songSelect = new TestSongSelect());
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(backButtonPosition));

            // BackButton handles hover using its child button, so this checks whether or not any of BackButton's children are hovered.
            AddUntilStep("Back button is hovered", () => InputManager.HoveredDrawables.Any(d => d.Parent == game.BackButton));

            AddStep("Click back button", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
            exitViaBackButtonAndConfirm();
        }

        [Test]
        public void TestExitMultiWithEscape()
        {
            pushAndConfirm(() => new Screens.Multi.Multiplayer());
            exitViaEscapeAndConfirm();
        }

        [Test]
        public void TestExitMultiWithBackButton()
        {
            pushAndConfirm(() => new Screens.Multi.Multiplayer());
            exitViaBackButtonAndConfirm();
        }

        [Test]
        public void TestOpenOptionsAndExitWithEscape()
        {
            AddUntilStep("Wait for options to load", () => game.Settings.IsLoaded);
            AddStep("Enter menu", () => pressAndRelease(Key.Enter));
            AddStep("Move mouse to options overlay", () => InputManager.MoveMouseTo(optionsButtonPosition));
            AddStep("Click options overlay", () => InputManager.Click(MouseButton.Left));
            AddAssert("Options overlay was opened", () => game.Settings.State.Value == Visibility.Visible);
            AddStep("Hide options overlay using escape", () => pressAndRelease(Key.Escape));
            AddAssert("Options overlay was closed", () => game.Settings.State.Value == Visibility.Hidden);
        }

        private void pushAndConfirm(Func<Screen> newScreen)
        {
            Screen screen = null;
            AddStep("Push new screen", () => game.ScreenStack.Push(screen = newScreen()));
            AddUntilStep("Wait for new screen", () => game.ScreenStack.CurrentScreen == screen && screen.IsLoaded);
        }

        private void pushEscape() =>
            AddStep("Press escape", () => pressAndRelease(Key.Escape));

        private void exitViaEscapeAndConfirm()
        {
            pushEscape();
            confirmAtMainMenu();
        }

        private void exitViaBackButtonAndConfirm()
        {
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(backButtonPosition));
            AddStep("Click back button", () => InputManager.Click(MouseButton.Left));
            confirmAtMainMenu();
        }

        private void confirmAtMainMenu() => AddUntilStep("Wait for main menu", () => game.ScreenStack.CurrentScreen is MainMenu menu && menu.IsLoaded);

        private void pressAndRelease(Key key)
        {
            InputManager.PressKey(key);
            InputManager.ReleaseKey(key);
        }

        private class TestOsuGame : OsuGame
        {
            public new ScreenStack ScreenStack => base.ScreenStack;

            public new BackButton BackButton => base.BackButton;

            public new SettingsPanel Settings => base.Settings;

            public new OsuConfigManager LocalConfig => base.LocalConfig;

            public new Bindable<WorkingBeatmap> Beatmap => base.Beatmap;

            protected override Loader CreateLoader() => new TestLoader();

            public TestOsuGame(Storage storage, IAPIProvider api)
            {
                Storage = storage;
                API = api;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                API.Login("Rhythm Champion", "osu!");
            }
        }

        private class TestSongSelect : PlaySongSelect
        {
            public ModSelectOverlay ModSelectOverlay => ModSelect;
        }

        private class TestLoader : Loader
        {
            protected override ShaderPrecompiler CreateShaderPrecompiler() => new TestShaderPrecompiler();

            private class TestShaderPrecompiler : ShaderPrecompiler
            {
                protected override bool AllLoaded => true;
            }
        }
    }
}
