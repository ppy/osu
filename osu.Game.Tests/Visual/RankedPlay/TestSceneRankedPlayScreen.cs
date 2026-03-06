// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Extensions;
using osu.Framework.Testing;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay;
using osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay.Hand;
using osuTK.Input;

namespace osu.Game.Tests.Visual.RankedPlay
{
    public partial class TestSceneRankedPlayScreen : RankedPlayTestScene
    {
        private RankedPlayScreen screen = null!;

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("join room", () => JoinRoom(CreateDefaultRoom(MatchType.RankedPlay)));
            WaitForJoined();

            AddStep("join other user", () => MultiplayerClient.AddUser(new APIUser { Id = 2 }));

            AddStep("load screen", () => LoadScreen(screen = new RankedPlayScreen(MultiplayerClient.ClientRoom!)));
        }

        [Test]
        public void TestIntroStage()
        {
            AddStep("set round warmup phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.RoundWarmup, s => s.StarRating = 6.3f).WaitSafely());
        }

        [Test]
        public void TestDiscardCardsStage()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            AddWaitStep("wait", 3);

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep($"click card {i2}", () =>
                {
                    InputManager.MoveMouseTo(this.ChildrenOfType<PlayerHandOfCards.PlayerHandCard>().ElementAt(i2));
                    InputManager.Click(MouseButton.Left);
                });
            }

            AddWaitStep("wait", 3);

            AddStep("click discard button", () =>
            {
                var button = screen.ChildrenOfType<ShearedButton>().Single(it => it.Name == "Discard Button");

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });

            AddWaitStep("wait", 13);
            AddStep("set finish discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.FinishCardDiscard).WaitSafely());
        }

        [Test]
        public void TestAddRemoveCards()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            for (int i = 0; i < 3; i++)
                AddStep("add card", () => MultiplayerClient.RankedPlayAddCards([new RankedPlayCardItem()]).WaitSafely());

            for (int i = 0; i < 3; i++)
                AddStep("remove card", () => MultiplayerClient.RankedPlayRemoveCards(hand => [hand[0]]).WaitSafely());
        }

        [Test]
        public void TestRevealCards()
        {
            var requestHandler = new BeatmapRequestHandler();

            AddStep("setup request handler", () => ((DummyAPIAccess)API).HandleRequest = requestHandler.HandleRequest);

            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep("reveal card", () => MultiplayerClient.RankedPlayRevealCard(hand => hand[i2], new MultiplayerPlaylistItem
                {
                    ID = i2,
                    BeatmapID = requestHandler.Beatmaps[i2].OnlineID
                }).WaitSafely());
            }
        }

        [Test]
        public void TestPlayCardDirect()
        {
            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = API.LocalUser.Value.OnlineID).WaitSafely());
            AddWaitStep("wait", 3);
            AddStep("play card", () => MultiplayerClient.PlayCard(hand => hand[0]).WaitSafely());
        }

        [Test]
        public void TestDiscardCardsDirect()
        {
            AddStep("set discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardDiscard).WaitSafely());
            AddWaitStep("wait", 3);
            AddStep("discard cards", () => MultiplayerClient.DiscardCards(hand => hand.Take(3)).WaitSafely());
            AddWaitStep("wait", 13);
            AddStep("set finish discard phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.FinishCardDiscard).WaitSafely());
        }

        [Test]
        public void TestPlayStage()
        {
            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = API.LocalUser.Value.OnlineID).WaitSafely());
            AddUntilStep("wait until cards are present", () => this.ChildrenOfType<PlayerHandOfCards.PlayerHandCard>().Count() == 5);

            for (int i = 0; i < 3; i++)
            {
                int i2 = i;
                AddStep($"click card {i2}", () =>
                {
                    InputManager.MoveMouseTo(this.ChildrenOfType<PlayerHandOfCards.PlayerHandCard>().ElementAt(i2));
                    InputManager.Click(MouseButton.Left);
                });
            }

            AddWaitStep("wait", 3);

            AddStep("click play button", () =>
            {
                var button = screen
                             .ChildrenOfType<PlayerHandOfCards.PlayerHandCard>()
                             .First(it => it.Selected)
                             .ChildrenOfType<ShearedButton>()
                             .First();

                InputManager.MoveMouseTo(button);
                InputManager.Click(MouseButton.Left);
            });
        }

        [Test]
        public void TestOtherPlaysCard()
        {
            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = 2).WaitSafely());
            AddWaitStep("wait", 5);
            AddStep("play beatmap", () => MultiplayerClient.PlayUserCard(2, hand => hand[0]).WaitSafely());
            AddStep("reveal card", () => MultiplayerClient.RankedPlayRevealUserCard(2, hand => hand[0], new MultiplayerPlaylistItem
            {
                ID = 0,
                BeatmapID = 0
            }).WaitSafely());
        }

        [Test]
        public void TestHealthChange()
        {
            AddStep("set play phase", () => MultiplayerClient.RankedPlayChangeStage(RankedPlayStage.CardPlay, state => state.ActiveUserId = 2).WaitSafely());
            AddWaitStep("wait", 5);
            AddStep("change player 1 health", () => MultiplayerClient.RankedPlayChangeUserState(MultiplayerClient.LocalUser!.UserID, state => state.Life = 250_000).WaitSafely());
            AddWaitStep("wait", 5);
            AddStep("change player 2 health", () => MultiplayerClient.RankedPlayChangeUserState(2, state => state.Life = 250_000).WaitSafely());
        }
    }
}
