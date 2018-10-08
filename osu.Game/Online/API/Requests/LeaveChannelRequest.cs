// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class LeaveChannelRequest : APIRequest
    {
        private readonly Channel channel;
        private readonly User user;

        public LeaveChannelRequest(Channel channel, User user)
        {
            this.channel = channel;
            this.user = user;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Delete;
            return req;
        }

        protected override string Target => $@"chat/channels/{channel.Id}/users/{user.Id}";
    }
}
