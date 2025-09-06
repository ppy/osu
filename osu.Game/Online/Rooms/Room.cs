// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;
using osu.Game.IO.Serialization.Converters;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.Rooms
{
    [JsonObject(MemberSerialization.OptIn)]
    public partial class Room : INotifyPropertyChanged
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
        /// The duration for which the room will be open. Will be <c>null</c> after the room is created.
        /// </summary>
        /// <remarks>
        /// To check the room end time, use <see cref="EndDate"/>.
        /// </remarks>
        public TimeSpan? Duration
        {
            get => duration == null ? null : TimeSpan.FromMinutes(duration.Value);
            set => SetField(ref duration, value == null ? null : (int)value.Value.TotalMinutes);
        }

        /// <summary>
        /// The date at which the room was opened. Will be <c>null</c> while the room has not yet been created.
        /// </summary>
        public DateTimeOffset? StartDate
        {
            get => startDate;
            set => SetField(ref startDate, value);
        }

        /// <summary>
        /// The date at which the room will be closed.
        /// </summary>
        /// <remarks>
        /// To set the room duration, use <see cref="Duration"/>.
        /// </remarks>
        public DateTimeOffset? EndDate
        {
            get => endDate;
            set => SetField(ref endDate, value);
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
        /// The set of most recent participants in the room.
        /// </summary>
        public IReadOnlyList<APIUser> RecentParticipants
        {
            get => recentParticipants;
            set => SetList(ref recentParticipants, value);
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
        /// The maximum number of attempts on the playlist. Only valid for playlist rooms.
        /// </summary>
        public int? MaxAttempts
        {
            get => maxAttempts;
            set => SetField(ref maxAttempts, value);
        }

        /// <summary>
        /// The room playlist.
        /// </summary>
        public IReadOnlyList<PlaylistItem> Playlist
        {
            get => playlist;
            set => SetList(ref playlist, value);
        }

        /// <summary>
        /// Describes the items in the playlist.
        /// </summary>
        public RoomPlaylistItemStats? PlaylistItemStats
        {
            get => playlistItemStats;
            set => SetField(ref playlistItemStats, value);
        }

        /// <summary>
        /// Describes the range of difficulty of the room.
        /// </summary>
        public RoomDifficultyRange? DifficultyRange
        {
            get => difficultyRange;
            set => SetField(ref difficultyRange, value);
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
        /// Provides some extra scoring statistics for the local user in the room.
        /// </summary>
        public PlaylistAggregateScore? UserScore
        {
            get => userScore;
            set => SetField(ref userScore, value);
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
        /// The chat channel id for the room. Will be <c>0</c> while the room has not yet been created.
        /// </summary>
        public int ChannelId
        {
            get => channelId;
            set => SetField(ref channelId, value);
        }

        /// <summary>
        /// The current status of the room.
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

        public bool Pinned
        {
            get => pinned;
            set => SetField(ref pinned, value);
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

        [JsonProperty("duration")]
        private int? duration;

        [JsonProperty("starts_at")]
        private DateTimeOffset? startDate;

        [JsonProperty("ends_at")]
        private DateTimeOffset? endDate;

        // Not yet serialised (not implemented).
        private int? maxParticipants;

        [JsonProperty("participant_count")]
        private int participantCount;

        [JsonProperty("recent_participants")]
        private IReadOnlyList<APIUser> recentParticipants = [];

        [JsonProperty("max_attempts", DefaultValueHandling = DefaultValueHandling.Ignore)]
        private int? maxAttempts;

        [JsonProperty("playlist")]
        private IReadOnlyList<PlaylistItem> playlist = [];

        [JsonProperty("playlist_item_stats")]
        private RoomPlaylistItemStats? playlistItemStats;

        [JsonProperty("difficulty_range")]
        private RoomDifficultyRange? difficultyRange;

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

        [JsonProperty("current_user_score")]
        private PlaylistAggregateScore? userScore;

        [JsonProperty("current_playlist_item")]
        private PlaylistItem? currentPlaylistItem;

        [JsonProperty("channel_id")]
        private int channelId;

        [JsonProperty("status")]
        [JsonConverter(typeof(SnakeCaseStringEnumConverter))]
        private RoomStatus status;

        [JsonProperty("pinned")]
        private bool pinned;

        // Not yet serialised (not implemented).
        private RoomAvailability availability;

        public Room()
        {
        }

        public Room(MultiplayerRoom room)
        {
            RoomID = room.RoomID;
            Name = room.Settings.Name;
            Password = room.Settings.Password;
            Type = room.Settings.MatchType;
            QueueMode = room.Settings.QueueMode;
            AutoStartDuration = room.Settings.AutoStartDuration;
            AutoSkip = room.Settings.AutoSkip;
            Host = room.Host != null ? new APIUser { Id = room.Host.UserID } : null;
            Playlist = room.Playlist.Select(p => new PlaylistItem(p)).ToArray();
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
            Host = other.Host;
            ChannelId = other.ChannelId;
            Status = other.Status;
            Availability = other.Availability;
            HasPassword = other.HasPassword;
            Type = other.Type;
            MaxParticipants = other.MaxParticipants;
            ParticipantCount = other.ParticipantCount;
            StartDate = other.StartDate;
            EndDate = other.EndDate;
            UserScore = other.UserScore;
            QueueMode = other.QueueMode;
            AutoStartDuration = other.AutoStartDuration;
            DifficultyRange = other.DifficultyRange;
            PlaylistItemStats = other.PlaylistItemStats;
            CurrentPlaylistItem = other.CurrentPlaylistItem;
            AutoSkip = other.AutoSkip;
            Playlist = other.Playlist;
            RecentParticipants = other.RecentParticipants;
        }

        /// <summary>
        /// Whether the room is no longer available.
        /// </summary>
        /// <remarks>
        /// This property does not update in real-time and needs to be queried periodically.
        /// Subscribe to <see cref="EndDate"/> to be notified of any immediate changes.
        /// </remarks>
        public bool HasEnded => DateTimeOffset.Now >= EndDate;

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

        protected bool SetList<T>(ref IReadOnlyList<T> list, IReadOnlyList<T> value, [CallerMemberName] string propertyName = null!)
        {
            if (list.SequenceEqual(value))
                return false;

            list = value;
            OnPropertyChanged(propertyName);
            return true;
        }

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
