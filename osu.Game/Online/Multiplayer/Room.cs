// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.Multiplayer.GameTypes;
using osu.Game.Online.Multiplayer.RoomStatuses;
using osu.Game.Users;

namespace osu.Game.Online.Multiplayer
{
    public class Room
    {
        [Cached]
        [JsonProperty("id")]
        public Bindable<int?> RoomID { get; private set; } = new Bindable<int?>();

        [Cached]
        [JsonProperty("name")]
        public Bindable<string> Name { get; private set; } = new Bindable<string>();

        [Cached]
        [JsonProperty("host")]
        public Bindable<User> Host { get; private set; } = new Bindable<User>();

        [Cached]
        [JsonProperty("playlist")]
        public BindableList<PlaylistItem> Playlist { get; private set; } = new BindableList<PlaylistItem>();

        [Cached]
        [JsonProperty("channel_id")]
        public Bindable<int> ChannelId { get; private set; } = new Bindable<int>();

        [Cached]
        [JsonIgnore]
        public Bindable<TimeSpan> Duration { get; private set; } = new Bindable<TimeSpan>(TimeSpan.FromMinutes(30));

        [Cached]
        [JsonIgnore]
        public Bindable<int?> MaxAttempts { get; private set; } = new Bindable<int?>();

        [Cached]
        [JsonIgnore]
        public Bindable<RoomStatus> Status { get; private set; } = new Bindable<RoomStatus>(new RoomStatusOpen());

        [Cached]
        [JsonIgnore]
        public Bindable<RoomAvailability> Availability { get; private set; } = new Bindable<RoomAvailability>();

        [Cached]
        [JsonIgnore]
        public Bindable<GameType> Type { get; private set; } = new Bindable<GameType>(new GameTypeTimeshift());

        [Cached]
        [JsonIgnore]
        public Bindable<int?> MaxParticipants { get; private set; } = new Bindable<int?>();

        [Cached]
        [JsonIgnore]
        public BindableList<User> Participants { get; private set; } = new BindableList<User>();

        [Cached]
        public Bindable<int> ParticipantCount { get; private set; } = new Bindable<int>();

        // todo: TEMPORARY
        [JsonProperty("participant_count")]
        private int? participantCount
        {
            get => ParticipantCount.Value;
            set => ParticipantCount.Value = value ?? 0;
        }

        [JsonProperty("duration")]
        private int duration
        {
            get => (int)Duration.Value.TotalMinutes;
            set => Duration.Value = TimeSpan.FromMinutes(value);
        }

        // Only supports retrieval for now
        [Cached]
        [JsonProperty("ends_at")]
        public Bindable<DateTimeOffset> EndDate { get; private set; } = new Bindable<DateTimeOffset>();

        // Todo: Find a better way to do this (https://github.com/ppy/osu-framework/issues/1930)
        [JsonProperty("max_attempts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? maxAttempts
        {
            get => MaxAttempts.Value;
            set => MaxAttempts.Value = value;
        }

        /// <summary>
        /// The position of this <see cref="Room"/> in the list. This is not read from or written to the API.
        /// </summary>
        [JsonIgnore]
        public Bindable<int> Position { get; private set; } = new Bindable<int>(-1);

        public void CopyFrom(Room other)
        {
            RoomID.Value = other.RoomID.Value;
            Name.Value = other.Name.Value;

            if (other.Host.Value != null && Host.Value?.Id != other.Host.Value.Id)
                Host.Value = other.Host.Value;

            ChannelId.Value = other.ChannelId.Value;
            Status.Value = other.Status.Value;
            Availability.Value = other.Availability.Value;
            Type.Value = other.Type.Value;
            MaxParticipants.Value = other.MaxParticipants.Value;
            ParticipantCount.Value = other.ParticipantCount.Value;
            EndDate.Value = other.EndDate.Value;

            if (DateTimeOffset.Now >= EndDate.Value)
                Status.Value = new RoomStatusEnded();

            foreach (var removedItem in Playlist.Except(other.Playlist).ToArray())
                Playlist.Remove(removedItem);
            Playlist.AddRange(other.Playlist.Except(Playlist).ToArray());

            foreach (var removedItem in Participants.Except(other.Participants).ToArray())
                Participants.Remove(removedItem);
            Participants.AddRange(other.Participants.Except(Participants).ToArray());

            Position = other.Position;
        }

        public bool ShouldSerializeRoomID() => false;
        public bool ShouldSerializeHost() => false;
        public bool ShouldSerializeEndDate() => false;
    }
}
