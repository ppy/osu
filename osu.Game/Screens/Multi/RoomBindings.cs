// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Users;

namespace osu.Game.Screens.Multi
{
    /// <summary>
    /// Helper class which binds to values from a <see cref="Room"/>.
    /// </summary>
    public class RoomBindings
    {
        public RoomBindings()
        {
            Playlist.ItemsAdded += _ => updatePlaylist();
            Playlist.ItemsRemoved += _ => updatePlaylist();
        }

        private Room room;

        /// <summary>
        /// The <see cref="Room"/> to bind to.
        /// </summary>
        public Room Room
        {
            get => room;
            set
            {
                if (room == value)
                    return;

                if (room != null)
                {
                    RoomID.UnbindFrom(room.RoomID);
                    Name.UnbindFrom(room.Name);
                    Host.UnbindFrom(room.Host);
                    Status.UnbindFrom(room.Status);
                    Type.UnbindFrom(room.Type);
                    Playlist.UnbindFrom(room.Playlist);
                    Participants.UnbindFrom(room.Participants);
                    ParticipantCount.UnbindFrom(room.ParticipantCount);
                    MaxParticipants.UnbindFrom(room.MaxParticipants);
                    EndDate.UnbindFrom(room.EndDate);
                    Availability.UnbindFrom(room.Availability);
                    Duration.UnbindFrom(room.Duration);
                }

                room = value ?? new Room();

                RoomID.BindTo(room.RoomID);
                Name.BindTo(room.Name);
                Host.BindTo(room.Host);
                Status.BindTo(room.Status);
                Type.BindTo(room.Type);
                Playlist.BindTo(room.Playlist);
                Participants.BindTo(room.Participants);
                ParticipantCount.BindTo(room.ParticipantCount);
                MaxParticipants.BindTo(room.MaxParticipants);
                EndDate.BindTo(room.EndDate);
                Availability.BindTo(room.Availability);
                Duration.BindTo(room.Duration);
            }
        }

        private void updatePlaylist()
        {
            // Todo: We only ever have one playlist item for now. In the future, this will be user-settable

            var playlistItem = Playlist.FirstOrDefault();

            currentBeatmap.Value = playlistItem?.Beatmap;
            currentMods.Value = playlistItem?.RequiredMods ?? Enumerable.Empty<Mod>();
            currentRuleset.Value = playlistItem?.Ruleset;
        }

        public readonly Bindable<int?> RoomID = new Bindable<int?>();
        public readonly Bindable<string> Name = new Bindable<string>();
        public readonly Bindable<User> Host = new Bindable<User>();
        public readonly Bindable<RoomStatus> Status = new Bindable<RoomStatus>();
        public readonly Bindable<GameType> Type = new Bindable<GameType>();
        public readonly BindableList<PlaylistItem> Playlist = new BindableList<PlaylistItem>();
        public readonly Bindable<IEnumerable<User>> Participants = new Bindable<IEnumerable<User>>();
        public readonly Bindable<int> ParticipantCount = new Bindable<int>();
        public readonly Bindable<int?> MaxParticipants = new Bindable<int?>();
        public readonly Bindable<DateTimeOffset> EndDate = new Bindable<DateTimeOffset>();
        public readonly Bindable<RoomAvailability> Availability = new Bindable<RoomAvailability>();
        public readonly Bindable<TimeSpan> Duration = new Bindable<TimeSpan>();

        private readonly Bindable<BeatmapInfo> currentBeatmap = new Bindable<BeatmapInfo>();
        public IBindable<BeatmapInfo> CurrentBeatmap => currentBeatmap;

        private readonly Bindable<IEnumerable<Mod>> currentMods = new Bindable<IEnumerable<Mod>>();
        public IBindable<IEnumerable<Mod>> CurrentMods => currentMods;

        private readonly Bindable<RulesetInfo> currentRuleset = new Bindable<RulesetInfo>();
        public IBindable<RulesetInfo> CurrentRuleset => currentRuleset;
    }
}
