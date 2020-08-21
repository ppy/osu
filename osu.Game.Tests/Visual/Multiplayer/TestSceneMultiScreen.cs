// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Multiplayer
{
    [TestFixture]
    public class TestSceneMultiScreen : ScreenTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private MusicController musicController { get; set; } = new MusicController();

        public TestSceneMultiScreen()
        {
            Screens.Multi.Multiplayer multi = new Screens.Multi.Multiplayer();

            AddStep("show", () => LoadScreen(multi));
            AddUntilStep("wait for loaded", () => multi.IsLoaded);
        }
    }
}
