// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Collections.Generic;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
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
    public partial class TestSceneHitObjectSampleAdjustments : EditorTestScene
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
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, volume: 80)
                    }
                });

                EditorBeatmap.Add(new HitCircle
                {
                    StartTime = 500,
                    Position = (OsuPlayfield.BASE_SIZE + new Vector2(100, 0)) / 2,
                    Samples = new List<HitSampleInfo>
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT, volume: 60)
                    }
                });
            });
        }

        [Test]
        public void TestAddSampleAddition()
        {
            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            AddStep("add clap addition", () => InputManager.Key(Key.R));

            hitObjectHasSampleBank(0, "normal");
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_SOFT);
            hitObjectHasSamples(1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP);

            AddStep("remove clap addition", () => InputManager.Key(Key.R));

            hitObjectHasSampleBank(0, "normal");
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_SOFT);
            hitObjectHasSamples(1, HitSampleInfo.HIT_NORMAL);
        }

        [Test]
        public void TestPopoverHasFocus()
        {
            clickSamplePiece(0);
            samplePopoverHasFocus();
        }

        [Test]
        public void TestSingleSelection()
        {
            clickSamplePiece(0);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_NORMAL);
            samplePopoverHasSingleVolume(80);

            dismissPopover();

            // select first object to ensure that sample pieces for unselected objects
            // work independently from selection state.
            AddStep("select first object", () => EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects.First()));

            clickSamplePiece(1);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_SOFT);
            samplePopoverHasSingleVolume(60);

            setVolumeViaPopover(90);
            hitObjectHasSampleVolume(1, 90);

            setBankViaPopover(HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_DRUM);
        }

        [Test]
        public void TestUndo()
        {
            clickSamplePiece(1);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_SOFT);
            samplePopoverHasSingleVolume(60);

            setVolumeViaPopover(90);
            hitObjectHasSampleVolume(1, 90);
            dismissPopover();

            AddStep("undo", () => Editor.Undo());
            hitObjectHasSampleVolume(1, 60);
        }

        [Test]
        public void TestMultipleSelectionWithSameSampleVolume()
        {
            AddStep("unify sample volume", () =>
            {
                foreach (var h in EditorBeatmap.HitObjects)
                {
                    for (int i = 0; i < h.Samples.Count; i++)
                    {
                        h.Samples[i] = h.Samples[i].With(newVolume: 50);
                    }
                }
            });

            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            clickSamplePiece(0);
            samplePopoverHasSingleVolume(50);

            dismissPopover();

            clickSamplePiece(1);
            samplePopoverHasSingleVolume(50);

            setVolumeViaPopover(75);
            hitObjectHasSampleVolume(0, 75);
            hitObjectHasSampleVolume(1, 75);
        }

        [Test]
        public void TestMultipleSelectionWithDifferentSampleVolume()
        {
            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            clickSamplePiece(0);
            samplePopoverHasIndeterminateVolume();

            dismissPopover();

            clickSamplePiece(1);
            samplePopoverHasIndeterminateVolume();

            setVolumeViaPopover(30);
            hitObjectHasSampleVolume(0, 30);
            hitObjectHasSampleVolume(1, 30);
        }

        [Test]
        public void TestPopoverMultipleSelectionWithSameSampleBank()
        {
            AddStep("unify sample bank", () =>
            {
                foreach (var h in EditorBeatmap.HitObjects)
                {
                    for (int i = 0; i < h.Samples.Count; i++)
                    {
                        h.Samples[i] = h.Samples[i].With(newBank: HitSampleInfo.BANK_SOFT);
                    }
                }
            });

            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            clickSamplePiece(0);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_SOFT);

            dismissPopover();

            clickSamplePiece(1);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_SOFT);

            setBankViaPopover(string.Empty);
            hitObjectHasSampleBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_SOFT);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_SOFT);

            setBankViaPopover(HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_DRUM);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_DRUM);
        }

        [Test]
        public void TestPopoverMultipleSelectionWithDifferentSampleBank()
        {
            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));
            clickSamplePiece(0);
            samplePopoverHasIndeterminateBank();

            dismissPopover();

            clickSamplePiece(1);
            samplePopoverHasIndeterminateBank();

            setBankViaPopover(string.Empty);
            hitObjectHasSampleBank(0, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_SOFT);
            samplePopoverHasIndeterminateBank();

            setBankViaPopover(HitSampleInfo.BANK_NORMAL);
            hitObjectHasSampleBank(0, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_NORMAL);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_NORMAL);
        }

        [Test]
        public void TestHotkeysMultipleSelectionWithSameSampleBank()
        {
            AddStep("unify sample bank", () =>
            {
                foreach (var h in EditorBeatmap.HitObjects)
                {
                    for (int i = 0; i < h.Samples.Count; i++)
                    {
                        h.Samples[i] = h.Samples[i].With(newBank: HitSampleInfo.BANK_SOFT);
                    }
                }
            });

            AddStep("select both objects", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_SOFT);

            AddStep("Press normal bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.W);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_NORMAL);

            AddStep("Press drum bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_DRUM);

            AddStep("Press auto bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.Q);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            // Should be a noop.
            hitObjectHasSampleBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleBank(1, HitSampleInfo.BANK_DRUM);
        }

        [Test]
        public void TestHotkeysDuringPlacement()
        {
            AddStep("Enter placement mode", () => InputManager.Key(Key.Number2));
            AddStep("Move mouse to centre", () => InputManager.MoveMouseTo(Editor.ChildrenOfType<HitObjectComposer>().First().ScreenSpaceDrawQuad.Centre));

            AddStep("Move between two objects", () => EditorClock.Seek(250));

            AddStep("Press normal bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.W);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            checkPlacementSample(HitSampleInfo.BANK_NORMAL);

            AddStep("Press drum bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            checkPlacementSample(HitSampleInfo.BANK_DRUM);

            AddStep("Press auto bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.Q);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            checkPlacementSample(HitSampleInfo.BANK_NORMAL);

            AddStep("Move after second object", () => EditorClock.Seek(750));
            checkPlacementSample(HitSampleInfo.BANK_SOFT);

            AddStep("Move to first object", () => EditorClock.Seek(0));
            checkPlacementSample(HitSampleInfo.BANK_NORMAL);

            void checkPlacementSample(string expected) => AddAssert($"Placement sample is {expected}", () => EditorBeatmap.PlacementObject.Value.Samples.First().Bank, () => Is.EqualTo(expected));
        }

        private void clickSamplePiece(int objectIndex) => AddStep($"click {objectIndex.ToOrdinalWords()} sample piece", () =>
        {
            var samplePiece = this.ChildrenOfType<SamplePointPiece>().Single(piece => piece.HitObject == EditorBeatmap.HitObjects.ElementAt(objectIndex));

            InputManager.MoveMouseTo(samplePiece);
            InputManager.Click(MouseButton.Left);
        });

        private void samplePopoverHasFocus() => AddUntilStep("sample popover textbox focused", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault();
            var slider = popover?.ChildrenOfType<IndeterminateSliderWithTextBoxInput<int>>().Single();
            var textbox = slider?.ChildrenOfType<OsuTextBox>().Single();

            return textbox?.HasFocus == true;
        });

        private void samplePopoverHasSingleVolume(int volume) => AddUntilStep($"sample popover has volume {volume}", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault();
            var slider = popover?.ChildrenOfType<IndeterminateSliderWithTextBoxInput<int>>().Single();

            return slider?.Current.Value == volume;
        });

        private void samplePopoverHasIndeterminateVolume() => AddUntilStep("sample popover has indeterminate volume", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault();
            var slider = popover?.ChildrenOfType<IndeterminateSliderWithTextBoxInput<int>>().Single();

            return slider != null && slider.Current.Value == null;
        });

        private void samplePopoverHasSingleBank(string bank) => AddUntilStep($"sample popover has bank {bank}", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault();
            var textBox = popover?.ChildrenOfType<OsuTextBox>().First();

            return textBox?.Current.Value == bank && string.IsNullOrEmpty(textBox.PlaceholderText.ToString());
        });

        private void samplePopoverHasIndeterminateBank() => AddUntilStep("sample popover has indeterminate bank", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault();
            var textBox = popover?.ChildrenOfType<OsuTextBox>().First();

            return textBox != null && string.IsNullOrEmpty(textBox.Current.Value) && !string.IsNullOrEmpty(textBox.PlaceholderText.ToString());
        });

        private void dismissPopover()
        {
            AddStep("unfocus textbox", () => InputManager.Key(Key.Escape));
            AddStep("dismiss popover", () => InputManager.Key(Key.Escape));
            AddUntilStep("wait for dismiss", () => !this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().Any(popover => popover.IsPresent));
        }

        private void setVolumeViaPopover(int volume) => AddStep($"set volume {volume} via popover", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().Single();
            var slider = popover.ChildrenOfType<IndeterminateSliderWithTextBoxInput<int>>().Single();
            slider.Current.Value = volume;
        });

        private void hitObjectHasSampleVolume(int objectIndex, int volume) => AddAssert($"{objectIndex.ToOrdinalWords()} has volume {volume}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h.Samples.All(o => o.Volume == volume);
        });

        private void setBankViaPopover(string bank) => AddStep($"set bank {bank} via popover", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().Single();
            var textBox = popover.ChildrenOfType<LabelledTextBox>().First();
            textBox.Current.Value = bank;
            // force a commit via keyboard.
            // this is needed when testing attempting to set empty bank - which should revert to the previous value, but only on commit.
            InputManager.ChangeFocus(textBox);
            InputManager.Key(Key.Enter);
        });

        private void hitObjectHasSamples(int objectIndex, params string[] samples) => AddAssert($"{objectIndex.ToOrdinalWords()} has samples {string.Join(',', samples)}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h.Samples.Select(s => s.Name).SequenceEqual(samples);
        });

        private void hitObjectHasSampleBank(int objectIndex, string bank) => AddAssert($"{objectIndex.ToOrdinalWords()} has bank {bank}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h.Samples.All(o => o.Bank == bank);
        });
    }
}
