// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Screens;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Tests.Beatmaps.IO;
using osuTK.Input;
using static osu.Game.Tests.Visual.Navigation.TestSceneScreenNavigation;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestScenePerformFromScreen : OsuGameTestScene
    {
        private bool actionPerformed;

        public override void SetUpSteps()
        {
            AddStep("reset status", () => actionPerformed = false);

            base.SetUpSteps();
        }

        [Test]
        public void TestPerformAtMenu()
        {
            AddStep("perform immediately", () => Game.PerformFromScreen(_ => actionPerformed = true));
            AddAssert("did perform", () => actionPerformed);
        }

        [Test]
        public void TestPerformAtSongSelect()
        {
            PushAndConfirm(() => new TestPlaySongSelect());

            AddStep("perform immediately", () => Game.PerformFromScreen(_ => actionPerformed = true, new[] { typeof(TestPlaySongSelect) }));
            AddAssert("did perform", () => actionPerformed);
            AddAssert("screen didn't change", () => Game.ScreenStack.CurrentScreen is TestPlaySongSelect);
        }

        [Test]
        public void TestPerformAtMenuFromSongSelect()
        {
            PushAndConfirm(() => new TestPlaySongSelect());

            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true));
            AddUntilStep("returned to menu", () => Game.ScreenStack.CurrentScreen is MainMenu);
            AddAssert("did perform", () => actionPerformed);
        }

        [Test]
        public void TestPerformAtSongSelectFromPlayerLoader()
        {
            importAndWaitForSongSelect();

            AddStep("Press enter", () => InputManager.Key(Key.Enter));
            AddUntilStep("Wait for new screen", () => Game.ScreenStack.CurrentScreen is PlayerLoader);

            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true, new[] { typeof(TestPlaySongSelect) }));
            AddUntilStep("returned to song select", () => Game.ScreenStack.CurrentScreen is TestPlaySongSelect);
            AddAssert("did perform", () => actionPerformed);
        }

        [Test]
        public void TestPerformAtMenuFromPlayerLoader()
        {
            importAndWaitForSongSelect();

            AddStep("Press enter", () => InputManager.Key(Key.Enter));
            AddUntilStep("Wait for new screen", () => Game.ScreenStack.CurrentScreen is PlayerLoader);

            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true));
            AddUntilStep("returned to song select", () => Game.ScreenStack.CurrentScreen is MainMenu);
            AddAssert("did perform", () => actionPerformed);
        }

        [Test]
        public void TestOverlaysAlwaysClosed()
        {
            ChatOverlay chat = null;
            AddUntilStep("is at menu", () => Game.ScreenStack.CurrentScreen is MainMenu);
            AddUntilStep("wait for chat load", () => (chat = Game.ChildrenOfType<ChatOverlay>().SingleOrDefault()) != null);

            AddStep("show chat", () => InputManager.Key(Key.F8));

            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true));

            AddUntilStep("still at menu", () => Game.ScreenStack.CurrentScreen is MainMenu);
            AddAssert("did perform", () => actionPerformed);
            AddAssert("chat closed", () => chat.State.Value == Visibility.Hidden);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestPerformBlockedByDialog(bool confirmed)
        {
            DialogBlockingScreen blocker = null;

            PushAndConfirm(() => blocker = new DialogBlockingScreen());
            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true));

            AddWaitStep("wait a bit", 10);

            AddAssert("screen didn't change", () => Game.ScreenStack.CurrentScreen is DialogBlockingScreen);
            AddAssert("did not perform", () => !actionPerformed);
            AddAssert("only one exit attempt", () => blocker.ExitAttempts == 1);

            AddUntilStep("wait for dialog display", () => Game.Dependencies.Get<DialogOverlay>().IsLoaded);

            if (confirmed)
            {
                AddStep("accept dialog", () => InputManager.Key(Key.Number1));
                AddUntilStep("wait for dialog dismissed", () => Game.Dependencies.Get<DialogOverlay>().CurrentDialog == null);
                AddUntilStep("did perform", () => actionPerformed);
            }
            else
            {
                AddStep("cancel dialog", () => InputManager.Key(Key.Number2));
                AddAssert("screen didn't change", () => Game.ScreenStack.CurrentScreen is DialogBlockingScreen);
                AddAssert("did not perform", () => !actionPerformed);
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestPerformBlockedByDialogNested(bool confirmSecond)
        {
            DialogBlockingScreen blocker = null;
            DialogBlockingScreen blocker2 = null;

            PushAndConfirm(() => blocker = new DialogBlockingScreen());
            PushAndConfirm(() => blocker2 = new DialogBlockingScreen());

            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true));

            AddUntilStep("wait for dialog", () => blocker2.ExitAttempts == 1);

            AddWaitStep("wait a bit", 10);

            AddUntilStep("wait for dialog display", () => Game.Dependencies.Get<DialogOverlay>().IsLoaded);

            AddAssert("screen didn't change", () => Game.ScreenStack.CurrentScreen == blocker2);
            AddAssert("did not perform", () => !actionPerformed);
            AddAssert("only one exit attempt", () => blocker2.ExitAttempts == 1);

            AddStep("accept dialog", () => InputManager.Key(Key.Number1));
            AddUntilStep("screen changed", () => Game.ScreenStack.CurrentScreen == blocker);

            AddUntilStep("wait for second dialog", () => blocker.ExitAttempts == 1);
            AddAssert("did not perform", () => !actionPerformed);
            AddAssert("only one exit attempt", () => blocker.ExitAttempts == 1);

            if (confirmSecond)
            {
                AddStep("accept dialog", () => InputManager.Key(Key.Number1));
                AddUntilStep("did perform", () => actionPerformed);
            }
            else
            {
                AddStep("cancel dialog", () => InputManager.Key(Key.Number2));
                AddAssert("screen didn't change", () => Game.ScreenStack.CurrentScreen == blocker);
                AddAssert("did not perform", () => !actionPerformed);
            }
        }

        private void importAndWaitForSongSelect()
        {
            AddStep("import beatmap", () => ImportBeatmapTest.LoadQuickOszIntoOsu(Game).Wait());
            PushAndConfirm(() => new TestPlaySongSelect());
            AddUntilStep("beatmap updated", () => Game.Beatmap.Value.BeatmapSetInfo.OnlineID == 241526);
        }

        public class DialogBlockingScreen : OsuScreen
        {
            [Resolved]
            private DialogOverlay dialogOverlay { get; set; }

            private int dialogDisplayCount;

            public int ExitAttempts { get; private set; }

            public override bool OnExiting(IScreen next)
            {
                ExitAttempts++;

                if (dialogDisplayCount++ < 1)
                {
                    dialogOverlay.Push(new ConfirmExitDialog(this.Exit, () => { }));
                    return true;
                }

                return base.OnExiting(next);
            }
        }
    }
}
