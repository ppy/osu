// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public class RankedPlayCardWithPlaylistItem : IEquatable<RankedPlayCardWithPlaylistItem>
    {
        public readonly Bindable<MultiplayerPlaylistItem?> PlaylistItem = new Bindable<MultiplayerPlaylistItem?>();
        public readonly RankedPlayCardItem Card;

        public RankedPlayCardWithPlaylistItem(RankedPlayCardItem card)
        {
            Card = card;
        }

        public bool Equals(RankedPlayCardWithPlaylistItem? other)
            => other != null && Card.Equals(other.Card);

        public override bool Equals(object? obj)
            => obj is RankedPlayCardWithPlaylistItem other && Equals(other);

        public override int GetHashCode()
            => Card.GetHashCode();
    }
}
