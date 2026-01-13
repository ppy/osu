// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestScenePlaylistTray : OnlinePlayTestScene
    {
        private Room room = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add tray", () => Child = new PlaylistsSongSelectV2.PlaylistTray(room = new Room())
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre
            });
        }

        [Test]
        public void TestAddItem()
        {
            AddStep("add playlist item", () =>
            {
                room.Playlist = room.Playlist.Append(new PlaylistItem(CreateAPIBeatmap())).ToArray();
            });
        }
    }
}
