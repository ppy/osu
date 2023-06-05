// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Net.Http;
using osu.Framework.IO.Network;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.API.Requests
{
    /// <summary>
    /// A request which should be sent occasionally while interested in chat and online state.
    ///
    /// This will:
    ///  - Mark the user as "online" (for 10 minutes since the last invocation).
    ///  - Return any silences since the last invocation (if either <see cref="SinceMessageId"/> or <see cref="SinceSilenceId"/> is not null).
    ///
    /// For silence handling, a <see cref="SinceMessageId"/> should be provided as soon as a message is received by the client.
    /// From that point forward, <see cref="SinceSilenceId"/> should be preferred after the first <see cref="ChatSilence"/>
    /// arrives in a response from the ack request. Specifying both parameters will prioritise the latter.
    /// </summary>
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
