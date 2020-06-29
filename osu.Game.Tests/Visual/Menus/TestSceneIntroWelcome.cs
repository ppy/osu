// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Menus
{
    [TestFixture]
    public class TestSceneIntroWelcome : IntroTestScene
    {
        protected override IScreen CreateScreen() => new IntroWelcome();

        public TestSceneIntroWelcome()
        {
            AddAssert("check if menu music loops", () =>
            {
                var menu = IntroStack?.CurrentScreen as MainMenu;

                if (menu == null)
                    return false;

                return menu.Track.Looping;
            });
        }
    }
}
