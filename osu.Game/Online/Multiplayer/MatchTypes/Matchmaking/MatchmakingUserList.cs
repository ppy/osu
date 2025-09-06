// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections;
using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Describes the users of a matchmaking room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MatchmakingUserList : IEnumerable<MatchmakingUser>
    {
        /// <summary>
        /// A key-value-pair mapping of ids to users.
        /// </summary>
        [Key(0)]
        public IDictionary<int, MatchmakingUser> UserDictionary { get; set; } = new Dictionary<int, MatchmakingUser>();

        /// <summary>
        /// Creates or retrieves the user for the given id.
        /// </summary>
        /// <param name="userId">The user id.</param>
        public MatchmakingUser this[int userId]
        {
            get
            {
                if (UserDictionary.TryGetValue(userId, out MatchmakingUser? user))
                    return user;

                return UserDictionary[userId] = new MatchmakingUser { UserId = userId };
            }
        }

        /// <summary>
        /// The total number of users.
        /// </summary>
        [IgnoreMember]
        public int Count => UserDictionary.Count;

        public IEnumerator<MatchmakingUser> GetEnumerator() => UserDictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
