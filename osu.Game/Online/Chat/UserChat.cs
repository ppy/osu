// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Users;

namespace osu.Game.Online.Chat
{
    public class UserChat : ChatBase
    {
        public User User { get; private set; }
        public override TargetType Target => TargetType.User;
        public override long ChatID => User.Id;

        public Action<User> DetailsArrived;

        public UserChat(User user, Message[] messages = null)
        {
            User = user ?? throw new ArgumentNullException(nameof(user));

            if (messages != null) AddNewMessages(messages);
        }

        public void RequestDetails(IAPIProvider api)
        {
            if (api == null)
                throw new ArgumentNullException(nameof(api));

            var req = new GetUserRequest(User.Id);
            req.Success += user =>
            {
                User = user;
                DetailsArrived?.Invoke(user);
            };
            req.Failure += exception => Logger.Error(exception, $"Requesting details for user with Id:{User.Id} failed.");
            api.Queue(req);
        }

        public override string ToString() => User.Username ?? User.Id.ToString();
    }
}
