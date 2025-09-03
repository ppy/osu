// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestScenePlacementBlueprint : EditorTestScene
    {
        protected override Ruleset CreateEditorRuleset() => new OsuRuleset();

        protected override IBeatmap CreateBeatmap(RulesetInfo ruleset) => new TestBeatmap(ruleset, false);

        private GlobalActionContainer globalActionContainer => this.ChildrenOfType<GlobalActionContainer>().Single();

        [Test]
        public void TestDeleteUsingMiddleMouse()
        {
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));

            AddAssert("one circle added", () => EditorBeatmap.HitObjects, () => Has.One.Items);
            AddStep("delete with middle mouse", () => InputManager.Click(MouseButton.Middle));
            AddAssert("circle removed", () => EditorBeatmap.HitObjects, () => Is.Empty);
        }

        [Test]
        public void TestDeleteUsingShiftRightClick()
        {
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));

            AddAssert("one circle added", () => EditorBeatmap.HitObjects, () => Has.One.Items);
            AddStep("delete with right mouse", () =>
            {
                InputManager.PressKey(Key.ShiftLeft);
                InputManager.Click(MouseButton.Right);
                InputManager.ReleaseKey(Key.ShiftLeft);
            });
            AddAssert("circle removed", () => EditorBeatmap.HitObjects, () => Is.Empty);
        }

        [Test]
        public void TestRightClickDuringEmptyPlacementTogglesNewCombo()
        {
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddAssert("one circle added", () => EditorBeatmap.HitObjects, () => Has.One.Items);

            AddStep("move mouse away from placed circle", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single().ScreenSpaceDrawQuad.TopLeft + Vector2.One));

            AddAssert("new combo false", () => this.ChildrenOfType<NewComboTernaryButton>().Single().Current.Value, () => Is.EqualTo(TernaryState.False));
            AddStep("click right mouse", () => InputManager.Click(MouseButton.Right));
            AddAssert("new combo true", () => this.ChildrenOfType<NewComboTernaryButton>().Single().Current.Value, () => Is.EqualTo(TernaryState.True));
            AddAssert("context menu not visible", () => !Editor.ChildrenOfType<OsuContextMenu>().Any(c => c.IsPresent));

            AddStep("click right mouse", () => InputManager.Click(MouseButton.Right));
            AddAssert("new combo false", () => this.ChildrenOfType<NewComboTernaryButton>().Single().Current.Value, () => Is.EqualTo(TernaryState.False));
            AddAssert("context menu not visible", () => !Editor.ChildrenOfType<OsuContextMenu>().Any(c => c.IsPresent));
        }

        [Test]
        public void TestRightClickDuringPlacementDeletes()
        {
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddAssert("one circle added", () => EditorBeatmap.HitObjects, () => Has.One.Items);

            AddStep("click right mouse", () => InputManager.Click(MouseButton.Right));

            AddAssert("circle removed", () => EditorBeatmap.HitObjects, () => Has.Exactly(0).Items);
            AddAssert("circle not selected", () => EditorBeatmap.SelectedHitObjects, () => Has.Exactly(0).Items);
            AddAssert("context menu not visible", () => !Editor.ChildrenOfType<OsuContextMenu>().Any(c => c.IsPresent));
            AddAssert("new combo false", () => this.ChildrenOfType<NewComboTernaryButton>().Single().Current.Value, () => Is.EqualTo(TernaryState.False));
        }

        [Test]
        public void TestRightClickDuringSelectionShowsContextMenu()
        {
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));

            // ensure the circle we're selecting is not a new combo so we can assert
            // new combo doesn't happen to get toggled by right click.
            AddStep("seek forward", () => EditorClock.Seek(1000));
            AddStep("place second circle", () => InputManager.Click(MouseButton.Left));

            AddAssert("two circles added", () => EditorBeatmap.HitObjects, () => Has.Exactly(2).Items);
            AddAssert("context menu not visible", () => !Editor.ChildrenOfType<OsuContextMenu>().Any(c => c.IsPresent));

            AddStep("select selection tool", () => InputManager.Key(Key.Number1));
            AddStep("click right mouse", () => InputManager.Click(MouseButton.Right));

            AddAssert("circle not removed", () => EditorBeatmap.HitObjects, () => Has.Exactly(2).Items);
            AddAssert("circle selected", () => EditorBeatmap.SelectedHitObjects, () => Has.One.Items);
            AddAssert("context menu visible", () => Editor.ChildrenOfType<OsuContextMenu>().Any(c => c.IsPresent));
            AddAssert("new combo false", () => this.ChildrenOfType<NewComboTernaryButton>().Single().Current.Value, () => Is.EqualTo(TernaryState.False));
        }

        [Test]
        public void TestCommitPlacementViaRightClick()
        {
            Playfield playfield = null!;

            AddStep("select slider placement tool", () => InputManager.Key(Key.Number3));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("begin placement", () => InputManager.Click(MouseButton.Left));
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("confirm via right click", () => InputManager.Click(MouseButton.Right));
            AddAssert("slider placed", () => EditorBeatmap.HitObjects.Count, () => Is.EqualTo(1));
        }

        [Test]
        public void TestAbortPlacementViaGlobalAction()
        {
            Playfield playfield = null!;

            AddStep("select slider placement tool", () => InputManager.Key(Key.Number3));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("begin placement", () => InputManager.Click(MouseButton.Left));
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("abort via global action", () =>
            {
                globalActionContainer.TriggerPressed(GlobalAction.Back);
                globalActionContainer.TriggerReleased(GlobalAction.Back);
            });
            AddAssert("editor is still current", () => Editor.IsCurrentScreen());
            AddAssert("slider not placed", () => EditorBeatmap.HitObjects.Count, () => Is.EqualTo(0));
            AddAssert("no active placement", () => this.ChildrenOfType<ComposeBlueprintContainer>().Single().CurrentPlacement.PlacementActive,
                () => Is.EqualTo(PlacementBlueprint.PlacementState.Waiting));
        }

        [Test]
        public void TestCommitPlacementViaToolChange()
        {
            Playfield playfield = null!;

            AddStep("select slider placement tool", () => InputManager.Key(Key.Number3));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("begin placement", () => InputManager.Click(MouseButton.Left));
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });

            AddStep("change tool to circle", () => InputManager.Key(Key.Number2));
            AddAssert("slider placed", () => EditorBeatmap.HitObjects.Count, () => Is.EqualTo(1));
        }

        [Test]
        public void TestAutomaticBankAssignment()
        {
            AddStep("add object with soft bank", () => EditorBeatmap.Add(new HitCircle
            {
                StartTime = 0,
                Samples =
                {
                    new HitSampleInfo(name: HitSampleInfo.HIT_NORMAL, bank: HitSampleInfo.BANK_SOFT, volume: 70),
                    new HitSampleInfo(name: HitSampleInfo.HIT_WHISTLE, bank: HitSampleInfo.BANK_DRUM, volume: 70),
                }
            }));

            AddStep("seek to 500", () => EditorClock.Seek(500)); // previous object is the one at time 0
            AddStep("enable automatic bank assignment", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.PressKey(Key.LAlt);
                InputManager.Key(Key.Q);
                InputManager.ReleaseKey(Key.LAlt);
                InputManager.ReleaseKey(Key.LShift);
            });
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddAssert("circle has soft bank", () => EditorBeatmap.HitObjects[1].Samples.Single().Bank, () => Is.EqualTo(HitSampleInfo.BANK_SOFT));
            AddAssert("circle inherited volume", () => EditorBeatmap.HitObjects[1].Samples.All(s => s.Volume == 70));

            AddStep("seek to 250", () => EditorClock.Seek(250)); // previous object is the one at time 0
            AddStep("enable clap addition", () => InputManager.Key(Key.R));
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddAssert("circle has 2 samples", () => EditorBeatmap.HitObjects[1].Samples, () => Has.Count.EqualTo(2));
            AddAssert("normal sample has soft bank", () => EditorBeatmap.HitObjects[1].Samples.Single(s => s.Name == HitSampleInfo.HIT_NORMAL).Bank,
                () => Is.EqualTo(HitSampleInfo.BANK_SOFT));
            AddAssert("clap sample has drum bank", () => EditorBeatmap.HitObjects[1].Samples.Single(s => s.Name == HitSampleInfo.HIT_CLAP).Bank,
                () => Is.EqualTo(HitSampleInfo.BANK_DRUM));
            AddAssert("circle inherited volume", () => EditorBeatmap.HitObjects[1].Samples.All(s => s.Volume == 70));

            AddStep("seek to 1000", () => EditorClock.Seek(1000)); // previous object is the one at time 500, which has no additions
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddAssert("circle has 2 samples", () => EditorBeatmap.HitObjects[3].Samples, () => Has.Count.EqualTo(2));
            AddAssert("all samples have soft bank", () => EditorBeatmap.HitObjects[3].Samples.All(s => s.Bank == HitSampleInfo.BANK_SOFT));
            AddAssert("circle inherited volume", () => EditorBeatmap.HitObjects[3].Samples.All(s => s.Volume == 70));
        }

        [Test]
        public void TestVolumeIsInheritedFromLastObject()
        {
            AddStep("add object with soft bank", () => EditorBeatmap.Add(new HitCircle
            {
                StartTime = 0,
                Samples =
                {
                    new HitSampleInfo(name: HitSampleInfo.HIT_NORMAL, bank: HitSampleInfo.BANK_SOFT, volume: 70),
                }
            }));
            AddStep("seek to 500", () => EditorClock.Seek(500));
            AddStep("select drum bank", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.LShift);
            });
            AddStep("select circle placement tool", () => InputManager.Key(Key.Number2));
            AddStep("move mouse to center of playfield", () => InputManager.MoveMouseTo(this.ChildrenOfType<Playfield>().Single()));
            AddStep("place circle", () => InputManager.Click(MouseButton.Left));
            AddAssert("circle has drum bank", () => EditorBeatmap.HitObjects[1].Samples.All(s => s.Bank == HitSampleInfo.BANK_DRUM));
            AddAssert("circle inherited volume", () => EditorBeatmap.HitObjects[1].Samples.All(s => s.Volume == 70));
        }

        [Test]
        public void TestNodeSamplesAndSamplesAreSame()
        {
            Playfield playfield = null!;

            AddStep("select drum bank", () =>
            {
                InputManager.PressKey(Key.LShift);
                InputManager.PressKey(Key.LAlt);
                InputManager.Key(Key.R);
                InputManager.ReleaseKey(Key.LAlt);
                InputManager.ReleaseKey(Key.LShift);
            });
            AddStep("enable clap addition", () => InputManager.Key(Key.R));

            AddStep("select slider placement tool", () => InputManager.Key(Key.Number3));
            AddStep("move mouse to top left of playfield", () =>
            {
                playfield = this.ChildrenOfType<Playfield>().Single();
                var location = (3 * playfield.ScreenSpaceDrawQuad.TopLeft + playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("begin placement", () => InputManager.Click(MouseButton.Left));
            AddStep("move mouse to bottom right of playfield", () =>
            {
                var location = (playfield.ScreenSpaceDrawQuad.TopLeft + 3 * playfield.ScreenSpaceDrawQuad.BottomRight) / 4;
                InputManager.MoveMouseTo(location);
            });
            AddStep("confirm via right click", () => InputManager.Click(MouseButton.Right));
            AddAssert("slider placed", () => EditorBeatmap.HitObjects.Count, () => Is.EqualTo(1));

            AddAssert("slider samples have drum bank", () => EditorBeatmap.HitObjects[0].Samples.All(s => s.Bank == HitSampleInfo.BANK_DRUM));
            AddAssert("slider node samples have drum bank",
                () => ((IHasRepeats)EditorBeatmap.HitObjects[0]).NodeSamples.SelectMany(s => s).All(s => s.Bank == HitSampleInfo.BANK_DRUM));

            AddAssert("slider samples have clap addition",
                () => EditorBeatmap.HitObjects[0].Samples.Select(s => s.Name), () => Does.Contain(HitSampleInfo.HIT_CLAP));
            AddAssert("slider node samples have clap addition",
                () => ((IHasRepeats)EditorBeatmap.HitObjects[0]).NodeSamples.All(samples => samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP)));
        }
    }
}
