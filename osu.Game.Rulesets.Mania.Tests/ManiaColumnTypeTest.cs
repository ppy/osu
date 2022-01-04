// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Beatmaps;
using NUnit.Framework;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaColumnTypeTest
    {
        [TestCase(new[]
        {
            ColumnType.Special
        }, 1)]
        [TestCase(new[]
        {
            ColumnType.Odd,
            ColumnType.Even,
            ColumnType.Even,
            ColumnType.Odd
        }, 4)]
        [TestCase(new[]
        {
            ColumnType.Odd,
            ColumnType.Even,
            ColumnType.Odd,
            ColumnType.Special,
            ColumnType.Odd,
            ColumnType.Even,
            ColumnType.Odd
        }, 7)]
        public void Test(IEnumerable<ColumnType> expected, int columns)
        {
            var definition = new StageDefinition
            {
                Columns = columns
            };
            var results = getResults(definition);
            Assert.AreEqual(expected, results);
        }

        private IEnumerable<ColumnType> getResults(StageDefinition definition)
        {
            for (int i = 0; i < definition.Columns; i++)
                yield return definition.GetTypeOfColumn(i);
        }
    }
}
