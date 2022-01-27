// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Screens.Select;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public class TestSceneEditorSaving : EditorSavingTestScene
    {
        [Test]
        public void TestMetadata()
        {
            AddStep("Set artist and title", () =>
            {
                EditorBeatmap.BeatmapInfo.Metadata.Artist = "artist";
                EditorBeatmap.BeatmapInfo.Metadata.Title = "title";
            });
            AddStep("Set author", () => EditorBeatmap.BeatmapInfo.Metadata.Author.Username = "author");
            AddStep("Set difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName = "difficulty");

            SaveEditor();

            AddAssert("Beatmap has correct metadata", () => EditorBeatmap.BeatmapInfo.Metadata.Artist == "artist" && EditorBeatmap.BeatmapInfo.Metadata.Title == "title");
            AddAssert("Beatmap has correct author", () => EditorBeatmap.BeatmapInfo.Metadata.Author.Username == "author");
            AddAssert("Beatmap has correct difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName == "difficulty");
            AddAssert("Beatmap has correct .osu file path", () => EditorBeatmap.BeatmapInfo.Path == "artist - title (author) [difficulty].osu");

            ReloadEditorToSameBeatmap();

            AddAssert("Beatmap still has correct metadata", () => EditorBeatmap.BeatmapInfo.Metadata.Artist == "artist" && EditorBeatmap.BeatmapInfo.Metadata.Title == "title");
            AddAssert("Beatmap still has correct author", () => EditorBeatmap.BeatmapInfo.Metadata.Author.Username == "author");
            AddAssert("Beatmap still has correct difficulty name", () => EditorBeatmap.BeatmapInfo.DifficultyName == "difficulty");
            AddAssert("Beatmap still has correct .osu file path", () => EditorBeatmap.BeatmapInfo.Path == "artist - title (author) [difficulty].osu");
        }

        [Test]
        public void TestConfiguration()
        {
            double originalTimelineZoom = 0;
            double changedTimelineZoom = 0;

            AddStep("Set beat divisor", () => Editor.Dependencies.Get<BindableBeatDivisor>().Value = 16);
            AddStep("Set timeline zoom", () =>
            {
                originalTimelineZoom = EditorBeatmap.BeatmapInfo.TimelineZoom;

                var timeline = Editor.ChildrenOfType<Timeline>().Single();
                InputManager.MoveMouseTo(timeline);
                InputManager.PressKey(Key.AltLeft);
                InputManager.ScrollVerticalBy(15f);
                InputManager.ReleaseKey(Key.AltLeft);
            });

            AddAssert("Ensure timeline zoom changed", () =>
            {
                changedTimelineZoom = EditorBeatmap.BeatmapInfo.TimelineZoom;
                return !Precision.AlmostEquals(changedTimelineZoom, originalTimelineZoom);
            });

            SaveEditor();

            AddAssert("Beatmap has correct beat divisor", () => EditorBeatmap.BeatmapInfo.BeatDivisor == 16);
            AddAssert("Beatmap has correct timeline zoom", () => EditorBeatmap.BeatmapInfo.TimelineZoom == changedTimelineZoom);

            ReloadEditorToSameBeatmap();

            AddAssert("Beatmap still has correct beat divisor", () => EditorBeatmap.BeatmapInfo.BeatDivisor == 16);
            AddAssert("Beatmap still has correct timeline zoom", () => EditorBeatmap.BeatmapInfo.TimelineZoom == changedTimelineZoom);
        }

        [Test]
        public void TestDifficulty()
        {
            AddStep("Set overall difficulty", () => EditorBeatmap.Difficulty.OverallDifficulty = 7);

            SaveEditor();

            AddAssert("Beatmap has correct overall difficulty", () => EditorBeatmap.Difficulty.OverallDifficulty == 7);

            ReloadEditorToSameBeatmap();

            AddAssert("Beatmap still has correct overall difficulty", () => EditorBeatmap.Difficulty.OverallDifficulty == 7);
        }

        [Test]
        public void TestHitObjectPlacement()
        {
            AddStep("Add timing point", () => EditorBeatmap.ControlPointInfo.Add(500, new TimingControlPoint()));
            AddStep("Change to placement mode", () => InputManager.Key(Key.Number2));
            AddStep("Move to playfield", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre));
            AddStep("Place single hitcircle", () => InputManager.Click(MouseButton.Left));

            SaveEditor();

            AddAssert("Beatmap has correct timing point", () => EditorBeatmap.ControlPointInfo.TimingPoints.Single().Time == 500);

            // After placement these must be non-default as defaults are read-only.
            AddAssert("Placed object has non-default control points", () =>
                EditorBeatmap.HitObjects[0].SampleControlPoint != SampleControlPoint.DEFAULT &&
                EditorBeatmap.HitObjects[0].DifficultyControlPoint != DifficultyControlPoint.DEFAULT);

            ReloadEditorToSameBeatmap();

            AddAssert("Beatmap still has correct timing point", () => EditorBeatmap.ControlPointInfo.TimingPoints.Single().Time == 500);

            // After placement these must be non-default as defaults are read-only.
            AddAssert("Placed object still has non-default control points", () =>
                EditorBeatmap.HitObjects[0].SampleControlPoint != SampleControlPoint.DEFAULT &&
                EditorBeatmap.HitObjects[0].DifficultyControlPoint != DifficultyControlPoint.DEFAULT);
        }

        [Test]
        public void TestExitWithoutSaveFromExistingBeatmap()
        {
            const string tags_to_save = "these tags will be saved";
            const string tags_to_discard = "these tags should be discarded";

            AddStep("Set tags", () => EditorBeatmap.BeatmapInfo.Metadata.Tags = tags_to_save);
            SaveEditor();
            AddAssert("Tags saved correctly", () => EditorBeatmap.BeatmapInfo.Metadata.Tags == tags_to_save);

            ReloadEditorToSameBeatmap();
            AddAssert("Tags saved correctly", () => EditorBeatmap.BeatmapInfo.Metadata.Tags == tags_to_save);
            AddStep("Set tags again", () => EditorBeatmap.BeatmapInfo.Metadata.Tags = tags_to_discard);

            AddStep("Exit editor", () => Editor.Exit());
            AddUntilStep("Wait for song select", () => Game.ScreenStack.CurrentScreen is PlaySongSelect);
            AddAssert("Tags reverted correctly", () => Game.Beatmap.Value.BeatmapInfo.Metadata.Tags == tags_to_save);
        }
    }
}
