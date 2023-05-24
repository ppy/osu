// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Tests.Beatmaps;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSceneObjectBeatSnap : TestSceneOsuEditor
    {
        private OsuPlayfield playfield;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("get playfield", () => playfield = Editor.ChildrenOfType<OsuPlayfield>().First());
        }

        [Test]
        public void TestBeatSnapHitCircle()
        {
            double firstTimingPointTime() => Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.First().Time;

            AddStep("seek some milliseconds forward", () => EditorClock.Seek(firstTimingPointTime() + 10));

            AddStep("move mouse to centre", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));
            AddStep("enter placement mode", () => InputManager.Key(Key.Number2));
            AddStep("place first object", () => InputManager.Click(MouseButton.Left));

            AddAssert("ensure object snapped back to correct time", () => EditorBeatmap.HitObjects.First().StartTime == firstTimingPointTime());
        }
    }
}
