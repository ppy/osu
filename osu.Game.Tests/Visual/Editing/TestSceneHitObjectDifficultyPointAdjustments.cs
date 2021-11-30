// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneHitObjectDifficultyPointAdjustments : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add test objects", () =>
            {
                EditorBeatmap.Add(new Slider
                {
                    StartTime = 0,
                    Position = (OsuPlayfield.BASE_SIZE - new Vector2(0, 100)) / 2,
                    Path = new SliderPath
                    {
                        ControlPoints =
                        {
                            new PathControlPoint(new Vector2(0, 0)),
                            new PathControlPoint(new Vector2(0, 100))
                        }
                    }
                });

                EditorBeatmap.Add(new Slider
                {
                    StartTime = 500,
                    Position = (OsuPlayfield.BASE_SIZE - new Vector2(100, 0)) / 2,
                    Path = new SliderPath
                    {
                        ControlPoints =
                        {
                            new PathControlPoint(new Vector2(0, 0)),
                            new PathControlPoint(new Vector2(100, 0))
                        }
                    },
                    DifficultyControlPoint = new DifficultyControlPoint
                    {
                        SliderVelocity = 2
                    }
                });
            });
        }

        [Test]
        public void TestSingleSelection()
        {
            clickDifficultyPiece(0);
            velocityPopoverHasSingleValue(1);

            dismissPopover();

            // select first object to ensure that difficulty pieces for unselected objects
            // work independently from selection state.
            AddStep("select first object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.First()));

            clickDifficultyPiece(1);
            velocityPopoverHasSingleValue(2);

            setVelocityViaPopover(5);
            hitObjectHasVelocity(1, 5);
        }

        [Test]
        public void TestMultipleSelectionWithSameSliderVelocity()
        {
            AddStep("unify slider velocity", () =>
            {
                foreach (var h in EditorBeatmap.HitObjects)
                    h.DifficultyControlPoint.SliderVelocity = 1.5;
            });

            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            clickDifficultyPiece(0);
            velocityPopoverHasSingleValue(1.5);

            dismissPopover();

            clickDifficultyPiece(1);
            velocityPopoverHasSingleValue(1.5);

            setVelocityViaPopover(5);
            hitObjectHasVelocity(0, 5);
            hitObjectHasVelocity(1, 5);
        }

        [Test]
        public void TestMultipleSelectionWithDifferentSliderVelocity()
        {
            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            clickDifficultyPiece(0);
            velocityPopoverHasIndeterminateValue();

            dismissPopover();

            clickDifficultyPiece(1);
            velocityPopoverHasIndeterminateValue();

            setVelocityViaPopover(3);
            hitObjectHasVelocity(0, 3);
            hitObjectHasVelocity(1, 3);
        }

        private void clickDifficultyPiece(int objectIndex) => AddStep($"click {objectIndex.ToOrdinalWords()} difficulty piece", () =>
        {
            var difficultyPiece = this.ChildrenOfType<DifficultyPointPiece>().Single(piece => piece.HitObject == EditorBeatmap.HitObjects.ElementAt(objectIndex));

            InputManager.MoveMouseTo(difficultyPiece);
            InputManager.Click(MouseButton.Left);
        });

        private void velocityPopoverHasSingleValue(double velocity) => AddUntilStep($"velocity popover has {velocity}", () =>
        {
            var popover = this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().SingleOrDefault();
            var slider = popover?.ChildrenOfType<IndeterminateSliderWithTextBoxInput<double>>().Single();

            return slider?.Current.Value == velocity;
        });

        private void velocityPopoverHasIndeterminateValue() => AddUntilStep("velocity popover has indeterminate value", () =>
        {
            var popover = this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().SingleOrDefault();
            var slider = popover?.ChildrenOfType<IndeterminateSliderWithTextBoxInput<double>>().Single();

            return slider != null && slider.Current.Value == null;
        });

        private void dismissPopover()
        {
            AddStep("dismiss popover", () => InputManager.Key(Key.Escape));
            AddUntilStep("wait for dismiss", () => !this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().Any(popover => popover.IsPresent));
        }

        private void setVelocityViaPopover(double velocity) => AddStep($"set {velocity} via popover", () =>
        {
            var popover = this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().Single();
            var slider = popover.ChildrenOfType<IndeterminateSliderWithTextBoxInput<double>>().Single();
            slider.Current.Value = velocity;
        });

        private void hitObjectHasVelocity(int objectIndex, double velocity) => AddAssert($"{objectIndex.ToOrdinalWords()} has velocity {velocity}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h.DifficultyControlPoint.SliderVelocity == velocity;
        });
    }
}
