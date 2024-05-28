// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using Newtonsoft.Json;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.IO.Serialization.Converters;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms.RoomStatuses;

namespace osu.Game.Online.Rooms
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Room : IDependencyInjectionCandidate
    {
        [Cached]
        [JsonProperty("id")]
        public readonly Bindable<long?> RoomID = new Bindable<long?>();

        [Cached]
        [JsonProperty("name")]
        public readonly Bindable<string> Name = new Bindable<string>();

        [Cached]
        [JsonProperty("host")]
        public readonly Bindable<APIUser> Host = new Bindable<APIUser>();

        [Cached]
        [JsonProperty("playlist")]
        public readonly BindableList<PlaylistItem> Playlist = new BindableList<PlaylistItem>();

        [Cached]
        [JsonProperty("channel_id")]
        public readonly Bindable<int> ChannelId = new Bindable<int>();

        [JsonProperty("current_playlist_item")]
        [Cached]
        public readonly Bindable<PlaylistItem> CurrentPlaylistItem = new Bindable<PlaylistItem>();

        [JsonProperty("playlist_item_stats")]
        [Cached]
        public readonly Bindable<RoomPlaylistItemStats> PlaylistItemStats = new Bindable<RoomPlaylistItemStats>();

        [JsonProperty("difficulty_range")]
        [Cached]
        public readonly Bindable<RoomDifficultyRange> DifficultyRange = new Bindable<RoomDifficultyRange>();

        [Cached]
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
        public readonly Bindable<int?> MaxAttempts = new Bindable<int?>();

        [Cached]
        public readonly Bindable<RoomStatus> Status = new Bindable<RoomStatus>(new RoomStatusOpen());

        [Cached]
        public readonly Bindable<RoomAvailability> Availability = new Bindable<RoomAvailability>();

        [Cached]
        public readonly Bindable<MatchType> Type = new Bindable<MatchType>();

        // Todo: osu-framework bug (https://github.com/ppy/osu-framework/issues/4106)
        [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
        [JsonProperty("type")]
        private MatchType type
        {
            get => Type.Value;
            set => Type.Value = value;
        }

        [Cached]
        public readonly Bindable<QueueMode> QueueMode = new Bindable<QueueMode>();

        [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
        [JsonProperty("queue_mode")]
        private QueueMode queueMode
        {
            get => QueueMode.Value;
            set => QueueMode.Value = value;
        }

        [Cached]
        public readonly Bindable<TimeSpan> AutoStartDuration = new Bindable<TimeSpan>();

        [JsonProperty("auto_start_duration")]
        private ushort autoStartDuration
        {
            get => (ushort)AutoStartDuration.Value.TotalSeconds;
            set => AutoStartDuration.Value = TimeSpan.FromSeconds(value);
        }

        [Cached]
        public readonly Bindable<int?> MaxParticipants = new Bindable<int?>();

        [Cached]
        [JsonProperty("current_user_score")]
        public readonly Bindable<PlaylistAggregateScore> UserScore = new Bindable<PlaylistAggregateScore>();

        [JsonProperty("has_password")]
        public readonly Bindable<bool> HasPassword = new Bindable<bool>();

        [Cached]
        [JsonProperty("recent_participants")]
        public readonly BindableList<APIUser> RecentParticipants = new BindableList<APIUser>();

        [Cached]
        [JsonProperty("participant_count")]
        public readonly Bindable<int> ParticipantCount = new Bindable<int>();

        #region Properties only used for room creation request

        [Cached(Name = nameof(Password))]
        [JsonProperty("password")]
        public readonly Bindable<string> Password = new Bindable<string>();

        [Cached]
        public readonly Bindable<TimeSpan?> Duration = new Bindable<TimeSpan?>();

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

        #endregion

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

        [Cached]
        [JsonProperty("auto_skip")]
        public readonly Bindable<bool> AutoSkip = new Bindable<bool>();

        public Room()
        {
            Password.BindValueChanged(p => HasPassword.Value = !string.IsNullOrEmpty(p.NewValue));
        }

        /// <summary>
        /// Copies values from another <see cref="Room"/> into this one.
        /// </summary>
        /// <remarks>
        /// **Beware**: This will store references between <see cref="Room"/>s.
        /// </remarks>
        /// <param name="other">The <see cref="Room"/> to copy values from.</param>
        public void CopyFrom(Room other)
        {
            RoomID.Value = other.RoomID.Value;
            Name.Value = other.Name.Value;

            Category.Value = other.Category.Value;

            if (other.Host.Value != null && Host.Value?.Id != other.Host.Value.Id)
                Host.Value = other.Host.Value;

            ChannelId.Value = other.ChannelId.Value;
            Status.Value = other.Status.Value;
            Availability.Value = other.Availability.Value;
            HasPassword.Value = other.HasPassword.Value;
            Type.Value = other.Type.Value;
            MaxParticipants.Value = other.MaxParticipants.Value;
            ParticipantCount.Value = other.ParticipantCount.Value;
            EndDate.Value = other.EndDate.Value;
            UserScore.Value = other.UserScore.Value;
            QueueMode.Value = other.QueueMode.Value;
            AutoStartDuration.Value = other.AutoStartDuration.Value;
            DifficultyRange.Value = other.DifficultyRange.Value;
            PlaylistItemStats.Value = other.PlaylistItemStats.Value;
            CurrentPlaylistItem.Value = other.CurrentPlaylistItem.Value;
            AutoSkip.Value = other.AutoSkip.Value;

            other.RemoveExpiredPlaylistItems();

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
        }

        public void RemoveExpiredPlaylistItems()
        {
            // Todo: This is not the best way/place to do this, but the intention is to display all playlist items when the room has ended,
            // and display only the non-expired playlist items while the room is still active. In order to achieve this, all expired items are removed from the source Room.
            // More refactoring is required before this can be done locally instead - DrawableRoomPlaylist is currently directly bound to the playlist to display items in the room.
            if (!(Status.Value is RoomStatusEnded))
                Playlist.RemoveAll(i => i.Expired);
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class RoomPlaylistItemStats
        {
            [JsonProperty("count_active")]
            public int CountActive;

            [JsonProperty("count_total")]
            public int CountTotal;

            [JsonProperty("ruleset_ids")]
            public int[] RulesetIDs;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class RoomDifficultyRange
        {
            [JsonProperty("min")]
            public double Min;

            [JsonProperty("max")]
            public double Max;
        }
    }
}
