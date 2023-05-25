// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using osu.Game.Online.API;

namespace osu.Game.Online.Rooms
{
    [Serializable]
    [MessagePackObject]
    public class MultiplayerPlaylistItem
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

        [Key(5)]
        public IEnumerable<APIMod> RequiredMods { get; set; } = Enumerable.Empty<APIMod>();

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

        [SerializationConstructor]
        public MultiplayerPlaylistItem()
        {
        }
    }
}
