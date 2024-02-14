// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
