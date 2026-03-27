// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Online.RankedPlay;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand
{
    /// <summary>
    /// Card hand representing the opponent's current hand, intended to be placed at the top edge of the screen.
    /// </summary>
    public partial class OpponentHandOfCards : HandOfCards
    {
        protected override bool Flipped => true;

        public void SetState(Dictionary<Guid, RankedPlayCardState> state)
        {
            foreach (var card in Cards)
            {
                if (!state.TryGetValue(card.Item.Card.ID, out var cardState))
                    continue;

                card.State = cardState;
            }
        }

        protected override void CalculateDraggedCardLayout(Vector2 dragPosition, out Vector2 position, out float rotation, out float scale)
        {
            float maxExtent = TotalLayoutWidth / 2;

            float x = float.Clamp(dragPosition.X, -maxExtent, maxExtent);

            scale = HOVER_SCALE;
            rotation = GetArcRotation(x);
            position = GetArcPosition(x) + GetCardUpwardsDirection(rotation) * 60;
        }
    }
}
