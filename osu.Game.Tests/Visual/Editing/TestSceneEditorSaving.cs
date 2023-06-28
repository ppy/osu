// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Overlays;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components.Timeline;
using osu.Game.Screens.Select;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Editing
{
    public partial class TestSceneEditorSaving : EditorSavingTestScene
    {
        [Test]
        public void TestCantExitWithoutSaving()
        {
            AddUntilStep("Wait for dialog overlay load", () => ((Drawable)Game.Dependencies.Get<IDialogOverlay>()).IsLoaded);
            AddRepeatStep("Exit", () => InputManager.Key(Key.Escape), 10);
            AddAssert("Sample playback disabled", () => Editor.SamplePlaybackDisabled.Value);
            AddAssert("Editor is still active screen", () => Game.ScreenStack.CurrentScreen is Editor);
        }

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

            AddAssert("Hash updated", () => !string.IsNullOrEmpty(EditorBeatmap.BeatmapInfo.BeatmapSet?.Hash));

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

            AddUntilStep("wait for timeline load", () => Editor.ChildrenOfType<Timeline>().SingleOrDefault()?.IsLoaded == true);

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

            ReloadEditorToSameBeatmap();

            AddAssert("Beatmap still has correct timing point", () => EditorBeatmap.ControlPointInfo.TimingPoints.Single().Time == 500);
        }

        [Test]
        public void TestLengthAndStarRatingUpdated()
        {
            WorkingBeatmap working = null;
            double lastStarRating = 0;
            double lastLength = 0;

            AddStep("Add timing point", () => EditorBeatmap.ControlPointInfo.Add(200, new TimingControlPoint { BeatLength = 600 }));
            AddStep("Change to placement mode", () => InputManager.Key(Key.Number2));
            AddStep("Move to playfield", () => InputManager.MoveMouseTo(Game.ScreenSpaceDrawQuad.Centre));
            AddStep("Place single hitcircle", () => InputManager.Click(MouseButton.Left));
            AddAssert("One hitobject placed", () => EditorBeatmap.HitObjects.Count == 1);

            SaveEditor();
            AddStep("Get working beatmap", () => working = Game.BeatmapManager.GetWorkingBeatmap(EditorBeatmap.BeatmapInfo, true));

            AddAssert("Beatmap length is zero", () => working.BeatmapInfo.Length == 0);
            checkDifficultyIncreased();

            AddStep("Move forward", () => InputManager.Key(Key.Right));
            AddStep("Place another hitcircle", () => InputManager.Click(MouseButton.Left));
            AddAssert("Two hitobjects placed", () => EditorBeatmap.HitObjects.Count == 2);

            SaveEditor();
            AddStep("Get working beatmap", () => working = Game.BeatmapManager.GetWorkingBeatmap(EditorBeatmap.BeatmapInfo, true));

            checkDifficultyIncreased();
            checkLengthIncreased();

            void checkLengthIncreased()
            {
                AddStep("Beatmap length increased", () =>
                {
                    Assert.That(working.BeatmapInfo.Length, Is.GreaterThan(lastLength));
                    lastLength = working.BeatmapInfo.Length;
                });
            }

            void checkDifficultyIncreased()
            {
                AddStep("Beatmap difficulty increased", () =>
                {
                    Assert.That(working.BeatmapInfo.StarRating, Is.GreaterThan(lastStarRating));
                    lastStarRating = working.BeatmapInfo.StarRating;
                });
            }
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
