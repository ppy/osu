// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;

namespace osu.Game.Tests.NonVisual.Filtering
{
    [TestFixture]
    public class FilterMatchingTest
    {
        private readonly BeatmapInfo exampleBeatmapInfo = new BeatmapInfo
        {
            Ruleset = new RulesetInfo { ID = 5 },
            StarDifficulty = 4.0d,
            BaseDifficulty = new BeatmapDifficulty
            {
                ApproachRate = 5.0f,
                DrainRate = 3.0f,
                CircleSize = 2.0f,
            },
            Metadata = new BeatmapMetadata
            {
                Artist = "The Artist",
                ArtistUnicode = "The Artist",
                Title = "Title goes here",
                TitleUnicode = "Title goes here",
                AuthorString = "Author",
                Source = "unit tests",
                Tags = "look for tags too",
            },
            Version = "version as well",
            Length = 2500,
            BPM = 160,
            BeatDivisor = 12,
            Status = BeatmapSetOnlineStatus.Loved
        };

        [Test]
        public void TestCriteriaMatchingNoRuleset()
        {
            var criteria = new FilterCriteria();
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.IsFalse(carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaMatchingSpecificRuleset()
        {
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { ID = 6 }
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.IsTrue(carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaMatchingConvertedBeatmaps()
        {
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { ID = 6 },
                AllowConvertedBeatmaps = true
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.IsFalse(carouselItem.Filtered.Value);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TestCriteriaMatchingRangeMin(bool inclusive)
        {
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { ID = 6 },
                AllowConvertedBeatmaps = true,
                ApproachRate = new FilterCriteria.OptionalRange<float>
                {
                    IsLowerInclusive = inclusive,
                    Min = 5.0f
                }
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(!inclusive, carouselItem.Filtered.Value);
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void TestCriteriaMatchingRangeMax(bool inclusive)
        {
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { ID = 6 },
                AllowConvertedBeatmaps = true,
                BPM = new FilterCriteria.OptionalRange<double>
                {
                    IsUpperInclusive = inclusive,
                    Max = 160d
                }
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(!inclusive, carouselItem.Filtered.Value);
        }

        [Test]
        [TestCase("artist", false)]
        [TestCase("artist title author", false)]
        [TestCase("an artist", true)]
        [TestCase("tags too", false)]
        [TestCase("version", false)]
        [TestCase("an auteur", true)]
        public void TestCriteriaMatchingTerms(string terms, bool filtered)
        {
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { ID = 6 },
                AllowConvertedBeatmaps = true,
                SearchText = terms
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }
    }
}
