// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
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
    public partial class TestSceneHitObjectDifficultyPointAdjustments : EditorTestScene
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
                    SliderVelocityMultiplier = 2
                });
            });
        }

        [Test]
        public void TestPopoverHasFocus()
        {
            clickDifficultyPiece(0);
            velocityPopoverHasFocus();
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
        public void TestUndo()
        {
            clickDifficultyPiece(1);
            velocityPopoverHasSingleValue(2);

            setVelocityViaPopover(5);
            hitObjectHasVelocity(1, 5);
            dismissPopover();

            AddStep("undo", () => Editor.Undo());
            hitObjectHasVelocity(1, 2);
        }

        [Test]
        public void TestMultipleSelectionWithSameSliderVelocity()
        {
            AddStep("unify slider velocity", () =>
            {
                foreach (var h in EditorBeatmap.HitObjects.OfType<IHasSliderVelocity>())
                    h.SliderVelocityMultiplier = 1.5;
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

        [Test]
        public void TestPresetInteractions()
        {
            clickDifficultyPiece(0);
            AddAssert("three presets displayed",
                () => getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Select(b => b.Velocity),
                () => Is.EquivalentTo([0.75d, 1d, 1.5d]));
            AddAssert("one preset selected",
                () => getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Count(b => b.Current.Value == TernaryState.True),
                () => Is.EqualTo(1));
            AddAssert("selected preset is 1.0x",
                () => getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Single(b => b.Current.Value == TernaryState.True).Velocity,
                () => Is.EqualTo(1));

            AddStep("press first preset", () =>
            {
                InputManager.MoveMouseTo(getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().First());
                InputManager.Click(MouseButton.Left);
            });
            hitObjectHasVelocity(0, 0.75);

            dismissPopover();

            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            clickDifficultyPiece(0);
            AddAssert("three presets displayed",
                () => getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Select(b => b.Velocity),
                () => Is.EquivalentTo([0.75d, 1d, 1.5d]));
            AddAssert("no preset fully selected",
                () => getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Count(b => b.Current.Value == TernaryState.True),
                () => Is.EqualTo(0));
            AddAssert("one preset indeterminate",
                () => getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Count(b => b.Current.Value == TernaryState.Indeterminate),
                () => Is.EqualTo(1));

            AddStep("remove second preset", () =>
            {
                InputManager.MoveMouseTo(getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().ElementAt(1));
                InputManager.Click(MouseButton.Middle);
            });
            AddAssert("two presets displayed",
                () => getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Select(b => b.Velocity),
                () => Is.EquivalentTo([0.75d, 1.5d]));
            hitObjectHasVelocity(0, 0.75);
            hitObjectHasVelocity(1, 2);

            AddStep("press last preset", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Last());
                InputManager.Click(MouseButton.Left);
            });
            hitObjectHasVelocity(0, 1.5);
            hitObjectHasVelocity(1, 1.5);

            setVelocityViaPopover(2);
            hitObjectHasVelocity(0, 2);
            hitObjectHasVelocity(1, 2);

            AddStep("add preset", () =>
            {
                var popover = getPopover().ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().SingleOrDefault();
                InputManager.MoveMouseTo(popover.ChildrenOfType<RoundedButton>().First());
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("three presets displayed",
                () => getPopover().ChildrenOfType<SliderVelocityAdjustmentControl.SliderVelocityPresetTernaryButton>().Select(b => b.Velocity),
                () => Is.EquivalentTo([0.75d, 1.5d, 2d]));

            DifficultyPointPiece.DifficultyEditPopover getPopover() => this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().Single();
        }

        private void clickDifficultyPiece(int objectIndex) => AddStep($"click {objectIndex.ToOrdinalWords()} difficulty piece", () =>
        {
            var difficultyPiece = this.ChildrenOfType<DifficultyPointPiece>().Single(piece => piece.HitObject == EditorBeatmap.HitObjects.ElementAt(objectIndex));

            InputManager.MoveMouseTo(difficultyPiece);
            InputManager.Click(MouseButton.Left);
        });

        private void velocityPopoverHasFocus() => AddUntilStep("velocity popover textbox focused", () =>
        {
            var popover = this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().SingleOrDefault();
            var slider = popover?.ChildrenOfType<SliderVelocityAdjustmentControl>().Single();
            var textbox = slider?.ChildrenOfType<OsuTextBox>().Single();

            return textbox?.HasFocus == true;
        });

        private void velocityPopoverHasSingleValue(double velocity) => AddUntilStep($"velocity popover has {velocity}", () =>
        {
            var popover = this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().SingleOrDefault();
            var control = popover?.ChildrenOfType<SliderVelocityAdjustmentControl>().Single();

            return control?.Current.Value == velocity && !control.IsMultipleValues;
        });

        private void velocityPopoverHasIndeterminateValue() => AddUntilStep("velocity popover has indeterminate value", () =>
        {
            var popover = this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().SingleOrDefault();
            var control = popover?.ChildrenOfType<SliderVelocityAdjustmentControl>().Single();

            return control != null && control.IsMultipleValues;
        });

        private void dismissPopover()
        {
            AddStep("unfocus textbox", () => InputManager.Key(Key.Escape));
            AddStep("dismiss popover", () => InputManager.Key(Key.Escape));
            AddUntilStep("wait for dismiss", () => !this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().Any(popover => popover.IsPresent));
        }

        private void setVelocityViaPopover(double velocity) => AddStep($"set {velocity} via popover", () =>
        {
            var popover = this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().Single();
            var slider = popover.ChildrenOfType<SliderVelocityAdjustmentControl>().Single();
            slider.Current.Value = velocity;
        });

        private void hitObjectHasVelocity(int objectIndex, double velocity) => AddAssert($"{objectIndex.ToOrdinalWords()} has velocity {velocity}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h is IHasSliderVelocity hasSliderVelocity && hasSliderVelocity.SliderVelocityMultiplier == velocity;
        });
    }
}
