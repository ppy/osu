// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets;
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
    public class TestSceneHitObjectSamplePointAdjustments : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("add test objects", () =>
            {
                EditorBeatmap.Add(new HitCircle
                {
                    StartTime = 0,
                    Position = (OsuPlayfield.BASE_SIZE - new Vector2(100, 0)) / 2,
                    SampleControlPoint = new SampleControlPoint
                    {
                        SampleBank = "normal",
                        SampleVolume = 80
                    }
                });

                EditorBeatmap.Add(new HitCircle()
                {
                    StartTime = 500,
                    Position = (OsuPlayfield.BASE_SIZE + new Vector2(100, 0)) / 2,
                    SampleControlPoint = new SampleControlPoint
                    {
                        SampleBank = "soft",
                        SampleVolume = 60
                    }
                });
            });
        }

        [Test]
        public void TestSingleSelection()
        {
            clickSamplePiece(0);
            samplePopoverHasSingleBank("normal");
            samplePopoverHasSingleVolume(80);

            dismissPopover();

            // select first object to ensure that sample pieces for unselected objects
            // work independently from selection state.
            AddStep("select first object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.First()));

            clickSamplePiece(1);
            samplePopoverHasSingleBank("soft");
            samplePopoverHasSingleVolume(60);

            setVolumeViaPopover(90);
            hitObjectHasSampleVolume(1, 90);

            setBankViaPopover("drum");
            hitObjectHasSampleBank(1, "drum");
        }

        private void clickSamplePiece(int objectIndex) => AddStep($"click {objectIndex.ToOrdinalWords()} difficulty piece", () =>
        {
            var difficultyPiece = this.ChildrenOfType<SamplePointPiece>().Single(piece => piece.HitObject == EditorBeatmap.HitObjects.ElementAt(objectIndex));

            InputManager.MoveMouseTo(difficultyPiece);
            InputManager.Click(MouseButton.Left);
        });

        private void samplePopoverHasSingleVolume(int volume) => AddUntilStep($"sample popover has volume {volume}", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault();
            var slider = popover?.ChildrenOfType<SliderWithTextBoxInput<int>>().Single();

            return slider?.Current.Value == volume;
        });

        private void samplePopoverHasSingleBank(string bank) => AddUntilStep($"sample popover has bank {bank}", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault();
            var textBox = popover?.ChildrenOfType<LabelledTextBox>().First();

            return textBox?.Current.Value == bank;
        });

        private void dismissPopover()
        {
            AddStep("dismiss popover", () => InputManager.Key(Key.Escape));
            AddUntilStep("wait for dismiss", () => !this.ChildrenOfType<DifficultyPointPiece.DifficultyEditPopover>().Any(popover => popover.IsPresent));
        }

        private void setVolumeViaPopover(int volume) => AddStep($"set volume {volume} via popover", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().Single();
            var slider = popover.ChildrenOfType<SliderWithTextBoxInput<int>>().Single();
            slider.Current.Value = volume;
        });

        private void hitObjectHasSampleVolume(int objectIndex, int volume) => AddAssert($"{objectIndex.ToOrdinalWords()} has volume {volume}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h.SampleControlPoint.SampleVolume == volume;
        });

        private void setBankViaPopover(string bank) => AddStep($"set bank {bank} via popover", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().Single();
            var textBox = popover.ChildrenOfType<LabelledTextBox>().First();
            textBox.Current.Value = bank;
        });

        private void hitObjectHasSampleBank(int objectIndex, string bank) => AddAssert($"{objectIndex.ToOrdinalWords()} has bank {bank}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h.SampleControlPoint.SampleBank == bank;
        });
    }
}
