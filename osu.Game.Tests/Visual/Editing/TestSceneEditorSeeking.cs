// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorSeeking : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = base.CreateBeatmap(ruleset);

            beatmap.BeatmapInfo.BeatDivisor = 1;

            beatmap.ControlPointInfo.Clear();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 });
            beatmap.ControlPointInfo.Add(2000, new TimingControlPoint { BeatLength = 500 });

            return beatmap;
        }

        [Test]
        public void TestSnappedSeeking()
        {
            AddStep("seek to 0", () => EditorClock.Seek(0));
            AddAssert("time is 0", () => EditorClock.CurrentTime == 0);

            pressAndCheckTime(Key.Right, 1000);
            pressAndCheckTime(Key.Right, 2000);
            pressAndCheckTime(Key.Right, 2500);
            pressAndCheckTime(Key.Right, 3000);

            pressAndCheckTime(Key.Left, 2500);
            pressAndCheckTime(Key.Left, 2000);
            pressAndCheckTime(Key.Left, 1000);
        }

        [Test]
        public void TestSnappedSeekingAfterControlPointChange()
        {
            AddStep("seek to 0", () => EditorClock.Seek(0));
            AddAssert("time is 0", () => EditorClock.CurrentTime == 0);

            pressAndCheckTime(Key.Right, 1000);
            pressAndCheckTime(Key.Right, 2000);
            pressAndCheckTime(Key.Right, 2500);
            pressAndCheckTime(Key.Right, 3000);

            AddStep("remove 2nd timing point", () =>
            {
                EditorBeatmap.BeginChange();
                var group = EditorBeatmap.ControlPointInfo.GroupAt(2000);
                EditorBeatmap.ControlPointInfo.RemoveGroup(group);
                EditorBeatmap.EndChange();
            });

            pressAndCheckTime(Key.Left, 2000);
            pressAndCheckTime(Key.Left, 1000);

            pressAndCheckTime(Key.Right, 2000);
            pressAndCheckTime(Key.Right, 3000);
        }

        private void pressAndCheckTime(Key key, double expectedTime)
        {
            AddStep($"press {key}", () => InputManager.Key(key));
            AddUntilStep($"time is {expectedTime}", () => Precision.AlmostEquals(expectedTime, EditorClock.CurrentTime, 1));
        }
    }
}
