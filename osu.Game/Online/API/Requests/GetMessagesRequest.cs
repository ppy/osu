// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class GetMessagesRequest : APIMessagesRequest
    {
        private readonly IEnumerable<Channel> channels;

        public GetMessagesRequest(IEnumerable<Channel> channels, long? sinceId) : base(sinceId)
        {
            if (channels == null)
                throw new ArgumentNullException(nameof(channels));
            if (channels.Any(c => c.Target != TargetType.Channel))
                throw new ArgumentException($"All channels in the argument channels must have a {nameof(Channel.Target)} of {nameof(TargetType.Channel)}");

            this.channels = channels;
        }

        protected override WebRequest CreateWebRequest()
        {
            string channelString = string.Join(",", channels.Select(x => x.Id));

            var req = base.CreateWebRequest();
            req.AddParameter(@"channels", channelString);

            return req;
        }

        protected override string Target => @"chat/messages";
    }
}
