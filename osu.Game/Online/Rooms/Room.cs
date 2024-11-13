// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
    public partial class Room : IDependencyInjectionCandidate, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// The online room ID. Will be <c>null</c> while the room has not yet been created.
        /// </summary>
        public long? RoomID
        {
            get => roomId;
            set => SetField(ref roomId, value);
        }

        /// <summary>
        /// The room name.
        /// </summary>
        public string Name
        {
            get => name;
            set => SetField(ref name, value);
        }

        /// <summary>
        /// Sets the room password. Will be <c>null</c> after the room is created.
        /// </summary>
        /// <remarks>
        /// To check if the room has a password, use <see cref="HasPassword"/>.
        /// </remarks>
        public string? Password
        {
            get => password;
            set
            {
                SetField(ref password, value);
                HasPassword = !string.IsNullOrEmpty(value);
            }
        }

        /// <summary>
        /// Whether the room has a password.
        /// </summary>
        /// <remarks>
        /// To set a password, use <see cref="Password"/>.
        /// </remarks>
        [JsonProperty("has_password")]
        public bool HasPassword
        {
            get => hasPassword;
            private set => SetField(ref hasPassword, value);
        }

        /// <summary>
        /// The room host. Will be <c>null</c> while the room has not yet been created.
        /// </summary>
        public APIUser? Host
        {
            get => host;
            set => SetField(ref host, value);
        }

        /// <summary>
        /// The room category.
        /// </summary>
        public RoomCategory Category
        {
            get => category;
            set => SetField(ref category, value);
        }

        /// <summary>
        /// The maximum number of users allowed in the room.
        /// </summary>
        public int? MaxParticipants
        {
            get => maxParticipants;
            set => SetField(ref maxParticipants, value);
        }

        /// <summary>
        /// The current number of users in the room.
        /// </summary>
        public int ParticipantCount
        {
            get => participantCount;
            set => SetField(ref participantCount, value);
        }

        /// <summary>
        /// The match type.
        /// </summary>
        public MatchType Type
        {
            get => type;
            set => SetField(ref type, value);
        }

        /// <summary>
        /// The playlist queueing mode. Only valid for multiplayer rooms.
        /// </summary>
        public QueueMode QueueMode
        {
            get => queueMode;
            set => SetField(ref queueMode, value);
        }

        /// <summary>
        /// Whether to automatically skip map intros. Only valid for multiplayer rooms.
        /// </summary>
        public bool AutoSkip
        {
            get => autoSkip;
            set => SetField(ref autoSkip, value);
        }

        /// <summary>
        /// The amount of time before the match is automatically started. Only valid for multiplayer rooms.
        /// </summary>
        public TimeSpan AutoStartDuration
        {
            get => TimeSpan.FromSeconds(autoStartDuration);
            set => SetField(ref autoStartDuration, (ushort)value.TotalSeconds);
        }

        /// <summary>
        /// Represents the current item selected within the room.
        /// </summary>
        /// <remarks>
        /// Only valid for room listing requests (i.e. in the lounge screen), and may not be valid while inside the room.
        /// </remarks>
        public PlaylistItem? CurrentPlaylistItem
        {
            get => currentPlaylistItem;
            set => SetField(ref currentPlaylistItem, value);
        }

        /// <summary>
        /// The current room status.
        /// </summary>
        public RoomStatus Status
        {
            get => status;
            set => SetField(ref status, value);
        }

        /// <summary>
        /// Describes which players are able to join the room.
        /// </summary>
        public RoomAvailability Availability
        {
            get => availability;
            set => SetField(ref availability, value);
        }

        [JsonProperty("id")]
        private long? roomId;

        [JsonProperty("name")]
        private string name = string.Empty;

        [JsonProperty("password")]
        private string? password;

        // Not serialised (internal use only).
        private bool hasPassword;

        [JsonProperty("host")]
        private APIUser? host;

        [JsonProperty("category")]
        [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
        private RoomCategory category;

        // Not yet serialised (not implemented).
        private int? maxParticipants;

        [JsonProperty("participant_count")]
        private int participantCount;

        [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
        [JsonProperty("type")]
        private MatchType type;

        [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
        [JsonProperty("queue_mode")]
        private QueueMode queueMode;

        [JsonProperty("auto_skip")]
        private bool autoSkip;

        [JsonProperty("auto_start_duration")]
        private ushort autoStartDuration;

        [JsonProperty("current_playlist_item")]
        private PlaylistItem? currentPlaylistItem;

        // Not serialised (see: GetRoomsRequest).
        private RoomStatus status = new RoomStatusOpen();

        // Not yet serialised (not implemented).
        private RoomAvailability availability;

        [Cached]
        [JsonProperty("playlist")]
        public readonly BindableList<PlaylistItem> Playlist = new BindableList<PlaylistItem>();

        [Cached]
        [JsonProperty("channel_id")]
        public readonly Bindable<int> ChannelId = new Bindable<int>();

        [JsonProperty("playlist_item_stats")]
        [Cached]
        public readonly Bindable<RoomPlaylistItemStats> PlaylistItemStats = new Bindable<RoomPlaylistItemStats>();

        [JsonProperty("difficulty_range")]
        [Cached]
        public readonly Bindable<RoomDifficultyRange> DifficultyRange = new Bindable<RoomDifficultyRange>();

        [Cached]
        public readonly Bindable<int?> MaxAttempts = new Bindable<int?>();

        [Cached]
        [JsonProperty("current_user_score")]
        public readonly Bindable<PlaylistAggregateScore> UserScore = new Bindable<PlaylistAggregateScore>();

        [Cached]
        [JsonProperty("recent_participants")]
        public readonly BindableList<APIUser> RecentParticipants = new BindableList<APIUser>();

        #region Properties only used for room creation request

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
        [JsonProperty("starts_at")]
        public readonly Bindable<DateTimeOffset?> StartDate = new Bindable<DateTimeOffset?>();

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
        /// Copies values from another <see cref="Room"/> into this one.
        /// </summary>
        /// <remarks>
        /// **Beware**: This will store references between <see cref="Room"/>s.
        /// </remarks>
        /// <param name="other">The <see cref="Room"/> to copy values from.</param>
        public void CopyFrom(Room other)
        {
            RoomID = other.RoomID;
            Name = other.Name;

            Category = other.Category;

            if (other.Host != null && Host?.Id != other.Host.Id)
                Host = other.Host;

            ChannelId.Value = other.ChannelId.Value;
            Status = other.Status;
            Availability = other.Availability;
            HasPassword = other.HasPassword;
            Type = other.Type;
            MaxParticipants = other.MaxParticipants;
            ParticipantCount = other.ParticipantCount;
            EndDate.Value = other.EndDate.Value;
            UserScore.Value = other.UserScore.Value;
            QueueMode = other.QueueMode;
            AutoStartDuration = other.AutoStartDuration;
            DifficultyRange.Value = other.DifficultyRange.Value;
            PlaylistItemStats.Value = other.PlaylistItemStats.Value;
            CurrentPlaylistItem = other.CurrentPlaylistItem;
            AutoSkip = other.AutoSkip;

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
            if (Status is not RoomStatusEnded)
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
            public int[] RulesetIDs = [];
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class RoomDifficultyRange
        {
            [JsonProperty("min")]
            public double Min;

            [JsonProperty("max")]
            public double Max;
        }

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null!)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}
