// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
using osu.Framework.IO.Network;
using osu.Game.Overlays;
using osu.Game.Overlays.Direct;
using osu.Game.Rulesets;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsRequest : APIRequest<SearchBeatmapSetsResponse>
    {
        private readonly string query;
        private readonly RulesetInfo ruleset;
        private readonly BeatmapSearchCategory searchCategory;
        private readonly DirectSortCriteria sortCriteria;
        private readonly SortDirection direction;
        private readonly BeatmapSearchGenre genre;
        private readonly BeatmapSearchLanguage language;
        private string directionString => direction == SortDirection.Descending ? @"desc" : @"asc";

        public SearchBeatmapSetsRequest(string query, RulesetInfo ruleset, BeatmapSearchCategory searchCategory = BeatmapSearchCategory.Any, DirectSortCriteria sortCriteria = DirectSortCriteria.Ranked, SortDirection direction = SortDirection.Descending, BeatmapSearchGenre genre = BeatmapSearchGenre.Any, BeatmapSearchLanguage language = BeatmapSearchLanguage.Any)
        {
            this.query = string.IsNullOrEmpty(query) ? string.Empty : System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;
            this.searchCategory = searchCategory;
            this.sortCriteria = sortCriteria;
            this.direction = direction;
            this.genre = genre;
            this.language = language;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();
            req.AddParameter("q", query);

            if (ruleset.ID.HasValue)
                req.AddParameter("m", ruleset.ID.Value.ToString());

            req.AddParameter("s", searchCategory.ToString().ToLowerInvariant());

            if (genre != BeatmapSearchGenre.Any)
                req.AddParameter("g", ((int)genre).ToString());

            if (language != BeatmapSearchLanguage.Any)
                req.AddParameter("l", ((int)language).ToString());

            req.AddParameter("sort", $"{sortCriteria.ToString().ToLowerInvariant()}_{directionString}");

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
        Any,
        Unspecified,

        [Description("Video Game")]
        VideoGame,
        Anime,
        Rock,
        Pop,
        Other,
        Novelty,

        [Description("Hip Hop")]
        HipHop = 9,
        Electronic
    }

    public enum BeatmapSearchLanguage
    {
        Any,
        English = 2,
        Chinese = 4,
        French = 7,
        German,
        Italian = 11,
        Japanese = 3,
        Korean = 6,
        Spanish = 10,
        Swedish = 9,
        Instrumental = 5,
        Other = 1
    }
}
