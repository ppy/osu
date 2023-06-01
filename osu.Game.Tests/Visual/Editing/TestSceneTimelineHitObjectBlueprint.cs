// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Audio;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Screens.Edit.Components.TernaryButtons;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Screens.Edit.Timing;
using osuTK;
using osuTK.Input;
using static osu.Game.Screens.Edit.Compose.Components.Timeline.TimelineHitObjectBlueprint;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneTimelineHitObjectBlueprint : TimelineTestScene
    {
        public override Drawable CreateTestComponent() => new TimelineBlueprintContainer(Composer);

        [Test]
        public void TestContextMenu()
        {
            TimelineHitObjectBlueprint blueprint;

            AddStep("add object", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new HitCircle { StartTime = 3000 });
            });

            AddStep("click object", () =>
            {
                blueprint = this.ChildrenOfType<TimelineHitObjectBlueprint>().Single();
                InputManager.MoveMouseTo(blueprint);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("right click", () => InputManager.Click(MouseButton.Right));
            AddAssert("context menu open", () => this.ChildrenOfType<OsuContextMenu>().SingleOrDefault()?.State == MenuState.Open);
        }

        [Test]
        public void TestDisallowZeroDurationObjects()
        {
            DragArea dragArea;

            AddStep("add spinner", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Spinner
                {
                    Position = new Vector2(256, 256),
                    StartTime = 2700,
                    Duration = 500
                });
            });

            AddStep("hold down drag bar", () =>
            {
                // distinguishes between the actual drag bar and its "underlay shadow".
                dragArea = this.ChildrenOfType<DragArea>().Single(bar => bar.HandlePositionalInput);
                InputManager.MoveMouseTo(dragArea);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("try to drag bar past start", () =>
            {
                var blueprint = this.ChildrenOfType<TimelineHitObjectBlueprint>().Single();
                InputManager.MoveMouseTo(blueprint.SelectionQuad.TopLeft - new Vector2(100, 0));
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("object has non-zero duration", () => EditorBeatmap.HitObjects.OfType<IHasDuration>().Single().Duration > 0);
        }

        [Test]
        public void TestDisallowRepeatsOnZeroDurationObjects()
        {
            DragArea dragArea;

            AddStep("add zero length slider", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new Slider
                {
                    Position = new Vector2(256, 256),
                    StartTime = 2700
                });
            });

            AddStep("hold down drag bar", () =>
            {
                // distinguishes between the actual drag bar and its "underlay shadow".
                dragArea = this.ChildrenOfType<DragArea>().Single(bar => bar.HandlePositionalInput);
                InputManager.MoveMouseTo(dragArea);
                InputManager.PressButton(MouseButton.Left);
            });

            AddStep("try to extend drag bar", () =>
            {
                var blueprint = this.ChildrenOfType<TimelineHitObjectBlueprint>().Single();
                InputManager.MoveMouseTo(blueprint.SelectionQuad.TopLeft + new Vector2(100, 0));
            });

            AddStep("release button", () => InputManager.PressButton(MouseButton.Left));

            AddAssert("object has zero repeats", () => EditorBeatmap.HitObjects.OfType<IHasRepeats>().Single().RepeatCount == 0);
        }

        [Test]
        public void TestSamplePointPiece()
        {
            SamplePointPiece.SampleEditPopover popover = null!;

            AddStep("add circle", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(new HitCircle
                {
                    Position = new Vector2(256, 256),
                    StartTime = 2700,
                    Samples =
                    {
                        new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
                    }
                });
            });

            AddStep("open hitsound popover", () =>
            {
                var samplePointPiece = this.ChildrenOfType<SamplePointPiece>().Single();
                InputManager.MoveMouseTo(samplePointPiece);
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddStep("add whistle addition", () =>
            {
                popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().First();
                var whistleTernaryButton = popover.ChildrenOfType<DrawableTernaryButton>().First();
                InputManager.MoveMouseTo(whistleTernaryButton);
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("has whistle sample", () => EditorBeatmap.HitObjects.First().Samples.Any(o => o.Name == HitSampleInfo.HIT_WHISTLE));

            AddStep("change bank name", () =>
            {
                var bankTextBox = popover.ChildrenOfType<LabelledTextBox>().First();
                bankTextBox.Current.Value = "soft";
            });

            AddAssert("bank name changed", () =>
                EditorBeatmap.HitObjects.First().Samples.Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(o => o.Bank == "soft")
                && EditorBeatmap.HitObjects.First().Samples.Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(o => o.Bank == "normal"));

            AddStep("change addition bank name", () =>
            {
                var bankTextBox = popover.ChildrenOfType<LabelledTextBox>().ToArray()[1];
                bankTextBox.Current.Value = "drum";
            });

            AddAssert("addition bank name changed", () =>
                EditorBeatmap.HitObjects.First().Samples.Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(o => o.Bank == "soft")
                && EditorBeatmap.HitObjects.First().Samples.Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(o => o.Bank == "drum"));

            AddStep("change volume", () =>
            {
                var bankTextBox = popover.ChildrenOfType<IndeterminateSliderWithTextBoxInput<int>>().Single();
                bankTextBox.Current.Value = 30;
            });

            AddAssert("volume changed", () => EditorBeatmap.HitObjects.First().Samples.All(o => o.Volume == 30));

            AddStep("close popover", () =>
            {
                InputManager.MoveMouseTo(popover, new Vector2(200, 0));
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
                popover = null;
            });
        }

        [Test]
        public void TestNodeSamplePointPiece()
        {
            Slider slider = null!;
            SamplePointPiece.SampleEditPopover popover = null!;

            AddStep("add slider", () =>
            {
                EditorBeatmap.Clear();
                EditorBeatmap.Add(slider = new Slider
                {
                    Position = new Vector2(256, 256),
                    StartTime = 2700,
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

            AddStep("open slider end hitsound popover", () =>
            {
                var samplePointPiece = this.ChildrenOfType<NodeSamplePointPiece>().Last();
                InputManager.MoveMouseTo(samplePointPiece);
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddStep("add whistle addition", () =>
            {
                popover = this.ChildrenOfType<SamplePointPiece.SampleEditPopover>().First();
                var whistleTernaryButton = popover.ChildrenOfType<DrawableTernaryButton>().First();
                InputManager.MoveMouseTo(whistleTernaryButton);
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
            });

            AddAssert("has whistle sample", () => slider.NodeSamples[1].Any(o => o.Name == HitSampleInfo.HIT_WHISTLE));

            AddStep("change bank name", () =>
            {
                var bankTextBox = popover.ChildrenOfType<LabelledTextBox>().First();
                bankTextBox.Current.Value = "soft";
            });

            AddAssert("bank name changed", () =>
                slider.NodeSamples[1].Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(o => o.Bank == "soft")
                && slider.NodeSamples[1].Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(o => o.Bank == "normal"));

            AddStep("change addition bank name", () =>
            {
                var bankTextBox = popover.ChildrenOfType<LabelledTextBox>().ToArray()[1];
                bankTextBox.Current.Value = "drum";
            });

            AddAssert("addition bank name changed", () =>
                slider.NodeSamples[1].Where(o => o.Name == HitSampleInfo.HIT_NORMAL).All(o => o.Bank == "soft")
                && slider.NodeSamples[1].Where(o => o.Name != HitSampleInfo.HIT_NORMAL).All(o => o.Bank == "drum"));

            AddStep("change volume", () =>
            {
                var bankTextBox = popover.ChildrenOfType<IndeterminateSliderWithTextBoxInput<int>>().Single();
                bankTextBox.Current.Value = 30;
            });

            AddAssert("volume changed", () => slider.NodeSamples[1].All(o => o.Volume == 30));

            AddStep("close popover", () =>
            {
                InputManager.MoveMouseTo(popover, new Vector2(200, 0));
                InputManager.PressButton(MouseButton.Left);
                InputManager.ReleaseButton(MouseButton.Left);
                popover = null;
            });
        }
    }
}
