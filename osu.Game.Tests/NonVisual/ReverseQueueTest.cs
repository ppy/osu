// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class ReverseQueueTest
    {
        private ReverseQueue<char> queue;

        [SetUp]
        public void Setup()
        {
            queue = new ReverseQueue<char>(4);
        }

        [Test]
        public void TestEmptyQueue()
        {
            Assert.AreEqual(0, queue.Count);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                char unused = queue[0];
            });

            int count = 0;
            foreach (char unused in queue)
                count++;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void TestEnqueue()
        {
            // Assert correct values and reverse index after enqueueing
            queue.Enqueue('a');
            queue.Enqueue('b');
            queue.Enqueue('c');

            Assert.AreEqual('c', queue[0]);
            Assert.AreEqual('b', queue[1]);
            Assert.AreEqual('a', queue[2]);

            // Assert correct values and reverse index after enqueueing beyond initial capacity of 4
            queue.Enqueue('d');
            queue.Enqueue('e');
            queue.Enqueue('f');

            Assert.AreEqual('f', queue[0]);
            Assert.AreEqual('e', queue[1]);
            Assert.AreEqual('d', queue[2]);
            Assert.AreEqual('c', queue[3]);
            Assert.AreEqual('b', queue[4]);
            Assert.AreEqual('a', queue[5]);
        }

        [Test]
        public void TestDequeue()
        {
            queue.Enqueue('a');
            queue.Enqueue('b');
            queue.Enqueue('c');
            queue.Enqueue('d');
            queue.Enqueue('e');
            queue.Enqueue('f');

            // Assert correct item return and no longer in queue after dequeueing
            Assert.AreEqual('a', queue[5]);
            char dequeuedItem = queue.Dequeue();

            Assert.AreEqual('a', dequeuedItem);
            Assert.AreEqual(5, queue.Count);
            Assert.AreEqual('f', queue[0]);
            Assert.AreEqual('b', queue[4]);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                char unused = queue[5];
            });

            // Assert correct state after enough enqueues and dequeues to wrap around array (queue.start = 0 again)
            queue.Enqueue('g');
            queue.Enqueue('h');
            queue.Enqueue('i');
            queue.Dequeue();
            queue.Dequeue();
            queue.Dequeue();
            queue.Dequeue();
            queue.Dequeue();
            queue.Dequeue();
            queue.Dequeue();

            Assert.AreEqual(1, queue.Count);
            Assert.AreEqual('i', queue[0]);
        }

        [Test]
        public void TestClear()
        {
            queue.Enqueue('a');
            queue.Enqueue('b');
            queue.Enqueue('c');
            queue.Enqueue('d');
            queue.Enqueue('e');
            queue.Enqueue('f');

            // Assert queue is empty after clearing
            queue.Clear();

            Assert.AreEqual(0, queue.Count);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                char unused = queue[0];
            });
        }

        [Test]
        public void TestEnumerator()
        {
            queue.Enqueue('a');
            queue.Enqueue('b');
            queue.Enqueue('c');
            queue.Enqueue('d');
            queue.Enqueue('e');
            queue.Enqueue('f');

            char[] expectedValues = { 'f', 'e', 'd', 'c', 'b', 'a' };
            int expectedValueIndex = 0;

            // Assert items are enumerated in correct order
            foreach (char item in queue)
            {
                Assert.AreEqual(expectedValues[expectedValueIndex], item);
                expectedValueIndex++;
            }
        }
    }
}
