// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select.Filter;
using osu.Game.Utils;

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

            if (!match) return false;

            if (criteria.SearchTerms.Length > 0)
            {
                match = BeatmapInfo.Match(criteria.SearchTerms);

                // if a match wasn't found via text matching of terms, do a second catch-all check matching against online IDs.
                // this should be done after text matching so we can prioritise matching numbers in metadata.
                if (!match && criteria.SearchNumber.HasValue)
                {
                    match = (BeatmapInfo.OnlineID == criteria.SearchNumber.Value) ||
                            (BeatmapInfo.BeatmapSet?.OnlineID == criteria.SearchNumber.Value);
                }
            }

            if (!match) return false;

            match &= !criteria.StarDifficulty.HasFilter || criteria.StarDifficulty.IsInRange(BeatmapInfo.StarRating.FloorToDecimalDigits(2));
            match &= !criteria.ApproachRate.HasFilter || criteria.ApproachRate.IsInRange(BeatmapInfo.Difficulty.ApproachRate);
            match &= !criteria.DrainRate.HasFilter || criteria.DrainRate.IsInRange(BeatmapInfo.Difficulty.DrainRate);
            match &= !criteria.CircleSize.HasFilter || criteria.CircleSize.IsInRange(BeatmapInfo.Difficulty.CircleSize);
            match &= !criteria.OverallDifficulty.HasFilter || criteria.OverallDifficulty.IsInRange(BeatmapInfo.Difficulty.OverallDifficulty);
            match &= !criteria.Length.HasFilter || criteria.Length.IsInRange(BeatmapInfo.Length);
            match &= !criteria.LastPlayed.HasFilter || criteria.LastPlayed.IsInRange(BeatmapInfo.LastPlayed ?? DateTimeOffset.MinValue);
            match &= !criteria.DateRanked.HasFilter || (BeatmapInfo.BeatmapSet?.DateRanked != null && criteria.DateRanked.IsInRange(BeatmapInfo.BeatmapSet.DateRanked.Value));
            match &= !criteria.DateSubmitted.HasFilter || (BeatmapInfo.BeatmapSet?.DateSubmitted != null && criteria.DateSubmitted.IsInRange(BeatmapInfo.BeatmapSet.DateSubmitted.Value));
            match &= !criteria.BPM.HasFilter || criteria.BPM.IsInRange(BeatmapInfo.BPM);

            match &= !criteria.BeatDivisor.HasFilter || criteria.BeatDivisor.IsInRange(BeatmapInfo.BeatDivisor);
            match &= !criteria.OnlineStatus.HasFilter || criteria.OnlineStatus.IsInRange(BeatmapInfo.Status);

            if (!match) return false;

            match &= !criteria.Creator.HasFilter || criteria.Creator.Matches(BeatmapInfo.Metadata.Author.Username);

            if (criteria.Artist.HasFilter)
            {
                if (criteria.Artist.ExcludeTerm)
                    match &= criteria.Artist.Matches(BeatmapInfo.Metadata.Artist) && criteria.Artist.Matches(BeatmapInfo.Metadata.ArtistUnicode);
                else
                    match &= criteria.Artist.Matches(BeatmapInfo.Metadata.Artist) || criteria.Artist.Matches(BeatmapInfo.Metadata.ArtistUnicode);
            }

            if (criteria.Title.HasFilter)
            {
                if (criteria.Title.ExcludeTerm)
                    match &= criteria.Title.Matches(BeatmapInfo.Metadata.Title) && criteria.Title.Matches(BeatmapInfo.Metadata.TitleUnicode);
                else
                    match &= criteria.Title.Matches(BeatmapInfo.Metadata.Title) || criteria.Title.Matches(BeatmapInfo.Metadata.TitleUnicode);
            }

            match &= !criteria.DifficultyName.HasFilter || criteria.DifficultyName.Matches(BeatmapInfo.DifficultyName);
            match &= !criteria.Source.HasFilter || criteria.Source.Matches(BeatmapInfo.Metadata.Source);

            if (criteria.UserTags.Any())
            {
                foreach (var tagFilter in criteria.UserTags)
                {
                    if (tagFilter.ExcludeTerm)
                    {
                        // if `ExcludeTerm` is true, `Matches()` will return true if a user tag *doesn't match* the excluded term.
                        // thus, every user tag must pass this filter.
                        foreach (string tag in BeatmapInfo.Metadata.UserTags)
                            match &= tagFilter.Matches(tag);
                    }
                    else
                    {
                        // if `ExcludeTerm` is false, `Matches()` will return true if a user tag *matches* the expected term.
                        // the expected behaviour is that a beatmap should be displayed if at least one of the user tags passes the filter.
                        bool anyTagMatched = false;

                        foreach (string tag in BeatmapInfo.Metadata.UserTags)
                            anyTagMatched |= tagFilter.Matches(tag);

                        match &= anyTagMatched;
                    }
                }
            }

            match &= !criteria.UserStarDifficulty.HasFilter || criteria.UserStarDifficulty.IsInRange(BeatmapInfo.StarRating);

            if (!match) return false;

            match &= criteria.CollectionBeatmapMD5Hashes?.Contains(BeatmapInfo.MD5Hash) ?? true;
            if (match && criteria.RulesetCriteria != null)
                match &= criteria.RulesetCriteria.Matches(BeatmapInfo, criteria);

            if (match && criteria.HasOnlineID == true)
                match &= BeatmapInfo.OnlineID >= 0;

            if (match && criteria.BeatmapSetId != null)
                match &= criteria.BeatmapSetId == BeatmapInfo.BeatmapSet?.OnlineID;

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
