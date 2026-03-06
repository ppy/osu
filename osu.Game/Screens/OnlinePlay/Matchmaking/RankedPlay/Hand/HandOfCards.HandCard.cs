// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand
{
    public abstract partial class HandOfCards
    {
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
            private HandOfCards handOfCards { get; set; } = null!;

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
                handOfCards.OnCardStateChanged(this, state.NewValue);

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
