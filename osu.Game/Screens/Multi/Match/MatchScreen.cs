// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Mods;
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

        public override string Title => room.Name.Value;

        public override string ShortTitle => "room";

        private readonly Components.Header header;
        private readonly Info info;

        [Cached]
        private readonly Bindable<IEnumerable<Mod>> mods = new Bindable<IEnumerable<Mod>>(Enumerable.Empty<Mod>());

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
                header = new Components.Header
                {
                    Depth = -1,
                },
                info = new Info
                {
                    Margin = new MarginPadding { Top = Components.Header.HEIGHT },
                    OnStart = onStart
                },
                participants = new Participants
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Components.Header.HEIGHT + Info.HEIGHT },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Top = Components.Header.HEIGHT },
                    Child = settings = new RoomSettingsOverlay
                    {
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.9f,
                    },
                },
            };

            header.OnRequestSelectBeatmap = () => Push(new MatchSongSelect());

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
            info.Mods.BindTo(mods);

            participants.Users.BindTo(participantsBind);
            participants.MaxParticipants.BindTo(maxParticipantsBind);

            playlistBind.ItemsAdded += _ => updatePlaylist();
            playlistBind.ItemsRemoved += _ => updatePlaylist();
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Beatmap.BindValueChanged(b =>
            {
                playlistBind.Clear();

                var newItem = new PlaylistItem
                {
                    Beatmap = b.BeatmapInfo,
                    Ruleset = Ruleset.Value
                };

                newItem.RequiredMods.Clear();
                newItem.RequiredMods.AddRange(b.Mods.Value);

                playlistBind.Add(newItem);
            });

            playlistBind.BindTo(room.Playlist);
        }

        private void updatePlaylist()
        {
            if (playlistBind.Count == 0)
                return;

            // For now, only the first playlist item is supported
            var item = playlistBind.First();

            header.Beatmap.Value = item.Beatmap;
            info.Beatmap.Value = item.Beatmap;

            if (Beatmap.Value?.BeatmapInfo != item.Beatmap)
            {
                Beatmap.Value = beatmapManager.GetWorkingBeatmap(item.Beatmap);
                Beatmap.Value.Mods.Value = item.RequiredMods.ToArray();
            }
        }

        private void onStart()
        {
            switch (typeBind.Value)
            {
                default:
                case GameTypeTimeshift _:
                    multiplayer.Push(new PlayerLoader(new TimeshiftPlayer()));
                    break;
            }
        }
    }
}
