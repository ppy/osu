// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Utils;
using osu.Game.Online.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osuTK;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand
{
    public abstract partial class HandOfCards
    {
        public partial class HandCard : CompositeDrawable
        {
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

            public bool CardDragged
            {
                get => State.Dragged;
                set => State = State with { Dragged = value };
            }

            public bool CardHoveredOrDragged => CardHovered || CardDragged;

            public Vector2 DragPosition
            {
                get => State.DragPosition;
                set => State = State with { DragPosition = value };
            }

            public int Order
            {
                get => State.Order;
                set => State = State with { Order = value };
            }

            public CardLayout LayoutTarget { get; set; }

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

                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                positionSpring.Current = positionSpring.PreviousTarget = Position;
                scaleSpring.Current = scaleSpring.PreviousTarget = 1;
                rotationSpring.Current = rotationSpring.PreviousTarget = Rotation;

                state.BindValueChanged(OnStateChanged, true);
            }

            protected virtual void OnStateChanged(ValueChangedEvent<RankedPlayCardState> state)
            {
                handOfCards.OnCardStateChanged(this, state);

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

                if (state.NewValue.Dragged)
                {
                    // while card is being dragged card should slowly swing from side to side,
                    // so frequency is lowered and elasticity is increased
                    rotationSpring.NaturalFrequency = 2f;
                    rotationSpring.Damping = 0.4f;
                    rotationSpring.Response = 1.2f;
                }
                else
                {
                    // otherwise rotation should be more snappy and not feel elastic
                    rotationSpring.NaturalFrequency = 3f;
                    rotationSpring.Damping = 0.75f;
                    rotationSpring.Response = 0.8f;
                }
            }

            public RankedPlayCard Detach()
            {
                Card.ShowSelectionOutline = false;
                Card.Elevation = 0;

                RemoveInternal(Card, false);

                return Card;
            }

            private bool updateMovement = true;

            private static readonly SpringParameters default_position_spring_parameters = new SpringParameters
            {
                NaturalFrequency = 4f,
                Response = 1.1f,
                Damping = 0.8f
            };

            private readonly Vector2Spring positionSpring = new Vector2Spring { Parameters = default_position_spring_parameters };

            private readonly FloatSpring rotationSpring = new FloatSpring
            {
                NaturalFrequency = 2f,
                Damping = 0.4f,
                Response = 1.2f,
            };

            private readonly FloatSpring scaleSpring = new FloatSpring
            {
                NaturalFrequency = 4f,
                Response = 1.3f,
                Damping = 0.75f,
                Current = 1,
                PreviousTarget = 1,
            };

            protected override void Update()
            {
                base.Update();

                if (updateMovement)
                {
                    Position = positionSpring.Update(Time.Elapsed, LayoutTarget.Position);
                    Scale = new Vector2(scaleSpring.Update(Time.Elapsed, LayoutTarget.Scale));

                    float targetRotation = LayoutTarget.Rotation;

                    if (CardDragged)
                    {
                        targetRotation += positionSpring.Velocity.X * 0.006f;
                    }

                    Rotation = rotationSpring.Update(Time.Elapsed, targetRotation);

                    Card.Elevation = (float)Interpolation.DampContinuously(Card.Elevation, CardHoveredOrDragged ? 1 : 0, 25, Time.Elapsed);
                }
            }

            /// <summary>
            /// Delays the time until a card starts to move to its layout position, intended to use for staggered movement when adding multiple cards to the hand at once.
            /// Movement is slowed down a bit while it's moving towards the target position to make the transition appear less abrupt.
            /// </summary>
            public void DelayMovementOnEntering(double delay)
            {
                const double approximate_time_until_position_reached = 200;

                updateMovement = false;

                this.Delay(delay)
                    .Schedule(() =>
                    {
                        updateMovement = true;
                        positionSpring.NaturalFrequency = 2.5f;
                    })
                    .Delay(approximate_time_until_position_reached)
                    .Schedule(() =>
                    {
                        positionSpring.Parameters = default_position_spring_parameters;
                    });
            }

            /// <summary>
            /// Makes the card move towards its layout position from a given <paramref name="position"/> and updates
            /// movement parameters so the card moves towards it's target position more slowly and less springy.
            /// </summary>
            public void SetupMovementForDrawnCard(Vector2 position)
            {
                const double approximate_time_until_position_reached = 200;

                Position = position;

                positionSpring.NaturalFrequency = 2f;
                positionSpring.Damping = 1f;

                Scheduler.AddDelayed(() => positionSpring.Parameters = default_position_spring_parameters, approximate_time_until_position_reached);
            }
        }
    }
}
