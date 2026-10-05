// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    /// <summary>
    /// Test editor hotkeys at a high level to ensure they all work well together.
    /// </summary>
    public partial class TestSceneEditorBindings : EditorSavingTestScene
    {
        [Test]
        public void TestBeatDivisorChangeHotkeys()
        {
            AddStep("hold shift", () => InputManager.PressKey(Key.LShift));

            AddStep("press 4", () => InputManager.Key(Key.Number4));
            AddAssert("snap updated to 4", () => EditorBeatmap.BeatmapInfo.BeatDivisor, () => Is.EqualTo(4));

            AddStep("press 6", () => InputManager.Key(Key.Number6));
            AddAssert("snap updated to 6", () => EditorBeatmap.BeatmapInfo.BeatDivisor, () => Is.EqualTo(6));

            AddStep("release shift", () => InputManager.ReleaseKey(Key.LShift));
        }

        [Test]
        public void TestAltScrollHandling()
        {
            double originalTimelineZoom = 0;

            AddUntilStep("wait for timeline load", () => Editor.ChildrenOfType<Timeline>().SingleOrDefault()?.IsLoaded == true);

            AddStep("Get timeline zoom", () => originalTimelineZoom = EditorBeatmap.TimelineZoom);

            AddStep("Alt-scroll outside of timeline", () =>
            {
                var composer = Editor.ChildrenOfType<HitObjectComposer>().Single();
                InputManager.MoveMouseTo(composer);
                InputManager.PressKey(Key.AltLeft);
                InputManager.ScrollVerticalBy(15f);
                InputManager.ReleaseKey(Key.AltLeft);
            });
            AddAssert("Volume meter displayed", () => Game.ChildrenOfType<VolumeOverlay>().Any(overlay => overlay.State.Value == Visibility.Visible));
            AddAssert("Timeline zoom unchanged", () => EditorBeatmap.TimelineZoom, () => Is.EqualTo(originalTimelineZoom));

            AddUntilStep("Wait for volume meter to hide", () => Game.ChildrenOfType<VolumeOverlay>().All(overlay => overlay.State.Value == Visibility.Hidden));

            AddStep("Set timeline zoom", () =>
            {
                var timeline = Editor.ChildrenOfType<Timeline>().Single();
                InputManager.MoveMouseTo(timeline);
                InputManager.PressKey(Key.AltLeft);
                InputManager.ScrollVerticalBy(15f);
                InputManager.ReleaseKey(Key.AltLeft);
            });
            AddAssert("Timeline zoom changed", () => EditorBeatmap.TimelineZoom, () => Is.Not.EqualTo(originalTimelineZoom));
            AddUntilStep("Volume meter not displayed", () => Game.ChildrenOfType<VolumeOverlay>().All(overlay => overlay.State.Value == Visibility.Hidden));
        }
    }
}
