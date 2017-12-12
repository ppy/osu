// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select.Carousel
{
    public class CarouselBeatmap : CarouselItem
    {
        public readonly BeatmapInfo Beatmap;

        public CarouselBeatmap(BeatmapInfo beatmap)
        {
            Beatmap = beatmap;
            State.Value = CarouselItemState.Hidden;
        }

        protected override DrawableCarouselItem CreateDrawableRepresentation() => new DrawableCarouselBeatmap(this)
        {
            /*GainedSelection = panelGainedSelection,
            HideRequested = p => HideDifficultyRequested?.Invoke(p),
            StartRequested = p => StartRequested?.Invoke(p.beatmap),
            EditRequested = p => EditRequested?.Invoke(p.beatmap),*/
        };

        public override void Filter(FilterCriteria criteria)
        {
            base.Filter(criteria);

            bool match = criteria.Ruleset == null || (Beatmap.RulesetID == criteria.Ruleset.ID || Beatmap.RulesetID == 0 && criteria.Ruleset.ID > 0 && criteria.AllowConvertedBeatmaps);

            if (!string.IsNullOrEmpty(criteria.SearchText))
                match &=
                    Beatmap.Metadata.SearchableTerms.Any(term => term.IndexOf(criteria.SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0) ||
                    Beatmap.Version.IndexOf(criteria.SearchText, StringComparison.InvariantCultureIgnoreCase) >= 0;

            Filtered.Value = !match;
        }
    }
}
