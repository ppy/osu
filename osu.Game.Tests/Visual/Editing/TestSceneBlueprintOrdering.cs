// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Edit.Blueprints.HitCircles;
using osu.Game.Rulesets.Osu.Edit.Blueprints.Sliders.Components;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneBlueprintOrdering : EditorTestScene
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
                var pos = blueprintContainer.ChildrenOfType<PathControlPointPiece<Slider>>().ElementAt(1).ScreenSpaceDrawQuad.Centre;
                InputManager.MoveMouseTo(pos);
            });
            AddStep("right click", () => InputManager.Click(MouseButton.Right));

            AddAssert("selection is unchanged", () => EditorBeatmap.SelectedHitObjects.Single() == firstSlider);
        }

        [Test]
        public void TestOverlappingObjectsWithSameStartTime()
        {
            AddStep("add overlapping circles", () =>
            {
                EditorBeatmap.Add(createHitCircle(50, OsuPlayfield.BASE_SIZE / 2));
                EditorBeatmap.Add(createHitCircle(50, OsuPlayfield.BASE_SIZE / 2 + new Vector2(-10, -20)));
                EditorBeatmap.Add(createHitCircle(50, OsuPlayfield.BASE_SIZE / 2 + new Vector2(10, -20)));
            });

            AddStep("click at centre of playfield", () =>
            {
                var hitObjectContainer = Editor.ChildrenOfType<HitObjectContainer>().Single();
                var centre = hitObjectContainer.ToScreenSpace(OsuPlayfield.BASE_SIZE / 2);
                InputManager.MoveMouseTo(centre);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("frontmost object selected", () =>
            {
                var hasCombo = Editor.ChildrenOfType<HitCircleSelectionBlueprint>().Single(b => b.IsSelected).Item as IHasComboInformation;
                return hasCombo?.IndexInCurrentCombo == 0;
            });
        }

        [Test]
        public void TestPlacementOfConcurrentObjectWithDuration()
        {
            AddStep("seek to timing point", () => EditorClock.Seek(2170));
            AddStep("add hit circle", () => EditorBeatmap.Add(createHitCircle(2170, Vector2.Zero)));

            AddStep("choose spinner placement tool", () =>
            {
                InputManager.Key(Key.Number4);
                var hitObjectContainer = Editor.ChildrenOfType<HitObjectContainer>().Single();
                InputManager.MoveMouseTo(hitObjectContainer.ScreenSpaceDrawQuad.Centre);
            });

            AddStep("begin placing spinner", () =>
            {
                InputManager.Click(MouseButton.Left);
            });
            AddStep("end placing spinner", () =>
            {
                EditorClock.Seek(2500);
                InputManager.Click(MouseButton.Right);
            });

            AddAssert("two timeline blueprints present", () => Editor.ChildrenOfType<TimelineHitObjectBlueprint>().Count() == 2);
        }

        private HitCircle createHitCircle(double startTime, Vector2 position) => new HitCircle
        {
            StartTime = startTime,
            Position = position,
        };
    }
}
