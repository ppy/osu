// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Osu.Tests.Editor
{
    [TestFixture]
    public partial class TestSliderScaling : TestSceneOsuEditor
    {
        private OsuPlayfield playfield;

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(Ruleset.Value, false);

        public override void SetUpSteps()
        {
            base.SetUpSteps();
            AddStep("get playfield", () => playfield = Editor.ChildrenOfType<OsuPlayfield>().First());
            AddStep("seek to first timing point", () => EditorClock.Seek(Beatmap.Value.Beatmap.ControlPointInfo.TimingPoints.First().Time));
        }

        [Test]
        public void TestScalingLinearSlider()
        {
            Slider slider = null;

            AddStep("Add slider", () =>
            {
                slider = new Slider { StartTime = EditorClock.CurrentTime, Position = new Vector2(300) };

                PathControlPoint[] points =
                {
                    new PathControlPoint(new Vector2(0), PathType.LINEAR),
                    new PathControlPoint(new Vector2(100, 0)),
                };

                slider.Path = new SliderPath(points);
                EditorBeatmap.Add(slider);
            });

            AddAssert("ensure object placed", () => EditorBeatmap.HitObjects.Count == 1);

            moveMouse(new Vector2(300));
            AddStep("select slider", () => InputManager.Click(MouseButton.Left));

            double distanceBefore = 0;

            AddStep("store distance", () => distanceBefore = slider.Path.Distance);

            AddStep("move mouse to handle", () => InputManager.MoveMouseTo(Editor.ChildrenOfType<SelectionBoxDragHandle>().Skip(1).First()));
            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));
            moveMouse(new Vector2(300, 300));
            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("slider length shrunk", () => slider.Path.Distance < distanceBefore);
        }

        private void moveMouse(Vector2 pos) =>
            AddStep($"move mouse to {pos}", () => InputManager.MoveMouseTo(playfield.ToScreenSpace(pos)));
    }
}
