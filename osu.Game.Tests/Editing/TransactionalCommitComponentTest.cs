// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using NUnit.Framework;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public partial class TransactionalCommitComponentTest
    {
        private TestHandler handler;

        [SetUp]
        public void SetUp()
        {
            handler = new TestHandler();
        }

        [Test]
        public void TestCommitTransaction()
        {
            Assert.That(handler.StateUpdateCount, Is.EqualTo(0));

            handler.BeginChange();
            Assert.That(handler.StateUpdateCount, Is.EqualTo(0));
            handler.EndChange();

            Assert.That(handler.StateUpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void TestSaveOutsideOfTransactionTriggersUpdates()
        {
            Assert.That(handler.StateUpdateCount, Is.EqualTo(0));

            handler.SaveState();
            Assert.That(handler.StateUpdateCount, Is.EqualTo(1));

            handler.SaveState();
            Assert.That(handler.StateUpdateCount, Is.EqualTo(2));
        }

        [Test]
        public void TestEventsFire()
        {
            int transactionBegan = 0;
            int transactionEnded = 0;
            int stateSaved = 0;

            handler.TransactionBegan += () => transactionBegan++;
            handler.TransactionEnded += () => transactionEnded++;
            handler.SaveStateTriggered += () => stateSaved++;

            handler.BeginChange();
            Assert.That(transactionBegan, Is.EqualTo(1));

            handler.EndChange();
            Assert.That(transactionEnded, Is.EqualTo(1));

            Assert.That(stateSaved, Is.EqualTo(0));
            handler.SaveState();
            Assert.That(stateSaved, Is.EqualTo(1));
        }

        [Test]
        public void TestSaveDuringTransactionDoesntTriggerUpdate()
        {
            Assert.That(handler.StateUpdateCount, Is.EqualTo(0));

            handler.BeginChange();

            handler.SaveState();
            Assert.That(handler.StateUpdateCount, Is.EqualTo(0));

            handler.EndChange();

            Assert.That(handler.StateUpdateCount, Is.EqualTo(1));
        }

        [Test]
        public void TestEndWithoutBeginThrows()
        {
            handler.BeginChange();
            handler.EndChange();
            Assert.That(() => handler.EndChange(), Throws.TypeOf<InvalidOperationException>());
        }

        private partial class TestHandler : TransactionalCommitComponent
        {
            public int StateUpdateCount { get; private set; }

            protected override void UpdateState()
            {
                StateUpdateCount++;
            }
        }
    }
}
