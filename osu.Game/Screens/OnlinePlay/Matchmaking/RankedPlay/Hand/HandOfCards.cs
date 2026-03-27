// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using osu.Framework.Allocation;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand
{
    /// <summary>
    /// Drawable that layouts cards as if held in a player's hands.
    /// </summary>
    [Cached]
    public abstract partial class HandOfCards : CompositeDrawable
    {
        protected const float HOVER_SCALE = 1.2f;

        private const float card_spacing = -20;

        public IEnumerable<HandCard> Cards => cardContainer.Children;

        /// <summary>
        /// How far a card slides upwards when hovered.
        /// Used for making sure a card moves entirely into frame when the hand is partially off-screen.
        /// </summary>
        public float HoverYOffset = 15;

        /// <summary>
        /// If true, card layout will be flipped on both axes for a card hand placed at the top edge of the screen, while keeping the cards upright.
        /// Used for <see cref="OpponentHandOfCards"/>.
        /// </summary>
        protected virtual bool Flipped => false;

        private readonly Container<HandCard> cardContainer;

        private readonly Dictionary<RankedPlayCardItem, HandCard> cardLookup = new Dictionary<RankedPlayCardItem, HandCard>();

        protected HandOfCards()
        {
            AddInternal(cardContainer = new Container<HandCard>
            {
                RelativeSizeAxes = Axes.Both,
            });
        }

        protected override void Update()
        {
            base.Update();

            if (!layoutBacking.IsValid)
            {
                updateLayout();
                layoutBacking.Validate();
            }
        }

        protected bool Contracted { get; private set; }

        /// <summary>
        /// Contracts all cards towards the bottom (or top when <see cref="Flipped"/>).
        /// Cards will no longer get layouted after this method is called.
        /// </summary>
        public void Contract()
        {
            Contracted = true;

            double delay = 0;

            foreach (var card in cardContainer)
            {
                card.Delay(delay)
                    .MoveTo(new Vector2(0, Flipped ? -220 : 220), 400, Easing.OutExpo)
                    .RotateTo(0, 400, Easing.OutExpo)
                    .ScaleTo(1, 400, Easing.OutExpo);

                delay += 50;
            }
        }

        private Anchor cardAnchor => Flipped ? Anchor.TopCentre : Anchor.BottomCentre;

        public void AddCard(RankedPlayCardWithPlaylistItem item, Action<HandCard>? setupAction = null) => AddCard(new RankedPlayCard(item), setupAction);

        public void AddCard(RankedPlayCard card, Action<HandCard>? setupAction = null)
        {
            if (cardLookup.ContainsKey(card.Item.Card))
                return;

            var drawable = CreateHandCard(card);
            drawable.Anchor = drawable.Origin = cardAnchor;

            cardLookup[card.Item.Card] = drawable;

            cardContainer.Add(drawable);
            layoutBacking.Invalidate();

            setupAction?.Invoke(drawable);
        }

        public void Clear() => cardContainer.Clear();

        public bool RemoveCard(RankedPlayCardWithPlaylistItem item)
        {
            if (!cardLookup.Remove(item.Card, out var drawable))
                return false;

            cardContainer.Remove(drawable, true);
            layoutBacking.Invalidate();
            return false;
        }

        /// <summary>
        /// Removes a card and detaches it's contained card so it can be attached to a new card facade.
        /// </summary>
        /// <param name="item">Item to remove the card for</param>
        /// <param name="card">Contained <see cref="RankedPlayCard"/></param>
        /// <param name="screenSpaceDrawQuad"><see cref="Drawable.ScreenSpaceDrawQuad"/> of the removed card</param>
        /// <returns>Whether a card was found for the provided item</returns>
        public bool RemoveCard(RankedPlayCardWithPlaylistItem item, [MaybeNullWhen(false)] out RankedPlayCard card, out Quad screenSpaceDrawQuad)
        {
            if (!cardLookup.Remove(item.Card, out var drawable))
            {
                card = null;
                screenSpaceDrawQuad = default;
                return false;
            }

            screenSpaceDrawQuad = drawable.ScreenSpaceDrawQuad;
            card = drawable.Detach();

            cardContainer.Remove(drawable, true);
            layoutBacking.Invalidate();

            return true;
        }

        protected virtual HandCard CreateHandCard(RankedPlayCard card) => new HandCard(card);

        protected virtual void OnCardStateChanged(HandCard card, RankedPlayCardState state)
        {
            InvalidateLayout();

            // hovered state can be caused by keyboard focus, in which case we have to clean up after the other cards manually
            if (state.Hovered)
            {
                foreach (var c in cardContainer)
                {
                    if (c != card)
                        c.CardHovered = false;
                }
            }
        }

        #region Layout

        private readonly Cached layoutBacking = new Cached();

        protected void InvalidateLayout() => layoutBacking.Invalidate();

        public void UpdateLayout(double stagger = 0)
        {
            updateLayout(stagger);
            layoutBacking.Validate();
        }

        private void updateLayout(double stagger = 0)
        {
            if (Contracted)
                return;

            double delay = 0;

            int activeCardIndex = getActiveCardIndex();

            for (int i = 0; i < cardContainer.Count; i++)
            {
                var child = cardContainer[i];

                Vector2 position;
                float rotation;
                float scale;

                if (child.CardDragged)
                    CalculateDraggedCardLayout(child.DragPosition, out position, out rotation, out scale);
                else
                    CalculateCardLayout(i, activeCardIndex, out position, out rotation, out scale);

                if (Flipped)
                    position *= -1;

                child.Delay(delay)
                     .MoveTo(position, 300, Easing.OutExpo)
                     .RotateTo(rotation, 300, Easing.OutExpo)
                     .ScaleTo(scale, 400, Easing.OutElasticQuarter);

                delay += stagger;
            }
        }

        private int getActiveCardIndex()
        {
            // the mouse can temporarily leave the dragged card, so dragged card should take precedence
            for (int i = 0; i < cardContainer.Count; i++)
            {
                if (cardContainer[i].CardDragged)
                    return i;
            }

            for (int i = 0; i < cardContainer.Count; i++)
            {
                if (cardContainer[i].CardHovered)
                    return i;
            }

            return -1;
        }

        protected void CalculateCardLayout(
            int index,
            int activeIndex,
            out Vector2 position,
            out float rotation,
            out float scale)
        {
            float x = GetCardX(index, activeIndex);

            position = GetArcPosition(x);
            rotation = GetArcRotation(x);
            scale = index == activeIndex ? HOVER_SCALE : 1;

            if (index == activeIndex)
                position += GetCardUpwardsDirection(rotation) * HoverYOffset;
        }

        protected virtual void CalculateDraggedCardLayout(Vector2 dragPosition, out Vector2 position, out float rotation, out float scale)
        {
            position = dragPosition;
            rotation = 0;
            scale = HOVER_SCALE;
        }

        /// <summary>
        /// Represents the total width of the layout for all cards in the hand.
        /// </summary>
        /// <remarks>
        /// Does not account for extra space needed for spreading the cards adjacent to the active card apart.
        /// </remarks>
        protected float TotalLayoutWidth => cardContainer.Count * (RankedPlayCard.SIZE.X + card_spacing) - card_spacing;

        protected float GetCardX(int index, int activeIndex)
        {
            float x = -TotalLayoutWidth / 2
                      + index * (RankedPlayCard.SIZE.X + card_spacing)
                      + RankedPlayCard.SIZE.X / 2;

            if (activeIndex < 0 || cardContainer.Count <= 1)
                return x;

            // if a card is hovered or dragged, the adjacent cards should get spread apart
            int distance = Math.Abs(index - activeIndex);
            int direction = Math.Sign(index - activeIndex);

            float baseOffset = RankedPlayCard.SIZE.X * 0.1f;

            switch (direction)
            {
                // special case for the left card when there's only 2 cards
                // too much offset looks kinda odd here so it's reduced
                case -1 when cardContainer.Count == 2:
                    x -= baseOffset + 3;
                    break;

                case -1:
                    x -= baseOffset + 10 / MathF.Pow(distance, 2);
                    break;

                case 1:
                    // cards right to the active card have a higher offset because they are partially
                    // covering the cards to their left
                    x += baseOffset + 20 / MathF.Pow(distance, 2);
                    break;
            }

            return x;
        }

        protected static Vector2 GetArcPosition(float x) =>
            new Vector2(x, MathF.Pow(MathF.Abs(x / 250), 2) * 20 - 10);

        protected static float GetArcRotation(float x) => x * 0.03f;

        protected static Vector2 GetCardUpwardsDirection(float rotation)
        {
            float angle = MathHelper.DegreesToRadians(rotation - 90);

            return new Vector2(MathF.Cos(angle), MathF.Sin(angle));
        }

        #endregion
    }
}
