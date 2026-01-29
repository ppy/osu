// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.RankedPlay
{
    public interface IRankedPlayClient
    {
        /// <summary>
        /// Indicates that a card has been added to a user's hand.
        /// </summary>
        /// <param name="userId">The user whose hand has changed.</param>
        /// <param name="card">The card added to the user's hand.</param>
        Task RankedPlayCardAdded(int userId, RankedPlayCardItem card);

        /// <summary>
        /// Indicates that a card has been removed from a user's hand.
        /// </summary>
        /// <param name="userId">The user whose hand has changed.</param>
        /// <param name="card">The card removed from the user's hand.</param>
        Task RankedPlayCardRemoved(int userId, RankedPlayCardItem card);

        /// <summary>
        /// Indicates that a card has been revealed to the local user.
        /// </summary>
        /// <param name="card">The card that was revealed.</param>
        /// <param name="item">The playlist item the card corresponds to.</param>
        Task RankedPlayCardRevealed(RankedPlayCardItem card, MultiplayerPlaylistItem item);

        /// <summary>
        /// Indicates a card was played.
        /// </summary>
        /// <param name="card">The card played.</param>
        Task RankedPlayCardPlayed(RankedPlayCardItem card);
    }
}
