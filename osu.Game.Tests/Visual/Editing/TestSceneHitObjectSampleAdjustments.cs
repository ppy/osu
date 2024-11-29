// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Collections.Generic;
using Humanizer;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
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
        public void TestPopoverHasNoFocus()
        {
            clickSamplePiece(0);
            samplePopoverHasNoFocus();
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
        public void TestPopoverAddSampleAddition()
        {
            clickSamplePiece(0);

            setBankViaPopover(HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleBank(0, HitSampleInfo.BANK_SOFT);

            toggleAdditionViaPopover(0);

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);

            setAdditionBankViaPopover(HitSampleInfo.BANK_DRUM);

            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_DRUM);

            toggleAdditionViaPopover(0);

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL);
        }

        [Test]
        public void TestNodeSamplePopover()
        {
            AddStep("add slider", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Slider
                {
                    Position = new Vector2(256, 256),
                    StartTime = 0,
                    Path = new SliderPath(new[] { new PathControlPoint(Vector2.Zero), new PathControlPoint(new Vector2(250, 0)) }),
                    Samples =
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    },
                    NodeSamples =
                    {
                        new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) },
                        new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) },
                    }
                });
            });

            clickNodeSamplePiece(0, 1);

            setBankViaPopover(HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSampleBank(0, 0, HitSampleInfo.BANK_NORMAL);
            hitObjectNodeHasSampleBank(0, 1, HitSampleInfo.BANK_SOFT);

            toggleAdditionViaPopover(0);

            hitObjectNodeHasSampleBank(0, 0, HitSampleInfo.BANK_NORMAL);
            hitObjectNodeHasSampleBank(0, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(0, 0, HitSampleInfo.HIT_NORMAL);
            hitObjectNodeHasSamples(0, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);

            setAdditionBankViaPopover(HitSampleInfo.BANK_DRUM);

            hitObjectNodeHasSampleBank(0, 0, HitSampleInfo.BANK_NORMAL);
            hitObjectNodeHasSampleNormalBank(0, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSampleAdditionBank(0, 1, HitSampleInfo.BANK_DRUM);

            toggleAdditionViaPopover(0);

            hitObjectNodeHasSampleBank(0, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(0, 0, HitSampleInfo.HIT_NORMAL);
            hitObjectNodeHasSamples(0, 1, HitSampleInfo.HIT_NORMAL);

            setVolumeViaPopover(10);

            hitObjectNodeHasSampleVolume(0, 0, 100);
            hitObjectNodeHasSampleVolume(0, 1, 10);
        }

        [Test]
        public void TestSamplePointSeek()
        {
            AddStep("add slider", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Slider
                {
                    Position = new Vector2(256, 256),
                    StartTime = 0,
                    Path = new SliderPath(new[] { new PathControlPoint(Vector2.Zero), new PathControlPoint(new Vector2(250, 0)) }),
                    Samples =
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    },
                    NodeSamples =
                    {
                        new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) },
                        new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL) },
                    },
                    RepeatCount = 1
                });
            });

            seekSamplePiece(-1);
            editorTimeIs(0);
            samplePopoverIsOpen();
            seekSamplePiece(-1);
            editorTimeIs(0);
            samplePopoverIsOpen();
            seekSamplePiece(1);
            editorTimeIs(406);
            seekSamplePiece(1);
            editorTimeIs(813);
            seekSamplePiece(1);
            editorTimeIs(1627);
            seekSamplePiece(1);
            editorTimeIs(1627);
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

            AddStep("add whistle addition", () =>
            {
                foreach (var h in EditorBeatmap.HitObjects)
                    h.Samples.Add(new HitSampleInfo(HitSampleInfo.HIT_WHISTLE, HitSampleInfo.BANK_SOFT));
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

            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSampleNormalBank(1, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleAdditionBank(1, HitSampleInfo.BANK_SOFT);

            AddStep("Press drum bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleNormalBank(1, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleAdditionBank(1, HitSampleInfo.BANK_SOFT);

            AddStep("Press auto bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.Q);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            // Should be a noop.
            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleNormalBank(1, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleAdditionBank(1, HitSampleInfo.BANK_SOFT);

            AddStep("Press addition normal bank shortcut", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.W);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleNormalBank(1, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSampleAdditionBank(1, HitSampleInfo.BANK_NORMAL);

            AddStep("Press addition drum bank shortcut", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleNormalBank(1, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleAdditionBank(1, HitSampleInfo.BANK_DRUM);

            AddStep("Press auto bank shortcut", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.Q);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            // Should be a noop.
            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleNormalBank(1, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleAdditionBank(1, HitSampleInfo.BANK_DRUM);
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

            AddStep("Press soft addition bank shortcut", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.E);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            checkPlacementSampleBank(HitSampleInfo.BANK_NORMAL);

            AddStep("Press finish sample shortcut", () =>
            {
                InputManager.Key(Key.E);
            });

            checkPlacementSampleAdditionBank(HitSampleInfo.BANK_SOFT);

            AddStep("Press drum bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            checkPlacementSampleBank(HitSampleInfo.BANK_DRUM);
            checkPlacementSampleAdditionBank(HitSampleInfo.BANK_SOFT);

            AddStep("Press drum addition bank shortcut", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            checkPlacementSampleBank(HitSampleInfo.BANK_DRUM);
            checkPlacementSampleAdditionBank(HitSampleInfo.BANK_DRUM);

            AddStep("Press auto bank shortcut", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Key(Key.Q);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });

            checkPlacementSampleBank(HitSampleInfo.BANK_NORMAL);
            checkPlacementSampleAdditionBank(HitSampleInfo.BANK_DRUM);

            AddStep("Press auto addition bank shortcut", () =>
            {
                InputManager.PressKey(Key.AltLeft);
                InputManager.Key(Key.Q);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            checkPlacementSampleBank(HitSampleInfo.BANK_NORMAL);
            checkPlacementSampleAdditionBank(HitSampleInfo.BANK_NORMAL);

            AddStep("Move after second object", () => EditorClock.Seek(750));
            checkPlacementSampleBank(HitSampleInfo.BANK_SOFT);
            checkPlacementSampleAdditionBank(HitSampleInfo.BANK_SOFT);

            AddStep("Move to first object", () => EditorClock.Seek(0));
            checkPlacementSampleBank(HitSampleInfo.BANK_NORMAL);
            checkPlacementSampleAdditionBank(HitSampleInfo.BANK_NORMAL);

            void checkPlacementSampleBank(string expected) => AddAssert($"Placement sample is {expected}",
                () => EditorBeatmap.PlacementObject.Value.Samples.First(s => s.Name == HitSampleInfo.HIT_NORMAL).Bank, () => Is.EqualTo(expected));

            void checkPlacementSampleAdditionBank(string expected) => AddAssert($"Placement sample addition is {expected}",
                () => EditorBeatmap.PlacementObject.Value.Samples.First(s => s.Name != HitSampleInfo.HIT_NORMAL).Bank, () => Is.EqualTo(expected));
        }

        [Test]
        public void PopoverForMultipleSelectionChangesAllSamples()
        {
            AddStep("add slider", () =>
            {
                EditorBeatmap.Add(new Slider
                {
                    Position = new Vector2(256, 256),
                    StartTime = 1000,
                    Path = new SliderPath(new[] { new PathControlPoint(Vector2.Zero), new PathControlPoint(new Vector2(250, 0)) }),
                    Samples =
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    },
                    NodeSamples = new List<IList<HitSampleInfo>>
                    {
                        new List<HitSampleInfo>
                        {
                            new HitSampleInfo(HitSampleInfo.HIT_NORMAL, bank: HitSampleInfo.BANK_DRUM),
                            new HitSampleInfo(HitSampleInfo.HIT_CLAP, bank: HitSampleInfo.BANK_DRUM),
                        },
                        new List<HitSampleInfo>
                        {
                            new HitSampleInfo(HitSampleInfo.HIT_NORMAL, bank: HitSampleInfo.BANK_SOFT),
                            new HitSampleInfo(HitSampleInfo.HIT_WHISTLE, bank: HitSampleInfo.BANK_SOFT),
                        },
                    }
                });
            });
            AddStep("select everything", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            clickSamplePiece(0);

            setBankViaPopover(HitSampleInfo.BANK_DRUM);
            samplePopoverHasSingleBank(HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleNormalBank(1, HitSampleInfo.BANK_DRUM);
            hitObjectHasSampleNormalBank(2, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSampleNormalBank(2, 0, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSampleNormalBank(2, 1, HitSampleInfo.BANK_DRUM);

            setVolumeViaPopover(30);
            samplePopoverHasSingleVolume(30);
            hitObjectHasSampleVolume(0, 30);
            hitObjectHasSampleVolume(1, 30);
            hitObjectHasSampleVolume(2, 30);
            hitObjectNodeHasSampleVolume(2, 0, 30);
            hitObjectNodeHasSampleVolume(2, 1, 30);

            toggleAdditionViaPopover(0);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);
            hitObjectHasSamples(1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);
            hitObjectHasSamples(2, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);
            hitObjectNodeHasSamples(2, 0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP, HitSampleInfo.HIT_WHISTLE);
            hitObjectNodeHasSamples(2, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);

            setAdditionBankViaPopover(HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleAdditionBank(1, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleAdditionBank(2, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSampleAdditionBank(2, 0, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSampleAdditionBank(2, 1, HitSampleInfo.BANK_SOFT);
        }

        [Test]
        public void TestHotkeysAffectNodeSamples()
        {
            AddStep("add slider", () =>
            {
                EditorBeatmap.Add(new Slider
                {
                    Position = new Vector2(256, 256),
                    StartTime = 1000,
                    Path = new SliderPath(new[] { new PathControlPoint(Vector2.Zero), new PathControlPoint(new Vector2(250, 0)) }),
                    Samples =
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    },
                    NodeSamples = new List<IList<HitSampleInfo>>
                    {
                        new List<HitSampleInfo>
                        {
                            new HitSampleInfo(HitSampleInfo.HIT_NORMAL, bank: HitSampleInfo.BANK_DRUM),
                            new HitSampleInfo(HitSampleInfo.HIT_CLAP, bank: HitSampleInfo.BANK_DRUM),
                        },
                        new List<HitSampleInfo>
                        {
                            new HitSampleInfo(HitSampleInfo.HIT_NORMAL, bank: HitSampleInfo.BANK_SOFT),
                            new HitSampleInfo(HitSampleInfo.HIT_WHISTLE, bank: HitSampleInfo.BANK_SOFT),
                        },
                    }
                });
            });
            AddStep("select everything", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            AddStep("add clap addition", () => InputManager.Key(Key.R));

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP);

            hitObjectHasSampleBank(1, HitSampleInfo.BANK_SOFT);
            hitObjectHasSamples(1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP);

            hitObjectHasSampleBank(2, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSamples(2, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP);
            hitObjectNodeHasSampleBank(2, 0, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSamples(2, 0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP);
            hitObjectNodeHasSampleBank(2, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(2, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE, HitSampleInfo.HIT_CLAP);

            AddStep("remove clap addition", () => InputManager.Key(Key.R));

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL);

            hitObjectHasSampleBank(1, HitSampleInfo.BANK_SOFT);
            hitObjectHasSamples(1, HitSampleInfo.HIT_NORMAL);

            hitObjectHasSampleBank(2, HitSampleInfo.BANK_NORMAL);
            hitObjectHasSamples(2, HitSampleInfo.HIT_NORMAL);
            hitObjectNodeHasSampleBank(2, 0, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSamples(2, 0, HitSampleInfo.HIT_NORMAL);
            hitObjectNodeHasSampleBank(2, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(2, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);

            AddStep("set drum bank", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.LShift);
            });

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL);

            hitObjectHasSampleBank(1, HitSampleInfo.BANK_DRUM);
            hitObjectHasSamples(1, HitSampleInfo.HIT_NORMAL);

            hitObjectHasSampleBank(2, HitSampleInfo.BANK_DRUM);
            hitObjectHasSamples(2, HitSampleInfo.HIT_NORMAL);
            hitObjectNodeHasSampleBank(2, 0, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSamples(2, 0, HitSampleInfo.HIT_NORMAL);
            hitObjectNodeHasSampleNormalBank(2, 1, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSampleAdditionBank(2, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(2, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);

            AddStep("set normal addition bank", () =>
            {
                InputManager.PressKey(Key.LAlt);
                InputManager.Key(Key.W);
                InputManager.ReleaseKey(Key.LAlt);
            });

            hitObjectHasSampleBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL);

            hitObjectHasSampleBank(1, HitSampleInfo.BANK_DRUM);
            hitObjectHasSamples(1, HitSampleInfo.HIT_NORMAL);

            hitObjectHasSampleBank(2, HitSampleInfo.BANK_DRUM);
            hitObjectHasSamples(2, HitSampleInfo.HIT_NORMAL);
            hitObjectNodeHasSampleBank(2, 0, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSamples(2, 0, HitSampleInfo.HIT_NORMAL);
            hitObjectNodeHasSampleNormalBank(2, 1, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSampleAdditionBank(2, 1, HitSampleInfo.BANK_NORMAL);
            hitObjectNodeHasSamples(2, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);
        }

        [Test]
        public void TestHotkeysUnifySliderSamplesAndNodeSamples()
        {
            AddStep("add slider", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Slider
                {
                    Position = new Vector2(256, 256),
                    StartTime = 1000,
                    Path = new SliderPath(new[] { new PathControlPoint(Vector2.Zero), new PathControlPoint(new Vector2(250, 0)) }),
                    Samples =
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_SOFT),
                        new HitSampleInfo(HitSampleInfo.HIT_WHISTLE, bank: HitSampleInfo.BANK_DRUM),
                    },
                    NodeSamples = new List<IList<HitSampleInfo>>
                    {
                        new List<HitSampleInfo>
                        {
                            new HitSampleInfo(HitSampleInfo.HIT_NORMAL, bank: HitSampleInfo.BANK_DRUM),
                            new HitSampleInfo(HitSampleInfo.HIT_CLAP, bank: HitSampleInfo.BANK_DRUM),
                        },
                        new List<HitSampleInfo>
                        {
                            new HitSampleInfo(HitSampleInfo.HIT_NORMAL, bank: HitSampleInfo.BANK_SOFT),
                            new HitSampleInfo(HitSampleInfo.HIT_WHISTLE, bank: HitSampleInfo.BANK_SOFT),
                        },
                    }
                });
            });
            AddStep("select everything", () => EditorBeatmap.SelectedHitObjects.AddRange(EditorBeatmap.HitObjects));

            AddStep("set soft bank", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.Key(Key.E);
                InputManager.ReleaseKey(Key.LShift);
            });

            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);
            hitObjectNodeHasSampleNormalBank(0, 0, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(0, 0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP);
            hitObjectNodeHasSampleNormalBank(0, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(0, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);

            AddStep("unify whistle addition", () => InputManager.Key(Key.W));

            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);
            hitObjectNodeHasSampleNormalBank(0, 0, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(0, 0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP, HitSampleInfo.HIT_WHISTLE);
            hitObjectNodeHasSampleNormalBank(0, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSamples(0, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);

            AddStep("set drum addition bank", () =>
            {
                InputManager.PressKey(Key.LAlt);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.LAlt);
            });

            hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_SOFT);
            hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_DRUM);
            hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);
            hitObjectNodeHasSampleNormalBank(0, 0, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSampleAdditionBank(0, 0, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSamples(0, 0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_CLAP, HitSampleInfo.HIT_WHISTLE);
            hitObjectNodeHasSampleNormalBank(0, 1, HitSampleInfo.BANK_SOFT);
            hitObjectNodeHasSampleAdditionBank(0, 1, HitSampleInfo.BANK_DRUM);
            hitObjectNodeHasSamples(0, 1, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_WHISTLE);
        }

        [Test]
        public void TestSelectingObjectDoesNotMutateSamples()
        {
            clickSamplePiece(0);
            toggleAdditionViaPopover(1);
            setAdditionBankViaPopover(HitSampleInfo.BANK_SOFT);
            dismissPopover();

            assertNoChanges();

            AddStep("select first object", () =>
            {
                EditorBeatmap.SelectedHitObjects.Clear();
                EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects[0]);
            });
            assertNoChanges();

            AddStep("select second object", () =>
            {
                EditorBeatmap.SelectedHitObjects.Clear();
                EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects[1]);
            });
            assertNoChanges();

            AddStep("select first object", () =>
            {
                EditorBeatmap.SelectedHitObjects.Clear();
                EditorBeatmap.SelectedHitObjects.Add(EditorBeatmap.HitObjects[0]);
            });
            assertNoChanges();

            void assertNoChanges()
            {
                hitObjectHasSamples(0, HitSampleInfo.HIT_NORMAL, HitSampleInfo.HIT_FINISH);
                hitObjectHasSampleNormalBank(0, HitSampleInfo.BANK_NORMAL);
                hitObjectHasSampleAdditionBank(0, HitSampleInfo.BANK_SOFT);

                hitObjectHasSamples(1, HitSampleInfo.HIT_NORMAL);
                hitObjectHasSampleNormalBank(1, HitSampleInfo.BANK_SOFT);
                hitObjectHasSampleAdditionBank(1, HitSampleInfo.BANK_SOFT);
            }
        }

        private void clickSamplePiece(int objectIndex) => AddStep($"click {objectIndex.ToOrdinalWords()} sample piece", () =>
        {
            var samplePiece = this.ChildrenOfType<SamplePointPiece>().Single(piece => piece is not NodeSamplePointPiece && piece.HitObject == EditorBeatmap.HitObjects.ElementAt(objectIndex));

            InputManager.MoveMouseTo(samplePiece);
            InputManager.Click(MouseButton.Left);
        });

        private void clickNodeSamplePiece(int objectIndex, int nodeIndex) => AddStep($"click {objectIndex.ToOrdinalWords()} object {nodeIndex.ToOrdinalWords()} node sample piece", () =>
        {
            var samplePiece = this.ChildrenOfType<NodeSamplePointPiece>().Where(piece => piece.HitObject == EditorBeatmap.HitObjects.ElementAt(objectIndex)).ToArray()[nodeIndex];

            InputManager.MoveMouseTo(samplePiece);
            InputManager.Click(MouseButton.Left);
        });

        private void seekSamplePiece(int direction) => AddStep($"seek sample piece {direction}", () =>
        {
            InputManager.PressKey(Key.ControlLeft);
            InputManager.PressKey(Key.ShiftLeft);
            InputManager.Key(direction < 1 ? Key.Left : Key.Right);
            InputManager.ReleaseKey(Key.ShiftLeft);
            InputManager.ReleaseKey(Key.ControlLeft);
        });

        private void samplePopoverIsOpen() => AddUntilStep("sample popover is open", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault(o => o.IsPresent);
            return popover != null;
        });

        private void samplePopoverHasNoFocus() => AddUntilStep("sample popover textbox not focused", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().SingleOrDefault();
            var slider = popover?.ChildrenOfType<IndeterminateSliderWithTextBoxInput<int>>().Single();
            var textbox = slider?.ChildrenOfType<OsuTextBox>().Single();

            return textbox?.HasFocus == false;
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

        private void hitObjectNodeHasSampleVolume(int objectIndex, int nodeIndex, int volume) => AddAssert(
            $"{objectIndex.ToOrdinalWords()} object {nodeIndex.ToOrdinalWords()} node has volume {volume}", () =>
            {
                var h = EditorBeatmap.HitObjects.ElementAt(objectIndex) as IHasRepeats;
                return h is not null && h.NodeSamples[nodeIndex].All(o => o.Volume == volume);
            });

        private void setBankViaPopover(string bank) => AddStep($"set bank {bank} via popover", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().Single();
            var textBox = popover.ChildrenOfType<LabelledTextBox>().First();
            textBox.Current.Value = bank;
            // force a commit via keyboard.
            // this is needed when testing attempting to set empty bank - which should revert to the previous value, but only on commit.
            ((IFocusManager)InputManager).ChangeFocus(textBox);
            InputManager.Key(Key.Enter);
        });

        private void setAdditionBankViaPopover(string bank) => AddStep($"set addition bank {bank} via popover", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().Single();
            var textBox = popover.ChildrenOfType<LabelledTextBox>().ToArray()[1];
            textBox.Current.Value = bank;
            // force a commit via keyboard.
            // this is needed when testing attempting to set empty bank - which should revert to the previous value, but only on commit.
            ((IFocusManager)InputManager).ChangeFocus(textBox);
            InputManager.Key(Key.Enter);
        });

        private void toggleAdditionViaPopover(int index) => AddStep($"toggle addition {index} via popover", () =>
        {
            var popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().First();
            var ternaryButton = popover.ChildrenOfType<DrawableTernaryButton>().ToArray()[index];
            InputManager.MoveMouseTo(ternaryButton);
            InputManager.PressButton(MouseButton.Left);
            InputManager.ReleaseButton(MouseButton.Left);
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

        private void hitObjectHasSampleNormalBank(int objectIndex, string bank) => AddAssert($"{objectIndex.ToOrdinalWords()} has normal bank {bank}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h.Samples.Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(o => o.Bank == bank);
        });

        private void hitObjectHasSampleAdditionBank(int objectIndex, string bank) => AddAssert($"{objectIndex.ToOrdinalWords()} has addition bank {bank}", () =>
        {
            var h = EditorBeatmap.HitObjects.ElementAt(objectIndex);
            return h.Samples.Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(o => o.Bank == bank);
        });

        private void hitObjectNodeHasSamples(int objectIndex, int nodeIndex, params string[] samples) => AddAssert(
            $"{objectIndex.ToOrdinalWords()} object {nodeIndex.ToOrdinalWords()} node has samples {string.Join(',', samples)}", () =>
            {
                var h = EditorBeatmap.HitObjects.ElementAt(objectIndex) as IHasRepeats;
                return h is not null && h.NodeSamples[nodeIndex].Select(s => s.Name).SequenceEqual(samples);
            });

        private void hitObjectNodeHasSampleBank(int objectIndex, int nodeIndex, string bank) => AddAssert($"{objectIndex.ToOrdinalWords()} object {nodeIndex.ToOrdinalWords()} node has bank {bank}",
            () =>
            {
                var h = EditorBeatmap.HitObjects.ElementAt(objectIndex) as IHasRepeats;
                return h is not null && h.NodeSamples[nodeIndex].All(o => o.Bank == bank);
            });

        private void hitObjectNodeHasSampleNormalBank(int objectIndex, int nodeIndex, string bank) => AddAssert(
            $"{objectIndex.ToOrdinalWords()} object {nodeIndex.ToOrdinalWords()} node has normal bank {bank}", () =>
            {
                var h = EditorBeatmap.HitObjects.ElementAt(objectIndex) as IHasRepeats;
                return h is not null && h.NodeSamples[nodeIndex].Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(o => o.Bank == bank);
            });

        private void hitObjectNodeHasSampleAdditionBank(int objectIndex, int nodeIndex, string bank) => AddAssert(
            $"{objectIndex.ToOrdinalWords()} object {nodeIndex.ToOrdinalWords()} node has addition bank {bank}", () =>
            {
                var h = EditorBeatmap.HitObjects.ElementAt(objectIndex) as IHasRepeats;
                return h is not null && h.NodeSamples[nodeIndex].Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(o => o.Bank == bank);
            });

        private void editorTimeIs(double time) => AddAssert($"editor time is {time}", () => Precision.AlmostEquals(EditorClock.CurrentTimeAccurate, time, 1));
    }
}
