// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.IO.Network;
using osu.Game.Overlays;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;
using osu.Game.Utils;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsRequest : APIRequest<SearchBeatmapSetsResponse>
    {
        public BeatmapSearchCategory SearchCategory { get; set; }

        public DirectSortCriteria SortCriteria { get; set; }

        public SortDirection SortDirection { get; set; }

        public BeatmapSearchGenre Genre { get; set; }

        public BeatmapSearchLanguage Language { get; set; }

        private readonly string query;
        private readonly RulesetInfo ruleset;

        private string directionString => SortDirection == SortDirection.Descending ? @"desc" : @"asc";

        public SearchBeatmapSetsRequest(string query, RulesetInfo ruleset)
        {
            this.query = string.IsNullOrEmpty(query) ? string.Empty : System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;

            SearchCategory = BeatmapSearchCategory.Any;
            SortCriteria = DirectSortCriteria.Ranked;
            SortDirection = SortDirection.Descending;
            Genre = BeatmapSearchGenre.Any;
            Language = BeatmapSearchLanguage.Any;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.AddParameter("q", query);

            if (ruleset.ID.HasValue)
                req.AddParameter("m", ruleset.ID.Value.ToString());

            req.AddParameter("s", SearchCategory.ToString().ToLowerInvariant());

            if (Genre != BeatmapSearchGenre.Any)
                req.AddParameter("g", ((int)Genre).ToString());

            if (Language != BeatmapSearchLanguage.Any)
                req.AddParameter("l", ((int)Language).ToString());

            req.AddParameter("sort", $"{SortCriteria.ToString().ToLowerInvariant()}_{directionString}");

            return req;
        }

        protected override string Target => @"beatmapsets/search";
    }

    public enum BeatmapSearchCategory
    {
        Any,

        [Description("Has Leaderboard")]
        Leaderboard,
        Ranked,
        Qualified,
        Loved,
        Favourites,

        [Description("Pending & WIP")]
        Pending,
        Graveyard,

        [Description("My Maps")]
        Mine,
    }

    public enum BeatmapSearchGenre
    {
        Any = 0,
        Unspecified = 1,

        [Description("Video Game")]
        VideoGame = 2,
        Anime = 3,
        Rock = 4,
        Pop = 5,
        Other = 6,
        Novelty = 7,

        [Description("Hip Hop")]
        HipHop = 9,
        Electronic = 10
    }

    [HasOrderedElements]
    public enum BeatmapSearchLanguage
    {
        [Order(0)]
        Any,

        [Order(11)]
        Other,

        [Order(1)]
        English,

        [Order(6)]
        Japanese,

        [Order(2)]
        Chinese,

        [Order(10)]
        Instrumental,

        [Order(7)]
        Korean,

        [Order(3)]
        French,

        [Order(4)]
        German,

        [Order(9)]
        Swedish,

        [Order(8)]
        Spanish,

        [Order(5)]
        Italian
    }
}
