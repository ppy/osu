// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorSeeking : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var beatmap = base.CreateBeatmap(ruleset);

            beatmap.BeatmapInfo.BeatDivisor = 1;

            beatmap.ControlPointInfo.Clear();
            beatmap.ControlPointInfo.Add(0, new TimingControlPoint { BeatLength = 1000 });
            beatmap.ControlPointInfo.Add(2000, new TimingControlPoint { BeatLength = 500 });
            beatmap.ControlPointInfo.Add(20000, new TimingControlPoint { BeatLength = 500 });

            return beatmap;
        }

        [Test]
        public void TestSeekToFirst()
        {
            pressAndCheckTime(Key.Z, 2170);
            pressAndCheckTime(Key.Z, 0);
            pressAndCheckTime(Key.Z, 2170);

            AddAssert("track not running", () => !EditorClock.IsRunning);
        }

        [Test]
        public void TestRestart()
        {
            pressAndCheckTime(Key.V, 227170);

            AddAssert("track not running", () => !EditorClock.IsRunning);

            AddStep("press X", () => InputManager.Key(Key.X));

            AddAssert("track running", () => EditorClock.IsRunning);
            AddAssert("time restarted", () => EditorClock.CurrentTime < 100000);
        }

        [Test]
        public void TestPauseResume()
        {
            AddAssert("track not running", () => !EditorClock.IsRunning);

            AddStep("press C", () => InputManager.Key(Key.C));
            AddAssert("track running", () => EditorClock.IsRunning);

            AddStep("press C", () => InputManager.Key(Key.C));
            AddAssert("track not running", () => !EditorClock.IsRunning);
        }

        [Test]
        public void TestSeekToLast()
        {
            pressAndCheckTime(Key.V, 227170);
            pressAndCheckTime(Key.V, 229170);
            pressAndCheckTime(Key.V, 227170);

            AddAssert("track not running", () => !EditorClock.IsRunning);
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

        [Test]
        public void TestSeekBetweenControlPoints()
        {
            AddStep("seek to 0", () => EditorClock.Seek(0));
            AddAssert("time is 0", () => EditorClock.CurrentTime == 0);

            // already at first control point, noop
            pressAndCheckTime(Key.Up, 0);

            pressAndCheckTime(Key.Down, 2000);

            pressAndCheckTime(Key.Down, 20000);
            // at last control point, noop
            pressAndCheckTime(Key.Down, 20000);

            pressAndCheckTime(Key.Up, 2000);
            pressAndCheckTime(Key.Up, 0);
            pressAndCheckTime(Key.Up, 0);
        }

        private void pressAndCheckTime(Key key, double expectedTime)
        {
            AddStep($"press {key}", () => InputManager.Key(key));
            AddUntilStep($"time is {expectedTime}", () => EditorClock.CurrentTime, () => Is.EqualTo(expectedTime).Within(1));
        }
    }
}
