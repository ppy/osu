// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MessagePack;
using osu.Game.Online.API;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;

namespace osu.Game.Online.Rooms
{
    [Serializable]
    [MessagePackObject]
    public class MultiplayerPlaylistItem : IEquatable<MultiplayerPlaylistItem>
    {
        [Key(0)]
        public long ID { get; set; }

        [Key(1)]
        public int OwnerID { get; set; }

        [Key(2)]
        public int BeatmapID { get; set; }

        [Key(3)]
        public string BeatmapChecksum { get; set; } = string.Empty;

        [Key(4)]
        public int RulesetID { get; set; }

        /// <summary>
        /// Mods that should be applied for every participant in the room.
        /// </summary>
        [Key(5)]
        public IEnumerable<APIMod> RequiredMods { get; set; } = Enumerable.Empty<APIMod>();

        /// <summary>
        /// Mods that participants are allowed to apply at their own discretion.
        /// </summary>
        /// <remarks>
        /// This will be empty when <see cref="Freestyle"/> is <c>true</c>, but participants may still select any mods from their choice of ruleset,
        /// provided the mod <see cref="IMod.ValidForMultiplayerAsFreeMod">implementation</see> indicates free-mod validity
        /// and is <see cref="ModUtils.CheckCompatibleSet(IEnumerable{Mod})">compatible</see> with the rest of the user's selection.
        /// </remarks>
        [Key(6)]
        public IEnumerable<APIMod> AllowedMods { get; set; } = Enumerable.Empty<APIMod>();

        [Key(7)]
        public bool Expired { get; set; }

        /// <summary>
        /// The order in which this <see cref="MultiplayerPlaylistItem"/> will be played relative to others.
        /// Playlist items should be played in increasing order (lower values are played first).
        /// </summary>
        /// <remarks>
        /// This is only valid for items which are not <see cref="Expired"/>. The value for expired items is undefined and should not be used.
        /// </remarks>
        [Key(8)]
        public ushort PlaylistOrder { get; set; }

        /// <summary>
        /// The date when this <see cref="MultiplayerPlaylistItem"/> was played.
        /// </summary>
        [Key(9)]
        public DateTimeOffset? PlayedAt { get; set; }

        [Key(10)]
        public double StarRating { get; set; }

        /// <summary>
        /// Indicates whether participants in the room are able to pick their own choice of beatmap difficulty, ruleset, and mods.
        /// </summary>
        [Key(11)]
        public bool Freestyle { get; set; }

        /// <summary>
        /// Creates a new <see cref="MultiplayerPlaylistItem"/>.
        /// </summary>
        [SerializationConstructor]
        public MultiplayerPlaylistItem()
        {
        }

        /// <summary>
        /// Creates a new <see cref="MultiplayerPlaylistItem"/> from an API <see cref="PlaylistItem"/>.
        /// </summary>
        /// <remarks>
        /// This will create unique instances of the <see cref="RequiredMods"/> and <see cref="AllowedMods"/> arrays but NOT unique instances of the contained <see cref="APIMod"/>s.
        /// </remarks>
        public MultiplayerPlaylistItem(PlaylistItem item)
        {
            ID = item.ID;
            OwnerID = item.OwnerID;
            BeatmapID = item.Beatmap.OnlineID;
            BeatmapChecksum = item.Beatmap.MD5Hash;
            RulesetID = item.RulesetID;
            RequiredMods = item.RequiredMods.ToArray();
            AllowedMods = item.AllowedMods.ToArray();
            Expired = item.Expired;
            PlaylistOrder = item.PlaylistOrder ?? 0;
            PlayedAt = item.PlayedAt;
            StarRating = item.Beatmap.StarRating;
            Freestyle = item.Freestyle;
        }

        /// <summary>
        /// Creates a copy of this <see cref="MultiplayerPlaylistItem"/>.
        /// </summary>
        /// <remarks>
        /// This will create unique instances of the <see cref="RequiredMods"/> and <see cref="AllowedMods"/> arrays but NOT unique instances of the contained <see cref="APIMod"/>s.
        /// </remarks>
        public MultiplayerPlaylistItem Clone()
        {
            MultiplayerPlaylistItem clone = (MultiplayerPlaylistItem)MemberwiseClone();
            clone.RequiredMods = RequiredMods.ToArray();
            clone.AllowedMods = AllowedMods.ToArray();
            return clone;
        }

        public bool Equals(MultiplayerPlaylistItem? other)
            => other != null
               && ID == other.ID
               && OwnerID == other.OwnerID
               && BeatmapID == other.BeatmapID
               && BeatmapChecksum == other.BeatmapChecksum
               && RulesetID == other.RulesetID
               && RequiredMods.SequenceEqual(other.RequiredMods)
               && AllowedMods.SequenceEqual(other.AllowedMods)
               && Expired == other.Expired
               && PlaylistOrder == other.PlaylistOrder
               && PlayedAt == other.PlayedAt
               && StarRating == other.StarRating
               && Freestyle == other.Freestyle;

        public override bool Equals(object? obj)
            => obj is MultiplayerPlaylistItem other && Equals(other);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(ID);
            hashCode.Add(OwnerID);
            hashCode.Add(BeatmapID);
            hashCode.Add(BeatmapChecksum);
            hashCode.Add(RulesetID);
            hashCode.Add(RequiredMods);
            hashCode.Add(AllowedMods);
            hashCode.Add(Expired);
            hashCode.Add(PlaylistOrder);
            hashCode.Add(PlayedAt);
            hashCode.Add(StarRating);
            hashCode.Add(Freestyle);
            return hashCode.ToHashCode();
        }
    }
}
