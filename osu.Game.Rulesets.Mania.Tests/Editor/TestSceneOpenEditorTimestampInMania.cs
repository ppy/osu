// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Mania.Tests.Editor
{
    public partial class TestSceneOpenEditorTimestampInMania : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new ManiaRuleset();

        private void addStepClickLink(string timestamp, string step = "", bool displayTimestamp = true)
        {
            AddStep(displayTimestamp ? $"{step} {timestamp}" : step, () => Editor.HandleTimestamp(timestamp));
            AddUntilStep("wait for seek", () => EditorClock.SeekingOrStopped.Value);
        }

        private void addReset()
        {
            addStepClickLink("00:00:000", "reset", false);
        }

        private bool checkSnapAndSelectColumn(double startTime, IReadOnlyCollection<(int, int)>? columnPairs = null)
        {
            bool checkColumns = columnPairs != null
                ? EditorBeatmap.SelectedHitObjects.All(x => columnPairs.Any(col => isNoteAt(x, col.Item1, col.Item2)))
                : !EditorBeatmap.SelectedHitObjects.Any();

            return EditorClock.CurrentTime == startTime
                   && EditorBeatmap.SelectedHitObjects.Count == (columnPairs?.Count ?? 0)
                   && checkColumns;
        }

        private bool isNoteAt(HitObject hitObject, double time, int column)
        {
            return hitObject is ManiaHitObject maniaHitObject
                   && maniaHitObject.StartTime == time
                   && maniaHitObject.Column == column;
        }

        [Test]
        public void TestNormalSelection()
        {
            addStepClickLink("00:05:920 (5920|3,6623|3,6857|2,7326|1)");
            AddAssert("selected group", () => checkSnapAndSelectColumn(5_920, new List<(int, int)>
                { (5_920, 3), (6_623, 3), (6_857, 2), (7_326, 1) }
            ));

            addReset();
            addStepClickLink("00:42:716 (42716|3,43420|2,44123|0,44357|1,45295|1)");
            AddAssert("selected ungrouped", () => checkSnapAndSelectColumn(42_716, new List<(int, int)>
                { (42_716, 3), (43_420, 2), (44_123, 0), (44_357, 1), (45_295, 1) }
            ));

            addReset();
            AddStep("add notes to row", () =>
            {
                if (EditorBeatmap.HitObjects.Any(x => x is ManiaHitObject m && m.StartTime == 11_545 && m.Column is 1 or 2 or 3))
                    return;

                ManiaHitObject first = (ManiaHitObject)EditorBeatmap.HitObjects.First(x => x is ManiaHitObject m && m.StartTime == 11_545 && m.Column == 0);
                ManiaHitObject second = new Note { Column = 1, StartTime = first.StartTime };
                ManiaHitObject third = new Note { Column = 2, StartTime = first.StartTime };
                ManiaHitObject forth = new Note { Column = 3, StartTime = first.StartTime };
                EditorBeatmap.AddRange(new[] { second, third, forth });
            });
            addStepClickLink("00:11:545 (11545|0,11545|1,11545|2,11545|3)");
            AddAssert("selected in row", () => checkSnapAndSelectColumn(11_545, new List<(int, int)>
                { (11_545, 0), (11_545, 1), (11_545, 2), (11_545, 3) }
            ));

            addReset();
            addStepClickLink("01:36:623 (96623|1,97560|1,97677|1,97795|1,98966|1)");
            AddAssert("selected in column", () => checkSnapAndSelectColumn(96_623, new List<(int, int)>
                { (96_623, 1), (97_560, 1), (97_677, 1), (97_795, 1), (98_966, 1) }
            ));
        }

        [Test]
        public void TestUnusualSelection()
        {
            addStepClickLink("00:00:000 (0|1)", "wrong offset");
            AddAssert("snap to 1, select none", () => checkSnapAndSelectColumn(2_170));

            addReset();
            addStepClickLink("00:00:000 (2)", "std link");
            AddAssert("snap to 1, select none", () => checkSnapAndSelectColumn(2_170));

            addReset();
            addStepClickLink("00:00:000 (1,2)", "std link");
            AddAssert("snap to 1, select none", () => checkSnapAndSelectColumn(2_170));
        }
    }
}
