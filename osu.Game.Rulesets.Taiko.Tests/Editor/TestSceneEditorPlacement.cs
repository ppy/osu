// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Testing;
using osu.Game.Configuration;
using osu.Game.Rulesets.Taiko.Edit;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.Taiko.Objects.Drawables;
using osu.Game.Rulesets.Taiko.UI;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    public partial class TestSceneEditorPlacement : EditorTestScene
    {
        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        protected override Ruleset CreateEditorRuleset() => new TaikoRuleset();

        [Test]
        public void TestPlacementBlueprintDoesNotCauseCrashes()
        {
            AddStep("clear objects", () => EditorBeatmap.Clear());
            AddStep("add two objects", () =>
            {
                EditorBeatmap.Add(new Hit { StartTime = 1818 });
                EditorBeatmap.Add(new Hit { StartTime = 1584 });
            });
            AddStep("seek back", () => EditorClock.Seek(1584));
            AddStep("choose hit placement tool", () => InputManager.Key(Key.Number2));
            AddStep("hover over first hit", () => InputManager.MoveMouseTo(Editor.ChildrenOfType<DrawableHit>().ElementAt(1)));
            AddStep("hover over second hit", () => InputManager.MoveMouseTo(Editor.ChildrenOfType<DrawableHit>().ElementAt(0)));
            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("second hit deleted", () => Editor.ChildrenOfType<DrawableHit>().Count(), () => Is.EqualTo(1));
        }

        [Test]
        public void TestLimitedDistanceSnapUsesClockTime()
        {
            TaikoPlayfield playfield = null!;
            double placementTime = 0;

            AddStep("enable limited distance snap", () => config.SetValue(OsuSetting.EditorLimitedDistanceSnap, true));
            AddStep("disable auto seek", () => config.SetValue(OsuSetting.EditorAutoSeekOnPlacement, false));
            AddStep("clear objects", () => EditorBeatmap.Clear());
            AddStep("seek to first timing point", () =>
            {
                placementTime = EditorBeatmap.ControlPointInfo.TimingPoints.First().Time;
                EditorClock.Seek(placementTime);
            });
            AddStep("get playfield", () => playfield = (TaikoPlayfield)Editor.ChildrenOfType<TaikoHitObjectComposer>().Single().Playfield);

            AddStep("choose hit placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move to playfield", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));
            AddStep("place hit", () => InputManager.Click(MouseButton.Left));
            AddAssert("hit placed at clock time", () => EditorBeatmap.HitObjects.OfType<Hit>().Single().StartTime, () => Is.EqualTo(placementTime));

            AddStep("seek forward", () =>
            {
                placementTime += 1000;
                EditorClock.Seek(placementTime);
            });
            AddStep("choose drum roll tool", () => InputManager.Key(Key.Number3));
            AddStep("move to playfield", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));
            AddStep("start drum roll", () => InputManager.PressButton(MouseButton.Left));
            AddStep("drag to set duration", () => InputManager.MoveMouseTo(playfield.ScreenSpacePositionAtTime(placementTime + 500)));
            AddStep("end drum roll", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("drum roll placed", () => EditorBeatmap.HitObjects.OfType<DrumRoll>().Single().Duration, () => Is.GreaterThan(0));

            AddStep("seek forward", () =>
            {
                placementTime += 1000;
                EditorClock.Seek(placementTime);
            });
            AddStep("choose swell tool", () => InputManager.Key(Key.Number4));
            AddStep("move to playfield", () => InputManager.MoveMouseTo(playfield.ScreenSpaceDrawQuad.Centre));
            AddStep("start swell", () => InputManager.PressButton(MouseButton.Left));
            AddStep("drag to set duration", () => InputManager.MoveMouseTo(playfield.ScreenSpacePositionAtTime(placementTime + 500)));
            AddStep("end swell", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("swell placed", () => EditorBeatmap.HitObjects.OfType<Swell>().Single().Duration, () => Is.GreaterThan(0));
        }
    }
}
