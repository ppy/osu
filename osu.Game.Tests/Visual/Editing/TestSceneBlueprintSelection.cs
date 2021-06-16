// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneBlueprintSelection : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        private EditorBlueprintContainer blueprintContainer
            => Editor.ChildrenOfType<EditorBlueprintContainer>().First();

        [Test]
        public void TestSelectedObjectHasPriorityWhenOverlapping()
        {
            var firstSlider = new Slider
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(new Vector2()),
                    new PathControlPoint(new Vector2(150, -50)),
                    new PathControlPoint(new Vector2(300, 0))
                }),
                Position = new Vector2(0, 100)
            };
            var secondSlider = new Slider
            {
                Path = new SliderPath(new[]
                {
                    new PathControlPoint(new Vector2()),
                    new PathControlPoint(new Vector2(-50, 50)),
                    new PathControlPoint(new Vector2(-100, 100))
                }),
                Position = new Vector2(200, 0)
            };

            AddStep("add overlapping sliders", () =>
            {
                EditorBeatmap.Add(firstSlider);
                EditorBeatmap.Add(secondSlider);
            });
            AddStep("select first slider", () => EditorBeatmap.SelectedHitObjects.Add(firstSlider));

            AddStep("move mouse to common point", () =>
            {
                var pos = blueprintContainer.ChildrenOfType<PathControlPointPiece>().ElementAt(1).ScreenSpaceDrawQuad.Centre;
                InputManager.MoveMouseTo(pos);
            });
            AddStep("right click", () => InputManager.Click(MouseButton.Right));

            AddAssert("selection is unchanged", () => EditorBeatmap.SelectedHitObjects.Single() == firstSlider);
        }
    }
}
