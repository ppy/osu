// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Online.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Card;
using osuTK;
using osuTK.Input;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand
{
    /// <summary>
    /// Card hand representing the player's current hand, intended to be placed at the bottom edge of the screen.
    /// This version of the card hand reacts to player inputs like hovering a card.
    /// </summary>
    public partial class PlayerHandOfCards : HandOfCards
    {
        /// <summary>
        /// Fired if any card is selected or deselected
        /// </summary>
        public event Action? SelectionChanged;

        /// <summary>
        /// Fired if a card's <see cref="HandOfCards.HandCard.State"/> has changed
        /// </summary>
        public event Action? StateChanged;

        private HandSelectionMode selectionMode;

        /// <summary>
        /// Current selection mode.
        /// </summary>
        /// <remarks>
        /// <see cref="HandSelectionMode.Disabled"/> will disable some of the card's mouse interactions.
        /// </remarks>
        public HandSelectionMode SelectionMode
        {
            get => selectionMode;
            set
            {
                selectionMode = value;
                allowSelection.Value = value != HandSelectionMode.Disabled;

                if (value == HandSelectionMode.Disabled)
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
            Dragged = cardDragged,
            AllowSelection = allowSelection.GetBoundCopy(),
            PlayAction = PlayCardAction,
        };

        private void cardClicked(PlayerHandCard card)
        {
            if (selectionMode == HandSelectionMode.Disabled)
                return;

            try
            {
                if (selectionMode == HandSelectionMode.Single)
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

        protected override void OnCardStateChanged(HandCard card, ValueChangedEvent<RankedPlayCardState> evt)
        {
            StateChanged?.Invoke();

            base.OnCardStateChanged(card, evt);
        }

        public Dictionary<Guid, RankedPlayCardState> State => Cards.Select(static card => new KeyValuePair<Guid, RankedPlayCardState>(card.Item.Card.ID, card.State)).ToDictionary();

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat || Contracted || Cards.Any(static c => c.CardDragged))
                return false;

            switch (e.Key)
            {
                case >= Key.Number1 and <= Key.Number9:
                    focusCard(e.Key - Key.Number1);
                    return true;

                case Key.Space:
                    if (selectionMode == HandSelectionMode.Disabled)
                        return false;

                    if (Cards.FirstOrDefault(it => it.HasFocus) is not PlayerHandCard card)
                        return false;

                    if (card.Selected && card.PlayAction != null)
                        card.PlayButton.TriggerClick();
                    else
                        card.TriggerClick();

                    return true;

                case Key.Left:
                    moveCardFocus(-1);
                    return true;

                case Key.Right:
                    moveCardFocus(1);
                    return true;
            }

            return base.OnKeyDown(e);
        }

        private void moveCardFocus(int direction)
        {
            int currentIndex = Cards.ToList().FindIndex(c => c.HasFocus);

            // default behaviour is to start from either end of the cards if no card is focused currently
            // in single-selection mode we can however use the current selection as a fallback index if there's no focus
            if (selectionMode == HandSelectionMode.Single && currentIndex == -1)
                currentIndex = Cards.ToList().FindIndex(c => c.Selected);

            int newIndex = currentIndex + direction;

            if (newIndex < 0)
                newIndex = Cards.Count - 1;
            else if (newIndex >= Cards.Count)
                newIndex = 0;

            focusCard(newIndex);
        }

        private void focusCard(int index)
        {
            var card = Cards.ElementAtOrDefault(index);

            if (card == null)
                return;

            GetContainingFocusManager()?.ChangeFocus(card);

            if (SelectionMode == HandSelectionMode.Single && !card.Selected)
                card.TriggerClick();
        }

        private void cardDragged(PlayerHandCard card, Vector2 screenSpacePosition)
        {
            var cards = Cards.OrderBy(static c => c.Order).ToArray();

            int newIndex = cardIndexInLayout(cards, card.ScreenSpaceDrawQuad.Centre);

            card.Order = newIndex;

            int order = 0;

            foreach (var c in cards)
            {
                if (order == newIndex)
                    order++;

                if (c == card)
                    continue;

                c.Order = order++;
            }

            foreach (var c in Cards)
                c.Item.DisplayOrder = c.Order;
        }

        private int cardIndexInLayout(HandCard[] cards, Vector2 screenSpacePosition)
        {
            Debug.Assert(cards.Length > 0);

            var position = ToLocalSpace(screenSpacePosition) - DrawSize / 2;

            int activeIndex = GetActiveCardIndex(cards);

            int minIndex = 0;
            float minDistance = float.MaxValue;

            for (int i = 0; i < cards.Length; i++)
            {
                float distance = MathF.Abs(GetCardX(i, activeIndex) - position.X);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    minIndex = i;
                }
            }

            return minIndex;
        }
    }
}
