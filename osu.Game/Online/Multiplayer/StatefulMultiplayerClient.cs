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
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Extensions;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Rulesets;
using osu.Game.Users;
using osu.Game.Utils;

namespace osu.Game.Online.Multiplayer
{
    public abstract class StatefulMultiplayerClient : Component, IMultiplayerClient, IMultiplayerRoomServer
    {
        /// <summary>
        /// Invoked when any change occurs to the multiplayer room.
        /// </summary>
        public event Action? RoomUpdated;

        /// <summary>
        /// Invoked when the multiplayer server requests the current beatmap to be loaded into play.
        /// </summary>
        public event Action? LoadRequested;

        /// <summary>
        /// Invoked when the multiplayer server requests gameplay to be started.
        /// </summary>
        public event Action? MatchStarted;

        /// <summary>
        /// Invoked when the multiplayer server has finished collating results.
        /// </summary>
        public event Action? ResultsReady;

        /// <summary>
        /// Whether the <see cref="StatefulMultiplayerClient"/> is currently connected.
        /// </summary>
        public abstract IBindable<bool> IsConnected { get; }

        /// <summary>
        /// The joined <see cref="MultiplayerRoom"/>.
        /// </summary>
        public MultiplayerRoom? Room { get; private set; }

        /// <summary>
        /// The users currently in gameplay.
        /// </summary>
        public readonly BindableList<int> PlayingUsers = new BindableList<int>();

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private RulesetStore rulesets { get; set; } = null!;

        private Room? apiRoom;

        // Todo: This is temporary, until the multiplayer server returns the item id on match start or otherwise.
        private int playlistItemId;

        protected StatefulMultiplayerClient()
        {
            IsConnected.BindValueChanged(connected =>
            {
                // clean up local room state on server disconnect.
                if (!connected.NewValue)
                {
                    Logger.Log("Connection to multiplayer server was lost.", LoggingTarget.Runtime, LogLevel.Important);
                    LeaveRoom().CatchUnobservedExceptions();
                }
            });
        }

        /// <summary>
        /// Joins the <see cref="MultiplayerRoom"/> for a given API <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The API <see cref="Room"/>.</param>
        public async Task JoinRoom(Room room)
        {
            if (Room != null)
                throw new InvalidOperationException("Cannot join a multiplayer room while already in one.");

            Debug.Assert(room.RoomID.Value != null);

            apiRoom = room;
            playlistItemId = room.Playlist.SingleOrDefault()?.ID ?? 0;

            Room = await JoinRoom(room.RoomID.Value.Value);

            Debug.Assert(Room != null);

            var users = getRoomUsers();

            await Task.WhenAll(users.Select(PopulateUser));

            updateLocalRoomSettings(Room.Settings);
        }

        /// <summary>
        /// Joins the <see cref="MultiplayerRoom"/> with a given ID.
        /// </summary>
        /// <param name="roomId">The room ID.</param>
        /// <returns>The joined <see cref="MultiplayerRoom"/>.</returns>
        protected abstract Task<MultiplayerRoom> JoinRoom(long roomId);

        public virtual Task LeaveRoom()
        {
            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                apiRoom = null;
                Room = null;

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Change the current <see cref="MultiplayerRoom"/> settings.
        /// </summary>
        /// <remarks>
        /// A room must be joined for this to have any effect.
        /// </remarks>
        /// <param name="name">The new room name, if any.</param>
        /// <param name="item">The new room playlist item, if any.</param>
        public Task ChangeSettings(Optional<string> name = default, Optional<PlaylistItem> item = default)
        {
            if (Room == null)
                throw new InvalidOperationException("Must be joined to a match to change settings.");

            // A dummy playlist item filled with the current room settings (except mods).
            var existingPlaylistItem = new PlaylistItem
            {
                Beatmap =
                {
                    Value = new BeatmapInfo
                    {
                        OnlineBeatmapID = Room.Settings.BeatmapID,
                        MD5Hash = Room.Settings.BeatmapChecksum
                    }
                },
                RulesetID = Room.Settings.RulesetID
            };

            return ChangeSettings(new MultiplayerRoomSettings
            {
                Name = name.GetOr(Room.Settings.Name),
                BeatmapID = item.GetOr(existingPlaylistItem).BeatmapID,
                BeatmapChecksum = item.GetOr(existingPlaylistItem).Beatmap.Value.MD5Hash,
                RulesetID = item.GetOr(existingPlaylistItem).RulesetID,
                Mods = item.HasValue ? item.Value.AsNonNull().RequiredMods.Select(m => new APIMod(m)).ToList() : Room.Settings.Mods
            });
        }

        public abstract Task TransferHost(int userId);

        public abstract Task ChangeSettings(MultiplayerRoomSettings settings);

        public abstract Task ChangeState(MultiplayerUserState newState);

        public abstract Task StartMatch();

        Task IMultiplayerClient.RoomStateChanged(MultiplayerRoomState state)
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(apiRoom != null);

                Room.State = state;

                switch (state)
                {
                    case MultiplayerRoomState.Open:
                        apiRoom.Status.Value = new RoomStatusOpen();
                        break;

                    case MultiplayerRoomState.Playing:
                        apiRoom.Status.Value = new RoomStatusPlaying();
                        break;

                    case MultiplayerRoomState.Closed:
                        apiRoom.Status.Value = new RoomStatusEnded();
                        break;
                }

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        async Task IMultiplayerClient.UserJoined(MultiplayerRoomUser user)
        {
            if (Room == null)
                return;

            await PopulateUser(user);

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                // for sanity, ensure that there can be no duplicate users in the room user list.
                if (Room.Users.Any(existing => existing.UserID == user.UserID))
                    return;

                Room.Users.Add(user);

                RoomUpdated?.Invoke();
            }, false);
        }

        Task IMultiplayerClient.UserLeft(MultiplayerRoomUser user)
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Room.Users.Remove(user);
                PlayingUsers.Remove(user.UserID);

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.HostChanged(int userId)
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(apiRoom != null);

                var user = Room.Users.FirstOrDefault(u => u.UserID == userId);

                Room.Host = user;
                apiRoom.Host.Value = user?.User;

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.SettingsChanged(MultiplayerRoomSettings newSettings)
        {
            updateLocalRoomSettings(newSettings);
            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserStateChanged(int userId, MultiplayerUserState state)
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Room.Users.Single(u => u.UserID == userId).State = state;

                if (state != MultiplayerUserState.Playing)
                    PlayingUsers.Remove(userId);

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.LoadRequested()
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                LoadRequested?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.MatchStarted()
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                PlayingUsers.AddRange(Room.Users.Where(u => u.State == MultiplayerUserState.Playing).Select(u => u.UserID));

                MatchStarted?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.ResultsReady()
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                ResultsReady?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Populates the <see cref="User"/> for a given <see cref="MultiplayerRoomUser"/>.
        /// </summary>
        /// <param name="multiplayerUser">The <see cref="MultiplayerRoomUser"/> to populate.</param>
        protected async Task PopulateUser(MultiplayerRoomUser multiplayerUser) => multiplayerUser.User ??= await userLookupCache.GetUserAsync(multiplayerUser.UserID);

        /// <summary>
        /// Retrieve a copy of users currently in the joined <see cref="Room"/> in a thread-safe manner.
        /// This should be used whenever accessing users from outside of an Update thread context (ie. when not calling <see cref="Drawable.Schedule"/>).
        /// </summary>
        /// <returns>A copy of users in the current room, or null if unavailable.</returns>
        private List<MultiplayerRoomUser>? getRoomUsers()
        {
            List<MultiplayerRoomUser>? users = null;

            ManualResetEventSlim resetEvent = new ManualResetEventSlim();

            // at some point we probably want to replace all these schedule calls with Room.LockForUpdate.
            // for now, as this would require quite some consideration due to the number of accesses to the room instance,
            // let's just add a manual schedule for the non-scheduled usages instead.
            Scheduler.Add(() =>
            {
                users = Room?.Users.ToList();
                resetEvent.Set();
            }, false);

            resetEvent.Wait(100);

            return users;
        }

        /// <summary>
        /// Updates the local room settings with the given <see cref="MultiplayerRoomSettings"/>.
        /// </summary>
        /// <remarks>
        /// This updates both the joined <see cref="MultiplayerRoom"/> and the respective API <see cref="Room"/>.
        /// </remarks>
        /// <param name="settings">The new <see cref="MultiplayerRoomSettings"/> to update from.</param>
        private void updateLocalRoomSettings(MultiplayerRoomSettings settings)
        {
            if (Room == null)
                return;

            Scheduler.Add(() =>
            {
                if (Room == null)
                    return;

                Debug.Assert(apiRoom != null);

                // Update a few properties of the room instantaneously.
                Room.Settings = settings;
                apiRoom.Name.Value = Room.Settings.Name;

                // The playlist update is delayed until an online beatmap lookup (below) succeeds.
                // In-order for the client to not display an outdated beatmap, the playlist is forcefully cleared here.
                apiRoom.Playlist.Clear();

                RoomUpdated?.Invoke();

                var req = new GetBeatmapSetRequest(settings.BeatmapID, BeatmapSetLookupType.BeatmapId);
                req.Success += res => updatePlaylist(settings, res);

                api.Queue(req);
            }, false);
        }

        private void updatePlaylist(MultiplayerRoomSettings settings, APIBeatmapSet onlineSet)
        {
            if (Room == null || !Room.Settings.Equals(settings))
                return;

            Debug.Assert(apiRoom != null);

            var beatmapSet = onlineSet.ToBeatmapSet(rulesets);
            var beatmap = beatmapSet.Beatmaps.Single(b => b.OnlineBeatmapID == settings.BeatmapID);
            beatmap.MD5Hash = settings.BeatmapChecksum;

            var ruleset = rulesets.GetRuleset(settings.RulesetID).CreateInstance();
            var mods = settings.Mods.Select(m => m.ToMod(ruleset));

            PlaylistItem playlistItem = new PlaylistItem
            {
                ID = playlistItemId,
                Beatmap = { Value = beatmap },
                Ruleset = { Value = ruleset.RulesetInfo },
            };

            playlistItem.RequiredMods.AddRange(mods);

            apiRoom.Playlist.Clear(); // Clearing should be unnecessary, but here for sanity.
            apiRoom.Playlist.Add(playlistItem);
        }
    }
}
