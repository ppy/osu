// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Lounge;

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    public partial class Tournaments : OnlinePlayScreen
    {
        protected override string ScreenTitle => "Tournaments";

        protected override LoungeSubScreen CreateLounge() => new TournamentsLoungeSubScreen();

        public void Join(Room room, string password = "") => Schedule(() => Lounge.Join(room, password));
    }
}
