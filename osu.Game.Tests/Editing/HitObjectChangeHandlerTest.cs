// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Beatmaps;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Changes;

namespace osu.Game.Tests.Editing
{
    [TestFixture]
    public class HitObjectChangeHandlerTest
    {
        private int stateChangedFired;

        [SetUp]
        public void SetUp()
        {
            stateChangedFired = 0;
        }

        [Test]
        public void TestSaveRestoreStateUsingTransaction()
        {
            var (handler, beatmap) = createChangeHandler();

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.BeginChange();

            Assert.That(stateChangedFired, Is.EqualTo(0));

            addArbitraryChange(handler, beatmap);
            handler.EndChange();

            Assert.That(stateChangedFired, Is.EqualTo(1));

            Assert.That(handler.CanUndo.Value, Is.True);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.Undo();

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.True);

            Assert.That(stateChangedFired, Is.EqualTo(2));
        }

        [Test]
        public void TestSaveRestoreState()
        {
            var (handler, beatmap) = createChangeHandler();

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.SaveState();
            Assert.That(stateChangedFired, Is.EqualTo(0));

            addArbitraryChange(handler, beatmap);
            handler.SaveState();

            Assert.That(stateChangedFired, Is.EqualTo(1));

            Assert.That(handler.CanUndo.Value, Is.True);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.Undo();

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.True);

            Assert.That(stateChangedFired, Is.EqualTo(2));
        }

        [Test]
        public void TestApplyThenUndoThenApplySameChange()
        {
            var (handler, beatmap) = createChangeHandler();

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.SaveState();
            Assert.That(stateChangedFired, Is.EqualTo(0));

            var originalState = handler.CurrentState;

            addArbitraryChange(handler, beatmap);
            handler.SaveState();

            Assert.That(handler.CanUndo.Value, Is.True);
            Assert.That(handler.CanRedo.Value, Is.False);
            Assert.That(stateChangedFired, Is.EqualTo(1));

            var state = handler.CurrentState;

            // undo a change without saving
            handler.Undo();

            Assert.That(originalState, Is.EqualTo(handler.CurrentState));
            Assert.That(stateChangedFired, Is.EqualTo(2));

            addArbitraryChange(handler, beatmap);
            handler.SaveState();
            // The change handler does not know that it is the same change applied again, so it will assume a new state.
            Assert.That(state, Is.Not.EqualTo(handler.CurrentState));
        }

        [Test]
        public void TestSaveSameStateDoesNotSave()
        {
            var (handler, beatmap) = createChangeHandler();

            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.False);

            handler.SaveState();
            Assert.That(stateChangedFired, Is.EqualTo(0));

            addArbitraryChange(handler, beatmap);
            handler.SaveState();

            Assert.That(handler.CanUndo.Value, Is.True);
            Assert.That(handler.CanRedo.Value, Is.False);
            Assert.That(stateChangedFired, Is.EqualTo(1));

            var state = handler.CurrentState;

            // save a save without making any changes
            handler.SaveState();

            Assert.That(state, Is.EqualTo(handler.CurrentState));
            Assert.That(stateChangedFired, Is.EqualTo(1));

            handler.Undo();

            Assert.That(state, Is.Not.EqualTo(handler.CurrentState));

            // we should only be able to restore once even though we saved twice.
            Assert.That(handler.CanUndo.Value, Is.False);
            Assert.That(handler.CanRedo.Value, Is.True);
            Assert.That(stateChangedFired, Is.EqualTo(2));
        }

        private (HitObjectChangeHandler, EditorBeatmap) createChangeHandler()
        {
            var beatmap = new EditorBeatmap(new OsuBeatmap
            {
                BeatmapInfo =
                {
                    Ruleset = new OsuRuleset().RulesetInfo,
                },
            });

            var changeHandler = new HitObjectChangeHandler(beatmap);

            beatmap.AddChangeHandler(changeHandler);

            changeHandler.OnStateChange += () => stateChangedFired++;
            return (changeHandler, beatmap);
        }

        private void addArbitraryChange(HitObjectChangeHandler changeHandler, EditorBeatmap beatmap)
        {
            new AddHitObjectChange(beatmap, new HitCircle { StartTime = 2760 }).Apply(changeHandler, true);
        }
    }
}
