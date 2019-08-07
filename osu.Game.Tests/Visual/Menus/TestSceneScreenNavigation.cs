// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneScreenNavigation : ManualInputManagerTestScene
    {
        private GameHost gameHost;
        private TestOsuGame osuGame;

        private Vector2 backButtonPosition => osuGame.ToScreenSpace(new Vector2(25, osuGame.LayoutRectangle.Bottom - 25));

        private Vector2 optionsButtonPosition => osuGame.ToScreenSpace(new Vector2(25, 25));

        [BackgroundDependencyLoader]
        private void load(GameHost gameHost)
        {
            this.gameHost = gameHost;

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
                if (osuGame != null)
                {
                    Remove(osuGame);
                    osuGame.Dispose();
                }

                osuGame = new TestOsuGame();
                osuGame.SetHost(gameHost);

                Add(osuGame);
            });
            AddUntilStep("Wait for load", () => osuGame.IsLoaded);
            AddUntilStep("Wait for intro", () => osuGame.ScreenStack.CurrentScreen is IntroScreen);
            AddUntilStep("Wait for main menu", () => osuGame.ScreenStack.CurrentScreen is MainMenu menu && menu.IsLoaded);
        }

        [Test]
        public void TestExitSongSelectWithEscape()
        {
            TestSongSelect songSelect = null;

            pushAndConfirm(() => songSelect = new TestSongSelect(), "song select");
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
            AddStep("Press escape", () => pressAndRelease(Key.Escape));
            AddAssert("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
            exitViaEscapeAndConfirm();
        }

        [Test]
        public void TestExitSongSelectWithClick()
        {
            TestSongSelect songSelect = null;

            pushAndConfirm(() => songSelect = new TestSongSelect(), "song select");
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(backButtonPosition));

            // BackButton handles hover using its child button, so this checks whether or not any of BackButton's children are hovered.
            AddUntilStep("Back button is hovered", () => InputManager.HoveredDrawables.Any(d => d.Parent == osuGame.BackButton));

            AddStep("Click back button", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
            exitViaBackButtonAndConfirm();
        }

        [Test]
        public void TestExitMultiWithEscape()
        {
            pushAndConfirm(() => new Screens.Multi.Multiplayer(), "multiplayer");
            exitViaEscapeAndConfirm();
        }

        [Test]
        public void TestExitMultiWithBackButton()
        {
            pushAndConfirm(() => new Screens.Multi.Multiplayer(), "multiplayer");
            exitViaBackButtonAndConfirm();
        }

        [Test]
        public void TestOpenOptionsAndExitWithEscape()
        {
            AddUntilStep("Wait for options to load", () => osuGame.Settings.IsLoaded);
            AddStep("Enter menu", () => pressAndRelease(Key.Enter));
            AddStep("Move mouse to options overlay", () => InputManager.MoveMouseTo(optionsButtonPosition));
            AddStep("Click options overlay", () => InputManager.Click(MouseButton.Left));
            AddAssert("Options overlay was opened", () => osuGame.Settings.State.Value == Visibility.Visible);
            AddStep("Hide options overlay using escape", () => pressAndRelease(Key.Escape));
            AddAssert("Options overlay was closed", () => osuGame.Settings.State.Value == Visibility.Hidden);
        }

        private void pushAndConfirm(Func<Screen> newScreen, string screenName)
        {
            Screen screen = null;
            AddStep($"Push new {screenName}", () => osuGame.ScreenStack.Push(screen = newScreen()));
            AddUntilStep($"Wait for new {screenName}", () => osuGame.ScreenStack.CurrentScreen == screen && screen.IsLoaded);
        }

        private void exitViaEscapeAndConfirm()
        {
            AddStep("Press escape", () => pressAndRelease(Key.Escape));
            AddUntilStep("Wait for main menu", () => osuGame.ScreenStack.CurrentScreen is MainMenu);
        }

        private void exitViaBackButtonAndConfirm()
        {
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(backButtonPosition));
            AddStep("Click back button", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("Wait for main menu", () => osuGame.ScreenStack.CurrentScreen is MainMenu);
        }

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

            protected override Loader CreateLoader() => new TestLoader();

            private DependencyContainer dependencies;

            private DummyAPIAccess dummyAPI;

            protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent) =>
                dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            protected override void LoadComplete()
            {
                base.LoadComplete();
                dependencies.CacheAs<IAPIProvider>(dummyAPI = new DummyAPIAccess());

                dummyAPI.Login("Rhythm Champion", "osu!");
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
