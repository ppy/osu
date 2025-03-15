// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Multiplayer
{
    [Serializable]
    [MessagePackObject]
    public class MultiplayerRoomSettings : IEquatable<MultiplayerRoomSettings>
    {
        [Key(0)]
        public string Name { get; set; } = "Unnamed room";

        [Key(1)]
        public long PlaylistItemId { get; set; }

        [Key(2)]
        public string Password { get; set; } = string.Empty;

        [Key(3)]
        public MatchType MatchType { get; set; } = MatchType.HeadToHead;

        [Key(4)]
        public QueueMode QueueMode { get; set; } = QueueMode.HostOnly;

        [Key(5)]
        public TimeSpan AutoStartDuration { get; set; }

        [Key(6)]
        public bool AutoSkip { get; set; }

        [IgnoreMember]
        public bool AutoStartEnabled => AutoStartDuration != TimeSpan.Zero;

        public MultiplayerRoomSettings()
        {
        }

        public MultiplayerRoomSettings(Room room)
        {
            Name = room.Name;
            Password = room.Password ?? string.Empty;
            MatchType = room.Type;
            QueueMode = room.QueueMode;
            AutoStartDuration = room.AutoStartDuration;
            AutoSkip = room.AutoSkip;
        }

        public bool Equals(MultiplayerRoomSettings? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return Password.Equals(other.Password, StringComparison.Ordinal)
                   && Name.Equals(other.Name, StringComparison.Ordinal)
                   && PlaylistItemId == other.PlaylistItemId
                   && MatchType == other.MatchType
                   && QueueMode == other.QueueMode
                   && AutoStartDuration == other.AutoStartDuration
                   && AutoSkip == other.AutoSkip;
        }

        public override string ToString() => $"Name:{Name}"
                                             + $" Password:{(string.IsNullOrEmpty(Password) ? "no" : "yes")}"
                                             + $" Type:{MatchType}"
                                             + $" Item:{PlaylistItemId}"
                                             + $" Queue:{QueueMode}"
                                             + $" Start:{AutoStartDuration}"
                                             + $" AutoSkip:{AutoSkip}";
    }
}
