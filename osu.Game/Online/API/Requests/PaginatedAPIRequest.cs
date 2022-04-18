// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Globalization;
using osu.Framework.IO.Network;

namespace osu.Game.Online.API.Requests
{
    public abstract class PaginatedAPIRequest<T> : APIRequest<T> where T : class
    {
        private readonly int page;
        private readonly int initialItems;
        private readonly int itemsPerPage;

        protected PaginatedAPIRequest(int page, int itemsPerPage, int initialItems)
        {
            this.page = page;
            this.initialItems = initialItems;
            this.itemsPerPage = itemsPerPage;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (page == 0)
                req.AddParameter("limit", initialItems.ToString(CultureInfo.InvariantCulture));
            else
            {
                req.AddParameter("offset", (initialItems + (page - 1) * itemsPerPage).ToString(CultureInfo.InvariantCulture));
                req.AddParameter("limit", itemsPerPage.ToString(CultureInfo.InvariantCulture));
            }

            return req;
        }
    }
}
