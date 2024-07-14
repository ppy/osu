// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class PostMessageRequest : APIRequest<Message>
    {
        public readonly Message Message;

        public PostMessageRequest(Message message)
        {
            Message = message;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.Method = HttpMethod.Post;
            req.AddParameter(@"is_action", Message.IsAction.ToString().ToLowerInvariant());
            req.AddParameter(@"message", Message.Content);
            req.AddParameter(@"uuid", Message.Uuid);

            return req;
        }

        protected override string Target => $@"chat/channels/{Message.ChannelId}/messages";
    }
}
