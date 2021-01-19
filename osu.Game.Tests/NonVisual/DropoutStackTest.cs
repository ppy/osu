// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class DropoutStackTest
    {
        private DropoutStack<char> stack;

        [SetUp]
        public void Setup()
        {
            stack = new DropoutStack<char>();
        }

        [Test]
        public void TestEmptyStack()
        {
            Assert.AreEqual(0, stack.Count);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                char unused = stack[0];
            });

            int count = 0;
            foreach (var unused in stack)
                count++;

            Assert.AreEqual(0, count);
        }

        [Test]
        public void TestPush()
        {
            stack.Push('a');
            stack.Push('b');
            stack.Push('c');

            // Assert correct values and reverse index
            Assert.AreEqual('c', stack[0]);
            Assert.AreEqual('b', stack[1]);
            Assert.AreEqual('a', stack[2]);
        }

        [Test]
        public void TestDrop()
        {
            stack.Push('a');
            stack.Push('b');
            stack.Push('c');

            var droppedItem = stack.Drop();

            // Assert correct item return and no longer in stack
            Assert.AreEqual('a', droppedItem);
            Assert.AreEqual(2, stack.Count);
            Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                char unused = stack[2];
            });
        }

        [Test]
        public void TestClear()
        {
            stack.Push('a');
            stack.Push('b');
            stack.Push('c');

            stack.Clear();

            // Assert stack is empty
            Assert.AreEqual(0, stack.Count);
        }

        [Test]
        public void TestEnumerator()
        {
            stack.Push('a');
            stack.Push('b');
            stack.Push('c');

            char[] expectedValues = { 'c', 'b', 'a' };
            int expectedValueIndex = 0;

            // Assert items are enumerated in correct order
            foreach (var item in stack)
            {
                Assert.AreEqual(expectedValues[expectedValueIndex], item);
                expectedValueIndex++;
            }
        }
    }
}
