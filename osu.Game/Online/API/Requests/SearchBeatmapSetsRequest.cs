﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
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
        public SearchCategory SearchCategory { get; }

        public SortCriteria SortCriteria { get; }

        public SortDirection SortDirection { get; }

        public SearchGenre Genre { get; }

        public SearchLanguage Language { get; }

        [CanBeNull]
        public IReadOnlyCollection<SearchExtra> Extra { get; }

        public SearchPlayed Played { get; }

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
            SearchCategory searchCategory = SearchCategory.Any,
            SortCriteria sortCriteria = SortCriteria.Ranked,
            SortDirection sortDirection = SortDirection.Descending,
            SearchGenre genre = SearchGenre.Any,
            SearchLanguage language = SearchLanguage.Any,
            IReadOnlyCollection<SearchExtra> extra = null,
            IReadOnlyCollection<ScoreRank> ranks = null,
            SearchPlayed played = SearchPlayed.Any)
        {
            this.query = string.IsNullOrEmpty(query) ? string.Empty : System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;
            this.cursor = cursor;

            SearchCategory = searchCategory;
            SortCriteria = sortCriteria;
            SortDirection = sortDirection;
            Genre = genre;
            Language = language;
            Extra = extra;
            Ranks = ranks;
            Played = played;
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

            if (Extra != null && Extra.Any())
                req.AddParameter("e", string.Join('.', Extra.Select(e => e.ToString().ToLowerInvariant())));

            if (Ranks != null && Ranks.Any())
                req.AddParameter("r", string.Join('.', Ranks.Select(r => r.ToString())));

            if (Played != SearchPlayed.Any)
                req.AddParameter("played", Played.ToString().ToLowerInvariant());

            req.AddCursor(cursor);

            return req;
        }

        protected override string Target => @"beatmapsets/search";
    }
}
