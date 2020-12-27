// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Online.API;
using osu.Game.Online.Multiplayer;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestMultiplayerClient : StatefulMultiplayerClient
    {
        public override IBindable<bool> IsConnected => isConnected;
        private readonly Bindable<bool> isConnected = new Bindable<bool>(true);

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        public void Connect() => isConnected.Value = true;

        public void Disconnect() => isConnected.Value = false;

        public void AddUser(User user) => ((IMultiplayerClient)this).UserJoined(new MultiplayerRoomUser(user.Id) { User = user });

        public void RemoveUser(User user)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserLeft(new MultiplayerRoomUser(user.Id));

            Schedule(() =>
            {
                if (Room.Users.Any())
                    TransferHost(Room.Users.First().UserID);
            });
        }

        public void ChangeUserState(int userId, MultiplayerUserState newState)
        {
            Debug.Assert(Room != null);

            ((IMultiplayerClient)this).UserStateChanged(userId, newState);

            Schedule(() =>
            {
                switch (newState)
                {
                    case MultiplayerUserState.Loaded:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.Loaded))
                                ChangeUserState(u.UserID, MultiplayerUserState.Playing);

                            ((IMultiplayerClient)this).MatchStarted();
                        }

                        break;

                    case MultiplayerUserState.FinishedPlay:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.Playing))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                                ChangeUserState(u.UserID, MultiplayerUserState.Results);

                            ((IMultiplayerClient)this).ResultsReady();
                        }

                        break;
                }
            });
        }

        protected override Task<MultiplayerRoom> JoinRoom(long roomId)
        {
            var user = new MultiplayerRoomUser(api.LocalUser.Value.Id) { User = api.LocalUser.Value };

            var room = new MultiplayerRoom(roomId);
            room.Users.Add(user);

            if (room.Users.Count == 1)
                room.Host = user;

            return Task.FromResult(room);
        }

        public override Task TransferHost(int userId) => ((IMultiplayerClient)this).HostChanged(userId);

        public override async Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            Debug.Assert(Room != null);

            await ((IMultiplayerClient)this).SettingsChanged(settings);

            foreach (var user in Room.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.Idle);
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            ChangeUserState(api.LocalUser.Value.Id, newState);
            return Task.CompletedTask;
        }

        public override Task StartMatch()
        {
            Debug.Assert(Room != null);

            foreach (var user in Room.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.WaitingForLoad);

            return ((IMultiplayerClient)this).LoadRequested();
        }
    }
}
