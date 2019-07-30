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
using osu.Game.Overlays.Mods;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Menus
{
    public class TestSceneExitingScreens : ManualInputManagerTestScene
    {
        private readonly TestOsuGame osuGame = new TestOsuGame();

        [BackgroundDependencyLoader]
        private void load(GameHost gameHost)
        {
            osuGame.SetHost(gameHost);

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                },
                osuGame
            };
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddUntilStep("wait for load", () => osuGame.IsLoaded);
            AddUntilStep("exit to main menu", () =>
            {
                var current = osuGame.ScreenStack.CurrentScreen;

                switch (current)
                {
                    case null:
                    case Intro _:
                    case Disclaimer _:
                        return false;

                    case MainMenu _:
                        return true;

                    default:
                        current.Exit();
                        return false;
                }
            });
        }

        [Test]
        public void TestExitingSongSelectWithEscape()
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
        public void TestExitingSongSelectWithClick()
        {
            TestSongSelect songSelect = null;

            pushAndConfirm(() => songSelect = new TestSongSelect(), "song select");
            AddStep("Show mods overlay", () => songSelect.ModSelectOverlay.Show());
            AddAssert("Overlay was shown", () => songSelect.ModSelectOverlay.State.Value == Visibility.Visible);
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(osuGame.BackButton));

            // BackButton handles hover using its child button, so this checks whether or not any of BackButton's children are hovered.
            AddUntilStep("Back button is hovered", () => InputManager.HoveredDrawables.Any(d => d.Parent == osuGame.BackButton));

            AddStep("Click back button", () => InputManager.Click(MouseButton.Left));
            AddAssert("Overlay was hidden", () => songSelect.ModSelectOverlay.State.Value == Visibility.Hidden);
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

        private void pushAndConfirm(Func<Screen> newScreen, string screenName)
        {
            Screen screen = null;
            AddStep($"Push new {screenName}", () => osuGame.ScreenStack.Push(screen = newScreen()));
            AddUntilStep($"Wait for new {screenName}", () => screen.IsCurrentScreen());
        }

        private void exitViaEscapeAndConfirm()
        {
            AddStep("Press escape", () => pressAndRelease(Key.Escape));
            AddUntilStep("Wait for main menu", () => osuGame.ScreenStack.CurrentScreen is MainMenu);
        }

        private void exitViaBackButtonAndConfirm()
        {
            AddStep("Move mouse to backButton", () => InputManager.MoveMouseTo(osuGame.BackButton));
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
        }

        private class TestSongSelect : PlaySongSelect
        {
            public ModSelectOverlay ModSelectOverlay => ModSelect;
        }
    }
}
