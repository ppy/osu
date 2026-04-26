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

        protected override CardLayout CalculateDraggedCardLayout(Vector2 dragPosition)
        {
            // the opponent shouldn't be able to drag his card across the entire screen.
            // card movement is limited to roughly the width of the hand horizontally
            // and has a fixed vertical offset (extended slightly further than when hovered)
            float maxExtent = TotalLayoutWidth / 2;

            float x = float.Clamp(dragPosition.X, -maxExtent, maxExtent);

            return new CardLayout
            {
                Position = GetArcPosition(x) + new Vector2(0, -60),
                Rotation = 0,
                Scale = HOVER_SCALE,
            };
        }
    }
}
