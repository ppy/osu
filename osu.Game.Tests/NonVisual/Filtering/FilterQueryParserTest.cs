﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Filter;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Tests.NonVisual.Filtering
{
    [TestFixture]
    public class FilterQueryParserTest
    {
        [Test]
        public void TestApplyQueriesBareWords()
        {
            const string query = "looking for a beatmap";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("looking for a beatmap", filterCriteria.SearchText);
            Assert.AreEqual(4, filterCriteria.SearchTerms.Length);
        }

        /*
         * The following tests have been written a bit strangely (they don't check exact
         * bound equality with what the filter says).
         * This is to account for floating-point arithmetic issues.
         * For example, specifying a bpm<140 filter would previously match beatmaps with BPM
         * of 139.99999, which would be displayed in the UI as 140.
         * Due to this the tests check the last tick inside the range and the first tick
         * outside of the range.
         */

        [TestCase("star")]
        [TestCase("stars")]
        public void TestApplyStarQueries(string variant)
        {
            string query = $"{variant}<4 easy";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("easy", filterCriteria.SearchText.Trim());
            Assert.AreEqual(1, filterCriteria.SearchTerms.Length);
            Assert.IsNotNull(filterCriteria.StarDifficulty.Max);
            Assert.Greater(filterCriteria.StarDifficulty.Max, 3.99d);
            Assert.Less(filterCriteria.StarDifficulty.Max, 4.00d);
            Assert.IsNull(filterCriteria.StarDifficulty.Min);
        }

        [Test]
        public void TestApplyApproachRateQueries()
        {
            const string query = "ar>=9 difficult";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("difficult", filterCriteria.SearchText.Trim());
            Assert.AreEqual(1, filterCriteria.SearchTerms.Length);
            Assert.IsNotNull(filterCriteria.ApproachRate.Min);
            Assert.Greater(filterCriteria.ApproachRate.Min, 8.9f);
            Assert.Less(filterCriteria.ApproachRate.Min, 9.0f);
            Assert.IsNull(filterCriteria.ApproachRate.Max);
        }

        [Test]
        public void TestApplyDrainRateQueriesByDrKeyword()
        {
            const string query = "dr>2 quite specific dr<:6";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("quite specific", filterCriteria.SearchText.Trim());
            Assert.AreEqual(2, filterCriteria.SearchTerms.Length);
            Assert.Greater(filterCriteria.DrainRate.Min, 2.0f);
            Assert.Less(filterCriteria.DrainRate.Min, 2.1f);
            Assert.Greater(filterCriteria.DrainRate.Max, 6.0f);
            Assert.Less(filterCriteria.DrainRate.Min, 6.1f);
        }

        [Test]
        public void TestApplyDrainRateQueriesByHpKeyword()
        {
            const string query = "hp>2 quite specific hp<=6";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("quite specific", filterCriteria.SearchText.Trim());
            Assert.AreEqual(2, filterCriteria.SearchTerms.Length);
            Assert.Greater(filterCriteria.DrainRate.Min, 2.0f);
            Assert.Less(filterCriteria.DrainRate.Min, 2.1f);
            Assert.Greater(filterCriteria.DrainRate.Max, 6.0f);
            Assert.Less(filterCriteria.DrainRate.Min, 6.1f);
        }

        [Test]
        public void TestApplyOverallDifficultyQueries()
        {
            const string query = "od>4 easy od<8";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("easy", filterCriteria.SearchText.Trim());
            Assert.AreEqual(1, filterCriteria.SearchTerms.Length);
            Assert.Greater(filterCriteria.OverallDifficulty.Min, 4.0);
            Assert.Less(filterCriteria.OverallDifficulty.Min, 4.1);
            Assert.Greater(filterCriteria.OverallDifficulty.Max, 7.9);
            Assert.Less(filterCriteria.OverallDifficulty.Max, 8.0);
        }

        [Test]
        public void TestApplyBPMQueries()
        {
            const string query = "bpm>:200 gotta go fast";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("gotta go fast", filterCriteria.SearchText.Trim());
            Assert.AreEqual(3, filterCriteria.SearchTerms.Length);
            Assert.IsNotNull(filterCriteria.BPM.Min);
            Assert.Greater(filterCriteria.BPM.Min, 199.99d);
            Assert.Less(filterCriteria.BPM.Min, 200.00d);
            Assert.IsNull(filterCriteria.BPM.Max);
        }

        private static readonly object[] correct_length_query_examples =
        {
            new object[] { "23s", TimeSpan.FromSeconds(23), TimeSpan.FromSeconds(1) },
            new object[] { "9m", TimeSpan.FromMinutes(9), TimeSpan.FromMinutes(1) },
            new object[] { "0.25h", TimeSpan.FromHours(0.25), TimeSpan.FromHours(1) },
            new object[] { "70", TimeSpan.FromSeconds(70), TimeSpan.FromSeconds(1) },
            new object[] { "7m27s", TimeSpan.FromSeconds(447), TimeSpan.FromSeconds(1) },
            new object[] { "7:27", TimeSpan.FromSeconds(447), TimeSpan.FromSeconds(1) },
            new object[] { "1h2m3s", TimeSpan.FromSeconds(3723), TimeSpan.FromSeconds(1) },
            new object[] { "1h2m3.5s", TimeSpan.FromSeconds(3723.5), TimeSpan.FromSeconds(1) },
            new object[] { "1:2:3", TimeSpan.FromSeconds(3723), TimeSpan.FromSeconds(1) },
            new object[] { "1:02:03", TimeSpan.FromSeconds(3723), TimeSpan.FromSeconds(1) },
            new object[] { "6", TimeSpan.FromSeconds(6), TimeSpan.FromSeconds(1) },
            new object[] { "6.5", TimeSpan.FromSeconds(6.5), TimeSpan.FromSeconds(1) },
            new object[] { "6.5s", TimeSpan.FromSeconds(6.5), TimeSpan.FromSeconds(1) },
            new object[] { "6.5m", TimeSpan.FromMinutes(6.5), TimeSpan.FromMinutes(1) },
            new object[] { "6h5m", TimeSpan.FromMinutes(365), TimeSpan.FromMinutes(1) },
            new object[] { "65m", TimeSpan.FromMinutes(65), TimeSpan.FromMinutes(1) },
            new object[] { "90s", TimeSpan.FromSeconds(90), TimeSpan.FromSeconds(1) },
            new object[] { "80m20s", TimeSpan.FromSeconds(4820), TimeSpan.FromSeconds(1) },
            new object[] { "1h20s", TimeSpan.FromSeconds(3620), TimeSpan.FromSeconds(1) },
        };

        [Test]
        [TestCaseSource(nameof(correct_length_query_examples))]
        public void TestApplyLengthQueries(string lengthQuery, TimeSpan expectedLength, TimeSpan scale)
        {
            string query = $"length={lengthQuery} time";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("time", filterCriteria.SearchText.Trim());
            Assert.AreEqual(1, filterCriteria.SearchTerms.Length);
            Assert.AreEqual(expectedLength.TotalMilliseconds - scale.TotalMilliseconds / 2.0, filterCriteria.Length.Min);
            Assert.AreEqual(expectedLength.TotalMilliseconds + scale.TotalMilliseconds / 2.0, filterCriteria.Length.Max);
        }

        private static readonly object[] incorrect_length_query_examples =
        {
            new object[] { "7.5m27s" },
            new object[] { "7m27" },
            new object[] { "7m7m7m" },
            new object[] { "7m70s" },
            new object[] { "5s6m" },
            new object[] { "0:" },
            new object[] { ":0" },
            new object[] { "0:3:" },
            new object[] { "3:15.5" },
        };

        [Test]
        [TestCaseSource(nameof(incorrect_length_query_examples))]
        public void TestInvalidLengthQueries(string lengthQuery)
        {
            string query = $"length={lengthQuery} time";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual(false, filterCriteria.Length.HasFilter);
        }

        [Test]
        public void TestApplyDivisorQueries()
        {
            const string query = "that's a time signature alright! divisor:12";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("that's a time signature alright!", filterCriteria.SearchText.Trim());
            Assert.AreEqual(5, filterCriteria.SearchTerms.Length);
            Assert.AreEqual(12, filterCriteria.BeatDivisor.Min);
            Assert.IsTrue(filterCriteria.BeatDivisor.IsLowerInclusive);
            Assert.AreEqual(12, filterCriteria.BeatDivisor.Max);
            Assert.IsTrue(filterCriteria.BeatDivisor.IsUpperInclusive);
        }

        [Test]
        public void TestPartialStatusMatch()
        {
            const string query = "status=r";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual(BeatmapOnlineStatus.Ranked, filterCriteria.OnlineStatus.Min);
            Assert.AreEqual(BeatmapOnlineStatus.Ranked, filterCriteria.OnlineStatus.Max);
        }

        [Test]
        public void TestApplyStatusQueries()
        {
            const string query = "I want the pp status=ranked";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("I want the pp", filterCriteria.SearchText.Trim());
            Assert.AreEqual(4, filterCriteria.SearchTerms.Length);
            Assert.AreEqual(BeatmapOnlineStatus.Ranked, filterCriteria.OnlineStatus.Min);
            Assert.IsTrue(filterCriteria.OnlineStatus.IsLowerInclusive);
            Assert.AreEqual(BeatmapOnlineStatus.Ranked, filterCriteria.OnlineStatus.Max);
            Assert.IsTrue(filterCriteria.OnlineStatus.IsUpperInclusive);
        }

        [Test]
        public void TestApplyCreatorQueries()
        {
            const string query = "beatmap specifically by creator=my_fav";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("beatmap specifically by", filterCriteria.SearchText.Trim());
            Assert.AreEqual(3, filterCriteria.SearchTerms.Length);
            Assert.AreEqual("my_fav", filterCriteria.Creator.SearchTerm);
        }

        [Test]
        public void TestApplyArtistQueries()
        {
            const string query = "find me songs by artist=singer please";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("find me songs by  please", filterCriteria.SearchText.Trim());
            Assert.AreEqual(5, filterCriteria.SearchTerms.Length);
            Assert.AreEqual("singer", filterCriteria.Artist.SearchTerm);
        }

        [Test]
        public void TestApplyArtistQueriesWithSpaces()
        {
            const string query = "really like artist=\"name with space\" yes";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("really like  yes", filterCriteria.SearchText.Trim());
            Assert.AreEqual(3, filterCriteria.SearchTerms.Length);
            Assert.AreEqual("name with space", filterCriteria.Artist.SearchTerm);
        }

        [Test]
        public void TestApplyArtistQueriesOneDoubleQuote()
        {
            const string query = "weird artist=double\"quote";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("weird", filterCriteria.SearchText.Trim());
            Assert.AreEqual(1, filterCriteria.SearchTerms.Length);
            Assert.AreEqual("double\"quote", filterCriteria.Artist.SearchTerm);
        }

        [Test]
        public void TestOperatorParsing()
        {
            const string query = "artist=><something";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("><something", filterCriteria.Artist.SearchTerm);
        }

        [Test]
        public void TestUnrecognisedKeywordIsIgnored()
        {
            const string query = "unrecognised=keyword";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("unrecognised=keyword", filterCriteria.SearchText);
        }

        [TestCase("cs=nope")]
        [TestCase("bpm>=bad")]
        [TestCase("divisor<nah")]
        [TestCase("status=noidea")]
        public void TestInvalidKeywordValueIsIgnored(string query)
        {
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual(query, filterCriteria.SearchText);
        }

        [Test]
        public void TestCustomKeywordIsParsed()
        {
            var customCriteria = new CustomFilterCriteria();
            const string query = "custom=readme unrecognised=keyword";
            var filterCriteria = new FilterCriteria { RulesetCriteria = customCriteria };
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("readme", customCriteria.CustomValue);
            Assert.AreEqual("unrecognised=keyword", filterCriteria.SearchText.Trim());
        }

        private class CustomFilterCriteria : IRulesetFilterCriteria
        {
            public string? CustomValue { get; set; }

            public bool Matches(BeatmapInfo beatmapInfo) => true;

            public bool TryParseCustomKeywordCriteria(string key, Operator op, string value)
            {
                if (key == "custom" && op == Operator.Equal)
                {
                    CustomValue = value;
                    return true;
                }

                return false;
            }
        }
    }
}
