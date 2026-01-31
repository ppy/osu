// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.RankedPlay;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards
{
    /// <summary>
    /// Card hand representing the player's current hand, intended to be placed at the bottom edge of the screen.
    /// This version of the card hand reacts to player inputs like hovering a card.
    /// </summary>
    public partial class PlayerCardHand : CardHand
    {
        /// <summary>
        /// Fired if any card is selected or deselected
        /// </summary>
        public event Action? SelectionChanged;

        /// <summary>
        /// Fired if a card's <see cref="CardHand.HandCard.State"/> has changed
        /// </summary>
        public event Action? StateChanged;

        private CardSelectionMode selectionMode;

        /// <summary>
        /// Current selection mode.
        /// </summary>
        /// <remarks>
        /// <see cref="CardSelectionMode.Disabled"/> will disable some of the card's mouse interactions.
        /// </remarks>
        public CardSelectionMode SelectionMode
        {
            get => selectionMode;
            set
            {
                selectionMode = value;
                allowSelection.Value = value != CardSelectionMode.Disabled;

                if (value == CardSelectionMode.Disabled)
                {
                    foreach (var card in Cards)
                        card.Selected = false;
                }
            }
        }

        private Action? playCardAction;

        /// <summary>
        /// When set to non-null, displays a "Play" button on the selected card that invokes this action.
        /// </summary>
        public Action? PlayCardAction
        {
            get => playCardAction;
            set
            {
                playCardAction = value;

                foreach (var card in Cards.OfType<PlayerHandCard>())
                    card.PlayAction = value;
            }
        }

        private IEnumerable<PlayerHandCard> selection => Cards.OfType<PlayerHandCard>().Where(it => it.Selected);

        /// <summary>
        /// Currently selected cards.
        /// </summary>
        public IEnumerable<RankedPlayCardWithPlaylistItem> Selection => selection.Select(it => it.Card.Item);

        private readonly BindableBool allowSelection = new BindableBool();

        private const int select_samples = 1;
        private const int deselect_samples = 2;

        private Sample?[]? cardSelectSamples;
        private Sample?[]? cardDeselectSamples;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            cardSelectSamples = new Sample?[select_samples];
            for (int i = 0; i < select_samples; i++)
                cardSelectSamples[i] = audio.Samples.Get(@$"Multiplayer/Matchmaking/Ranked/card-select-{i + 1}");

            cardDeselectSamples = new Sample?[deselect_samples];
            for (int i = 0; i < deselect_samples; i++)
                cardDeselectSamples[i] = audio.Samples.Get(@$"Multiplayer/Matchmaking/Ranked/card-deselect-{i + 1}");
        }

        protected override HandCard CreateHandCard(RankedPlayCard card) => new PlayerHandCard(card)
        {
            Clicked = cardClicked,
            AllowSelection = allowSelection.GetBoundCopy(),
            PlayAction = PlayCardAction,
        };

        private void cardClicked(PlayerHandCard card)
        {
            if (selectionMode == CardSelectionMode.Disabled)
                return;

            try
            {
                if (selectionMode == CardSelectionMode.Single)
                {
                    // only play feedback SFX if the selected card has changed
                    if (!card.Selected)
                        SamplePlaybackHelper.PlayWithRandomPitch(cardSelectSamples);

                    foreach (var c in Cards)
                    {
                        ((PlayerHandCard)c).Selected = c == card;
                    }

                    return;
                }

                card.Selected = !card.Selected;

                SamplePlaybackHelper.PlayWithRandomPitch(card.Selected ? cardSelectSamples : cardDeselectSamples);
            }
            finally
            {
                SelectionChanged?.Invoke();
            }
        }

        protected override void OnCardStateChanged(HandCard card, RankedPlayCardState state)
        {
            StateChanged?.Invoke();

            base.OnCardStateChanged(card, state);
        }

        public Dictionary<Guid, RankedPlayCardState> State => Cards.Select(static card => new KeyValuePair<Guid, RankedPlayCardState>(card.Item.Card.ID, card.State)).ToDictionary();

        public partial class PlayerHandCard : HandCard
        {
            public Action? PlayAction
            {
                set
                {
                    playButton.Action = value;
                    updatePlayButtonVisibility();
                }
            }

            public required Action<PlayerHandCard> Clicked;

            public required IBindable<bool> AllowSelection;

            private readonly Drawable cardInputArea;
            private readonly Drawable fullInputArea;

            private readonly ShearedButton playButton;

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
                            Child = playButton = new ShearedButton(width: 90f, height: 30f)
                            {
                                Name = "Play Button",
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
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
                playButton.Alpha = playButton.Action != null && Selected ? 1 : 0;
            }

            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
            {
                if (playButton.Alpha > 0)
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
        }
    }
}
