// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Game.Extensions;

namespace osu.Game.Online.API.Requests
{
    public class GetNewsRequest : APIRequest<GetNewsResponse>
    {
        private readonly int? year;
        private readonly Cursor? cursor;

        public GetNewsRequest(int? year = null, Cursor? cursor = null)
        {
            this.year = year;
            this.cursor = cursor;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (cursor != null)
                req.AddCursor(cursor);

            if (year.HasValue)
                req.AddParameter("year", year.Value.ToString());

            return req;
        }

        protected override string Target => "news";
    }
}
