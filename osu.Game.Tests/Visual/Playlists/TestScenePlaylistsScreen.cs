// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Playlists
{
    [TestFixture]
    public class TestScenePlaylistsScreen : ScreenTestScene
    {
        protected override bool UseOnlineAPI => true;

        [Cached]
        private MusicController musicController { get; set; } = new MusicController();

        public TestScenePlaylistsScreen()
        {
            var multi = new Screens.OnlinePlay.Playlists.Playlists();

            AddStep("show", () => LoadScreen(multi));
            AddUntilStep("wait for loaded", () => multi.IsLoaded);
        }
    }
}
