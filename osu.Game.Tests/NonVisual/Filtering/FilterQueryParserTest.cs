// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Screens.Select;

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

        [Test]
        public void TestApplyStarQueries()
        {
            const string query = "stars<4 easy";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("easy", filterCriteria.SearchText.Trim());
            Assert.AreEqual(1, filterCriteria.SearchTerms.Length);
            Assert.AreEqual(4.0f, filterCriteria.StarDifficulty.Max);
            Assert.IsFalse(filterCriteria.StarDifficulty.IsUpperInclusive);
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
            Assert.AreEqual(9.0f, filterCriteria.ApproachRate.Min);
            Assert.IsTrue(filterCriteria.ApproachRate.IsLowerInclusive);
            Assert.IsNull(filterCriteria.ApproachRate.Max);
        }

        [Test]
        public void TestApplyDrainRateQueries()
        {
            const string query = "dr>2 quite specific dr<:6";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("quite specific", filterCriteria.SearchText.Trim());
            Assert.AreEqual(2, filterCriteria.SearchTerms.Length);
            Assert.AreEqual(2.0f, filterCriteria.DrainRate.Min);
            Assert.IsFalse(filterCriteria.DrainRate.IsLowerInclusive);
            Assert.AreEqual(6.0f, filterCriteria.DrainRate.Max);
            Assert.IsTrue(filterCriteria.DrainRate.IsUpperInclusive);
        }

        [Test]
        public void TestApplyBPMQueries()
        {
            const string query = "bpm>:200 gotta go fast";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("gotta go fast", filterCriteria.SearchText.Trim());
            Assert.AreEqual(3, filterCriteria.SearchTerms.Length);
            Assert.AreEqual(200d, filterCriteria.BPM.Min);
            Assert.IsTrue(filterCriteria.BPM.IsLowerInclusive);
            Assert.IsNull(filterCriteria.BPM.Max);
        }

        private static object[] lengthQueryExamples =
        {
            new object[] { "6ms", TimeSpan.FromMilliseconds(6), TimeSpan.FromMilliseconds(1) },
            new object[] { "23s", TimeSpan.FromSeconds(23), TimeSpan.FromSeconds(1) },
            new object[] { "9m", TimeSpan.FromMinutes(9), TimeSpan.FromMinutes(1) },
            new object[] { "0.25h", TimeSpan.FromHours(0.25), TimeSpan.FromHours(1) },
            new object[] { "70", TimeSpan.FromSeconds(70), TimeSpan.FromSeconds(1) },
        };

        [Test]
        [TestCaseSource(nameof(lengthQueryExamples))]
        public void TestApplyLengthQueries(string lengthQuery, TimeSpan expectedLength, TimeSpan scale)
        {
            string query = $"length={lengthQuery} time";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("time", filterCriteria.SearchText.Trim());
            Assert.AreEqual(1, filterCriteria.SearchTerms.Length);
            Assert.AreEqual(expectedLength.TotalMilliseconds - scale.TotalMilliseconds / 2.0, filterCriteria.Length.Min);
            Assert.IsTrue(filterCriteria.Length.IsLowerInclusive);
            Assert.AreEqual(expectedLength.TotalMilliseconds + scale.TotalMilliseconds / 2.0, filterCriteria.Length.Max);
            Assert.IsTrue(filterCriteria.Length.IsUpperInclusive);
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
        public void TestApplyStatusQueries()
        {
            const string query = "I want the pp status=ranked";
            var filterCriteria = new FilterCriteria();
            FilterQueryParser.ApplyQueries(filterCriteria, query);
            Assert.AreEqual("I want the pp", filterCriteria.SearchText.Trim());
            Assert.AreEqual(4, filterCriteria.SearchTerms.Length);
            Assert.AreEqual(BeatmapSetOnlineStatus.Ranked, filterCriteria.OnlineStatus.Min);
            Assert.IsTrue(filterCriteria.OnlineStatus.IsLowerInclusive);
            Assert.AreEqual(BeatmapSetOnlineStatus.Ranked, filterCriteria.OnlineStatus.Max);
            Assert.IsTrue(filterCriteria.OnlineStatus.IsUpperInclusive);
        }
    }
}
