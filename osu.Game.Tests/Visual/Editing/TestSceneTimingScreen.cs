// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Timing;
using osu.Game.Screens.Edit.Timing.RowAttributes;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    [TestFixture]
    public partial class TestSceneTimingScreen : EditorClockTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private TimingScreen timingScreen;

        protected override bool ScrollUsingMouseWheel => false;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Beatmap.Value = CreateWorkingBeatmap(Ruleset.Value);
            Beatmap.Disabled = true;

            var editorBeatmap = new EditorBeatmap(Beatmap.Value.GetPlayableBeatmap(Ruleset.Value));

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

            AddUntilStep("wait for rows to load", () => Child.ChildrenOfType<EffectRowAttribute>().Any());
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

        protected override void Dispose(bool isDisposing)
        {
            Beatmap.Disabled = false;
            base.Dispose(isDisposing);
        }
    }
}
