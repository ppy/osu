// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;
using osu.Game.Users;

namespace osu.Game.Online.API.Requests
{
    public class JoinChannelRequest : APIRequest
    {
        private readonly Channel channel;
        private readonly User user;

        public JoinChannelRequest(Channel channel, User user)
        {
            this.channel = channel;
            this.user = user;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            return req;
        }

        protected override string Target => $@"chat/channels/{channel.Id}/users/{user.Id}";
    }
}
