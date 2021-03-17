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
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
using osu.Game.Online.Rooms.RoomStatuses;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
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
        /// This is NOT thread safe and usage should be scheduled.
        /// </summary>
        public abstract IBindable<bool> IsConnected { get; }

        /// <summary>
        /// The joined <see cref="MultiplayerRoom"/>.
        /// </summary>
        public MultiplayerRoom? Room { get; private set; }

        /// <summary>
        /// The users in the joined <see cref="Room"/> which are participating in the current gameplay loop.
        /// </summary>
        public readonly BindableList<int> CurrentMatchPlayingUserIds = new BindableList<int>();

        public readonly Bindable<PlaylistItem?> CurrentMatchPlayingItem = new Bindable<PlaylistItem?>();

        /// <summary>
        /// The <see cref="MultiplayerRoomUser"/> corresponding to the local player, if available.
        /// </summary>
        public MultiplayerRoomUser? LocalUser => Room?.Users.SingleOrDefault(u => u.User?.Id == API.LocalUser.Value.Id);

        /// <summary>
        /// Whether the <see cref="LocalUser"/> is the host in <see cref="Room"/>.
        /// </summary>
        public bool IsHost
        {
            get
            {
                var localUser = LocalUser;
                return localUser != null && Room?.Host != null && localUser.Equals(Room.Host);
            }
        }

        [Resolved]
        protected IAPIProvider API { get; private set; } = null!;

        [Resolved]
        protected RulesetStore Rulesets { get; private set; } = null!;

        [Resolved]
        private UserLookupCache userLookupCache { get; set; } = null!;

        // Only exists for compatibility with old osu-server-spectator build.
        // Todo: Can be removed on 2021/02/26.
        private long defaultPlaylistItemId;

        private Room? apiRoom;

        [BackgroundDependencyLoader]
        private void load()
        {
            IsConnected.BindValueChanged(connected =>
            {
                // clean up local room state on server disconnect.
                if (!connected.NewValue && Room != null)
                {
                    Logger.Log("Connection to multiplayer server was lost.", LoggingTarget.Runtime, LogLevel.Important);
                    LeaveRoom();
                }
            });
        }

        private readonly TaskChain joinOrLeaveTaskChain = new TaskChain();
        private CancellationTokenSource? joinCancellationSource;

        /// <summary>
        /// Joins the <see cref="MultiplayerRoom"/> for a given API <see cref="Room"/>.
        /// </summary>
        /// <param name="room">The API <see cref="Room"/>.</param>
        public async Task JoinRoom(Room room)
        {
            var cancellationSource = joinCancellationSource = new CancellationTokenSource();

            await joinOrLeaveTaskChain.Add(async () =>
            {
                if (Room != null)
                    throw new InvalidOperationException("Cannot join a multiplayer room while already in one.");

                Debug.Assert(room.RoomID.Value != null);

                // Join the server-side room.
                var joinedRoom = await JoinRoom(room.RoomID.Value.Value).ConfigureAwait(false);
                Debug.Assert(joinedRoom != null);

                // Populate users.
                Debug.Assert(joinedRoom.Users != null);
                await Task.WhenAll(joinedRoom.Users.Select(PopulateUser)).ConfigureAwait(false);

                // Update the stored room (must be done on update thread for thread-safety).
                await scheduleAsync(() =>
                {
                    Room = joinedRoom;
                    apiRoom = room;
                    defaultPlaylistItemId = apiRoom.Playlist.FirstOrDefault()?.ID ?? 0;
                }, cancellationSource.Token).ConfigureAwait(false);

                // Update room settings.
                await updateLocalRoomSettings(joinedRoom.Settings, cancellationSource.Token).ConfigureAwait(false);
            }, cancellationSource.Token).ConfigureAwait(false);
        }

        /// <summary>
        /// Joins the <see cref="MultiplayerRoom"/> with a given ID.
        /// </summary>
        /// <param name="roomId">The room ID.</param>
        /// <returns>The joined <see cref="MultiplayerRoom"/>.</returns>
        protected abstract Task<MultiplayerRoom> JoinRoom(long roomId);

        public Task LeaveRoom()
        {
            // The join may have not completed yet, so certain tasks that either update the room or reference the room should be cancelled.
            // This includes the setting of Room itself along with the initial update of the room settings on join.
            joinCancellationSource?.Cancel();

            // Leaving rooms is expected to occur instantaneously whilst the operation is finalised in the background.
            // However a few members need to be reset immediately to prevent other components from entering invalid states whilst the operation hasn't yet completed.
            // For example, if a room was left and the user immediately pressed the "create room" button, then the user could be taken into the lobby if the value of Room is not reset in time.
            var scheduledReset = scheduleAsync(() =>
            {
                apiRoom = null;
                Room = null;
                CurrentMatchPlayingUserIds.Clear();

                RoomUpdated?.Invoke();
            });

            return joinOrLeaveTaskChain.Add(async () =>
            {
                await scheduledReset.ConfigureAwait(false);
                await LeaveRoomInternal().ConfigureAwait(false);
            });
        }

        protected abstract Task LeaveRoomInternal();

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
                RequiredMods = item.HasValue ? item.Value.AsNonNull().RequiredMods.Select(m => new APIMod(m)).ToList() : Room.Settings.RequiredMods,
                AllowedMods = item.HasValue ? item.Value.AsNonNull().AllowedMods.Select(m => new APIMod(m)).ToList() : Room.Settings.AllowedMods,
            });
        }

        /// <summary>
        /// Toggles the <see cref="LocalUser"/>'s ready state.
        /// </summary>
        /// <exception cref="InvalidOperationException">If a toggle of ready state is not valid at this time.</exception>
        public async Task ToggleReady()
        {
            var localUser = LocalUser;

            if (localUser == null)
                return;

            switch (localUser.State)
            {
                case MultiplayerUserState.Idle:
                    await ChangeState(MultiplayerUserState.Ready).ConfigureAwait(false);
                    return;

                case MultiplayerUserState.Ready:
                    await ChangeState(MultiplayerUserState.Idle).ConfigureAwait(false);
                    return;

                default:
                    throw new InvalidOperationException($"Cannot toggle ready when in {localUser.State}");
            }
        }

        public abstract Task TransferHost(int userId);

        public abstract Task ChangeSettings(MultiplayerRoomSettings settings);

        public abstract Task ChangeState(MultiplayerUserState newState);

        public abstract Task ChangeBeatmapAvailability(BeatmapAvailability newBeatmapAvailability);

        /// <summary>
        /// Change the local user's mods in the currently joined room.
        /// </summary>
        /// <param name="newMods">The proposed new mods, excluding any required by the room itself.</param>
        public Task ChangeUserMods(IEnumerable<Mod> newMods) => ChangeUserMods(newMods.Select(m => new APIMod(m)).ToList());

        public abstract Task ChangeUserMods(IEnumerable<APIMod> newMods);

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

            await PopulateUser(user).ConfigureAwait(false);

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
                CurrentMatchPlayingUserIds.Remove(user.UserID);

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

                updateUserPlayingState(userId, state);

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        Task IMultiplayerClient.UserBeatmapAvailabilityChanged(int userId, BeatmapAvailability beatmapAvailability)
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                var user = Room?.Users.SingleOrDefault(u => u.UserID == userId);

                // errors here are not critical - beatmap availability state is mostly for display.
                if (user == null)
                    return;

                user.BeatmapAvailability = beatmapAvailability;

                RoomUpdated?.Invoke();
            }, false);

            return Task.CompletedTask;
        }

        public Task UserModsChanged(int userId, IEnumerable<APIMod> mods)
        {
            if (Room == null)
                return Task.CompletedTask;

            Scheduler.Add(() =>
            {
                var user = Room?.Users.SingleOrDefault(u => u.UserID == userId);

                // errors here are not critical - user mods are mostly for display.
                if (user == null)
                    return;

                user.Mods = mods;

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
        protected async Task PopulateUser(MultiplayerRoomUser multiplayerUser) => multiplayerUser.User ??= await userLookupCache.GetUserAsync(multiplayerUser.UserID).ConfigureAwait(false);

        /// <summary>
        /// Updates the local room settings with the given <see cref="MultiplayerRoomSettings"/>.
        /// </summary>
        /// <remarks>
        /// This updates both the joined <see cref="MultiplayerRoom"/> and the respective API <see cref="Room"/>.
        /// </remarks>
        /// <param name="settings">The new <see cref="MultiplayerRoomSettings"/> to update from.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> to cancel the update.</param>
        private Task updateLocalRoomSettings(MultiplayerRoomSettings settings, CancellationToken cancellationToken = default) => scheduleAsync(() =>
        {
            if (Room == null)
                return;

            Debug.Assert(apiRoom != null);

            // Update a few properties of the room instantaneously.
            Room.Settings = settings;
            apiRoom.Name.Value = Room.Settings.Name;

            // The current item update is delayed until an online beatmap lookup (below) succeeds.
            // In-order for the client to not display an outdated beatmap, the current item is forcefully cleared here.
            CurrentMatchPlayingItem.Value = null;

            RoomUpdated?.Invoke();

            GetOnlineBeatmapSet(settings.BeatmapID, cancellationToken).ContinueWith(set => Schedule(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                    return;

                updatePlaylist(settings, set.Result);
            }), TaskContinuationOptions.OnlyOnRanToCompletion);
        }, cancellationToken);

        private void updatePlaylist(MultiplayerRoomSettings settings, BeatmapSetInfo beatmapSet)
        {
            if (Room == null || !Room.Settings.Equals(settings))
                return;

            Debug.Assert(apiRoom != null);

            var beatmap = beatmapSet.Beatmaps.Single(b => b.OnlineBeatmapID == settings.BeatmapID);
            beatmap.MD5Hash = settings.BeatmapChecksum;

            var ruleset = Rulesets.GetRuleset(settings.RulesetID).CreateInstance();
            var mods = settings.RequiredMods.Select(m => m.ToMod(ruleset));
            var allowedMods = settings.AllowedMods.Select(m => m.ToMod(ruleset));

            // Try to retrieve the existing playlist item from the API room.
            var playlistItem = apiRoom.Playlist.FirstOrDefault(i => i.ID == settings.PlaylistItemId);

            if (playlistItem != null)
                updateItem(playlistItem);
            else
            {
                // An existing playlist item does not exist, so append a new one.
                updateItem(playlistItem = new PlaylistItem());
                apiRoom.Playlist.Add(playlistItem);
            }

            CurrentMatchPlayingItem.Value = playlistItem;

            void updateItem(PlaylistItem item)
            {
                item.ID = settings.PlaylistItemId == 0 ? defaultPlaylistItemId : settings.PlaylistItemId;
                item.Beatmap.Value = beatmap;
                item.Ruleset.Value = ruleset.RulesetInfo;
                item.RequiredMods.Clear();
                item.RequiredMods.AddRange(mods);
                item.AllowedMods.Clear();
                item.AllowedMods.AddRange(allowedMods);
            }
        }

        /// <summary>
        /// Retrieves a <see cref="BeatmapSetInfo"/> from an online source.
        /// </summary>
        /// <param name="beatmapId">The beatmap set ID.</param>
        /// <param name="cancellationToken">A token to cancel the request.</param>
        /// <returns>The <see cref="BeatmapSetInfo"/> retrieval task.</returns>
        protected abstract Task<BeatmapSetInfo> GetOnlineBeatmapSet(int beatmapId, CancellationToken cancellationToken = default);

        /// <summary>
        /// For the provided user ID, update whether the user is included in <see cref="CurrentMatchPlayingUserIds"/>.
        /// </summary>
        /// <param name="userId">The user's ID.</param>
        /// <param name="state">The new state of the user.</param>
        private void updateUserPlayingState(int userId, MultiplayerUserState state)
        {
            bool wasPlaying = CurrentMatchPlayingUserIds.Contains(userId);
            bool isPlaying = state >= MultiplayerUserState.WaitingForLoad && state <= MultiplayerUserState.FinishedPlay;

            if (isPlaying == wasPlaying)
                return;

            if (isPlaying)
                CurrentMatchPlayingUserIds.Add(userId);
            else
                CurrentMatchPlayingUserIds.Remove(userId);
        }

        private Task scheduleAsync(Action action, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();

            Scheduler.Add(() =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.SetCanceled();
                    return;
                }

                try
                {
                    action();
                    tcs.SetResult(true);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            return tcs.Task;
        }
    }
}
