// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Caching;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.RankedPlay;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards
{
    /// <summary>
    /// Drawable that layouts cards as if held in a player's hands.
    /// </summary>
    [Cached]
    public abstract partial class CardHand : CompositeDrawable
    {
        private const float hover_scale = 1.2f;

        public IEnumerable<HandCard> Cards => cardContainer.Children;

        /// <summary>
        /// How far a card slides upwards when hovered.
        /// Used for making sure a card moves entirely into frame when the hand is partially off-screen.
        /// </summary>
        public float HoverYOffset = 15;

        /// <summary>
        /// If true, card layout will be flipped on both axes for a card hand placed at the top edge of the screen, while keeping the cards upright.
        /// Used for <see cref="OpponentCardHand"/>.
        /// </summary>
        protected virtual bool Flipped => false;

        private readonly Container<HandCard> cardContainer;

        private readonly Dictionary<RankedPlayCardItem, HandCard> cardLookup = new Dictionary<RankedPlayCardItem, HandCard>();

        protected CardHand()
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

            const float spacing = -20;

            float totalWidth = cardContainer.Sum(it => it.LayoutWidth + spacing) - spacing;

            float x = -totalWidth / 2;

            const int no_card_hovered = -1;
            int hoverIndex = no_card_hovered;

            for (int i = 0; i < cardContainer.Count; i++)
            {
                if (cardContainer[i].CardHovered)
                {
                    hoverIndex = i;
                    break;
                }
            }

            double delay = 0;

            for (int i = 0; i < cardContainer.Count; i++)
            {
                var child = cardContainer[i];

                x += child.LayoutWidth / 2;

                float yOffset = 0;

                var position = new Vector2(x, MathF.Pow(MathF.Abs(x / 250), 2) * 20 - 10);

                if (hoverIndex != no_card_hovered && cardContainer.Children.Count > 1)
                {
                    int distance = Math.Abs(i - hoverIndex);
                    int direction = Math.Sign(i - hoverIndex);

                    position.X += direction switch
                    {
                        0 => 0,

                        // special case for the left card when there's only 2 cards
                        // too much offset looks kinda odd here so it's reduced
                        < 0 when cardContainer.Count == 2 => -3,

                        < 0 => -10 / MathF.Pow(distance, 3),

                        // cards right to the hovered card have a higher offset because they are partially
                        // covering the cards to their left
                        > 0 => 20 / MathF.Pow(distance, 2),
                    };
                }

                if (child.CardHovered)
                    yOffset = -HoverYOffset;

                float rotation = x * 0.03f;

                float angle = MathHelper.DegreesToRadians(rotation + 90);

                position += new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * yOffset;

                position *= Flipped ? -1 : 1;

                child
                    .Delay(delay)
                    .MoveTo(position, 300, Easing.OutExpo)
                    .RotateTo(rotation, 300, Easing.OutExpo)
                    .ScaleTo(child.CardHovered ? hover_scale : 1f, 400, Easing.OutElasticQuarter);

                x += child.LayoutWidth / 2 + spacing;

                delay += stagger;
            }
        }

        #endregion

        public partial class HandCard : CompositeDrawable
        {
            public float LayoutWidth => DrawWidth * (State.Hovered ? hover_scale : 1);

            private readonly Bindable<RankedPlayCardState> state = new Bindable<RankedPlayCardState>();

            public RankedPlayCardState State
            {
                get => state.Value;
                set => state.Value = value;
            }

            public bool Selected
            {
                get => State.Selected;
                set => State = State with { Selected = value };
            }

            public bool CardHovered
            {
                get => State.Hovered;
                set => State = State with { Hovered = value };
            }

            public bool CardPressed
            {
                get => State.Pressed;
                set => State = State with { Pressed = value };
            }

            [Resolved]
            private CardHand cardHand { get; set; } = null!;

            public readonly RankedPlayCard Card;

            public RankedPlayCardWithPlaylistItem Item => Card.Item;

            public HandCard(RankedPlayCard card)
            {
                Size = card.DrawSize;

                card.Anchor = Anchor.Centre;
                card.Origin = Anchor.Centre;
                card.Position = Vector2.Zero;
                card.Rotation = 0;
                card.Scale = Vector2.One;

                AddInternal(Card = card);

                Anchor = Anchor.BottomCentre;
                Origin = Anchor.BottomCentre;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                state.BindValueChanged(OnStateChanged, true);
            }

            protected virtual void OnStateChanged(ValueChangedEvent<RankedPlayCardState> state)
            {
                cardHand.OnCardStateChanged(this, state.NewValue);

                Card.ShowSelectionOutline = state.NewValue.Selected;

                switch (state.NewValue.Pressed, state.OldValue.Pressed)
                {
                    case (true, false):
                        Card.ScaleTo(0.95f, 300, Easing.OutExpo);
                        break;

                    case (false, true):
                        Card.ScaleTo(1f, 400, Easing.OutElasticHalf);
                        break;
                }
            }

            public RankedPlayCard Detach()
            {
                Card.ShowSelectionOutline = false;
                Card.Elevation = 0;

                RemoveInternal(Card, false);

                return Card;
            }

            protected override void Update()
            {
                base.Update();

                Card.Elevation = float.Lerp(CardHovered ? 1 : 0, Card.Elevation, (float)Math.Exp(-0.03f * Time.Elapsed));
            }
        }
    }
}
