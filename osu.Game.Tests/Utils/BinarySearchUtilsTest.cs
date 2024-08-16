// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Utils;

namespace osu.Game.Tests.Utils
{
    [TestFixture]
    public class BinarySearchUtilsTest
    {
        [Test]
        public void TestEmptyList()
        {
            Assert.That(BinarySearchUtils.BinarySearch(Array.Empty<int>(), 0, x => x), Is.EqualTo(-1));
            Assert.That(BinarySearchUtils.BinarySearch(Array.Empty<int>(), 0, x => x, EqualitySelection.Leftmost), Is.EqualTo(-1));
            Assert.That(BinarySearchUtils.BinarySearch(Array.Empty<int>(), 0, x => x, EqualitySelection.Rightmost), Is.EqualTo(-1));
        }

        [TestCase(new[] { 1 }, 0, -1)]
        [TestCase(new[] { 1 }, 1, 0)]
        [TestCase(new[] { 1 }, 2, -2)]
        [TestCase(new[] { 1, 3 }, 0, -1)]
        [TestCase(new[] { 1, 3 }, 1, 0)]
        [TestCase(new[] { 1, 3 }, 2, -2)]
        [TestCase(new[] { 1, 3 }, 3, 1)]
        [TestCase(new[] { 1, 3 }, 4, -3)]
        public void TestUniqueScenarios(int[] values, int search, int expectedIndex)
        {
            Assert.That(BinarySearchUtils.BinarySearch(values, search, x => x, EqualitySelection.FirstFound), Is.EqualTo(expectedIndex));
            Assert.That(BinarySearchUtils.BinarySearch(values, search, x => x, EqualitySelection.Leftmost), Is.EqualTo(expectedIndex));
            Assert.That(BinarySearchUtils.BinarySearch(values, search, x => x, EqualitySelection.Rightmost), Is.EqualTo(expectedIndex));
        }

        [TestCase(new[] { 1, 2, 2 }, 2, 1)]
        [TestCase(new[] { 1, 2, 2, 2 }, 2, 1)]
        [TestCase(new[] { 1, 2, 2, 2, 3 }, 2, 2)]
        [TestCase(new[] { 1, 2, 2, 3 }, 2, 1)]
        public void TestFirstFoundDuplicateScenarios(int[] values, int search, int expectedIndex)
        {
            Assert.That(BinarySearchUtils.BinarySearch(values, search, x => x), Is.EqualTo(expectedIndex));
        }

        [TestCase(new[] { 1, 2, 2 }, 2, 1)]
        [TestCase(new[] { 1, 2, 2, 2 }, 2, 1)]
        [TestCase(new[] { 1, 2, 2, 2, 3 }, 2, 1)]
        [TestCase(new[] { 1, 2, 2, 3 }, 2, 1)]
        public void TestLeftMostDuplicateScenarios(int[] values, int search, int expectedIndex)
        {
            Assert.That(BinarySearchUtils.BinarySearch(values, search, x => x, EqualitySelection.Leftmost), Is.EqualTo(expectedIndex));
        }

        [TestCase(new[] { 1, 2, 2 }, 2, 2)]
        [TestCase(new[] { 1, 2, 2, 2 }, 2, 3)]
        [TestCase(new[] { 1, 2, 2, 2, 3 }, 2, 3)]
        [TestCase(new[] { 1, 2, 2, 3 }, 2, 2)]
        public void TestRightMostDuplicateScenarios(int[] values, int search, int expectedIndex)
        {
            Assert.That(BinarySearchUtils.BinarySearch(values, search, x => x, EqualitySelection.Rightmost), Is.EqualTo(expectedIndex));
        }
    }
}
