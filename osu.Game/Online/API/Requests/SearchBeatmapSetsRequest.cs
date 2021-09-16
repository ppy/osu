// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using Humanizer;
using JetBrains.Annotations;
using osu.Framework.IO.Network;
using osu.Game.Extensions;
using osu.Game.Overlays;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Rulesets;
using osu.Game.Scoring;

namespace osu.Game.Online.API.Requests
{
    public class SearchBeatmapSetsRequest : APIRequest<SearchBeatmapSetsResponse>
    {
        [CanBeNull]
        public IReadOnlyCollection<SearchGeneral> General { get; }

        public SearchCategory SearchCategory { get; }

        public SortCriteria SortCriteria { get; }

        public SortDirection SortDirection { get; }

        public SearchGenre Genre { get; }

        public SearchLanguage Language { get; }

        [CanBeNull]
        public IReadOnlyCollection<SearchExtra> Extra { get; }

        public SearchPlayed Played { get; }

        public SearchExplicit ExplicitContent { get; }

        [CanBeNull]
        public IReadOnlyCollection<ScoreRank> Ranks { get; }

        private readonly string query;
        private readonly RulesetInfo ruleset;
        private readonly Cursor cursor;

        private string directionString => SortDirection == SortDirection.Descending ? @"desc" : @"asc";

        public SearchBeatmapSetsRequest(
            string query,
            RulesetInfo ruleset,
            Cursor cursor = null,
            IReadOnlyCollection<SearchGeneral> general = null,
            SearchCategory searchCategory = SearchCategory.Any,
            SortCriteria sortCriteria = SortCriteria.Ranked,
            SortDirection sortDirection = SortDirection.Descending,
            SearchGenre genre = SearchGenre.Any,
            SearchLanguage language = SearchLanguage.Any,
            IReadOnlyCollection<SearchExtra> extra = null,
            IReadOnlyCollection<ScoreRank> ranks = null,
            SearchPlayed played = SearchPlayed.Any,
            SearchExplicit explicitContent = SearchExplicit.Hide)
        {
            this.query = query;
            this.ruleset = ruleset;
            this.cursor = cursor;

            General = general;
            SearchCategory = searchCategory;
            SortCriteria = sortCriteria;
            SortDirection = sortDirection;
            Genre = genre;
            Language = language;
            Extra = extra;
            Ranks = ranks;
            Played = played;
            ExplicitContent = explicitContent;
        }

        protected override WebRequest CreateWebRequest()
        {
            var req = base.CreateWebRequest();

            if (query != null)
                req.AddParameter("q", query);

            if (General != null && General.Any())
                req.AddParameter("c", string.Join('.', General.Select(e => e.ToString().Underscore())));

            if (ruleset.ID.HasValue)
                req.AddParameter("m", ruleset.ID.Value.ToString());

            req.AddParameter("s", SearchCategory.ToString().ToLowerInvariant());

            if (Genre != SearchGenre.Any)
                req.AddParameter("g", ((int)Genre).ToString());

            if (Language != SearchLanguage.Any)
                req.AddParameter("l", ((int)Language).ToString());

            req.AddParameter("sort", $"{SortCriteria.ToString().ToLowerInvariant()}_{directionString}");

            if (Extra != null && Extra.Any())
                req.AddParameter("e", string.Join('.', Extra.Select(e => e.ToString().ToLowerInvariant())));

            if (Ranks != null && Ranks.Any())
                req.AddParameter("r", string.Join('.', Ranks.Select(r => r.ToString())));

            if (Played != SearchPlayed.Any)
                req.AddParameter("played", Played.ToString().ToLowerInvariant());

            req.AddParameter("nsfw", ExplicitContent == SearchExplicit.Show ? "true" : "false");

            req.AddCursor(cursor);

            return req;
        }

        protected override string Target => @"beatmapsets/search";
    }
}
