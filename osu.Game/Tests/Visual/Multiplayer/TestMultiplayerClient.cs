// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.MatchTypes.TeamVersus;
using osu.Game.Online.Multiplayer.Queueing;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using APIUser = osu.Game.Online.API.Requests.Responses.APIUser;

namespace osu.Game.Tests.Visual.Multiplayer
{
    /// <summary>
    /// A <see cref="MultiplayerClient"/> for use in multiplayer test scenes. Should generally not be used by itself outside of a <see cref="MultiplayerTestScene"/>.
    /// </summary>
    public class TestMultiplayerClient : MultiplayerClient
    {
        public override IBindable<bool> IsConnected => isConnected;
        private readonly Bindable<bool> isConnected = new Bindable<bool>(true);

        public new Room? APIRoom => base.APIRoom;

        public Action<MultiplayerRoom>? RoomSetupAction;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        private readonly TestMultiplayerRoomManager roomManager;
        private PlaylistItem? currentItem;

        public TestMultiplayerClient(TestMultiplayerRoomManager roomManager)
        {
            this.roomManager = roomManager;
        }

        public void Connect() => isConnected.Value = true;

        public void Disconnect() => isConnected.Value = false;

        public MultiplayerRoomUser AddUser(APIUser user, bool markAsPlaying = false)
        {
            var roomUser = new MultiplayerRoomUser(user.Id) { User = user };

            addUser(roomUser);

            if (markAsPlaying)
                PlayingUserIds.Add(user.Id);

            return roomUser;
        }

        public void TestAddUnresolvedUser() => addUser(new MultiplayerRoomUser(TestUserLookupCache.UNRESOLVED_USER_ID));

        private void addUser(MultiplayerRoomUser user)
        {
            ((IMultiplayerClient)this).UserJoined(user).Wait();

            // We want the user to be immediately available for testing, so force a scheduler update to run the update-bound continuation.
            Scheduler.Update();

            switch (Room?.MatchState)
            {
                case TeamVersusRoomState teamVersus:
                    Debug.Assert(Room != null);

                    // simulate the server's automatic assignment of users to teams on join.
                    // the "best" team is the one with the least users on it.
                    int bestTeam = teamVersus.Teams
                                             .Select(team => (teamID: team.ID, userCount: Room.Users.Count(u => (u.MatchState as TeamVersusUserState)?.TeamID == team.ID)))
                                             .OrderBy(pair => pair.userCount)
                                             .First().teamID;
                    ((IMultiplayerClient)this).MatchUserStateChanged(user.UserID, new TeamVersusUserState { TeamID = bestTeam }).Wait();
                    break;
            }
        }

        public void RemoveUser(APIUser user)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserLeft(new MultiplayerRoomUser(user.Id));

            Schedule(() =>
            {
                if (Room.Users.Any())
                    TransferHost(Room.Users.First().UserID);
            });
        }

        public void ChangeRoomState(MultiplayerRoomState newState)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).RoomStateChanged(newState);
        }

        public void ChangeUserState(int userId, MultiplayerUserState newState)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserStateChanged(userId, newState);

            Schedule(() =>
            {
                switch (Room.State)
                {
                    case MultiplayerRoomState.WaitingForLoad:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.WaitingForLoad))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.Loaded))
                                ChangeUserState(u.UserID, MultiplayerUserState.Playing);

                            ((IMultiplayerClient)this).MatchStarted();

                            ChangeRoomState(MultiplayerRoomState.Playing);
                        }

                        break;

                    case MultiplayerRoomState.Playing:
                        if (Room.Users.All(u => u.State != MultiplayerUserState.Playing))
                        {
                            foreach (var u in Room.Users.Where(u => u.State == MultiplayerUserState.FinishedPlay))
                                ChangeUserState(u.UserID, MultiplayerUserState.Results);
                            ChangeRoomState(MultiplayerRoomState.Open);

                            ((IMultiplayerClient)this).ResultsReady();

                            Task.Run(finishCurrentItem);
                        }

                        break;
                }
            });
        }

        public void ChangeUserBeatmapAvailability(int userId, BeatmapAvailability newBeatmapAvailability)
        {
            Debug.Assert(Room != null);

            ((IMultiplayerClient)this).UserBeatmapAvailabilityChanged(userId, newBeatmapAvailability);
        }

        protected override async Task<MultiplayerRoom> JoinRoom(long roomId, string? password = null)
        {
            var apiRoom = roomManager.ServerSideRooms.Single(r => r.RoomID.Value == roomId);

            if (password != apiRoom.Password.Value)
                throw new InvalidOperationException("Invalid password.");

            var localUser = new MultiplayerRoomUser(api.LocalUser.Value.Id)
            {
                User = api.LocalUser.Value
            };

            await updateCurrentItem(apiRoom: apiRoom).ConfigureAwait(false);
            Debug.Assert(currentItem != null);

            var room = new MultiplayerRoom(roomId)
            {
                Settings =
                {
                    Name = apiRoom.Name.Value,
                    MatchType = apiRoom.Type.Value,
                    PlaylistItemId = currentItem.ID,
                    Password = password,
                    QueueMode = apiRoom.QueueMode.Value
                },
                Users = { localUser },
                Host = localUser
            };

            RoomSetupAction?.Invoke(room);
            RoomSetupAction = null;

            return room;
        }

        protected override void OnRoomJoined()
        {
            Debug.Assert(Room != null);

            // emulate the server sending this after the join room. scheduler required to make sure the join room event is fired first (in Join).
            changeMatchType(Room.Settings.MatchType).Wait();
        }

        protected override Task LeaveRoomInternal() => Task.CompletedTask;

        public override Task TransferHost(int userId) => ((IMultiplayerClient)this).HostChanged(userId);

        public override Task KickUser(int userId)
        {
            Debug.Assert(Room != null);

            return ((IMultiplayerClient)this).UserKicked(Room.Users.Single(u => u.UserID == userId));
        }

        public override async Task ChangeSettings(MultiplayerRoomSettings settings)
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            // Server is authoritative for the time being.
            settings.PlaylistItemId = Room.Settings.PlaylistItemId;

            await changeQueueMode(settings.QueueMode).ConfigureAwait(false);

            await ((IMultiplayerClient)this).SettingsChanged(settings).ConfigureAwait(false);

            foreach (var user in Room.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.Idle);

            await changeMatchType(settings.MatchType).ConfigureAwait(false);
        }

        public override Task ChangeState(MultiplayerUserState newState)
        {
            ChangeUserState(api.LocalUser.Value.Id, newState);
            return Task.CompletedTask;
        }

        public override Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability)
        {
            ChangeUserBeatmapAvailability(api.LocalUser.Value.Id, newBeatmapAvailability);
            return Task.CompletedTask;
        }

        public void ChangeUserMods(int userId, IEnumerable<Mod> newMods)
            => ChangeUserMods(userId, newMods.Select(m => new APIMod(m)).ToList());

        public void ChangeUserMods(int userId, IEnumerable<APIMod> newMods)
        {
            Debug.Assert(Room != null);
            ((IMultiplayerClient)this).UserModsChanged(userId, newMods.ToList());
        }

        public override Task ChangeUserMods(IEnumerable<APIMod> newMods)
        {
            ChangeUserMods(api.LocalUser.Value.Id, newMods);
            return Task.CompletedTask;
        }

        public override async Task SendMatchRequest(MatchUserRequest request)
        {
            Debug.Assert(Room != null);
            Debug.Assert(LocalUser != null);

            switch (request)
            {
                case ChangeTeamRequest changeTeam:

                    TeamVersusRoomState roomState = (TeamVersusRoomState)Room.MatchState!;
                    TeamVersusUserState userState = (TeamVersusUserState)LocalUser.MatchState!;

                    var targetTeam = roomState.Teams.FirstOrDefault(t => t.ID == changeTeam.TeamID);

                    if (targetTeam != null)
                    {
                        userState.TeamID = targetTeam.ID;

                        await ((IMultiplayerClient)this).MatchUserStateChanged(LocalUser.UserID, userState).ConfigureAwait(false);
                    }

                    break;
            }
        }

        public override Task StartMatch()
        {
            Debug.Assert(Room != null);

            ChangeRoomState(MultiplayerRoomState.WaitingForLoad);
            foreach (var user in Room.Users.Where(u => u.State == MultiplayerUserState.Ready))
                ChangeUserState(user.UserID, MultiplayerUserState.WaitingForLoad);

            return ((IMultiplayerClient)this).LoadRequested();
        }

        public override async Task AddPlaylistItem(APIPlaylistItem item)
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            if (Room.Settings.QueueMode == QueueModes.HostOnly && Room.Host?.UserID != LocalUser?.UserID)
                throw new InvalidOperationException("Local user is not the room host.");

            switch (Room.Settings.QueueMode)
            {
                case QueueModes.HostOnly:
                    // In host-only mode, the current item is re-used.
                    item.ID = currentItem.ID;
                    await ((IMultiplayerClient)this).PlaylistItemChanged(item).ConfigureAwait(false);

                    // Note: Unlike the server, this is the easiest way to update the current item at this point.
                    await updateCurrentItem(false).ConfigureAwait(false);
                    break;

                default:
                    item.ID = APIRoom.Playlist.Last().ID + 1;
                    await ((IMultiplayerClient)this).PlaylistItemAdded(item).ConfigureAwait(false);
                    await updateCurrentItem().ConfigureAwait(false);
                    break;
            }
        }

        public override Task RemovePlaylistItem(long playlistItemId)
        {
            Debug.Assert(Room != null);

            if (Room.Host?.UserID != LocalUser?.UserID)
                throw new InvalidOperationException("Local user is not the room host.");

            return ((IMultiplayerClient)this).PlaylistItemRemoved(playlistItemId);
        }

        protected override Task<APIBeatmapSet> GetOnlineBeatmapSet(int beatmapId, CancellationToken cancellationToken = default)
        {
            Debug.Assert(Room != null);

            var apiRoom = roomManager.ServerSideRooms.Single(r => r.RoomID.Value == Room.RoomID);
            IBeatmapSetInfo? set = apiRoom.Playlist.FirstOrDefault(p => p.BeatmapID == beatmapId)?.Beatmap.Value.BeatmapSet
                                   ?? beatmaps.QueryBeatmap(b => b.OnlineID == beatmapId)?.BeatmapSet;

            if (set == null)
                throw new InvalidOperationException("Beatmap not found.");

            var apiSet = new APIBeatmapSet
            {
                OnlineID = set.OnlineID,
                Beatmaps = set.Beatmaps.Select(b => new APIBeatmap { OnlineID = b.OnlineID }).ToArray(),
            };

            return Task.FromResult(apiSet);
        }

        private async Task changeMatchType(MatchType type)
        {
            Debug.Assert(Room != null);

            switch (type)
            {
                case MatchType.HeadToHead:
                    await ((IMultiplayerClient)this).MatchRoomStateChanged(null).ConfigureAwait(false);

                    foreach (var user in Room.Users)
                        await ((IMultiplayerClient)this).MatchUserStateChanged(user.UserID, null).ConfigureAwait(false);
                    break;

                case MatchType.TeamVersus:
                    await ((IMultiplayerClient)this).MatchRoomStateChanged(TeamVersusRoomState.CreateDefault()).ConfigureAwait(false);

                    foreach (var user in Room.Users)
                        await ((IMultiplayerClient)this).MatchUserStateChanged(user.UserID, new TeamVersusUserState()).ConfigureAwait(false);
                    break;
            }
        }

        private async Task changeQueueMode(QueueModes newMode)
        {
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            if (newMode == QueueModes.HostOnly)
            {
                foreach (var playlistItem in APIRoom.Playlist.Where(i => !i.Expired && i.ID != currentItem.ID).ToArray())
                    await ((IMultiplayerClient)this).PlaylistItemRemoved(playlistItem.ID).ConfigureAwait(false);
            }

            // When changing modes, items could have been added (above) or the queueing order could have changed.
            await updateCurrentItem().ConfigureAwait(false);
        }

        private async Task finishCurrentItem()
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            // Expire the current playlist item.
            await ((IMultiplayerClient)this).PlaylistItemChanged(new APIPlaylistItem(currentItem) { Expired = true }).ConfigureAwait(false);

            // In host-only mode, a duplicate playlist item will be used for the next round.
            if (Room.Settings.QueueMode == QueueModes.HostOnly)
                await duplicateCurrentItem().ConfigureAwait(false);

            await updateCurrentItem().ConfigureAwait(false);
        }

        private async Task duplicateCurrentItem()
        {
            Debug.Assert(Room != null);
            Debug.Assert(APIRoom != null);
            Debug.Assert(currentItem != null);

            var newItem = new PlaylistItem
            {
                ID = APIRoom.Playlist.Last().ID + 1,
                BeatmapID = currentItem.BeatmapID,
                RulesetID = currentItem.RulesetID,
            };

            newItem.AllowedMods.AddRange(currentItem.AllowedMods);
            newItem.RequiredMods.AddRange(currentItem.AllowedMods);

            await ((IMultiplayerClient)this).PlaylistItemAdded(new APIPlaylistItem(newItem)).ConfigureAwait(false);
        }

        private async Task updateCurrentItem(bool notify = true, Room? apiRoom = null)
        {
            if (apiRoom == null)
            {
                Debug.Assert(APIRoom != null);
                apiRoom = APIRoom;
            }

            switch (apiRoom.QueueMode.Value)
            {
                default:
                    // Pick the single non-expired playlist item.
                    currentItem = apiRoom.Playlist.FirstOrDefault(i => !i.Expired) ?? apiRoom.Playlist.Last();
                    break;

                case QueueModes.FairRotate:
                    // Group playlist items by (user_id -> count_expired), and select the first available playlist item from a user that has available beatmaps where count_expired is the lowest.
                    throw new NotImplementedException();
            }

            if (Room != null)
            {
                long lastItem = Room.Settings.PlaylistItemId;
                Room.Settings.PlaylistItemId = currentItem.ID;

                if (notify && currentItem.ID != lastItem)
                    await ((IMultiplayerClient)this).SettingsChanged(Room.Settings).ConfigureAwait(false);
            }
        }
    }
}
