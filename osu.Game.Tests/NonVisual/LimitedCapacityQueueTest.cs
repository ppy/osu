// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Game.Utils;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class LimitedCapacityQueueTest
    {
        private const int capacity = 3;

        private LimitedCapacityQueue<int> queue;

        [SetUp]
        public void SetUp()
        {
            queue = new LimitedCapacityQueue<int>(capacity);
        }

        [Test]
        public void TestEmptyQueue()
        {
            Assert.AreEqual(0, queue.Count);

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = queue[0]);

            Assert.Throws<InvalidOperationException>(() => _ = queue.Dequeue());

            int count = 0;
            foreach (int _ in queue)
                count++;

            Assert.AreEqual(0, count);
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void TestBelowCapacity(int count)
        {
            for (int i = 0; i < count; ++i)
                queue.Enqueue(i);

            Assert.AreEqual(count, queue.Count);

            for (int i = 0; i < count; ++i)
                Assert.AreEqual(i, queue[i]);

            int j = 0;
            foreach (int item in queue)
                Assert.AreEqual(j++, item);

            for (int i = queue.Count; i < queue.Count + capacity; i++)
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = queue[i]);
        }

        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void TestEnqueueAtFullCapacity(int count)
        {
            for (int i = 0; i < count; ++i)
                queue.Enqueue(i);

            Assert.AreEqual(capacity, queue.Count);

            for (int i = 0; i < queue.Count; ++i)
                Assert.AreEqual(count - capacity + i, queue[i]);

            int j = count - capacity;
            foreach (int item in queue)
                Assert.AreEqual(j++, item);

            for (int i = queue.Count; i < queue.Count + capacity; i++)
                Assert.Throws<ArgumentOutOfRangeException>(() => _ = queue[i]);
        }

        [TestCase(4)]
        [TestCase(5)]
        [TestCase(6)]
        public void TestDequeueAtFullCapacity(int count)
        {
            for (int i = 0; i < count; ++i)
                queue.Enqueue(i);

            for (int i = 0; i < capacity; ++i)
            {
                Assert.AreEqual(count - capacity + i, queue.Dequeue());
                Assert.AreEqual(2 - i, queue.Count);
            }

            Assert.Throws<InvalidOperationException>(() => queue.Dequeue());
        }

        [Test]
        public void TestClearQueue()
        {
            queue.Enqueue(3);
            queue.Enqueue(5);
            Assert.AreEqual(2, queue.Count);

            queue.Clear();
            Assert.AreEqual(0, queue.Count);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = queue[0]);

            queue.Enqueue(7);
            Assert.AreEqual(1, queue.Count);
            Assert.AreEqual(7, queue[0]);
            Assert.Throws<ArgumentOutOfRangeException>(() => _ = queue[1]);

            queue.Enqueue(9);
            Assert.AreEqual(2, queue.Count);
            Assert.AreEqual(9, queue[1]);
        }
    }
}
