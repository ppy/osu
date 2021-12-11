// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Input;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Taiko.Beatmaps;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;
using osu.Game.Tests.Visual;
using osuTK.Input;

namespace osu.Game.Rulesets.Taiko.Tests.Editor
{
    public class TestSceneEditorSaving : OsuGameTestScene
    {
        private Screens.Edit.Editor editor => Game.ChildrenOfType<Screens.Edit.Editor>().FirstOrDefault();

        private EditorBeatmap editorBeatmap => (EditorBeatmap)editor.Dependencies.Get(typeof(EditorBeatmap));

        /// <summary>
        /// Tests the general expected flow of creating a new beatmap, saving it, then loading it back from song select.
        /// Emphasis is placed on <see cref="BeatmapDifficulty.SliderMultiplier"/>, since taiko has special handling for it to keep compatibility with stable.
        /// </summary>
        [Test]
        public void TestNewBeatmapSaveThenLoad()
        {
            AddStep("set default beatmap", () => Game.Beatmap.SetDefault());
            AddStep("set taiko ruleset", () => Ruleset.Value = new TaikoRuleset().RulesetInfo);

            PushAndConfirm(() => new EditorLoader());

            AddUntilStep("wait for editor load", () => editor?.IsLoaded == true);

            AddUntilStep("wait for metadata screen load", () => editor.ChildrenOfType<MetadataSection>().FirstOrDefault()?.IsLoaded == true);

            // We intentionally switch away from the metadata screen, else there is a feedback loop with the textbox handling which causes metadata changes below to get overwritten.

            AddStep("Enter compose mode", () => InputManager.Key(Key.F1));
            AddUntilStep("Wait for compose mode load", () => editor.ChildrenOfType<HitObjectComposer>().FirstOrDefault()?.IsLoaded == true);

            AddStep("Set slider multiplier", () => editorBeatmap.Difficulty.SliderMultiplier = 2);
            AddStep("Set artist and title", () =>
            {
                editorBeatmap.BeatmapInfo.Metadata.Artist = "artist";
                editorBeatmap.BeatmapInfo.Metadata.Title = "title";
            });
            AddStep("Set difficulty name", () => editorBeatmap.BeatmapInfo.DifficultyName = "difficulty");

            checkMutations();

            AddStep("Save", () => InputManager.Keys(PlatformAction.Save));

            checkMutations();

            AddStep("Exit", () => InputManager.Key(Key.Escape));

            AddUntilStep("Wait for main menu", () => Game.ScreenStack.CurrentScreen is MainMenu);

            PushAndConfirm(() => new PlaySongSelect());

            AddUntilStep("Wait for beatmap selected", () => !Game.Beatmap.IsDefault);
            AddStep("Open options", () => InputManager.Key(Key.F3));
            AddStep("Enter editor", () => InputManager.Key(Key.Number5));

            AddUntilStep("Wait for editor load", () => editor != null);

            checkMutations();
        }

        private void checkMutations()
        {
            AddAssert("Beatmap has correct slider multiplier", () =>
            {
                // we can only assert value correctness on TaikoMultiplierAppliedDifficulty, because that is the final difficulty converted taiko beatmaps use.
                // therefore, ensure that we have that difficulty type by calling .CopyFrom(), which is a no-op if the type is already correct.
                var taikoDifficulty = new TaikoBeatmapConverter.TaikoMultiplierAppliedDifficulty();
                taikoDifficulty.CopyFrom(editorBeatmap.Difficulty);
                return Precision.AlmostEquals(taikoDifficulty.SliderMultiplier, 2);
            });
            AddAssert("Beatmap has correct metadata", () => editorBeatmap.BeatmapInfo.Metadata.Artist == "artist" && editorBeatmap.BeatmapInfo.Metadata.Title == "title");
            AddAssert("Beatmap has correct difficulty name", () => editorBeatmap.BeatmapInfo.DifficultyName == "difficulty");
        }
    }
}
