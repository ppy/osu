// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmap : CarouselItem
    {
        public override float TotalHeight => DrawableCarouselBeatmap.HEIGHT;

        public readonly BeatmapInfo BeatmapInfo;

        public CarouselBeatmap(BeatmapInfo beatmapInfo)
        {
            BeatmapInfo = beatmapInfo;
            State.Value = CarouselItemState.Collapsed;
        }

        public override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmap(this);

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            Filtered.Value = !checkMatch(criteria);
        }

        private bool checkMatch(FilterCriteria criteria)
        {
            bool match =
                criteria.Ruleset == null ||
                BeatmapInfo.Ruleset.ShortName == criteria.Ruleset.ShortName ||
                (BeatmapInfo.Ruleset.OnlineID == 0 && criteria.Ruleset.OnlineID != 0 && criteria.AllowConvertedBeatmaps);

            if (BeatmapInfo.BeatmapSet?.Equals(criteria.SelectedBeatmapSet) == true)
            {
                // only check ruleset equality or convertability for selected beatmap
                return match;
            }

            match &= !criteria.StarDifficulty.HasFilter || criteria.StarDifficulty.IsInRange(BeatmapInfo.StarRating);
            match &= !criteria.ApproachRate.HasFilter || criteria.ApproachRate.IsInRange(BeatmapInfo.Difficulty.ApproachRate);
            match &= !criteria.DrainRate.HasFilter || criteria.DrainRate.IsInRange(BeatmapInfo.Difficulty.DrainRate);
            match &= !criteria.CircleSize.HasFilter || criteria.CircleSize.IsInRange(BeatmapInfo.Difficulty.CircleSize);
            match &= !criteria.OverallDifficulty.HasFilter || criteria.OverallDifficulty.IsInRange(BeatmapInfo.Difficulty.OverallDifficulty);
            match &= !criteria.Length.HasFilter || criteria.Length.IsInRange(BeatmapInfo.Length);
            match &= !criteria.BPM.HasFilter || criteria.BPM.IsInRange(BeatmapInfo.BPM);

            match &= !criteria.BeatDivisor.HasFilter || criteria.BeatDivisor.IsInRange(BeatmapInfo.BeatDivisor);
            match &= !criteria.OnlineStatus.HasFilter || criteria.OnlineStatus.IsInRange(BeatmapInfo.Status);

            if (!match) return false;

            match &= !criteria.Creator.HasFilter || criteria.Creator.Matches(BeatmapInfo.Metadata.Author.Username);
            match &= !criteria.Artist.HasFilter || criteria.Artist.Matches(BeatmapInfo.Metadata.Artist) ||
                     criteria.Artist.Matches(BeatmapInfo.Metadata.ArtistUnicode);
            match &= !criteria.Title.HasFilter || criteria.Title.Matches(BeatmapInfo.Metadata.Title) ||
                     criteria.Title.Matches(BeatmapInfo.Metadata.TitleUnicode);
            match &= !criteria.DifficultyName.HasFilter || criteria.DifficultyName.Matches(BeatmapInfo.DifficultyName);
            match &= !criteria.UserStarDifficulty.HasFilter || criteria.UserStarDifficulty.IsInRange(BeatmapInfo.StarRating);

            if (!match) return false;

            if (criteria.SearchTerms.Length > 0)
            {
                var searchableTerms = BeatmapInfo.GetSearchableTerms();

                foreach (FilterCriteria.OptionalTextFilter criteriaTerm in criteria.SearchTerms)
                {
                    bool any = false;

                    // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
                    foreach (string searchTerm in searchableTerms)
                    {
                        if (!criteriaTerm.Matches(searchTerm)) continue;

                        any = true;
                        break;
                    }

                    if (any) continue;

                    match = false;
                    break;
                }

                // if a match wasn't found via text matching of terms, do a second catch-all check matching against online IDs.
                // this should be done after text matching so we can prioritise matching numbers in metadata.
                if (!match && criteria.SearchNumber.HasValue)
                {
                    match = (BeatmapInfo.OnlineID == criteria.SearchNumber.Value) ||
                            (BeatmapInfo.BeatmapSet?.OnlineID == criteria.SearchNumber.Value);
                }
            }

            if (!match) return false;

            match &= criteria.CollectionBeatmapMD5Hashes?.Contains(BeatmapInfo.MD5Hash) ?? true;
            if (match && criteria.RulesetCriteria != null)
                match &= criteria.RulesetCriteria.Matches(BeatmapInfo);

            return match;
        }

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmap otherBeatmap))
                return base.CompareTo(criteria, other);

            switch (criteria.Sort)
            {
                default:
                case SortMode.Difficulty:
                    int ruleset = BeatmapInfo.Ruleset.CompareTo(otherBeatmap.BeatmapInfo.Ruleset);

                    if (ruleset != 0) return ruleset;

                    return BeatmapInfo.StarRating.CompareTo(otherBeatmap.BeatmapInfo.StarRating);
            }
        }

        public override string ToString() => BeatmapInfo.ToString();
    }
}
