// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class CreateChannelRequest : APIRequest<APIChatChannel>
    {
        public readonly Channel Channel;

        public CreateChannelRequest(Channel channel)
        {
            Channel = channel;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;

            req.AddParameter("type", $"{ChannelType.PM}");
            req.AddParameter("target_id", $"{Channel.Users.First().Id}");

            return req;
        }

        protected override string Target => @"chat/channels";
    }
}
