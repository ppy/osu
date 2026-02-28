// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Cards;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestScenePlayerCardHand : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private PlayerCardHand cardHand = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = cardHand = new PlayerCardHand
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
                Height = 0.5f,
            };
        }

        [Test]
        public void TestSingleSelectionMode()
        {
            AddStep("add cards", () =>
            {
                cardHand.Clear();
                for (int i = 0; i < 5; i++)
                    cardHand.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddStep("single selection mode", () => cardHand.SelectionMode = CardSelectionMode.Single);

            AddStep("click first card", () => cardHand.Cards.First().TriggerClick());
            AddAssert("first card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.First().Item]));

            AddStep("click second card", () => cardHand.Cards.ElementAt(1).TriggerClick());
            AddAssert("second card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(1).Item]));

            AddStep("click second card again", () => cardHand.Cards.ElementAt(1).TriggerClick());
            AddAssert("second card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(1).Item]));
        }

        [Test]
        public void TestMultiSelectionMode()
        {
            AddStep("add cards", () =>
            {
                cardHand.Clear();
                for (int i = 0; i < 5; i++)
                    cardHand.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddStep("multi selection mode", () => cardHand.SelectionMode = CardSelectionMode.Multiple);

            AddStep("click first card", () => cardHand.Cards.First().TriggerClick());
            AddAssert("first card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.First().Item]));

            AddStep("click second card", () => cardHand.Cards.ElementAt(1).TriggerClick());
            AddAssert("both cards selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(0).Item, cardHand.Cards.ElementAt(1).Item]));

            AddStep("click second card again", () => cardHand.Cards.ElementAt(1).TriggerClick());
            AddAssert("first card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(0).Item]));
        }

        [Test]
        public void TestCardCount()
        {
            for (int i = 1; i <= 8; i++)
            {
                int numCards = i;

                AddStep($"{i} {"cards".Pluralize(i == 1)}", () =>
                {
                    cardHand.Clear();

                    for (int j = 0; j < numCards; j++)
                        cardHand.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
                });
            }
        }

        [Test]
        public void TestKeyboardSelectionSingleSelection()
        {
            bool playActionTriggered = false;

            AddStep("add cards", () =>
            {
                playActionTriggered = false;
                cardHand.PlayCardAction = () => playActionTriggered = true;

                cardHand.Clear();
                for (int i = 0; i < 5; i++)
                    cardHand.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddStep("single selection mode", () => cardHand.SelectionMode = CardSelectionMode.Single);

            for (int i = 0; i < 5; i++)
            {
                int i1 = i;
                Key key = Key.Number1 + i;

                AddStep($"key {i + 1}", () => InputManager.Key(key));
                AddAssert("first card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(i1).Item]));
            }

            AddStep("right arrow", () => InputManager.Key(Key.Right));
            AddAssert("first card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(0).Item]));

            AddStep("right arrow", () => InputManager.Key(Key.Right));
            AddAssert("second card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(1).Item]));

            AddStep("left arrow", () => InputManager.Key(Key.Left));
            AddAssert("first card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(0).Item]));

            AddStep("left arrow", () => InputManager.Key(Key.Left));
            AddAssert("last card selected", () => cardHand.Selection.SequenceEqual([cardHand.Cards.ElementAt(^1).Item]));

            AddStep("space", () => InputManager.Key(Key.Space));
            AddAssert("play action triggered", () => playActionTriggered);
        }

        [Test]
        public void TestKeyboardSelectionMultiSelection()
        {
            AddStep("add cards", () =>
            {
                cardHand.Clear();
                for (int i = 0; i < 5; i++)
                    cardHand.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddStep("multi selection mode", () => cardHand.SelectionMode = CardSelectionMode.Multiple);

            for (int i = 0; i < 5; i++)
            {
                int i1 = i;
                Key key = Key.Number1 + i;

                AddStep($"key {i + 1}", () => InputManager.Key(key));
                AddAssert("card hovered", () => cardHand.Cards.ElementAt(i1).CardHovered);

                AddAssert("card not selected", () => !cardHand.Selection.Contains(cardHand.Cards.ElementAt(i1).Card.Item));
                AddStep("space", () => InputManager.Key(Key.Space));
                AddAssert("card selected", () => cardHand.Selection.Contains(cardHand.Cards.ElementAt(i1).Card.Item));
            }
        }
    }
}
