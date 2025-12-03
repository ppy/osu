// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
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
                UserTags =
                {
                    "song representation/simple",
                    "style/clean",
                }
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
        [TestCase("unit", false)]
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
        [TestCase("\"unit tests\"!", false)]
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
        [TestCase("artist")]
        [TestCase("unicode")]
        public void TestCriteriaNotMatchingArtist(string excludedTerm)
        {
            var beatmap = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                Artist = new FilterCriteria.OptionalTextFilter { SearchTerm = excludedTerm, ExcludeTerm = true }
            };

            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.Filter(criteria);

            Assert.True(carouselItem.Filtered.Value);
        }

        [TestCase("simple", false)]
        [TestCase("\"style/clean\"", false)]
        [TestCase("\"style/clean\"!", false)]
        [TestCase("iNiS-style", true)]
        [TestCase("\"reading/visually dense\"!", true)]
        public void TestCriteriaMatchingUserTags(string query, bool filtered)
        {
            var beatmap = getExampleBeatmap();
            var criteria = new FilterCriteria { UserTags = [new FilterCriteria.OptionalTextFilter { SearchTerm = query }] };
            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.Filter(criteria);

            Assert.AreEqual(filtered, carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaMatchingMultipleTagsAtOnce()
        {
            var beatmap = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                UserTags =
                [
                    new FilterCriteria.OptionalTextFilter { SearchTerm = "\"song representation/simple\"!" },
                    new FilterCriteria.OptionalTextFilter { SearchTerm = "\"style/clean\"!" }
                ]
            };
            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.Filter(criteria);

            Assert.AreEqual(false, carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaAllTagFiltersMustMatch()
        {
            var beatmap = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                UserTags =
                [
                    new FilterCriteria.OptionalTextFilter { SearchTerm = "\"song representation/simple\"!" },
                    new FilterCriteria.OptionalTextFilter { SearchTerm = "\"style/dirty\"!" }
                ]
            };
            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.Filter(criteria);

            Assert.AreEqual(true, carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaMatchingTagExcluded()
        {
            var beatmap = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                UserTags =
                [
                    new FilterCriteria.OptionalTextFilter { SearchTerm = "\"song representation/simple\"!", ExcludeTerm = true },
                ]
            };
            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.Filter(criteria);

            Assert.AreEqual(true, carouselItem.Filtered.Value);
        }

        [Test]
        public void TestCriteriaOneTagIncludedAndOneTagExcluded()
        {
            var beatmap = getExampleBeatmap();
            var criteria = new FilterCriteria
            {
                UserTags =
                [
                    new FilterCriteria.OptionalTextFilter { SearchTerm = "\"song representation/simple\"!" },
                    new FilterCriteria.OptionalTextFilter { SearchTerm = "\"style/clean\"!", ExcludeTerm = true }
                ]
            };
            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.Filter(criteria);

            Assert.AreEqual(true, carouselItem.Filtered.Value);
        }

        [Test]
        public void TestBeatmapMustHaveAtLeastOneTagIfUserTagFilterActive()
        {
            var beatmap = getExampleBeatmap();
            var criteria = new FilterCriteria { UserTags = [new FilterCriteria.OptionalTextFilter { SearchTerm = "simple" }] };
            var carouselItem = new CarouselBeatmap(beatmap);
            carouselItem.BeatmapInfo.Metadata.UserTags.Clear();
            carouselItem.Filter(criteria);

            Assert.True(carouselItem.Filtered.Value);
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

        [TestCase("title!=Title", new[] { 2, 4, 6 })]
        [TestCase("title!=\"Title1\"", new[] { 2, 3, 4, 5, 6 })]
        [TestCase("title!=\"Title1\"!", new[] { 2, 3, 4, 5, 6 })]
        public void TestNotEqualSearchForTextFilters(string query, int[] expectedBeatmapIndexes)
        {
            string[] titles =
            [
                "Title1",
                "Title1",
                "My[Favourite]Song",
                "Title",
                "Another One",
                "Diff in title",
                "a",
            ];

            var carouselBeatmaps = titles.Select(title => new CarouselBeatmap(new BeatmapInfo
            {
                Metadata = new BeatmapMetadata
                {
                    Title = title,
                },
            })).ToList();

            var criteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(criteria, query);
            carouselBeatmaps.ForEach(b => b.Filter(criteria));

            int[] visibleBeatmaps = carouselBeatmaps
                                    .Where(b => !b.Filtered.Value)
                                    .Select(b => carouselBeatmaps.IndexOf(b)).ToArray();
            Assert.That(visibleBeatmaps, Is.EqualTo(expectedBeatmapIndexes));
        }

        [Test]
        public void TestNotEqualSearchForNumberFilters()
        {
            double[] starRatings =
            [
                2.78,
                1.78,
                1.55,
                3.78,
                1.78,
                1.55,
                2.78
            ];

            var carouselBeatmaps = starRatings.Select(starRating => new CarouselBeatmap(new BeatmapInfo
            {
                StarRating = starRating,
            })).ToList();

            var criteria = new FilterCriteria();

            FilterQueryParser.ApplyQueries(criteria, "star!=1.78");
            carouselBeatmaps.ForEach(b => b.Filter(criteria));

            int[] visibleBeatmaps = carouselBeatmaps
                                    .Where(b => !b.Filtered.Value)
                                    .Select(b => carouselBeatmaps.IndexOf(b)).ToArray();

            Assert.That(visibleBeatmaps, Is.EqualTo(new[] { 0, 2, 3, 5, 6 }));
        }

        [TestCase("status!=ranked", new[] { 1, 2, 4, 5 })]
        [TestCase("status!=r", new[] { 1, 2, 4, 5 })]
        [TestCase("status!=loved", new[] { 0, 1, 2, 3, 4, 6 })]
        [TestCase("status!=l", new[] { 0, 1, 2, 3, 4, 6 })]
        [TestCase("status!=r,l", new[] { 1, 2, 4 })]
        public void TestNotEqualSearchForEnumFilter(string query, int[] expectedBeatmapIndexes)
        {
            var carouselBeatmaps = new[]
            {
                BeatmapOnlineStatus.Ranked,
                BeatmapOnlineStatus.Qualified,
                BeatmapOnlineStatus.Approved,
                BeatmapOnlineStatus.Ranked,
                BeatmapOnlineStatus.Approved,
                BeatmapOnlineStatus.Loved,
                BeatmapOnlineStatus.Ranked
            }.Select(info => new CarouselBeatmap(new BeatmapInfo
            {
                Status = info
            })).ToList();

            var criteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(criteria, query);
            carouselBeatmaps.ForEach(b => b.Filter(criteria));

            int[] visibleBeatmaps = carouselBeatmaps
                                    .Where(b => !b.Filtered.Value)
                                    .Select(b => carouselBeatmaps.IndexOf(b)).ToArray();

            Assert.That(visibleBeatmaps, Is.EqualTo(expectedBeatmapIndexes));
        }

        [TestCase("played!=1", new[] { 1, 4, 5 })]
        [TestCase("played!=0", new[] { 0, 2, 3, 6, 7 })]
        public void TestNotEqualSearchForBooleanFilter(string query, int[] expectedBeatmapIndexes)
        {
            var carouselBeatmaps = (new DateTimeOffset?[]
            {
                new DateTimeOffset(2012, 10, 21, 12, 13, 24, TimeSpan.Zero),
                null,
                new DateTimeOffset(2012, 11, 12, 23, 10, 13, TimeSpan.Zero),
                new DateTimeOffset(2013, 2, 13, 11, 43, 23, TimeSpan.Zero),
                null,
                null,
                new DateTimeOffset(2014, 1, 15, 20, 13, 24, TimeSpan.Zero),
                new DateTimeOffset(2014, 11, 16, 0, 13, 23, TimeSpan.Zero),
            }).Select(lastPlayed => new CarouselBeatmap(new BeatmapInfo
            {
                LastPlayed = lastPlayed
            })).ToList();

            var criteria = new FilterCriteria();

            FilterQueryParser.ApplyQueries(criteria, query);
            carouselBeatmaps.ForEach(b => b.Filter(criteria));

            int[] visibleBeatmaps = carouselBeatmaps
                                    .Where(b => !b.Filtered.Value)
                                    .Select(b => carouselBeatmaps.IndexOf(b)).ToArray();

            Assert.That(visibleBeatmaps, Is.EqualTo(expectedBeatmapIndexes));
        }

        [TestCase("ranked!=2012", new[] { 3, 4, 5, 6, 7 })]
        [TestCase("ranked!=2012.11", new[] { 0, 1, 3, 4, 5, 6, 7 })]
        [TestCase("ranked!=2012.10.21", new[] { 1, 2, 3, 4, 5, 6, 7 })]
        public void TestNotEqualSearchForDateFilter(string query, int[] expectedBeatmapIndexes)
        {
            var carouselBeatmaps = new[]
            {
                new DateTimeOffset(2012, 10, 21, 13, 42, 13, TimeSpan.Zero),
                new DateTimeOffset(2012, 10, 11, 2, 33, 43, TimeSpan.Zero),
                new DateTimeOffset(2012, 11, 12, 10, 22, 32, TimeSpan.Zero),
                new DateTimeOffset(2013, 2, 13, 5, 19, 0, TimeSpan.Zero),
                new DateTimeOffset(2013, 2, 13, 11, 23, 35, TimeSpan.Zero),
                new DateTimeOffset(2013, 3, 14, 9, 9, 1, TimeSpan.Zero),
                new DateTimeOffset(2014, 1, 15, 10, 5, 0, TimeSpan.Zero),
                new DateTimeOffset(2014, 11, 16, 23, 27, 0, TimeSpan.Zero),
            }.Select(dateRanked => new CarouselBeatmap(new BeatmapInfo
            {
                BeatmapSet = new BeatmapSetInfo
                {
                    DateRanked = dateRanked,
                }
            })).ToList();
            var criteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(criteria, query);
            carouselBeatmaps.ForEach(b => b.Filter(criteria));

            int[] visibleBeatmaps = carouselBeatmaps
                                    .Where(b => !b.Filtered.Value)
                                    .Select(b => carouselBeatmaps.IndexOf(b)).ToArray();

            Assert.That(visibleBeatmaps, Is.EqualTo(expectedBeatmapIndexes));
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
