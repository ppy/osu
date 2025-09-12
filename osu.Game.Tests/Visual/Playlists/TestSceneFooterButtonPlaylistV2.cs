// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Cursor;
using osu.Game.Online.Rooms;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Visual.OnlinePlay;

namespace osu.Game.Tests.Visual.Playlists
{
    public partial class TestSceneFooterButtonPlaylistV2 : OnlinePlayTestScene
    {
        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        public TestSceneFooterButtonPlaylistV2()
        {
            Room room = new Room();

            Add(new PopoverContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new FooterButtonPlaylistV2(room)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.CentreLeft,
                    X = -100,
                    CreateNewItem = () => room.Playlist = room.Playlist.Append(new PlaylistItem(CreateAPIBeatmap())).ToArray()
                }
            });
        }
    }
}
