// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class MarkChannelAsReadRequest : APIRequest
    {
        private readonly Channel channel;
        private readonly Message message;

        public MarkChannelAsReadRequest(Channel channel, Message message)
        {
            this.channel = channel;
            this.message = message;
        }

        protected override string Target => @"/chat/channels/{channel}/mark-as-read/{message}";
    }
}
