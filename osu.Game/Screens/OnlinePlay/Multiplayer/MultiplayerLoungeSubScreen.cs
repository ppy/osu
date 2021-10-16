// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Screens.OnlinePlay.Components;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Lounge.Components;
using osu.Game.Screens.OnlinePlay.Match;

namespace osu.Game.Screens.OnlinePlay.Multiplayer
{
    public class MultiplayerLoungeSubScreen : LoungeSubScreen
    {
        [Resolved]
        private IAPIProvider api { get; set; }

        [Resolved]
        private MultiplayerClient client { get; set; }

        public override void OnResuming(IScreen last)
        {
            base.OnResuming(last);

            // Upon having left a room, we don't know whether we were the only participant, and whether the room is now closed as a result of leaving it.
            // To work around this, temporarily remove the room and trigger an immediate listing poll.
            if (last is MultiplayerMatchSubScreen match)
            {
                RoomManager.RemoveRoom(match.Room);
                ListingPollingComponent.PollImmediately();
            }
        }

        protected override FilterCriteria CreateFilterCriteria()
        {
            var criteria = base.CreateFilterCriteria();
            criteria.Category = @"realtime";
            return criteria;
        }

        protected override OsuButton CreateNewRoomButton() => new CreateMultiplayerMatchButton();

        protected override Room CreateNewRoom() => new Room
        {
            Name = { Value = $"{api.LocalUser}'s awesome room" },
            Type = { Value = MatchType.HeadToHead },
        };

        protected override RoomSubScreen CreateRoomSubScreen(Room room) => new MultiplayerMatchSubScreen(room);

        protected override ListingPollingComponent CreatePollingComponent() => new MultiplayerListingPollingComponent();

        protected override void OpenNewRoom(Room room)
        {
            if (client?.IsConnected.Value != true)
            {
                Logger.Log("Not currently connected to the multiplayer server.", LoggingTarget.Runtime, LogLevel.Important);
                return;
            }

            base.OpenNewRoom(room);
        }

        private class MultiplayerListingPollingComponent : ListingPollingComponent
        {
            [Resolved]
            private MultiplayerClient client { get; set; }

            private readonly IBindable<bool> isConnected = new Bindable<bool>();

            [BackgroundDependencyLoader]
            private void load()
            {
                isConnected.BindTo(client.IsConnected);
                isConnected.BindValueChanged(c => Scheduler.AddOnce(poll), true);
            }

            private void poll()
            {
                if (isConnected.Value && IsLoaded)
                    PollImmediately();
            }

            protected override Task Poll()
            {
                if (!isConnected.Value)
                    return Task.CompletedTask;

                return base.Poll();
            }
        }
    }
}
