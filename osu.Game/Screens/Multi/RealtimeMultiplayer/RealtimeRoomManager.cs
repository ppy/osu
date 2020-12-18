// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.RealtimeMultiplayer;
using osu.Game.Screens.Multi.Components;

namespace osu.Game.Screens.Multi.RealtimeMultiplayer
{
    public class RealtimeRoomManager : RoomManager
    {
        [Resolved]
        private StatefulMultiplayerClient multiplayerClient { get; set; }

        public readonly Bindable<double> TimeBetweenListingPolls = new Bindable<double>();
        public readonly Bindable<double> TimeBetweenSelectionPolls = new Bindable<double>();
        private readonly IBindable<bool> isConnected = new Bindable<bool>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            isConnected.BindTo(multiplayerClient.IsConnected);
            isConnected.BindValueChanged(connected => Schedule(() =>
            {
                if (!connected.NewValue)
                    ClearRooms();
            }));
        }

        public override void CreateRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            => base.CreateRoom(room, r => joinMultiplayerRoom(r, onSuccess), onError);

        public override void JoinRoom(Room room, Action<Room> onSuccess = null, Action<string> onError = null)
            => base.JoinRoom(room, r => joinMultiplayerRoom(r, onSuccess), onError);

        private void joinMultiplayerRoom(Room room, Action<Room> onSuccess = null)
        {
            Debug.Assert(room.RoomID.Value != null);

            var joinTask = multiplayerClient.JoinRoom(room);
            joinTask.ContinueWith(_ => onSuccess?.Invoke(room));
            joinTask.ContinueWith(t =>
            {
                PartRoom();
                if (t.Exception != null)
                    Logger.Error(t.Exception, "Failed to join multiplayer room.");
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }

        protected override RoomPollingComponent[] CreatePollingComponents() => new RoomPollingComponent[]
        {
            new RealtimeListingPollingComponent
            {
                TimeBetweenPolls = { BindTarget = TimeBetweenListingPolls },
                AllowPolling = { BindTarget = isConnected }
            },
            new RealtimeSelectionPollingComponent
            {
                TimeBetweenPolls = { BindTarget = TimeBetweenSelectionPolls },
                AllowPolling = { BindTarget = isConnected }
            }
        };

        private class RealtimeListingPollingComponent : ListingPollingComponent
        {
            public readonly IBindable<bool> AllowPolling = new Bindable<bool>();

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AllowPolling.BindValueChanged(_ =>
                {
                    if (IsLoaded)
                        PollImmediately();
                });
            }

            protected override Task Poll() => !AllowPolling.Value ? Task.CompletedTask : base.Poll();
        }

        private class RealtimeSelectionPollingComponent : SelectionPollingComponent
        {
            public readonly IBindable<bool> AllowPolling = new Bindable<bool>();

            protected override void LoadComplete()
            {
                base.LoadComplete();

                AllowPolling.BindValueChanged(_ =>
                {
                    if (IsLoaded)
                        PollImmediately();
                });
            }

            protected override Task Poll() => !AllowPolling.Value ? Task.CompletedTask : base.Poll();
        }
    }
}
