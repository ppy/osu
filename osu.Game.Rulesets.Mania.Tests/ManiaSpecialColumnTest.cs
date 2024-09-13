// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Beatmaps;
using NUnit.Framework;

namespace osu.Game.Rulesets.Mania.Tests
{
    [TestFixture]
    public class ManiaSpecialColumnTest
    {
        [TestCase(new[]
        {
            true
        }, 1)]
        [TestCase(new[]
        {
            false,
            false,
            false,
            false
        }, 4)]
        [TestCase(new[]
        {
            false,
            false,
            false,
            true,
            false,
            false,
            false
        }, 7)]
        public void Test(IEnumerable<bool> special, int columns)
        {
            var definition = new StageDefinition(columns);
            var results = getResults(definition);
            Assert.AreEqual(special, results);
        }

        private IEnumerable<bool> getResults(StageDefinition definition)
        {
            for (int i = 0; i < definition.Columns; i++)
                yield return definition.IsSpecialColumn(i);
        }
    }
}
