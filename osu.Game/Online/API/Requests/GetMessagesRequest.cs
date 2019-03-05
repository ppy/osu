// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class GetMessagesRequest : APIRequest<List<Message>>
    {
        private readonly Channel channel;

        public GetMessagesRequest(Channel channel)
        {
            this.channel = channel;
        }

        protected override string Target => $@"chat/channels/{channel.Id}/messages";
    }
}
