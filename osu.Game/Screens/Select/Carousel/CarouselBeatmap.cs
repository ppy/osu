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
        public readonly BeatmapInfo Beatmap;

        public CarouselBeatmap(BeatmapInfo beatmap)
        {
            Beatmap = beatmap;
            State.Value = CarouselItemState.Collapsed;
        }

        protected override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmap(this);

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            bool match =
                criteria.Ruleset == null ||
                Beatmap.RulesetID == criteria.Ruleset.ID ||
                (Beatmap.RulesetID == 0 && criteria.Ruleset.ID > 0 && criteria.AllowConvertedBeatmaps);

            match &= criteria.StarDifficulty.IsInRange(Beatmap.StarDifficulty);
            match &= criteria.ApproachRate.IsInRange(Beatmap.BaseDifficulty.ApproachRate);
            match &= criteria.DrainRate.IsInRange(Beatmap.BaseDifficulty.DrainRate);
            match &= criteria.CircleSize.IsInRange(Beatmap.BaseDifficulty.CircleSize);
            match &= criteria.Length.IsInRange(Beatmap.Length);
            match &= criteria.BPM.IsInRange(Beatmap.BPM);

            match &= criteria.BeatDivisor.IsInRange(Beatmap.BeatDivisor);
            match &= criteria.OnlineStatus.IsInRange(Beatmap.Status);

            match &= criteria.Creator.Matches(Beatmap.Metadata.AuthorString);
            match &= criteria.Artist.Matches(Beatmap.Metadata.Artist) ||
                     criteria.Artist.Matches(Beatmap.Metadata.ArtistUnicode);

            if (match)
                foreach (var criteriaTerm in criteria.SearchTerms)
                    match &=
                        Beatmap.Metadata.SearchableTerms.Any(term => term.IndexOf(criteriaTerm, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                        Beatmap.Version.IndexOf(criteriaTerm, StringComparison.InvariantCultureIgnoreCase) >= 0;

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
                    var ruleset = Beatmap.RulesetID.CompareTo(otherBeatmap.Beatmap.RulesetID);
                    if (ruleset != 0) return ruleset;

                    return Beatmap.StarDifficulty.CompareTo(otherBeatmap.Beatmap.StarDifficulty);
            }
        }

        public override string ToString() => Beatmap.ToString();
    }
}
