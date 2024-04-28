// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Screens.Edit.Timing.RowAttributes;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneTimingScreen : EditorClockTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private TimingScreen timingScreen;
        private EditorBeatmap editorBeatmap;

        protected override bool ScrollUsingMouseWheel => false;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
            Beatmap.Disabled = true;
        }

        private void reloadEditorBeatmap()
        {
            editorBeatmap = new EditorBeatmap(Beatmap.Value.GetPlayableBeatmap(Ruleset.Value));

            Child = new DependencyProvidingContainer
            {
                RelativeSizeAxes = Axes.Both,
                CachedDependencies = new (Type, object)[]
                {
                    (typeof(EditorBeatmap), editorBeatmap),
                    (typeof(IBeatSnapProvider), editorBeatmap)
                },
                Child = timingScreen = new TimingScreen
                {
                    State = { Value = Visibility.Visible },
                },
            };
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Stop clock", () => EditorClock.Stop());

            AddStep("Reload Editor Beatmap", reloadEditorBeatmap);

            AddUntilStep("Wait for rows to load", () => Child.ChildrenOfType<EffectRowAttribute>().Any());
        }

        [Test]
        public void TestSelectedRetainedOverUndo()
        {
            AddStep("Select first timing point", () =>
            {
                InputManager.MoveMouseTo(Child.ChildrenOfType<TimingRowAttribute>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("Selection changed", () => timingScreen.SelectedGroup.Value.Time == 2170);
            AddUntilStep("Ensure seeked to correct time", () => EditorClock.CurrentTimeAccurate == 2170);

            AddStep("Adjust offset", () =>
            {
                InputManager.MoveMouseTo(timingScreen.ChildrenOfType<TimingAdjustButton>().First().ScreenSpaceDrawQuad.Centre + new Vector2(20, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for offset changed", () =>
            {
                return timingScreen.SelectedGroup.Value.ControlPoints.Any(c => c is TimingControlPoint) && timingScreen.SelectedGroup.Value.Time > 2170;
            });

            AddStep("simulate undo", () =>
            {
                var clone = editorBeatmap.ControlPointInfo.DeepClone();

                editorBeatmap.ControlPointInfo.Clear();

                foreach (var group in clone.Groups)
                {
                    foreach (var cp in group.ControlPoints)
                        editorBeatmap.ControlPointInfo.Add(group.Time, cp);
                }
            });

            AddUntilStep("selection retained", () =>
            {
                return timingScreen.SelectedGroup.Value.ControlPoints.Any(c => c is TimingControlPoint) && timingScreen.SelectedGroup.Value.Time > 2170;
            });
        }

        [Test]
        public void TestTrackingCurrentTimeWhileRunning()
        {
            AddStep("Select first effect point", () =>
            {
                InputManager.MoveMouseTo(Child.ChildrenOfType<EffectRowAttribute>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("Selection changed", () => timingScreen.SelectedGroup.Value.Time == 54670);
            AddUntilStep("Ensure seeked to correct time", () => EditorClock.CurrentTimeAccurate == 54670);

            AddStep("Seek to just before next point", () => EditorClock.Seek(69000));
            AddStep("Start clock", () => EditorClock.Start());

            AddUntilStep("Selection changed", () => timingScreen.SelectedGroup.Value.Time == 69670);
        }

        [Test]
        public void TestTrackingCurrentTimeWhilePaused()
        {
            AddStep("Select first effect point", () =>
            {
                InputManager.MoveMouseTo(Child.ChildrenOfType<EffectRowAttribute>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("Selection changed", () => timingScreen.SelectedGroup.Value.Time == 54670);
            AddUntilStep("Ensure seeked to correct time", () => EditorClock.CurrentTimeAccurate == 54670);

            AddStep("Seek to later", () => EditorClock.Seek(80000));
            AddUntilStep("Selection changed", () => timingScreen.SelectedGroup.Value.Time == 69670);
        }

        [Test]
        public void TestScrollControlGroupIntoView()
        {
            AddStep("Add many control points", () =>
            {
                editorBeatmap.ControlPointInfo.Clear();

                editorBeatmap.ControlPointInfo.Add(0, new TimingControlPoint());

                for (int i = 0; i < 100; i++)
                {
                    editorBeatmap.ControlPointInfo.Add((i + 1) * 1000, new EffectControlPoint
                    {
                        KiaiMode = Convert.ToBoolean(i % 2),
                    });
                }
            });

            AddStep("Select first effect point", () =>
            {
                InputManager.MoveMouseTo(Child.ChildrenOfType<EffectRowAttribute>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("Seek to beginning", () => EditorClock.Seek(0));

            AddStep("Seek to last point", () => EditorClock.Seek(101 * 1000));

            AddUntilStep("Scrolled to end", () => timingScreen.ChildrenOfType<OsuScrollContainer>().First().IsScrolledToEnd());
        }

        [Test]
        public void TestEditThenClickAwayAppliesChanges()
        {
            AddStep("Add two control points", () =>
            {
                editorBeatmap.ControlPointInfo.Clear();
                editorBeatmap.ControlPointInfo.Add(1000, new TimingControlPoint());
                editorBeatmap.ControlPointInfo.Add(2000, new TimingControlPoint());
            });

            AddStep("Select second timing point", () =>
            {
                InputManager.MoveMouseTo(Child.ChildrenOfType<TimingRowAttribute>().Last());
                InputManager.Click(MouseButton.Left);
            });

            AddStep("Scroll to end", () => timingScreen.ChildrenOfType<ControlPointSettings>().Single().ChildrenOfType<OsuScrollContainer>().Single().ScrollToEnd(false));
            AddStep("Modify time signature", () =>
            {
                var timeSignatureTextBox = Child.ChildrenOfType<LabelledTimeSignature.TimeSignatureBox>().Single().ChildrenOfType<TextBox>().Single();
                InputManager.MoveMouseTo(timeSignatureTextBox);
                InputManager.Click(MouseButton.Left);

                Debug.Assert(!timeSignatureTextBox.Current.Value.Equals("1", StringComparison.Ordinal));
                timeSignatureTextBox.Current.Value = "1";
            });

            AddStep("Select first timing point", () =>
            {
                InputManager.MoveMouseTo(Child.ChildrenOfType<TimingRowAttribute>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("Second timing point changed time signature", () => editorBeatmap.ControlPointInfo.TimingPoints.Last().TimeSignature.Numerator == 1);
            AddAssert("First timing point preserved time signature", () => editorBeatmap.ControlPointInfo.TimingPoints.First().TimeSignature.Numerator == 4);
        }

        protected override void Dispose(bool isDisposing)
        {
            Beatmap.Disabled = false;
            base.Dispose(isDisposing);
        }
    }
}
