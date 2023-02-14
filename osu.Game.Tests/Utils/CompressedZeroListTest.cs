// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Utils;

namespace osu.Game.Tests.Utils
{
    [TestFixture]
    public class CompressedZeroListTest
    {
        [Test]
        public void TestEmpty()
        {
            CompactList list = new CompactList();

            Assert.IsFalse(list.Any());
        }

        [Test]
        public void TestZeros()
        {
            CompactList list = new CompactList
            {
                0.0
            };

            list.AddZeros(2);
            list.Add(0.0);

            Assert.AreEqual(list.Count, 4);
            Assert.AreEqual(list.ToList(), new List<double> { 0, 0, 0, 0 });
        }

        [Test]
        public void TestNonZeros()
        {
#pragma warning disable IDE0028 // Simplify collection initialization
            CompactList list = new CompactList
            {
                1.0
            };
#pragma warning restore IDE0028 // Simplify collection initialization

            list.Add(2.0);
            list.Add(3.0);

            Assert.AreEqual(list.Count, 3);
            Assert.AreEqual(list.ToList(), new List<double> { 1.0, 2.0, 3.0 });
        }

        [Test]
        public void TestMixed()
        {
            CompactList list = new CompactList();

            list.AddZeros(2);
            list.Add(1.0);
            list.Add(0.0);
            list.AddZeros(1);
            list.Add(2.0);
            list.Add(3.0);

            Assert.AreEqual(list.Count, 7);
            Assert.AreEqual(list.ToList(), new List<double> { 0.0, 0.0, 1.0, 0.0, 0.0, 2.0, 3.0 });
        }
    }
}
