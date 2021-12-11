// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

            bool match =
                criteria.Ruleset == null ||
                BeatmapInfo.RulesetID == criteria.Ruleset.ID ||
                (BeatmapInfo.RulesetID == 0 && criteria.Ruleset.ID > 0 && criteria.AllowConvertedBeatmaps);

            if (BeatmapInfo.BeatmapSet?.Equals(criteria.SelectedBeatmapSet) == true)
            {
                // only check ruleset equality or convertability for selected beatmap
                Filtered.Value = !match;
                return;
            }

            match &= !criteria.StarDifficulty.HasFilter || criteria.StarDifficulty.IsInRange(BeatmapInfo.StarRating);
            match &= !criteria.ApproachRate.HasFilter || criteria.ApproachRate.IsInRange(BeatmapInfo.BaseDifficulty.ApproachRate);
            match &= !criteria.DrainRate.HasFilter || criteria.DrainRate.IsInRange(BeatmapInfo.BaseDifficulty.DrainRate);
            match &= !criteria.CircleSize.HasFilter || criteria.CircleSize.IsInRange(BeatmapInfo.BaseDifficulty.CircleSize);
            match &= !criteria.OverallDifficulty.HasFilter || criteria.OverallDifficulty.IsInRange(BeatmapInfo.BaseDifficulty.OverallDifficulty);
            match &= !criteria.Length.HasFilter || criteria.Length.IsInRange(BeatmapInfo.Length);
            match &= !criteria.BPM.HasFilter || criteria.BPM.IsInRange(BeatmapInfo.BPM);

            match &= !criteria.BeatDivisor.HasFilter || criteria.BeatDivisor.IsInRange(BeatmapInfo.BeatDivisor);
            match &= !criteria.OnlineStatus.HasFilter || criteria.OnlineStatus.IsInRange(BeatmapInfo.Status);

            match &= !criteria.Creator.HasFilter || criteria.Creator.Matches(BeatmapInfo.Metadata.Author.Username);
            match &= !criteria.Artist.HasFilter || criteria.Artist.Matches(BeatmapInfo.Metadata.Artist) ||
                     criteria.Artist.Matches(BeatmapInfo.Metadata.ArtistUnicode);

            match &= !criteria.UserStarDifficulty.HasFilter || criteria.UserStarDifficulty.IsInRange(BeatmapInfo.StarRating);

            if (match)
            {
                string[] terms = BeatmapInfo.GetSearchableTerms();

                foreach (string criteriaTerm in criteria.SearchTerms)
                    match &= terms.Any(term => term.Contains(criteriaTerm, StringComparison.InvariantCultureIgnoreCase));

                // if a match wasn't found via text matching of terms, do a second catch-all check matching against online IDs.
                // this should be done after text matching so we can prioritise matching numbers in metadata.
                if (!match && criteria.SearchNumber.HasValue)
                {
                    match = (BeatmapInfo.OnlineID == criteria.SearchNumber.Value) ||
                            (BeatmapInfo.BeatmapSet?.OnlineID == criteria.SearchNumber.Value);
                }
            }

            if (match)
                match &= criteria.Collection?.Beatmaps.Contains(BeatmapInfo) ?? true;

            if (match && criteria.RulesetCriteria != null)
                match &= criteria.RulesetCriteria.Matches(BeatmapInfo);

            Filtered.Value = !match;
        }

        public override int CompareTo(FilterCriteria criteria, CarouselItem other)
        {
            if (!(other is CarouselBeatmap otherBeatmap))
                return base.CompareTo(criteria, other);

            switch (criteria.Sort)
            {
                default:
                case SortMode.Difficulty:
                    int ruleset = BeatmapInfo.RulesetID.CompareTo(otherBeatmap.BeatmapInfo.RulesetID);
                    if (ruleset != 0) return ruleset;

                    return BeatmapInfo.StarRating.CompareTo(otherBeatmap.BeatmapInfo.StarRating);
            }
        }

        public override string ToString() => BeatmapInfo.ToString();
    }
}
