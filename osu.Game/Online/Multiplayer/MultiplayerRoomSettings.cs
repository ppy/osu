// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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

        /// <summary>
        /// Custom score multipliers for mods in this room.
        /// Key is the mod acronym, value is the custom multiplier.
        /// </summary>
        [Key(7)]
        public Dictionary<string, double> ModMultipliers { get; set; } = new Dictionary<string, double>();

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
            ModMultipliers = new Dictionary<string, double>();
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
                   && AutoSkip == other.AutoSkip
                   && modMultipliersEqual(other.ModMultipliers);
        }

        private bool modMultipliersEqual(Dictionary<string, double> other)
        {
            if (ModMultipliers.Count != other.Count)
                return false;

            foreach (var kvp in ModMultipliers)
            {
                if (!other.TryGetValue(kvp.Key, out double otherValue))
                    return false;

                // Use epsilon comparison for floating point
                if (Math.Abs(kvp.Value - otherValue) > 0.001)
                    return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hash = new HashCode();
            hash.Add(Name);
            hash.Add(PlaylistItemId);
            hash.Add(Password);
            hash.Add(MatchType);
            hash.Add(QueueMode);
            hash.Add(AutoStartDuration);
            hash.Add(AutoSkip);
            hash.Add(ModMultipliers.Count);
            return hash.ToHashCode();
        }

        public override string ToString() => $"Name:{Name}"
                                             + $" Password:{(string.IsNullOrEmpty(Password) ? "no" : "yes")}"
                                             + $" Type:{MatchType}"
                                             + $" Item:{PlaylistItemId}"
                                             + $" Queue:{QueueMode}"
                                             + $" Start:{AutoStartDuration}"
                                             + $" AutoSkip:{AutoSkip}"
                                             + $" ModMultipliers:{ModMultipliers.Count}";
    }
}
