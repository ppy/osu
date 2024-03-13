// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using BenchmarkDotNet.Attributes;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;

namespace osu.Game.Benchmarks
{
    public class BenchmarkCarouselFilter : BenchmarkTest
    {
        private BeatmapInfo getExampleBeatmap() => new BeatmapInfo
        {
            Ruleset = new RulesetInfo
            {
                ShortName = "osu",
                OnlineID = 0
            },
            StarRating = 4.0d,
            Difficulty = new BeatmapDifficulty
            {
                ApproachRate = 5.0f,
                DrainRate = 3.0f,
                CircleSize = 2.0f,
            },
            Metadata = new BeatmapMetadata
            {
                Artist = "The Artist",
                ArtistUnicode = "check unicode too",
                Title = "Title goes here",
                TitleUnicode = "Title goes here",
                Author = { Username = "The Author" },
                Source = "unit tests",
                Tags = "look for tags too",
            },
            DifficultyName = "version as well",
            Length = 2500,
            BPM = 160,
            BeatDivisor = 12,
            Status = BeatmapOnlineStatus.Loved
        };

        private CarouselBeatmap carouselBeatmap = null!;
        private FilterCriteria criteria1 = null!;
        private FilterCriteria criteria2 = null!;
        private FilterCriteria criteria3 = null!;
        private FilterCriteria criteria4 = null!;
        private FilterCriteria criteria5 = null!;
        private FilterCriteria criteria6 = null!;

        public override void SetUp()
        {
            var beatmap = getExampleBeatmap();
            beatmap.OnlineID = 20201010;
            beatmap.BeatmapSet = new BeatmapSetInfo { OnlineID = 1535 };
            carouselBeatmap = new CarouselBeatmap(beatmap);
            criteria1 = new FilterCriteria();
            criteria2 = new FilterCriteria
            {
                Ruleset = new RulesetInfo { ShortName = "catch" }
            };
            criteria3 = new FilterCriteria
            {
                Ruleset = new RulesetInfo { OnlineID = 6 },
                AllowConvertedBeatmaps = true,
                BPM = new FilterCriteria.OptionalRange<double>
                {
                    IsUpperInclusive = false,
                    Max = 160d
                }
            };
            criteria4 = new FilterCriteria
            {
                Ruleset = new RulesetInfo { OnlineID = 6 },
                AllowConvertedBeatmaps = true,
                SearchText = "an artist"
            };
            criteria5 = new FilterCriteria
            {
                Creator = new FilterCriteria.OptionalTextFilter { SearchTerm = "the author AND then something else" }
            };
            criteria6 = new FilterCriteria { SearchText = "20201010" };
        }

        [Benchmark]
        public void CarouselBeatmapFilter()
        {
            carouselBeatmap.Filter(criteria1);
        }

        [Benchmark]
        public void CriteriaMatchingSpecificRuleset()
        {
            carouselBeatmap.Filter(criteria2);
        }

        [Benchmark]
        public void CriteriaMatchingRangeMax()
        {
            carouselBeatmap.Filter(criteria3);
        }

        [Benchmark]
        public void CriteriaMatchingTerms()
        {
            carouselBeatmap.Filter(criteria4);
        }

        [Benchmark]
        public void CriteriaMatchingCreator()
        {
            carouselBeatmap.Filter(criteria5);
        }

        [Benchmark]
        public void CriteriaMatchingBeatmapIDs()
        {
            carouselBeatmap.Filter(criteria6);
        }
    }
}
