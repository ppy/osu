// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Screens.Multi.Match.Components;
using osu.Game.Screens.Multi.Play;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select;
using osu.Game.Users;

namespace osu.Game.Screens.Multi.Match
{
    public class MatchScreen : MultiplayerScreen
    {
        private readonly Participants participants;

        private readonly Bindable<string> nameBind = new Bindable<string>();
        private readonly Bindable<RoomStatus> statusBind = new Bindable<RoomStatus>();
        private readonly Bindable<RoomAvailability> availabilityBind = new Bindable<RoomAvailability>();
        private readonly Bindable<GameType> typeBind = new Bindable<GameType>();
        private readonly Bindable<int?> maxParticipantsBind = new Bindable<int?>();
        private readonly Bindable<IEnumerable<User>> participantsBind = new Bindable<IEnumerable<User>>();
        private readonly BindableCollection<PlaylistItem> playlistBind = new BindableCollection<PlaylistItem>();

        protected override Drawable TransitionContent => participants;

        public override bool AllowBeatmapRulesetChange => allowBeatmapRulesetChange;
        private bool allowBeatmapRulesetChange;

        public override string Title => room.Name.Value;

        public override string ShortTitle => "room";

        private readonly Components.Header header;
        private readonly Info info;
        private readonly MatchLeaderboard leaderboard;

        [Cached]
        private readonly Room room;

        [Resolved]
        private Multiplayer multiplayer { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved]
        private APIAccess api { get; set; }

        public MatchScreen(Room room)
        {
            this.room = room;

            nameBind.BindTo(room.Name);
            statusBind.BindTo(room.Status);
            availabilityBind.BindTo(room.Availability);
            typeBind.BindTo(room.Type);
            participantsBind.BindTo(room.Participants);
            maxParticipantsBind.BindTo(room.MaxParticipants);

            RoomSettingsOverlay settings;

            Children = new Drawable[]
            {
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[] { header = new Components.Header { Depth = -1 } },
                        new Drawable[] { info = new Info { OnStart = onStart } },
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        participants = new Participants { RelativeSizeAxes = Axes.Both },
                                        leaderboard = new MatchLeaderboard(room) { RelativeSizeAxes = Axes.Both }
                                    },
                                },
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.Distributed),
                                    new Dimension(GridSizeMode.Relative, 0.5f),
                                }
                            }
                        },
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Distributed),
                    }
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Components.Header.HEIGHT },
                    Child = settings = new RoomSettingsOverlay(room)
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.9f,
                    },
                },
            };

            header.OnRequestSelectBeatmap = () => Push(new MatchSongSelect { Selected = addPlaylistItem });

            header.Tabs.Current.ValueChanged += t =>
            {
                if (t is SettingsMatchPage)
                    settings.Show();
                else
                    settings.Hide();
            };

            info.Name.BindTo(nameBind);
            info.Status.BindTo(statusBind);
            info.Availability.BindTo(availabilityBind);
            info.Type.BindTo(typeBind);

            participants.Users.BindTo(participantsBind);
            participants.MaxParticipants.BindTo(maxParticipantsBind);

            playlistBind.ItemsAdded += _ => setFromPlaylist();
            playlistBind.ItemsRemoved += _ => setFromPlaylist();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            playlistBind.BindTo(room.Playlist);
        }

        protected override void OnResuming(Screen last)
        {
            base.OnResuming(last);

            leaderboard.RefreshScores();
        }

        private void addPlaylistItem(PlaylistItem item)
        {
            playlistBind.Clear();
            playlistBind.Add(item);
        }

        private void setFromPlaylist()
        {
            if (playlistBind.Count == 0)
                return;

            // For now, only the first playlist item is supported
            var item = playlistBind.First();

            header.Beatmap.Value = item.Beatmap;
            info.Beatmap.Value = item.Beatmap;
            info.Mods.Value = item.RequiredMods;

            allowBeatmapRulesetChange = true;

            // Todo: item.Beatmap can be null here...
            var localBeatmap = beatmapManager.QueryBeatmap(b => b.OnlineBeatmapID == item.BeatmapID) ?? item.Beatmap;

            Schedule(() =>
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(localBeatmap);
                Beatmap.Value.Mods.Value = item.RequiredMods.ToArray();
                Ruleset.Value = item.Ruleset;

                allowBeatmapRulesetChange = false;
            });
        }

        private void onStart()
        {
            switch (typeBind.Value)
            {
                default:
                case GameTypeTimeshift _:
                    multiplayer.Push(new PlayerLoader(new TimeshiftPlayer(room.RoomID.Value ?? 0, room.Playlist.First().ID)));
                    break;
            }
        }
    }
}
