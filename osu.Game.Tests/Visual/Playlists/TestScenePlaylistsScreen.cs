// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;

namespace osu.Game.Tests.Visual.Playlists
{
    [TestFixture]
    public partial class TestScenePlaylistsScreen : ScreenTestScene
    {
        protected override bool UseOnlineAPI => true;

        public TestScenePlaylistsScreen()
        {
            var multi = new Screens.OnlinePlay.Playlists.Playlists();

            AddStep("show", () => LoadScreen(multi));
            AddUntilStep("wait for loaded", () => multi.IsLoaded);
        }
    }
}
