// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class UserChat : ChatBase
    {
        public User User { get; }

        public UserChat(User user, Message[] messages = null)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));

            if (messages != null) AddNewMessages(messages);
        }

        public override TargetType Target => TargetType.User;
        public override long ChatID => User.Id;
    }
}
