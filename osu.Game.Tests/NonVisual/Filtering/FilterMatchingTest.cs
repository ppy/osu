// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Filter;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Carousel;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Tests.NonVisual.Filtering
{
    [TestFixture]
    public class FilterMatchingTest
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
                TitleUnicode = "TitleUnicode goes here",
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

        [Test]
        public void TestCriteriaMatchingNoRuleset()
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria();
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.IsFalse(carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaMatchingSpecificRuleset()
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { ShortName = "catch" }
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.IsTrue(carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaMatchingConvertedBeatmaps()
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { OnlineID = 6 },
                AllowConvertedBeatmaps = true
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.IsFalse(carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaMatchingConvertedBeatmapsForCustomRulesets()
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { OnlineID = -1 },
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
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { OnlineID = 6 },
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
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { OnlineID = 6 },
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
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { OnlineID = 6 },
                AllowConvertedBeatmaps = true,
                SearchText = terms
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }

        [Test]
        [TestCase("\"artist\"", false)]
        [TestCase("\"arti\"", true)]
        [TestCase("\"artist title author\"", true)]
        [TestCase("\"artist\" \"title\" \"author\"", false)]
        [TestCase("\"an artist\"", true)]
        [TestCase("\"tags too\"", false)]
        [TestCase("\"tags to\"", true)]
        [TestCase("\"version\"", false)]
        [TestCase("\"an auteur\"", true)]
        [TestCase("\"Artist\"!", true)]
        [TestCase("\"The Artist\"!", false)]
        [TestCase("\"the artist\"!", false)]
        [TestCase("\"\\\"", true)] // nasty case, covers properly escaping user input in underlying regex.
        public void TestCriteriaMatchingExactTerms(string terms, bool filtered)
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Ruleset = new RulesetInfo { OnlineID = 6 },
                AllowConvertedBeatmaps = true,
                SearchText = terms
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }

        [Test]
        [TestCase("", false)]
        [TestCase("The", false)]
        [TestCase("THE", false)]
        [TestCase("author", false)]
        [TestCase("the author", false)]
        [TestCase("the author AND then something else", true)]
        [TestCase("unknown", true)]
        public void TestCriteriaMatchingCreator(string creatorName, bool filtered)
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Creator = new FilterCriteria.OptionalTextFilter { SearchTerm = creatorName }
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }

        [Test]
        [TestCase("", false)]
        [TestCase("Goes", false)]
        [TestCase("GOES", false)]
        [TestCase("goes", false)]
        [TestCase("title goes", false)]
        [TestCase("title goes AND then something else", true)]
        [TestCase("titleunicode", false)]
        [TestCase("unknown", true)]
        public void TestCriteriaMatchingTitle(string titleName, bool filtered)
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Title = new FilterCriteria.OptionalTextFilter { SearchTerm = titleName }
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }

        [Test]
        [TestCase("", false)]
        [TestCase("The", false)]
        [TestCase("THE", false)]
        [TestCase("artist", false)]
        [TestCase("the artist", false)]
        [TestCase("the artist AND then something else", true)]
        [TestCase("unicode too", false)]
        [TestCase("unknown", true)]
        [TestCase("\"Artist\"!", true)]
        [TestCase("\"The Artist\"!", false)]
        [TestCase("\"the artist\"!", false)]
        public void TestCriteriaMatchingArtist(string artistName, bool filtered)
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Artist = new FilterCriteria.OptionalTextFilter { SearchTerm = artistName }
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }

        [Test]
        [TestCase("", false)]
        [TestCase("artist", false)]
        [TestCase("unknown", true)]
        public void TestCriteriaMatchingArtistWithNullUnicodeName(string artistName, bool filtered)
        {
            var exampleBeatmapInfo = getExampleBeatmap();
            exampleBeatmapInfo.Metadata.ArtistUnicode = string.Empty;

            var criteria = new FilterCriteria
            {
                Artist = new FilterCriteria.OptionalTextFilter { SearchTerm = artistName }
            };
            var carouselItem = new CarouselBeatmap(exampleBeatmapInfo);
            carouselItem.Filter(criteria);
            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }

        [TestCase("202010", true)]
        [TestCase("20201010", false)]
        [TestCase("153", true)]
        [TestCase("1535", false)]
        public void TestCriteriaMatchingBeatmapIDs(string query, bool filtered)
        {
            var beatmap = getExampleBeatmap();
            beatmap.OnlineID = 20201010;
            beatmap.BeatmapSet = new BeatmapSetInfo { OnlineID = 1535 };

            var criteria = new FilterCriteria { SearchText = query };
            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.Filter(criteria);

            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCustomRulesetCriteria([Values(null, true, false)] bool? matchCustomCriteria)
        {
            var beatmap = getExampleBeatmap();

            var customCriteria = matchCustomCriteria is bool match ? new CustomCriteria(match) : null;
            var criteria = new FilterCriteria { RulesetCriteria = customCriteria };
            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.Filter(criteria);

            Assert.AreEqual(matchCustomCriteria == false, carouselItem.Filtered.Value);
        }

        private class CustomCriteria : IRulesetFilterCriteria
        {
            private readonly bool match;

            public CustomCriteria(bool shouldMatch)
            {
                match = shouldMatch;
            }

            public bool Matches(BeatmapInfo beatmapInfo, FilterCriteria criteria) => match;
            public bool TryParseCustomKeywordCriteria(string key, Operator op, string value) => false;

            public bool FilterMayChangeFromMods(ValueChangedEvent<IReadOnlyList<Mod>> mods) => false;
        }
    }
}
