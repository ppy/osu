// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;

namespace osu.Game.Online.RankedPlay
{
    public interface IRankedPlayServer
    {
        Task DiscardCards(RankedPlayCardItem[] cards);

        Task PlayCard(RankedPlayCardItem card);
    }
}
