// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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

        private void checkSelection(Func<double> startTime, params int[] comboNumbers)
            => AddUntilStep($"seeked & selected {(comboNumbers.Any() ? string.Join(",", comboNumbers) : "nothing")}", () =>
            {
                bool checkCombos = comboNumbers.Any()
                    ? hasCombosInOrder(EditorBeatmap.SelectedHitObjects, comboNumbers)
                    : !EditorBeatmap.SelectedHitObjects.Any();

                return EditorClock.CurrentTime == startTime()
                       && EditorBeatmap.SelectedHitObjects.Count == comboNumbers.Length
                       && checkCombos;
            });

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
            checkSelection(() => 2_170, 1, 2, 3);

            addReset();
            addStepClickLink("00:04:748 (2,3,4,1,2)");
            checkSelection(() => 4_748, 2, 3, 4, 1, 2);

            addReset();
            addStepClickLink("00:02:170 (1,1,1)");
            checkSelection(() => 2_170, 1, 1, 1);

            addReset();
            addStepClickLink("00:02:873 (2,2,2,2)");
            checkSelection(() => 2_873, 2, 2, 2, 2);
        }

        [Test]
        public void TestUnusualSelection()
        {
            HitObject firstObject = null!;

            AddStep("retrieve first object", () => firstObject = EditorBeatmap.HitObjects.First());

            addStepClickLink("00:00:000 (0)", "invalid combo");
            checkSelection(() => firstObject.StartTime);

            addReset();
            addStepClickLink("00:00:000 (1)", "wrong offset");
            checkSelection(() => firstObject.StartTime, 1);

            addReset();
            addStepClickLink("00:00:956 (2,3,4)", "wrong offset");
            checkSelection(() => firstObject.StartTime, 2, 3, 4);

            addReset();
            addStepClickLink("00:00:956 (956|1,956|2)", "mania link");
            checkSelection(() => firstObject.StartTime);
        }
    }
}
