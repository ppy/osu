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
        /// The total number of users.
        /// </summary>
        [IgnoreMember]
        public int Count => UserDictionary.Count;

        /// <summary>
        /// Retrieves or adds a <see cref="MatchmakingUser"/> entry to this list.
        /// </summary>
        /// <param name="userId">The user ID.</param>
        public MatchmakingUser GetOrAdd(int userId)
        {
            if (UserDictionary.TryGetValue(userId, out MatchmakingUser? user))
                return user;

            return UserDictionary[userId] = new MatchmakingUser { UserId = userId };
        }

        public IEnumerator<MatchmakingUser> GetEnumerator() => UserDictionary.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
