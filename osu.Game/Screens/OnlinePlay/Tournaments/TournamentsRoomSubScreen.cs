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
using osu.Game.Graphics;
using osu.Framework.Allocation;
using osu.Game.Screens.OnlinePlay.Tournaments.Tabs;
using osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Results;
using osu.Framework.Graphics.Colour;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Graphics.Cursor;
using osu.Game.Online.API.Requests;
using System;
using osu.Game.Screens.OnlinePlay.Tournaments.Models;

namespace osu.Game.Screens.OnlinePlay.Tournaments
{
    public partial class TournamentsRoomSubScreen : RoomSubScreen
    {

        // Colours taken from TournamentGame
        public static ColourInfo GetTeamColour(TeamColour teamColour) => teamColour == TeamColour.Red ? COLOUR_RED : COLOUR_BLUE;

        public static readonly Color4 COLOUR_RED = new OsuColour().TeamColourRed;
        public static readonly Color4 COLOUR_BLUE = new OsuColour().TeamColourBlue;

        public static readonly Color4 ELEMENT_BACKGROUND_COLOUR = Color4Extensions.FromHex("#fff");
        public static readonly Color4 ELEMENT_FOREGROUND_COLOUR = Color4Extensions.FromHex("#000");

        public static readonly Color4 TEXT_COLOUR = Color4Extensions.FromHex("#fff");

        [Cached]
        private TournamentInfo tournamentInfo { get; set; } = new();

        public override string ShortTitle => "Name of Tornament. Testing long names.";

        public TournamentsRoomSubScreen(Room room, bool allowEdit = true) : base(room, allowEdit)
        {
        }

        public Container MainContent = null!;
        public Drawable? InfoTab;
        public Drawable? PlayersTab;
        public Drawable? QualifiersTab;
        public Drawable? MappoolsTab;
        public Drawable ResultsTab = null!;
        public Drawable? ScheduleTab;
        public Drawable? SettingsTab;
        public Drawable? CurrentTab;

        // todo : Selected tab will have different color.
        public TournamentsTabs CurrentTabType = TournamentsTabs.Info;

        protected override void LoadComplete()
        {
            foreach (var team in tournamentInfo.Teams)
            {
                foreach (var player in team.Players)
                {
                    PopulatePlayer(player);
                }
            }
        }

        protected override Drawable CreateFooter() => new TournamentsRoomFooter(this);

        protected override Drawable CreateMainContent()
        {
            MainContent = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Children = new Drawable[]
                {

                    ResultsTab = new TournamentsResultsTab()
                }
            };
            foreach (var tab in MainContent)
            {
                if (tab is TournamentsTabBase) tab.Hide();
            }

            return MainContent;
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
            if (tab == CurrentTabType)
                return;

            CurrentTab?.Hide();
            switch (tab)
            {
                case TournamentsTabs.Info:
                    break;

                case TournamentsTabs.Players:
                    break;

                case TournamentsTabs.Qualifiers:
                    break;

                case TournamentsTabs.Mappools:
                    break;

                case TournamentsTabs.Results:
                    ResultsTab.Show();
                    CurrentTab = ResultsTab;
                    break;

                case TournamentsTabs.Schedule:
                    break;

                case TournamentsTabs.Settings:
                    break;

                default:
                    break;
            }

            CurrentTabType = tab;
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

                case TournamentsTabs.Results:
                    return TournamentsTabsString.Results;

                case TournamentsTabs.Schedule:
                    return TournamentsTabsString.Schedule;

                case TournamentsTabs.Settings:
                    return TournamentsTabsString.Settings;

                default:
                    return @"Unsupported Tab!";
            }
        }

        public void PopulatePlayer(TournamentUser user, Action? success = null, Action? failure = null, bool immediate = false)
        {
            var req = new GetUserRequest(user.OnlineID, null);
            // var req = new GetUserRequest(user.OnlineID, ladder.Ruleset.Value);

            if (immediate)
            {
                API.Perform(req);
                populate();
            }
            else
            {
                req.Success += _ => { populate(); };
                req.Failure += _ =>
                {
                    user.OnlineID = 1;
                    failure?.Invoke();
                };

                API.Queue(req);
            }

            void populate()
            {
                var res = req.Response;

                if (res == null)
                    return;

                user.OnlineID = res.Id;

                user.Username = res.Username;
                user.CoverUrl = res.CoverUrl;
                user.CountryCode = res.CountryCode;
                user.Rank = res.Statistics?.GlobalRank;

                success?.Invoke();
            }
        }
    }

    public enum TournamentsTabs
    {
        Info,
        Players,
        Qualifiers,
        Mappools,
        Results,
        Schedule,
        Settings,
    }
}
