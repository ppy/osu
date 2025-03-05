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
using osu.Game.Screens.OnlinePlay.Tournaments.Tabs.Players;
using osu.Framework.Bindables;

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

        private readonly Bindable<TournamentsTab> currentTabType = new();
        private TournamentsTabBase? currentTab;

        public override string ShortTitle => "Name of Tornament. Testing long names.";

        public TournamentsRoomSubScreen(Room room, bool allowEdit = true) : base(room, allowEdit)
        {
        }

        public Container<TournamentsTabBase> MainContent = null!;

        protected override void LoadComplete()
        {
            currentTabType.BindTo(tournamentInfo.CurrentTabType);
            currentTabType.BindValueChanged((tab) => ChangeTab(tab.NewValue), true);

            foreach (var team in tournamentInfo.Teams)
            {
                foreach (var player in team.Players)
                {
                    PopulatePlayer(player);
                }
            }
        }

        protected override Drawable CreateFooter() => new TournamentsRoomFooter();

        protected override Drawable CreateMainContent()
        {
            return new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Child = MainContent = new()
                {
                    RelativeSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.Centre,
                    Children = [
                        new TournamentsPlayersTab(),
                        new TournamentsResultsTab()
                    ]
                }
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

        public void ChangeTab(TournamentsTab tab)
        {
            Logger.Log($@"Pressed {tab} Button!");
            if (tab == currentTab?.TabType)
                return;

            currentTab?.Hide();
            foreach (TournamentsTabBase tabScreen in MainContent)
                if (tabScreen.TabType == tab)
                    currentTab = tabScreen;

            currentTab?.Show();
            currentTabType.Value = tab;
        }

        public static LocalisableString GetTournamentsTabsName(TournamentsTab tab)
        {
            switch (tab)
            {
                case TournamentsTab.Info:
                    return TournamentsTabsString.Info;

                case TournamentsTab.Players:
                    return TournamentsTabsString.Players;

                case TournamentsTab.Qualifiers:
                    return TournamentsTabsString.Qualifiers;

                case TournamentsTab.Mappools:
                    return TournamentsTabsString.Mappools;

                case TournamentsTab.Results:
                    return TournamentsTabsString.Results;

                case TournamentsTab.Schedule:
                    return TournamentsTabsString.Schedule;

                case TournamentsTab.Settings:
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

    // todo : don't know where to put these, they can just stay here for now.
    public enum TournamentsTab
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
