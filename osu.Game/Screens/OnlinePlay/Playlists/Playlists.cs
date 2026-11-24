// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay.Playlists
{
    public partial class Playlists : OnlinePlayScreen
    {
        public override LocalisableString Title => OnlinePlayStrings.PlaylistsScreenTitle;

        protected override LoungeSubScreen CreateLounge() => new PlaylistsLoungeSubScreen();

        public void Join(Room room) => Schedule(() => Lounge.Join(room, string.Empty));
    }
}
