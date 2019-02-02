// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Configuration;
using osu.Game.Online.Multiplayer.GameTypes;
using osu.Game.Online.Multiplayer.RoomStatuses;
using osu.Game.Users;

namespace osu.Game.Online.Multiplayer
{
    public class Room
    {
        [JsonProperty("id")]
        public Bindable<int?> RoomID { get; private set; } = new Bindable<int?>();

        [JsonProperty("name")]
        public Bindable<string> Name { get; private set; } = new Bindable<string>();

        [JsonProperty("host")]
        public Bindable<User> Host { get; private set; } = new Bindable<User>();

        [JsonProperty("playlist")]
        public BindableList<PlaylistItem> Playlist { get; set; } = new BindableList<PlaylistItem>();

        [JsonProperty("channel_id")]
        public Bindable<int> ChannelId { get; private set; } = new Bindable<int>();

        [JsonIgnore]
        public Bindable<TimeSpan> Duration { get; private set; } = new Bindable<TimeSpan>(TimeSpan.FromMinutes(30));

        [JsonIgnore]
        public Bindable<int?> MaxAttempts { get; private set; } = new Bindable<int?>();

        [JsonIgnore]
        public Bindable<RoomStatus> Status { get; private set; } = new Bindable<RoomStatus>(new RoomStatusOpen());

        [JsonIgnore]
        public Bindable<RoomAvailability> Availability { get; private set; } = new Bindable<RoomAvailability>();

        [JsonIgnore]
        public Bindable<GameType> Type { get; private set; } = new Bindable<GameType>(new GameTypeTimeshift());

        [JsonIgnore]
        public Bindable<int?> MaxParticipants { get; private set; } = new Bindable<int?>();

        [JsonIgnore]
        public Bindable<IEnumerable<User>> Participants { get; private set; } = new Bindable<IEnumerable<User>>(Enumerable.Empty<User>());

        public Bindable<int> ParticipantCount { get; private set; } = new Bindable<int>();

        // todo: TEMPORARY
        [JsonProperty("participant_count")]
        private int? participantCount
        {
            get => ParticipantCount;
            set => ParticipantCount.Value = value ?? 0;
        }

        [JsonProperty("duration")]
        private int duration
        {
            get => (int)Duration.Value.TotalMinutes;
            set => Duration.Value = TimeSpan.FromMinutes(value);
        }

        // Only supports retrieval for now
        [JsonProperty("ends_at")]
        public Bindable<DateTimeOffset> EndDate { get; private set; } = new Bindable<DateTimeOffset>();

        // Todo: Find a better way to do this (https://github.com/ppy/osu-framework/issues/1930)
        [JsonProperty("max_attempts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? maxAttempts
        {
            get => MaxAttempts;
            set => MaxAttempts.Value = value;
        }

        /// <summary>
        /// The position of this <see cref="Room"/> in the list. This is not read from or written to the API.
        /// </summary>
        [JsonIgnore]
        public int Position = -1;

        public void CopyFrom(Room other)
        {
            RoomID.Value = other.RoomID;
            Name.Value = other.Name;

            if (other.Host.Value != null && Host.Value?.Id != other.Host.Value.Id)
                Host.Value = other.Host;

            Status.Value = other.Status;
            Availability.Value = other.Availability;
            Type.Value = other.Type;
            MaxParticipants.Value = other.MaxParticipants;
            ParticipantCount.Value = other.ParticipantCount.Value;
            Participants.Value = other.Participants.Value.ToArray();
            EndDate.Value = other.EndDate;

            if (DateTimeOffset.Now >= EndDate.Value)
                Status.Value = new RoomStatusEnded();

            // Todo: Temporary, should only remove/add new items (requires framework changes)
            if (Playlist.Count == 0)
                Playlist.AddRange(other.Playlist);
            else if (other.Playlist.Count > 0)
                Playlist.First().ID = other.Playlist.First().ID;

            Position = other.Position;
        }

        public bool ShouldSerializeRoomID() => false;
        public bool ShouldSerializeHost() => false;
        public bool ShouldSerializeEndDate() => false;
    }
}
