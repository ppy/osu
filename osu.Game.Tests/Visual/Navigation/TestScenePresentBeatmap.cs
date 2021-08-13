// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Menu;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestScenePresentBeatmap : OsuGameTestScene
    {
        [Test]
        public void TestFromMainMenu()
        {
            var firstImport = importBeatmap(1);
            var secondimport = importBeatmap(3);

            presentAndConfirm(firstImport);
            returnToMenu();
            presentAndConfirm(secondimport);
            returnToMenu();
            presentSecondDifficultyAndConfirm(firstImport, 1);
            returnToMenu();
            presentSecondDifficultyAndConfirm(secondimport, 3);
        }

        [Test]
        public void TestFromMainMenuDifferentRuleset()
        {
            var firstImport = importBeatmap(1);
            var secondimport = importBeatmap(3, new ManiaRuleset().RulesetInfo);

            presentAndConfirm(firstImport);
            returnToMenu();
            presentAndConfirm(secondimport);
            returnToMenu();
            presentSecondDifficultyAndConfirm(firstImport, 1);
            returnToMenu();
            presentSecondDifficultyAndConfirm(secondimport, 3);
        }

        [Test]
        public void TestFromSongSelect()
        {
            var firstImport = importBeatmap(1);
            presentAndConfirm(firstImport);

            var secondimport = importBeatmap(3);
            presentAndConfirm(secondimport);

            // Test presenting same beatmap more than once
            presentAndConfirm(secondimport);

            presentSecondDifficultyAndConfirm(firstImport, 1);
            presentSecondDifficultyAndConfirm(secondimport, 3);

            // Test presenting same beatmap more than once
            presentSecondDifficultyAndConfirm(secondimport, 3);
        }

        [Test]
        public void TestFromSongSelectDifferentRuleset()
        {
            var firstImport = importBeatmap(1);
            presentAndConfirm(firstImport);

            var secondimport = importBeatmap(3, new ManiaRuleset().RulesetInfo);
            presentAndConfirm(secondimport);

            presentSecondDifficultyAndConfirm(firstImport, 1);
            presentSecondDifficultyAndConfirm(secondimport, 3);
        }

        private void returnToMenu()
        {
            // if we don't pause, there's a chance the track may change at the main menu out of our control (due to reaching the end of the track).
            AddStep("pause audio", () =>
            {
                if (Game.MusicController.IsPlaying)
                    Game.MusicController.TogglePause();
            });

            AddStep("return to menu", () => Game.ScreenStack.CurrentScreen.Exit());
            AddUntilStep("wait for menu", () => Game.ScreenStack.CurrentScreen is MainMenu);
        }

        private Func<BeatmapSetInfo> importBeatmap(int i, RulesetInfo ruleset = null)
        {
            BeatmapSetInfo imported = null;
            AddStep($"import beatmap {i}", () =>
            {
                var difficulty = new BeatmapDifficulty();
                var metadata = new BeatmapMetadata
                {
                    Artist = "SomeArtist",
                    AuthorString = "SomeAuthor",
                    Title = $"import {i}"
                };

                imported = Game.BeatmapManager.Import(new BeatmapSetInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    OnlineBeatmapSetID = i,
                    Metadata = metadata,
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = i * 1024,
                            Metadata = metadata,
                            BaseDifficulty = difficulty,
                            Ruleset = ruleset ?? new OsuRuleset().RulesetInfo
                        },
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = i * 2048,
                            Metadata = metadata,
                            BaseDifficulty = difficulty,
                            Ruleset = ruleset ?? new OsuRuleset().RulesetInfo
                        },
                    }
                }).Result;
            });

            AddAssert($"import {i} succeeded", () => imported != null);

            return () => imported;
        }

        private void presentAndConfirm(Func<BeatmapSetInfo> getImport)
        {
            AddStep("present beatmap", () => Game.PresentBeatmap(getImport()));

            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is Screens.Select.SongSelect);
            AddUntilStep("correct beatmap displayed", () => Game.Beatmap.Value.BeatmapSetInfo.ID == getImport().ID);
            AddAssert("correct ruleset selected", () => Game.Ruleset.Value.ID == getImport().Beatmaps.First().Ruleset.ID);
        }

        private void presentSecondDifficultyAndConfirm(Func<BeatmapSetInfo> getImport, int importedID)
        {
            Predicate<BeatmapInfo> pred = b => b.OnlineBeatmapID == importedID * 2048;
            AddStep("present difficulty", () => Game.PresentBeatmap(getImport(), pred));

            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is Screens.Select.SongSelect);
            AddUntilStep("correct beatmap displayed", () => Game.Beatmap.Value.BeatmapInfo.OnlineBeatmapID == importedID * 2048);
            AddAssert("correct ruleset selected", () => Game.Ruleset.Value.ID == getImport().Beatmaps.First().Ruleset.ID);
        }
    }
}
