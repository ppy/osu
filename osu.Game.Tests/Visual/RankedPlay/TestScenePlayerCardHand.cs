// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Overlays;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestScenePlayerCardHand : OsuManualInputManagerTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Purple);

        private PlayerHandOfCards handOfCards = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = handOfCards = new PlayerHandOfCards
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
                Height = 0.5f,
            };
        }

        [SetUpSteps]
        public void SetupSteps()
        {
            AddStep("reset card hand", () => Child = handOfCards = new PlayerHandOfCards
            {
                Anchor = Anchor.BottomCentre,
                Origin = Anchor.BottomCentre,
                RelativeSizeAxes = Axes.Both,
                Height = 0.5f,
            });
        }

        [Test]
        public void TestSingleSelectionMode()
        {
            AddStep("add cards", () =>
            {
                for (int i = 0; i < 5; i++)
                    handOfCards.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddStep("single selection mode", () => handOfCards.SelectionMode = HandSelectionMode.Single);

            AddStep("click first card", () => handOfCards.Cards.First().TriggerClick());
            AddAssert("first card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.First().Item]));

            AddStep("click second card", () => handOfCards.Cards.ElementAt(1).TriggerClick());
            AddAssert("second card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(1).Item]));

            AddStep("click second card again", () => handOfCards.Cards.ElementAt(1).TriggerClick());
            AddAssert("second card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(1).Item]));
        }

        [Test]
        public void TestMultiSelectionMode()
        {
            AddStep("add cards", () =>
            {
                for (int i = 0; i < 5; i++)
                    handOfCards.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddStep("multi selection mode", () => handOfCards.SelectionMode = HandSelectionMode.Multiple);

            AddStep("click first card", () => handOfCards.Cards.First().TriggerClick());
            AddAssert("first card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.First().Item]));

            AddStep("click second card", () => handOfCards.Cards.ElementAt(1).TriggerClick());
            AddAssert("both cards selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(0).Item, handOfCards.Cards.ElementAt(1).Item]));

            AddStep("click second card again", () => handOfCards.Cards.ElementAt(1).TriggerClick());
            AddAssert("first card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(0).Item]));
        }

        [Test]
        public void TestCardCount()
        {
            for (int i = 1; i <= 8; i++)
            {
                int numCards = i;

                AddStep($"{i} {"cards".Pluralize(i == 1)}", () =>
                {
                    Child = handOfCards = new PlayerHandOfCards
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        RelativeSizeAxes = Axes.Both,
                        Height = 0.5f,
                    };

                    for (int j = 0; j < numCards; j++)
                        handOfCards.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
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
                handOfCards.PlayCardAction = () => playActionTriggered = true;

                handOfCards.Clear();
                for (int i = 0; i < 5; i++)
                    handOfCards.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddStep("single selection mode", () => handOfCards.SelectionMode = HandSelectionMode.Single);

            for (int i = 0; i < 5; i++)
            {
                int i1 = i;
                Key key = Key.Number1 + i;

                AddStep($"key {i + 1}", () => InputManager.Key(key));
                AddAssert("first card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(i1).Item]));
            }

            AddStep("right arrow", () => InputManager.Key(Key.Right));
            AddAssert("first card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(0).Item]));

            AddStep("right arrow", () => InputManager.Key(Key.Right));
            AddAssert("second card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(1).Item]));

            AddStep("left arrow", () => InputManager.Key(Key.Left));
            AddAssert("first card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(0).Item]));

            AddStep("left arrow", () => InputManager.Key(Key.Left));
            AddAssert("last card selected", () => handOfCards.Selection.SequenceEqual([handOfCards.Cards.ElementAt(^1).Item]));

            AddStep("space", () => InputManager.Key(Key.Space));
            AddAssert("play action triggered", () => playActionTriggered);
        }

        [Test]
        public void TestKeyboardSelectionMultiSelection()
        {
            AddStep("add cards", () =>
            {
                for (int i = 0; i < 5; i++)
                    handOfCards.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddStep("multi selection mode", () => handOfCards.SelectionMode = HandSelectionMode.Multiple);

            for (int i = 0; i < 5; i++)
            {
                int i1 = i;
                Key key = Key.Number1 + i;

                AddStep($"key {i + 1}", () => InputManager.Key(key));
                AddAssert("card hovered", () => handOfCards.Cards.ElementAt(i1).CardHovered);

                AddAssert("card not selected", () => !handOfCards.Selection.Contains(handOfCards.Cards.ElementAt(i1).Card.Item));
                AddStep("space", () => InputManager.Key(Key.Space));
                AddAssert("card selected", () => handOfCards.Selection.Contains(handOfCards.Cards.ElementAt(i1).Card.Item));
            }
        }

        [Test]
        public void TestContract()
        {
            AddStep("add cards", () =>
            {
                for (int i = 0; i < 5; i++)
                    handOfCards.AddCard(new RankedPlayCardWithPlaylistItem(new RankedPlayCardItem()));
            });
            AddWaitStep("wait", 5);
            AddStep("contract", () => handOfCards.Contract());
            AddWaitStep("wait", 5);
            AddAssert(
                "all cards outside bounds", () =>
                    handOfCards
                        .ChildrenOfType<HandOfCards.HandCard>()
                        .All(card => !card.ScreenSpaceDrawQuad.AABBFloat.IntersectsWith(handOfCards.ScreenSpaceDrawQuad.AABBFloat))
            );
        }
    }
}
