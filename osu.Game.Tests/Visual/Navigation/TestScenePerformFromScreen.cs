// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestScenePerformFromScreen : OsuGameTestScene
    {
        [Test]
        public void TestPerformAtMenu()
        {
            AddAssert("could perform immediately", () =>
            {
                bool actionPerformed = false;
                Game.PerformFromScreen(_ => actionPerformed = true);
                return actionPerformed;
            });
        }

        [Test]
        public void TestPerformAtSongSelect()
        {
            PushAndConfirm(() => new PlaySongSelect());

            AddAssert("could perform immediately", () =>
            {
                bool actionPerformed = false;
                Game.PerformFromScreen(_ => actionPerformed = true, new[] { typeof(PlaySongSelect) });
                return actionPerformed;
            });
        }

        [Test]
        public void TestPerformAtMenuFromSongSelect()
        {
            PushAndConfirm(() => new PlaySongSelect());

            bool actionPerformed = false;
            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true));
            AddUntilStep("returned to menu", () => Game.ScreenStack.CurrentScreen is MainMenu);
            AddAssert("did perform", () => actionPerformed);
        }

        [Test]
        public void TestPerformAtSongSelectFromPlayerLoader()
        {
            PushAndConfirm(() => new PlaySongSelect());
            PushAndConfirm(() => new PlayerLoader(() => new Player()));

            bool actionPerformed = false;
            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true, new[] { typeof(PlaySongSelect) }));
            AddUntilStep("returned to song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);
            AddAssert("did perform", () => actionPerformed);
        }

        [Test]
        public void TestPerformAtMenuFromPlayerLoader()
        {
            PushAndConfirm(() => new PlaySongSelect());
            PushAndConfirm(() => new PlayerLoader(() => new Player()));

            bool actionPerformed = false;
            AddStep("try to perform", () => Game.PerformFromScreen(_ => actionPerformed = true));
            AddUntilStep("returned to song select", () => Game.ScreenStack.CurrentScreen is MainMenu);
            AddAssert("did perform", () => actionPerformed);
        }
    }
}
