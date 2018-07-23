// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class PrivateChannel : Channel
    {
        public User User
        {
            set
            {
                Name = value.Username;
                Id = value.Id;
                JoinedUsers.Add(value);
            }
        }

        /// <summary>
        /// Contructs a private channel
        /// </summary>
        /// <param name="user">The user</param>
        public PrivateChannel()
        {
            Target = TargetType.User;
        }
    }
}
