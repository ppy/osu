// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.Mods;
using osu.Game.Screens.Select;
using osu.Game.Screens.Select.Filter;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaFilterCriteriaTest
    {
        [TestCase]
        public void TestKeysEqualSingleValue()
        {
            var criteria = new ManiaFilterCriteria();
            criteria.TryParseCustomKeywordCriteria("keys", Operator.Equal, "1");

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 1 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 2 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 3 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new RulesetInfo { OnlineID = 0 }, new BeatmapDifficulty { CircleSize = 4 }),
                new FilterCriteria
                {
                    Mods = [new ManiaModKey1()]
                }));
        }

        [TestCase]
        public void TestKeysEqualMultipleValues()
        {
            var criteria = new ManiaFilterCriteria();
            criteria.TryParseCustomKeywordCriteria("keys", Operator.Equal, "1,3,5,7");

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 1 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 2 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 3 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new RulesetInfo { OnlineID = 0 }, new BeatmapDifficulty { CircleSize = 4 }),
                new FilterCriteria
                {
                    Mods = [new ManiaModKey1()]
                }));
        }

        [TestCase]
        public void TestKeysNotEqualSingleValue()
        {
            var criteria = new ManiaFilterCriteria();
            criteria.TryParseCustomKeywordCriteria("keys", Operator.NotEqual, "1");

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 1 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 2 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 3 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new RulesetInfo { OnlineID = 0 }, new BeatmapDifficulty { CircleSize = 4 }),
                new FilterCriteria
                {
                    Mods = [new ManiaModKey1()]
                }));
        }

        [TestCase]
        public void TestKeysNotEqualMultipleValues()
        {
            var criteria = new ManiaFilterCriteria();
            criteria.TryParseCustomKeywordCriteria("keys", Operator.NotEqual, "1,3,5,7");

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 1 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 2 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 3 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new RulesetInfo { OnlineID = 0 }, new BeatmapDifficulty { CircleSize = 4 }),
                new FilterCriteria
                {
                    Mods = [new ManiaModKey1()]
                }));
        }

        [TestCase]
        public void TestKeysGreaterOrEqualThan()
        {
            var criteria = new ManiaFilterCriteria();
            criteria.TryParseCustomKeywordCriteria("keys", Operator.GreaterOrEqual, "4");

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 1 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 2 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 4 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 5 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new RulesetInfo { OnlineID = 0 }, new BeatmapDifficulty { CircleSize = 3 }),
                new FilterCriteria
                {
                    Mods = [new ManiaModKey7()]
                }));
        }

        [TestCase]
        public void TestFilterIntersection()
        {
            var criteria = new ManiaFilterCriteria();
            criteria.TryParseCustomKeywordCriteria("keys", Operator.Greater, "4");
            criteria.TryParseCustomKeywordCriteria("keys", Operator.NotEqual, "7");

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 3 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 4 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 5 }),
                new FilterCriteria()));

            Assert.False(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 7 }),
                new FilterCriteria()));

            Assert.True(criteria.Matches(
                new BeatmapInfo(new ManiaRuleset().RulesetInfo, new BeatmapDifficulty { CircleSize = 9 }),
                new FilterCriteria()));
        }

        [TestCase]
        public void TestInvalidFilters()
        {
            var criteria = new ManiaFilterCriteria();

            Assert.False(criteria.TryParseCustomKeywordCriteria("keys", Operator.Equal, "some text"));
            Assert.False(criteria.TryParseCustomKeywordCriteria("keys", Operator.NotEqual, "4,some text"));
            Assert.False(criteria.TryParseCustomKeywordCriteria("keys", Operator.GreaterOrEqual, "4,5,6"));
        }
    }
}
