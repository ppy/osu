// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class CreateNewPrivateMessageRequest : APIRequest<CreateNewPrivateMessageResponse>
    {
        private readonly APIUser user;
        private readonly Message message;

        public CreateNewPrivateMessageRequest(APIUser user, Message message)
        {
            this.user = user;
            this.message = message;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            req.AddParameter(@"target_id", user.Id.ToString());
            req.AddParameter(@"message", message.Content);
            req.AddParameter(@"is_action", message.IsAction.ToString().ToLowerInvariant());
            req.AddParameter(@"uuid", message.Uuid);
            return req;
        }

        protected override string Target => @"chat/new";
    }
}
