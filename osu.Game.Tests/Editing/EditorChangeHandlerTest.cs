// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class EditorChangeHandlerTest
    {
        private int stateChangedFired;

        [SetUp]
        public void SetUp()
        {
            stateChangedFired = 0;
        }

        [Test]
        public void TestSaveRestoreState()
        {
            var (handler, beatmap) = createChangeHandler();

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.False);

            addArbitraryChange(beatmap);
            handler.SaveState();

            Assert.That(stateChangedFired, Is.EqualTo(1));

            Assert.That(handler.CanUndo.Value, Is.True);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.RestoreState(-1);

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.True);

            Assert.That(stateChangedFired, Is.EqualTo(2));
        }

        [Test]
        public void TestSaveSameStateDoesNotSave()
        {
            var (handler, beatmap) = createChangeHandler();

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.False);

            addArbitraryChange(beatmap);
            handler.SaveState();

            Assert.That(handler.CanUndo.Value, Is.True);
            Assert.That(handler.CanRedo.Value, Is.False);
            Assert.That(stateChangedFired, Is.EqualTo(1));

            string hash = handler.CurrentStateHash;

            // save a save without making any changes
            handler.SaveState();

            Assert.That(hash, Is.EqualTo(handler.CurrentStateHash));
            Assert.That(stateChangedFired, Is.EqualTo(1));

            handler.RestoreState(-1);

            Assert.That(hash, Is.Not.EqualTo(handler.CurrentStateHash));

            // we should only be able to restore once even though we saved twice.
            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.True);
            Assert.That(stateChangedFired, Is.EqualTo(2));
        }

        [Test]
        public void TestMaxStatesSaved()
        {
            var (handler, beatmap) = createChangeHandler();

            Assert.That(handler.CanUndo.Value, Is.False);

            for (int i = 0; i < EditorChangeHandler.MAX_SAVED_STATES; i++)
            {
                Assert.That(stateChangedFired, Is.EqualTo(i));

                addArbitraryChange(beatmap);
                handler.SaveState();
            }

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
            var (handler, beatmap) = createChangeHandler();

            Assert.That(handler.CanUndo.Value, Is.False);

            for (int i = 0; i < EditorChangeHandler.MAX_SAVED_STATES * 2; i++)
            {
                addArbitraryChange(beatmap);
                handler.SaveState();
            }

            Assert.That(handler.CanUndo.Value, Is.True);

            for (int i = 0; i < EditorChangeHandler.MAX_SAVED_STATES; i++)
            {
                Assert.That(handler.CanUndo.Value, Is.True);
                handler.RestoreState(-1);
            }

            Assert.That(handler.CanUndo.Value, Is.False);
        }

        private (EditorChangeHandler, EditorBeatmap) createChangeHandler()
        {
            var beatmap = new EditorBeatmap(new Beatmap());

            var changeHandler = new EditorChangeHandler(beatmap);

            changeHandler.OnStateChange += () => stateChangedFired++;
            return (changeHandler, beatmap);
        }

        private void addArbitraryChange(EditorBeatmap beatmap)
        {
            beatmap.Add(new HitCircle { StartTime = RNG.Next(0, 100000) });
        }
    }
}
