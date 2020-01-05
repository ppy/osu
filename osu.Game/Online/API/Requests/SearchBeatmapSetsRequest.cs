// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.ComponentModel;
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
        private string directionString => direction == SortDirection.Descending ? @"desc" : @"asc";

        public SearchBeatmapSetsRequest(string query, RulesetInfo ruleset, BeatmapSearchCategory searchCategory = BeatmapSearchCategory.Any, DirectSortCriteria sortCriteria = DirectSortCriteria.Ranked, SortDirection direction = SortDirection.Descending)
        {
            this.query = string.IsNullOrEmpty(query) ? string.Empty : System.Uri.EscapeDataString(query);
            this.ruleset = ruleset;
            this.searchCategory = searchCategory;
            this.sortCriteria = sortCriteria;
            this.direction = direction;
        }

        // ReSharper disable once ImpureMethodCallOnReadonlyValueField
        protected override string Target => $@"beatmapsets/search?q={query}&m={ruleset.ID ?? 0}&s={searchCategory.ToString().ToLowerInvariant()}&sort={sortCriteria.ToString().ToLowerInvariant()}_{directionString}";
    }

    public enum BeatmapSearchCategory
    {
        [Description("所有谱面")]
        Any,

        [Description("拥有排行榜的谱面")]
        Leaderboard,
        [Description("计入排行的谱面")]
        Ranked,
        [Description("质量合格的谱面")]
        Qualified,
        [Description("Loved谱面")]
        Loved,
        [Description("喜欢的谱面")]
        Favourites,

        [Description("审核中 和 制作中的谱面")]
        Pending,
        [Description("坟图")]
        Graveyard,

        [Description("我制作的谱面")]
        Mine,
    }
}
