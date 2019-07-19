// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public abstract class PaginatedAPIRequest<T> : APIRequest<T>
    {
        private readonly int offset;
        private readonly int limit;

        protected PaginatedAPIRequest(int offset, int limit)
        {
            this.offset = offset;
            this.limit = limit;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("offset", offset.ToString(CultureInfo.InvariantCulture));
            req.AddParameter("limit", limit.ToString(CultureInfo.InvariantCulture));

            return req;
        }
    }
}
