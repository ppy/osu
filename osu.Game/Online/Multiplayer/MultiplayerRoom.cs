// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using Newtonsoft.Json;
using osu.Framework.Allocation;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A multiplayer room.
    /// </summary>
    [Serializable]
    public class MultiplayerRoom
    {
        /// <summary>
        /// The ID of the room, used for database persistence.
        /// </summary>
        public readonly long RoomID;

        /// <summary>
        /// The current state of the room (ie. whether it is in progress or otherwise).
        /// </summary>
        public MultiplayerRoomState State { get; set; }

        /// <summary>
        /// All currently enforced game settings for this room.
        /// </summary>
        public MultiplayerRoomSettings Settings { get; set; } = new MultiplayerRoomSettings();

        /// <summary>
        /// All users currently in this room.
        /// </summary>
        public List<MultiplayerRoomUser> Users { get; set; } = new List<MultiplayerRoomUser>();

        /// <summary>
        /// The host of this room, in control of changing room settings.
        /// </summary>
        public MultiplayerRoomUser? Host { get; set; }

        private object writeLock = new object();

        [JsonConstructor]
        public MultiplayerRoom(in long roomId)
        {
            RoomID = roomId;
        }

        private object updateLock = new object();

        private ManualResetEventSlim freeForWrite = new ManualResetEventSlim(true);

        /// <summary>
        /// Request a lock on this room to perform a thread-safe update.
        /// </summary>
        public IDisposable LockForUpdate()
        {
            // ReSharper disable once InconsistentlySynchronizedField
            freeForWrite.Wait();

            lock (updateLock)
            {
                freeForWrite.Wait();
                freeForWrite.Reset();

                return new ValueInvokeOnDisposal<MultiplayerRoom>(this, r => freeForWrite.Set());
            }
        }

        public override string ToString() => $"RoomID:{RoomID} Host:{Host?.UserID} Users:{Users.Count} State:{State} Settings: [{Settings}]";
    }
}
