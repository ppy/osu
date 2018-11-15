// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.IO.Network;
using osu.Game.Online.Chat;

namespace osu.Game.Online.API
{
    public abstract class APIMessagesRequest : APIRequest<List<Message>>
    {
        private readonly long? sinceId;

        protected APIMessagesRequest(long? sinceId)
        {
            this.sinceId = sinceId;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (sinceId.HasValue) req.AddParameter(@"since", sinceId.Value.ToString());

            return req;
        }
    }
}
