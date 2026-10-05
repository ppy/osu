// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Tests.Beatmaps;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorClipboardSnapping : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        private const double beat_length = 60_000 / 180.0; // 180 bpm
        private const double timing_point_time = 1500;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset)
        {
            var controlPointInfo = new ControlPointInfo();
            controlPointInfo.Add(timing_point_time, new TimingControlPoint { BeatLength = beat_length });
            return new TestBeatmap(ruleset, false)
            {
                ControlPointInfo = controlPointInfo
            };
        }

        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(4)]
        [TestCase(6)]
        [TestCase(8)]
        [TestCase(12)]
        [TestCase(16)]
        public void TestPasteSnapping(int divisor)
        {
            const double paste_time = timing_point_time + 1271; // arbitrary timestamp that doesn't snap to the timing point at any divisor

            var addedObjects = new HitObject[]
            {
                new HitCircle { StartTime = 1000 },
                new HitCircle { StartTime = 1200 },
            };

            AddStep("add hitobjects", () => EditorBeatmap.AddRange(addedObjects));
            AddStep("select added objects", () => EditorBeatmap.SelectedHitObjects.AddRange(addedObjects));
            AddStep("copy hitobjects", () => Editor.Copy());

            AddStep($"set beat divisor to 1/{divisor}", () =>
            {
                var beatDivisor = (BindableBeatDivisor)Editor.Dependencies.Get(typeof(BindableBeatDivisor));
                beatDivisor.SetArbitraryDivisor(divisor);
            });

            AddStep("move forward in time", () => EditorClock.Seek(paste_time));
            AddAssert("not at snapped time", () => EditorClock.CurrentTime != EditorBeatmap.SnapTime(EditorClock.CurrentTime, null));

            AddStep("paste hitobjects", () => Editor.Paste());

            AddAssert("first object is snapped", () => Precision.AlmostEquals(
                EditorBeatmap.SelectedHitObjects.MinBy(h => h.StartTime)!.StartTime,
                EditorBeatmap.ControlPointInfo.GetClosestSnappedTime(paste_time, divisor)
            ));

            AddAssert("duration between pasted objects is same", () =>
            {
                var firstObject = EditorBeatmap.SelectedHitObjects.MinBy(h => h.StartTime)!;
                var secondObject = EditorBeatmap.SelectedHitObjects.MaxBy(h => h.StartTime)!;

                return Precision.AlmostEquals(secondObject.StartTime - firstObject.StartTime, addedObjects[1].StartTime - addedObjects[0].StartTime);
            });
        }
    }
}
