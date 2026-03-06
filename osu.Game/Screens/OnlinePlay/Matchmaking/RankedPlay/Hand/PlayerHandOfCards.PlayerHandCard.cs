// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand
{
    public partial class PlayerHandOfCards
    {
        public partial class PlayerHandCard : HandCard
        {
            private Action? playAction;

            public Action? PlayAction
            {
                get => playAction;
                set
                {
                    playAction = value;
                    PlayButton.Action = value;
                    updatePlayButtonVisibility();
                }
            }

            public required Action<PlayerHandCard> Clicked;

            public required IBindable<bool> AllowSelection;

            private readonly Drawable cardInputArea;
            private readonly Drawable fullInputArea;

            public readonly ShearedButton PlayButton;

            public PlayerHandCard(RankedPlayCard card)
                : base(card)
            {
                AddRangeInternal(new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding(-10)
                        {
                            // card moves upwards on hover which can produce jitter if the hitbox doesn't extend all the way to the bottom of the screen
                            Bottom = -50
                        },
                        Child = cardInputArea = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Top = -40 },
                        Child = fullInputArea = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Child = PlayButton = new ShearedButton
                            {
                                Name = "Play Button",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(90, 30),
                                Text = "Play",
                                TextSize = 14,
                                LighterColour = Colour4.FromHex("87D8FA"),
                                DarkerColour = Colour4.FromHex("72D5FF")
                            }
                        }
                    }
                });
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AddInternal(new HoverSounds());
            }

            protected override void OnStateChanged(ValueChangedEvent<RankedPlayCardState> state)
            {
                base.OnStateChanged(state);
                updatePlayButtonVisibility();
            }

            private void updatePlayButtonVisibility()
            {
                PlayButton.Alpha = PlayButton.Action != null && Selected ? 1 : 0;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                if (PlayButton.Alpha > 0)
                    return fullInputArea.ReceivePositionalInputAt(screenSpacePos);

                // input events are handled for an area that's slightly larger than the actual card so the cursor always hovers a card when moving over a gap between two cards
                return cardInputArea.ReceivePositionalInputAt(screenSpacePos);
            }

            protected override bool OnHover(HoverEvent e)
            {
                CardHovered = true;

                return true;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                CardHovered = false;
            }

            protected override bool OnMouseDown(MouseDownEvent e)
            {
                if (e.Button == MouseButton.Left && AllowSelection.Value)
                {
                    CardPressed = true;

                    return true;
                }

                return false;
            }

            protected override void OnMouseUp(MouseUpEvent e)
            {
                if (e.Button == MouseButton.Left)
                    CardPressed = false;
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!AllowSelection.Value)
                    return false;

                Clicked(this);

                return true;
            }

            public override bool AcceptsFocus => true;

            public override bool ChangeFocusOnClick => false;

            protected override void OnFocus(FocusEvent e)
            {
                base.OnFocus(e);

                CardHovered = true;
            }

            protected override void OnFocusLost(FocusLostEvent e)
            {
                base.OnFocusLost(e);

                CardHovered = false;
            }
        }
    }
}
