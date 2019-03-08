// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class LimitedCapacityStackTest
    {
        private const int capacity = 3;

        private LimitedCapacityStack<int> stack;

        [SetUp]
        public void Setup()
        {
            stack = new LimitedCapacityStack<int>(capacity);
        }

        [Test]
        public void TestEmptyStack()
        {
            Assert.AreEqual(0, stack.Count);

            Assert.Throws<IndexOutOfRangeException>(() =>
            {
                int unused = stack[0];
            });

            int count = 0;
            foreach (var unused in stack)
                count++;

            Assert.AreEqual(0, count);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TestInRangeElements(int count)
        {
            // e.g. 0 -> 1 -> 2
            for (int i = 0; i < count; i++)
                stack.Push(i);

            Assert.AreEqual(count, stack.Count);

            // e.g. 2 -> 1 -> 0 (reverse order)
            for (int i = 0; i < stack.Count; i++)
                Assert.AreEqual(count - 1 - i, stack[i]);

            // e.g. indices 3, 4, 5, 6 (out of range)
            for (int i = stack.Count; i < stack.Count + capacity; i++)
            {
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    int unused = stack[i];
                });
            }
        }

        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void TestOverflowElements(int count)
        {
            // e.g. 0 -> 1 -> 2 -> 3
            for (int i = 0; i < count; i++)
                stack.Push(i);

            Assert.AreEqual(capacity, stack.Count);

            // e.g. 3 -> 2 -> 1 (reverse order)
            for (int i = 0; i < stack.Count; i++)
                Assert.AreEqual(count - 1 - i, stack[i]);

            // e.g. indices 3, 4, 5, 6 (out of range)
            for (int i = stack.Count; i < stack.Count + capacity; i++)
            {
                Assert.Throws<IndexOutOfRangeException>(() =>
                {
                    int unused = stack[i];
                });
            }
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void TestEnumerator(int count)
        {
            // e.g. 0 -> 1 -> 2 -> 3
            for (int i = 0; i < count; i++)
                stack.Push(i);

            int enumeratorCount = 0;
            int expectedValue = count - 1;

            foreach (var item in stack)
            {
                Assert.AreEqual(expectedValue, item);
                enumeratorCount++;
                expectedValue--;
            }

            Assert.AreEqual(stack.Count, enumeratorCount);
        }
    }
}
