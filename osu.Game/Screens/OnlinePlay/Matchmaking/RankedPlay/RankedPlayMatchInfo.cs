// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.RankedPlay;

namespace osu.Game.Screens.OnlinePlay.Matchmaking.RankedPlay
{
    public partial class RankedPlayMatchInfo : Component
    {
        /// <summary>
        /// Cards belonging to the player.
        /// </summary>
        public IReadOnlyList<RankedPlayCardWithPlaylistItem> PlayerCards => playerCards;

        /// <summary>
        /// Cards belonging to the opponent.
        /// </summary>
        public IReadOnlyList<RankedPlayCardWithPlaylistItem> OpponentCards => opponentCards;

        /// <summary>
        /// The last card that was played.
        /// </summary>
        public RankedPlayCardWithPlaylistItem? LastPlayedCard { get; private set; }

        /// <summary>
        /// The current room stage.
        /// </summary>
        public IBindable<RankedPlayStage> Stage => stage;

        /// <summary>
        /// Fired when a card gets added to the player's hand.
        /// </summary>
        public event Action<RankedPlayCardWithPlaylistItem>? PlayerCardAdded;

        /// <summary>
        /// Fired when a card gets removed from the player's hand, i.e. by being discarded.
        /// </summary>
        public event Action<RankedPlayCardWithPlaylistItem>? PlayerCardRemoved;

        /// <summary>
        /// Fired when a card gets added to the opponent's hand.
        /// </summary>
        public event Action<RankedPlayCardWithPlaylistItem>? OpponentCardAdded;

        /// <summary>
        /// Fired when a card gets removed from the player's hand, i.e. by being discarded.
        /// </summary>
        public event Action<RankedPlayCardWithPlaylistItem>? OpponentCardRemoved;

        /// <summary>
        /// Fired when the active player plays a card.
        /// </summary>
        public event Action<RankedPlayCardWithPlaylistItem>? CardPlayed;

        /// <summary>
        /// The player's health
        /// </summary>
        public readonly BindableInt PlayerHealth = new BindableInt { MinValue = 0, MaxValue = 1_000_000, Value = 1_000_000 };

        /// <summary>
        /// The opponent's health
        /// </summary>
        public readonly BindableInt OpponentHealth = new BindableInt { MinValue = 0, MaxValue = 1_000_000, Value = 1_000_000 };

        public RankedPlayRoomState RoomState { get; private set; } = null!;

        public bool IsOwnTurn => RoomState.ActiveUserId == client.LocalUser?.UserID;

        public int CurrentRound => RoomState.CurrentRound;

        public int OpponentId => RoomState.Users.Keys.Single(u => u != client.LocalUser?.UserID);

        private readonly List<RankedPlayCardWithPlaylistItem> playerCards = new List<RankedPlayCardWithPlaylistItem>();
        private readonly List<RankedPlayCardWithPlaylistItem> opponentCards = new List<RankedPlayCardWithPlaylistItem>();
        private readonly Bindable<RankedPlayStage> stage = new Bindable<RankedPlayStage>();

        [Resolved]
        private MultiplayerClient client { get; set; } = null!;

        private APIUser player = null!;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            player = client.LocalUser!.User!;

            client.MatchRoomStateChanged += onMatchRoomStateChanged;
            client.RankedPlayCardAdded += onCardAdded;
            client.RankedPlayCardRemoved += onCardRemoved;
            client.RankedPlayCardPlayed += onCardPlayed;

            var roomState = (RankedPlayRoomState)client.Room!.MatchState!;

            onMatchRoomStateChanged(roomState);

            foreach (var (userId, user) in roomState.Users)
            {
                foreach (var card in user.Hand)
                {
                    onCardAdded(userId, client.GetCardWithPlaylistItem(card));
                }
            }
        }

        private void onMatchRoomStateChanged(MatchRoomState state)
        {
            if (state is not RankedPlayRoomState roomState)
                return;

            RoomState = roomState;

            stage.Value = roomState.Stage;

            foreach (var (userId, userInfo) in roomState.Users)
            {
                if (userId == player.Id)
                    PlayerHealth.Value = userInfo.Life;
                else
                    OpponentHealth.Value = userInfo.Life;
            }
        }

        private void onCardAdded(int userId, RankedPlayCardWithPlaylistItem item)
        {
            if (userId == player.Id)
            {
                playerCards.Add(item);
                PlayerCardAdded?.Invoke(item);
            }
            else
            {
                opponentCards.Add(item);
                OpponentCardAdded?.Invoke(item);
            }
        }

        private void onCardRemoved(int userId, RankedPlayCardWithPlaylistItem item)
        {
            if (userId == player.Id)
            {
                playerCards.Remove(item);
                PlayerCardRemoved?.Invoke(item);
            }
            else
            {
                opponentCards.Remove(item);
                OpponentCardRemoved?.Invoke(item);
            }
        }

        private void onCardPlayed(RankedPlayCardWithPlaylistItem item)
        {
            LastPlayedCard = item;
            CardPlayed?.Invoke(item);
        }

        protected override void Dispose(bool isDisposing)
        {
            client.MatchRoomStateChanged -= onMatchRoomStateChanged;
            client.RankedPlayCardAdded -= onCardAdded;
            client.RankedPlayCardRemoved -= onCardRemoved;
            client.RankedPlayCardPlayed -= onCardPlayed;

            base.Dispose(isDisposing);
        }
    }
}
