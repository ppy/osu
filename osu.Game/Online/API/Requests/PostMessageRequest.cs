// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Extensions;
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

            req.Method = HttpMethod.POST;
            req.AddParameter(@"target_type", message.TargetType.GetDescription());
            req.AddParameter(@"target_id", message.TargetId.ToString());
            req.AddParameter(@"is_action", message.IsAction.ToString().ToLowerInvariant());
            req.AddParameter(@"message", message.Content);

            return req;
        }

        protected override string Target => @"chat/messages";
    }
}
