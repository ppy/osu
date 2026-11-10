// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public class RankedPlayCardItem : IEquatable<RankedPlayCardItem>
    {
        /// <summary>
        /// A unique identifier for this card.
        /// </summary>
        [Key(0)]
        public Guid ID { get; set; } = Guid.NewGuid();

        public bool Equals(RankedPlayCardItem? other)
            => other != null && ID.Equals(other.ID);

        public override bool Equals(object? obj)
            => obj is RankedPlayCardItem other && Equals(other);

        [SuppressMessage("ReSharper", "NonReadonlyMemberInGetHashCode")]
        public override int GetHashCode()
        {
            return ID.GetHashCode();
        }
    }
}
