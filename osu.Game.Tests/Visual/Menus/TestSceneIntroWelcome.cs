// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Audio.Track;
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
            AddUntilStep("wait for load", () => getTrack() != null);

            AddAssert("check if menu music loops", () => getTrack().Looping);
        }

        private Track getTrack() => (IntroStack?.CurrentScreen as MainMenu)?.Track;
    }
}
