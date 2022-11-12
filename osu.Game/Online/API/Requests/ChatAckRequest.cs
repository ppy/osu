// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    public class ChatAckRequest : APIRequest<ChatAckResponse>
    {
        public long? SinceMessageId;
        public uint? SinceSilenceId;

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.Method = HttpMethod.Post;
            if (SinceMessageId != null)
                req.AddParameter(@"since", SinceMessageId.ToString());
            if (SinceSilenceId != null)
                req.AddParameter(@"history_since", SinceSilenceId.Value.ToString());
            return req;
        }

        protected override string Target => "chat/ack";
    }
}
