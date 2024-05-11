// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using Newtonsoft.Json;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Multiplayer
{
    [Serializable]
    [MessagePackObject]
    public class MultiplayerRoomUser : IEquatable<MultiplayerRoomUser>
    {
        [Key(0)]
        public readonly int UserID;

        [Key(1)]
        public MultiplayerUserState State { get; set; } = MultiplayerUserState.Idle;

        [Key(4)]
        public MatchUserState? MatchState { get; set; }

        /// <summary>
        /// The availability state of the current beatmap.
        /// </summary>
        [Key(2)]
        public BeatmapAvailability BeatmapAvailability { get; set; } = BeatmapAvailability.Unknown();

        /// <summary>
        /// Any mods applicable only to the local user.
        /// </summary>
        [Key(3)]
        public IEnumerable<APIMod> Mods { get; set; } = Enumerable.Empty<APIMod>();

        [IgnoreMember]
        public APIUser? User { get; set; }

        [JsonConstructor]
        public MultiplayerRoomUser(int userId)
        {
            UserID = userId;
        }

        public bool Equals(MultiplayerRoomUser? other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;

            return UserID == other.UserID;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj?.GetType() != GetType()) return false;

            return Equals((MultiplayerRoomUser)obj);
        }

        public override int GetHashCode() => UserID.GetHashCode();

        /// <summary>
        /// Whether this user has finished loading and can start gameplay.
        /// </summary>
        public bool CanStartGameplay()
        {
            switch (State)
            {
                case MultiplayerUserState.Loaded:
                case MultiplayerUserState.ReadyForGameplay:
                    return true;

                default:
                    return false;
            }
        }
    }
}
