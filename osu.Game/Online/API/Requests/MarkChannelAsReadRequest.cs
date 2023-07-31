// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class MarkChannelAsReadRequest : APIRequest
    {
        public readonly Channel Channel;
        public readonly Message Message;

        public MarkChannelAsReadRequest(Channel channel, Message message)
        {
            Channel = channel;
            Message = message;
        }

        protected override string Target => $"chat/channels/{Channel.Id}/mark-as-read/{Message.Id}";

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Put;
            return req;
        }
    }
}
