// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;

namespace osu.Game.Online.RankedPlay
{
    public interface IRankedPlayServer
    {
        /// <summary>
        /// Discards cards from the local user's hand during the <see cref="RankedPlayStage.CardDiscard"/> stage.
        /// </summary>
        Task DiscardCards(RankedPlayCardItem[] cards);

        /// <summary>
        /// Plays a card from the local user's hand during the <see cref="RankedPlayStage.CardPlay"/> stage.
        /// Only usable while the local user is the <see cref="RankedPlayRoomState.ActiveUserId">active player</see>.
        /// </summary>
        Task PlayCard(RankedPlayCardItem card);
    }
}
