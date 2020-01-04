// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public abstract class PaginatedAPIRequest<T> : APIRequest<T>
    {
        private readonly int page;
        private readonly int itemsPerPage;

        protected PaginatedAPIRequest(int page, int itemsPerPage)
        {
            this.page = page;
            this.itemsPerPage = itemsPerPage;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            req.AddParameter("offset", (page * itemsPerPage).ToString(CultureInfo.InvariantCulture));
            req.AddParameter("limit", itemsPerPage.ToString(CultureInfo.InvariantCulture));

            return req;
        }
    }
}
