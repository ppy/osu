// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Utils;

namespace osu.Game.Tests.Utils
{
    [TestFixture]
    public class CompactListTest
    {
        [Test]
        public void TestEmpty()
        {
            CompactList list = new CompactList();

            Assert.IsFalse(list.Any());
        }

        [Test]
        public void TestOnlyEqual()
        {
            CompactList list = new CompactList
            {
                0.0,
                { 0.0, 2 },
                0.0
            };

            Assert.AreEqual(list.Count, 4);
            Assert.AreEqual(list.ToList(), new List<double> { 0, 0, 0, 0 });
        }

        [Test]
        public void TestOnlyDistinct()
        {
            CompactList list = new CompactList
            {
                1.0,
                2.0,
                3.0
            };

            Assert.AreEqual(list.Count, 3);
            Assert.AreEqual(list.ToList(), new List<double> { 1.0, 2.0, 3.0 });
        }

        [Test]
        public void TestMixed()
        {
            CompactList list = new CompactList
            {
                1.0,
                1.0 + 1e-14,
                { 0.0, 2 },
                2.0,
                2.0 + 1e-17,
                3.0
            };

            Assert.AreEqual(list.Count, 7);
            Assert.AreEqual(list.ToList(), new List<double> { 1.0, 1.0 + 1e-14, 0.0, 0.0, 2.0, 2.0, 3.0 });
        }
    }
}
