// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class EditorChangeHandlerTest
    {
        [Test]
        public void TestSaveRestoreState()
        {
            var handler = new EditorChangeHandler(new EditorBeatmap(new Beatmap()));

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.SaveState();

            Assert.That(handler.CanUndo.Value, Is.True);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.RestoreState(-1);

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.True);
        }

        [Test]
        public void TestMaxStatesSaved()
        {
            var handler = new EditorChangeHandler(new EditorBeatmap(new Beatmap()));

            Assert.That(handler.CanUndo.Value, Is.False);

            for (int i = 0; i < EditorChangeHandler.MAX_SAVED_STATES; i++)
                handler.SaveState();

            Assert.That(handler.CanUndo.Value, Is.True);

            for (int i = 0; i < EditorChangeHandler.MAX_SAVED_STATES; i++)
            {
                Assert.That(handler.CanUndo.Value, Is.True);
                handler.RestoreState(-1);
            }

            Assert.That(handler.CanUndo.Value, Is.False);
        }

        [Test]
        public void TestMaxStatesExceeded()
        {
            var handler = new EditorChangeHandler(new EditorBeatmap(new Beatmap()));

            Assert.That(handler.CanUndo.Value, Is.False);

            for (int i = 0; i < EditorChangeHandler.MAX_SAVED_STATES * 2; i++)
                handler.SaveState();

            Assert.That(handler.CanUndo.Value, Is.True);

            for (int i = 0; i < EditorChangeHandler.MAX_SAVED_STATES; i++)
            {
                Assert.That(handler.CanUndo.Value, Is.True);
                handler.RestoreState(-1);
            }

            Assert.That(handler.CanUndo.Value, Is.False);
        }
    }
}
