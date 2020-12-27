// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.IO.Serialization.Converters;
using osu.Game.Online.Rooms.GameTypes;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Users;

namespace osu.Game.Online.Rooms
{
    public class Room
    {
        [Cached]
        [JsonProperty("id")]
        public readonly Bindable<int?> RoomID = new Bindable<int?>();

        [Cached]
        [JsonProperty("name")]
        public readonly Bindable<string> Name = new Bindable<string>();

        [Cached]
        [JsonProperty("host")]
        public readonly Bindable<User> Host = new Bindable<User>();

        [Cached]
        [JsonProperty("playlist")]
        public readonly BindableList<PlaylistItem> Playlist = new BindableList<PlaylistItem>();

        [Cached]
        [JsonProperty("channel_id")]
        public readonly Bindable<int> ChannelId = new Bindable<int>();

        [Cached]
        [JsonIgnore]
        public readonly Bindable<RoomCategory> Category = new Bindable<RoomCategory>();

        // Todo: osu-framework bug (https://github.com/ppy/osu-framework/issues/4106)
        [JsonProperty("category")]
        [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
        private RoomCategory category
        {
            get => Category.Value;
            set => Category.Value = value;
        }

        [Cached]
        [JsonIgnore]
        public readonly Bindable<TimeSpan?> Duration = new Bindable<TimeSpan?>();

        [Cached]
        [JsonIgnore]
        public readonly Bindable<int?> MaxAttempts = new Bindable<int?>();

        [Cached]
        [JsonIgnore]
        public readonly Bindable<RoomStatus> Status = new Bindable<RoomStatus>(new RoomStatusOpen());

        [Cached]
        [JsonIgnore]
        public readonly Bindable<RoomAvailability> Availability = new Bindable<RoomAvailability>();

        [Cached]
        [JsonIgnore]
        public readonly Bindable<GameType> Type = new Bindable<GameType>(new GameTypePlaylists());

        [Cached]
        [JsonIgnore]
        public readonly Bindable<int?> MaxParticipants = new Bindable<int?>();

        [Cached]
        [JsonProperty("recent_participants")]
        public readonly BindableList<User> RecentParticipants = new BindableList<User>();

        [Cached]
        [JsonProperty("participant_count")]
        public readonly Bindable<int> ParticipantCount = new Bindable<int>();

        [JsonProperty("duration")]
        private int? duration
        {
            get => (int?)Duration.Value?.TotalMinutes;
            set
            {
                if (value == null)
                    Duration.Value = null;
                else
                    Duration.Value = TimeSpan.FromMinutes(value.Value);
            }
        }

        // Only supports retrieval for now
        [Cached]
        [JsonProperty("ends_at")]
        public readonly Bindable<DateTimeOffset?> EndDate = new Bindable<DateTimeOffset?>();

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
        public readonly Bindable<int> Position = new Bindable<int>(-1);

        /// <summary>
        /// Create a copy of this room without online information.
        /// Should be used to create a local copy of a room for submitting in the future.
        /// </summary>
        public Room CreateCopy()
        {
            var copy = new Room();

            copy.CopyFrom(this);
            copy.RoomID.Value = null;

            return copy;
        }

        public void CopyFrom(Room other)
        {
            RoomID.Value = other.RoomID.Value;
            Name.Value = other.Name.Value;

            if (other.Category.Value != RoomCategory.Spotlight)
                Category.Value = other.Category.Value;

            if (other.Host.Value != null && Host.Value?.Id != other.Host.Value.Id)
                Host.Value = other.Host.Value;

            ChannelId.Value = other.ChannelId.Value;
            Status.Value = other.Status.Value;
            Availability.Value = other.Availability.Value;
            Type.Value = other.Type.Value;
            MaxParticipants.Value = other.MaxParticipants.Value;
            ParticipantCount.Value = other.ParticipantCount.Value;
            EndDate.Value = other.EndDate.Value;

            if (EndDate.Value != null && DateTimeOffset.Now >= EndDate.Value)
                Status.Value = new RoomStatusEnded();

            if (!Playlist.SequenceEqual(other.Playlist))
            {
                Playlist.Clear();
                Playlist.AddRange(other.Playlist);
            }

            if (!RecentParticipants.SequenceEqual(other.RecentParticipants))
            {
                RecentParticipants.Clear();
                RecentParticipants.AddRange(other.RecentParticipants);
            }

            Position.Value = other.Position.Value;
        }

        public bool ShouldSerializeRoomID() => false;
        public bool ShouldSerializeHost() => false;
        public bool ShouldSerializeEndDate() => false;
    }
}
