// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    public partial class TestSceneOpenEditorTimestampInOsu : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        private void addStepClickLink(string timestamp, string step = "", bool displayTimestamp = true)
        {
            AddStep(displayTimestamp ? $"{step} {timestamp}" : step, () => Editor.HandleTimestamp(timestamp));
            AddUntilStep("wait for seek", () => EditorClock.SeekingOrStopped.Value);
        }

        private void addReset()
        {
            addStepClickLink("00:00:000", "reset", false);
        }

        private bool checkSnapAndSelectCombo(double startTime, params int[] comboNumbers)
        {
            bool checkCombos = comboNumbers.Any()
                ? hasCombosInOrder(EditorBeatmap.SelectedHitObjects, comboNumbers)
                : !EditorBeatmap.SelectedHitObjects.Any();

            return EditorClock.CurrentTime == startTime
                   && EditorBeatmap.SelectedHitObjects.Count == comboNumbers.Length
                   && checkCombos;
        }

        private bool hasCombosInOrder(IEnumerable<HitObject> selected, params int[] comboNumbers)
        {
            List<HitObject> hitObjects = selected.ToList();
            if (hitObjects.Count != comboNumbers.Length)
                return false;

            return !hitObjects.Select(x => (OsuHitObject)x)
                              .Where((x, i) => x.IndexInCurrentCombo + 1 != comboNumbers[i])
                              .Any();
        }

        [Test]
        public void TestNormalSelection()
        {
            addStepClickLink("00:02:170 (1,2,3)");
            AddAssert("snap and select 1-2-3", () => checkSnapAndSelectCombo(2_170, 1, 2, 3));

            addReset();
            addStepClickLink("00:04:748 (2,3,4,1,2)");
            AddAssert("snap and select 2-3-4-1-2", () => checkSnapAndSelectCombo(4_748, 2, 3, 4, 1, 2));

            addReset();
            addStepClickLink("00:02:170 (1,1,1)");
            AddAssert("snap and select 1-1-1", () => checkSnapAndSelectCombo(2_170, 1, 1, 1));

            addReset();
            addStepClickLink("00:02:873 (2,2,2,2)");
            AddAssert("snap and select 2-2-2-2", () => checkSnapAndSelectCombo(2_873, 2, 2, 2, 2));
        }

        [Test]
        public void TestUnusualSelection()
        {
            HitObject firstObject = null!;

            addStepClickLink("00:00:000 (0)", "invalid combo");
            AddAssert("snap to next, select none", () =>
            {
                firstObject = EditorBeatmap.HitObjects.First();
                return checkSnapAndSelectCombo(firstObject.StartTime);
            });

            addReset();
            addStepClickLink("00:00:000 (1)", "wrong offset");
            AddAssert("snap and select 1", () => checkSnapAndSelectCombo(firstObject.StartTime, 1));

            addReset();
            addStepClickLink("00:00:956 (2,3,4)", "wrong offset");
            AddAssert("snap to next, select 2-3-4", () => checkSnapAndSelectCombo(firstObject.StartTime, 2, 3, 4));

            addReset();
            addStepClickLink("00:00:956 (956|1,956|2)", "mania link");
            AddAssert("snap to next, select none", () => checkSnapAndSelectCombo(firstObject.StartTime));
        }
    }
}
