// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class PostMessageRequest : APIRequest<Message>
    {
        private readonly Message message;

        public PostMessageRequest(Message message)
        {
            this.message = message;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;
            req.AddParameter(@"is_action", message.IsAction.ToString().ToLowerInvariant());
            req.AddParameter(@"message", message.Content);

            return req;
        }

        protected override string Target => $@"chat/channels/{message.ChannelId}/messages";
    }
}
