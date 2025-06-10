// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Carousel;
using osu.Game.Screens.Select;

namespace osu.Game.Screens.SelectV2
{
    public class BeatmapCarouselFilterMatching : ICarouselFilter
    {
        private readonly Func<FilterCriteria> getCriteria;

        /// <summary>
        /// The total number of beatmap difficulties displayed post filter.
        /// </summary>
        public int BeatmapItemsCount { get; private set; }

        public BeatmapCarouselFilterMatching(Func<FilterCriteria> getCriteria)
        {
            this.getCriteria = getCriteria;
        }

        public async Task<List<CarouselItem>> Run(IEnumerable<CarouselItem> items, CancellationToken cancellationToken) => await Task.Run(() =>
        {
            var criteria = getCriteria();

            return matchItems(items, criteria).ToList();
        }, cancellationToken).ConfigureAwait(false);

        private IEnumerable<CarouselItem> matchItems(IEnumerable<CarouselItem> items, FilterCriteria criteria)
        {
            int countMatching = 0;

            foreach (var item in items)
            {
                var beatmap = (BeatmapInfo)item.Model;

                if (beatmap.Hidden)
                    continue;

                if (!checkCriteriaMatch(beatmap, criteria))
                    continue;

                countMatching++;
                yield return item;
            }

            BeatmapItemsCount = countMatching;
        }

        private static bool checkCriteriaMatch(BeatmapInfo beatmap, FilterCriteria criteria)
        {
            bool match = criteria.Ruleset == null ||
                         beatmap.Ruleset.ShortName == criteria.Ruleset.ShortName ||
                         (beatmap.Ruleset.OnlineID == 0 && criteria.Ruleset.OnlineID != 0 && criteria.AllowConvertedBeatmaps);

            if (beatmap.BeatmapSet?.Equals(criteria.SelectedBeatmapSet) == true)
            {
                // only check ruleset equality or convertability for selected beatmap
                return match;
            }

            if (!match) return false;

            if (criteria.SearchTerms.Length > 0)
            {
                match = beatmap.Match(criteria.SearchTerms);

                // if a match wasn't found via text matching of terms, do a second catch-all check matching against online IDs.
                // this should be done after text matching so we can prioritise matching numbers in metadata.
                if (!match && criteria.SearchNumber.HasValue)
                {
                    match = (beatmap.OnlineID == criteria.SearchNumber.Value) ||
                            (beatmap.BeatmapSet?.OnlineID == criteria.SearchNumber.Value);
                }
            }

            if (!match) return false;

            match &= !criteria.StarDifficulty.HasFilter || criteria.StarDifficulty.IsInRange(beatmap.StarRating);
            match &= !criteria.ApproachRate.HasFilter || criteria.ApproachRate.IsInRange(beatmap.Difficulty.ApproachRate);
            match &= !criteria.DrainRate.HasFilter || criteria.DrainRate.IsInRange(beatmap.Difficulty.DrainRate);
            match &= !criteria.CircleSize.HasFilter || criteria.CircleSize.IsInRange(beatmap.Difficulty.CircleSize);
            match &= !criteria.OverallDifficulty.HasFilter || criteria.OverallDifficulty.IsInRange(beatmap.Difficulty.OverallDifficulty);
            match &= !criteria.Length.HasFilter || criteria.Length.IsInRange(beatmap.Length);
            match &= !criteria.LastPlayed.HasFilter || criteria.LastPlayed.IsInRange(beatmap.LastPlayed ?? DateTimeOffset.MinValue);
            match &= !criteria.DateRanked.HasFilter || (beatmap.BeatmapSet?.DateRanked != null && criteria.DateRanked.IsInRange(beatmap.BeatmapSet.DateRanked.Value));
            match &= !criteria.DateSubmitted.HasFilter || (beatmap.BeatmapSet?.DateSubmitted != null && criteria.DateSubmitted.IsInRange(beatmap.BeatmapSet.DateSubmitted.Value));
            match &= !criteria.BPM.HasFilter || criteria.BPM.IsInRange(beatmap.BPM);

            match &= !criteria.BeatDivisor.HasFilter || criteria.BeatDivisor.IsInRange(beatmap.BeatDivisor);
            match &= !criteria.OnlineStatus.HasFilter || criteria.OnlineStatus.IsInRange(beatmap.Status);

            if (!match) return false;

            match &= !criteria.Creator.HasFilter || criteria.Creator.Matches(beatmap.Metadata.Author.Username);
            match &= !criteria.Artist.HasFilter || criteria.Artist.Matches(beatmap.Metadata.Artist) ||
                     criteria.Artist.Matches(beatmap.Metadata.ArtistUnicode);
            match &= !criteria.Title.HasFilter || criteria.Title.Matches(beatmap.Metadata.Title) ||
                     criteria.Title.Matches(beatmap.Metadata.TitleUnicode);
            match &= !criteria.DifficultyName.HasFilter || criteria.DifficultyName.Matches(beatmap.DifficultyName);
            match &= !criteria.Source.HasFilter || criteria.Source.Matches(beatmap.Metadata.Source);
            match &= !criteria.UserStarDifficulty.HasFilter || criteria.UserStarDifficulty.IsInRange(beatmap.StarRating);

            if (!match) return false;

            match &= criteria.CollectionBeatmapMD5Hashes?.Contains(beatmap.MD5Hash) ?? true;
            if (match && criteria.RulesetCriteria != null)
                match &= criteria.RulesetCriteria.Matches(beatmap, criteria);

            if (match && criteria.HasOnlineID == true)
                match &= beatmap.OnlineID >= 0;

            if (match && criteria.BeatmapSetId != null)
                match &= criteria.BeatmapSetId == beatmap.BeatmapSet?.OnlineID;

            return match;
        }
    }
}
