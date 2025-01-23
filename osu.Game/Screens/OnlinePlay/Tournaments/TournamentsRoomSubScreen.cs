// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Match;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Framework.Localisation;
using osu.Game.Localisation;
using osu.Framework.Logging;
using osu.Framework.Graphics.Containers;
namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    public partial class TournamentsRoomSubScreen : RoomSubScreen
    {

        public TournamentInfo TournamentInfo;

        public override string ShortTitle => "Name of Tornament. Testing long names.";

        public TournamentsRoomSubScreen(Room room, bool allowEdit = true) : base(room, allowEdit)
        {
            TournamentInfo = new TournamentInfo();
        }

        protected override Drawable CreateFooter() => new TournamentsRoomFooter(this);

        protected override Drawable CreateMainContent()
        {
            return new FillFlowContainer
            {

            };
        }

        protected override RoomSettingsOverlay CreateRoomSettingsOverlay(Room room)
        {
            return new TournamentsRoomSettingsOverlay(room)
            {
                EditTournament = () =>
                {
                    if (this.IsCurrentScreen())
                        this.Push(new PlaylistsSongSelect(Room));
                },
            };
        }

        protected override Screen CreateGameplayScreen(PlaylistItem selectedItem)
        {
            return new Screen();
        }

        public void ChangeTab(TournamentsTabs tab)
        {
            Logger.Log($@"Pressed {tab} Button!");
        }

        public static LocalisableString GetTournamentsTabsName(TournamentsTabs tab)
        {
            switch (tab)
            {
                case TournamentsTabs.Info:
                    return TournamentsTabsString.Info;
                case TournamentsTabs.Players:
                    return TournamentsTabsString.Players;
                case TournamentsTabs.Qualifiers:
                    return TournamentsTabsString.Qualifiers;
                case TournamentsTabs.Mappools:
                    return TournamentsTabsString.Mappools;
                case TournamentsTabs.Bracket:
                    return TournamentsTabsString.Bracket;
                case TournamentsTabs.Schedule:
                    return TournamentsTabsString.Schedule;
                case TournamentsTabs.Dangerous:
                    return TournamentsTabsString.Dangerous;
                default:
                    return @"Unsupported Tab!";
            }
        }
    }

    public enum TournamentsTabs
    {
        Info,
        Players,
        Qualifiers,
        Mappools,
        Bracket,
        Schedule,
        Dangerous,
    }
}
