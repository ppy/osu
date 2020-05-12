// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.IO.Network;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Rulesets;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsRequest : APIRequest<SearchBeatmapSetsResponse>
    {
        public class Cursor
        {
            [JsonExtensionData]
            public IDictionary<string, JToken> Properties;
        }

        public SearchCategory SearchCategory { get; set; }

        public SortCriteria SortCriteria { get; set; }

        public SortDirection SortDirection { get; set; }

        public SearchGenre Genre { get; set; }

        public SearchLanguage Language { get; set; }

        private readonly string query;
        private readonly RulesetInfo ruleset;
        private readonly Cursor cursor;

        private string directionString => SortDirection == SortDirection.Descending ? @"desc" : @"asc";

        public SearchBeatmapSetsRequest(string query, RulesetInfo ruleset, Cursor cursor = null,
            SearchCategory searchCategory = SearchCategory.Any, SortCriteria sortCriteria = SortCriteria.Ranked, SortDirection sortDirection = SortDirection.Descending)
        {
            this.query = string.IsNullOrEmpty(query) ? string.Empty : System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;
            this.cursor = cursor;

            SearchCategory = searchCategory;
            SortCriteria = sortCriteria;
            SortDirection = sortDirection;
            Genre = SearchGenre.Any;
            Language = SearchLanguage.Any;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.AddParameter("q", query);

            if (ruleset.ID.HasValue)
                req.AddParameter("m", ruleset.ID.Value.ToString());

            req.AddParameter("s", SearchCategory.ToString().ToLowerInvariant());

            if (Genre != SearchGenre.Any)
                req.AddParameter("g", ((int)Genre).ToString());

            if (Language != SearchLanguage.Any)
                req.AddParameter("l", ((int)Language).ToString());

            req.AddParameter("sort", $"{SortCriteria.ToString().ToLowerInvariant()}_{directionString}");

            cursor?.Properties.ForEach(x =>
            {
                req.AddParameter("cursor[" + x.Key + "]", x.Value?.ToString());
            });

            return req;
        }

        protected override string Target => @"beatmapsets/search";
    }
}
