// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API.Requests
{
    public class GetChannelMessagesRequest : APIRequest<List<Message>>
    {
        private readonly IEnumerable<Channel> channels;
        private long? since;

        public GetChannelMessagesRequest(IEnumerable<Channel> channels, long? sinceId)
        {
            if (channels == null)
                throw new ArgumentNullException(nameof(channels));
            if (channels.Any(c => c.Target != TargetType.Channel))
                throw new ArgumentException("All channels in the argument channels must have the targettype Channel");

            this.channels = channels;
            since = sinceId;
        }

        protected override WebRequest CreateWebRequest()
        {
            string channelString = string.Join(",", channels.Select(x => x.Id));

            var req = base.CreateWebRequest();
            req.AddParameter(@"channels", channelString);
            if (since.HasValue) req.AddParameter(@"since", since.Value.ToString());

            return req;
        }

        protected override string Target => @"chat/messages";
    }
}
