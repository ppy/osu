// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneTapTimingControl : EditorClockTestScene
    {
        private EditorBeatmap editorBeatmap => editorBeatmapContainer?.EditorBeatmap;

        private TestSceneHitObjectComposer.EditorBeatmapContainer editorBeatmapContainer;

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Cached]
        private Bindable<ControlPointGroup> selectedGroup = new Bindable<ControlPointGroup>();

        private TapTimingControl control;
        private OsuSpriteText timingInfo;

        [Resolved]
        private AudioManager audio { get; set; }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("create beatmap", () =>
            {
                Beatmap.Value = new WaveformTestBeatmap(audio);
            });

            AddStep("Create component", () =>
            {
                Child = editorBeatmapContainer = new TestSceneHitObjectComposer.EditorBeatmapContainer(Beatmap.Value)
                {
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            AutoSizeAxes = Axes.Y,
                            Width = 400,
                            Scale = new Vector2(1.5f),
                            Child = control = new TapTimingControl(),
                        },
                        timingInfo = new OsuSpriteText(),
                    }
                };

                selectedGroup.Value = editorBeatmap.ControlPointInfo.Groups.First();
            });
        }

        protected override void Update()
        {
            base.Update();

            if (selectedGroup.Value != null)
                timingInfo.Text = $"offset: {selectedGroup.Value.Time:N2} bpm: {selectedGroup.Value.ControlPoints.OfType<TimingControlPoint>().First().BPM:N2}";
        }

        [Test]
        public void TestBasic()
        {
            AddStep("set low bpm", () =>
            {
                editorBeatmap.ControlPointInfo.TimingPoints.First().BeatLength = 1000;
            });

            AddStep("click tap button", () =>
            {
                control.ChildrenOfType<OsuButton>()
                       .Last()
                       .TriggerClick();
            });

            AddSliderStep("BPM", 30, 400, 128, bpm =>
            {
                if (editorBeatmap == null)
                    return;

                editorBeatmap.ControlPointInfo.TimingPoints.First().BeatLength = 60000f / bpm;
            });
        }

        [Test]
        public void TestTapThenReset()
        {
            AddStep("click tap button", () =>
            {
                control.ChildrenOfType<OsuButton>()
                       .Last()
                       .TriggerClick();
            });

            AddUntilStep("wait for track playing", () => EditorClock.IsRunning);

            AddStep("click reset button", () =>
            {
                control.ChildrenOfType<OsuButton>()
                       .First()
                       .TriggerClick();
            });

            AddUntilStep("wait for track stopped", () => !EditorClock.IsRunning);
        }

        [Test]
        public void TestNoCrashesWhenNoGroupSelected()
        {
            AddStep("unset selected group", () => selectedGroup.Value = null);
            AddStep("press T to tap", () => InputManager.Key(Key.T));

            AddStep("click tap button", () =>
            {
                control.ChildrenOfType<OsuButton>()
                       .Last()
                       .TriggerClick();
            });

            AddStep("click reset button", () =>
            {
                control.ChildrenOfType<OsuButton>()
                       .First()
                       .TriggerClick();
            });

            AddStep("adjust offset", () =>
            {
                var adjustOffsetButton = control.ChildrenOfType<TimingAdjustButton>().First();
                InputManager.MoveMouseTo(adjustOffsetButton);
                InputManager.Click(MouseButton.Left);
            });

            AddStep("adjust BPM", () =>
            {
                var adjustBPMButton = control.ChildrenOfType<TimingAdjustButton>().Last();
                InputManager.MoveMouseTo(adjustBPMButton);
                InputManager.Click(MouseButton.Left);
            });
        }

        protected override void Dispose(bool isDisposing)
        {
            Beatmap.Disabled = false;
            base.Dispose(isDisposing);
        }
    }
}
