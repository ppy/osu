// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Catch;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Select;

namespace osu.Game.Tests.Visual.Navigation
{
    public partial class TestScenePresentBeatmap : OsuGameTestScene
    {
        [Test]
        public void TestFromMainMenu()
        {
            var firstImport = importBeatmap(1);
            var secondImport = importBeatmap(3);

            presentAndConfirm(firstImport);
            returnToMenu();
            presentAndConfirm(secondImport);
            returnToMenu();
            presentSecondDifficultyAndConfirm(firstImport, 1);
            returnToMenu();
            presentSecondDifficultyAndConfirm(secondImport, 3);
        }

        [Test]
        public void TestFromMainMenuDifferentRuleset()
        {
            var firstImport = importBeatmap(1);
            var secondImport = importBeatmap(3, new ManiaRuleset().RulesetInfo);

            presentAndConfirm(firstImport);
            returnToMenu();
            presentAndConfirm(secondImport);
            returnToMenu();
            presentSecondDifficultyAndConfirm(firstImport, 1);
            returnToMenu();
            presentSecondDifficultyAndConfirm(secondImport, 3);
        }

        [Test]
        public void TestFromSongSelect()
        {
            var firstImport = importBeatmap(1);
            presentAndConfirm(firstImport);

            var secondImport = importBeatmap(3);
            confirmBeatmapInSongSelect(secondImport);
            presentAndConfirm(secondImport);

            // Test presenting same beatmap more than once
            presentAndConfirm(secondImport);

            presentSecondDifficultyAndConfirm(firstImport, 1);
            presentSecondDifficultyAndConfirm(secondImport, 3);

            // Test presenting same beatmap more than once
            presentSecondDifficultyAndConfirm(secondImport, 3);
        }

        [Test]
        public void TestFromSongSelectDifferentRulesetWithConvertDisallowed()
        {
            AddStep("Set converts disallowed", () => Game.LocalConfig.SetValue(OsuSetting.ShowConvertedBeatmaps, false));

            var osuImport = importBeatmap(1);
            presentAndConfirm(osuImport);

            var maniaImport = importBeatmap(2, new ManiaRuleset().RulesetInfo);
            confirmBeatmapInSongSelect(maniaImport);
            presentAndConfirm(maniaImport);

            var catchImport = importBeatmap(3, new CatchRuleset().RulesetInfo);
            confirmBeatmapInSongSelect(catchImport);
            presentAndConfirm(catchImport);

            // Ruleset is always changed.
            presentSecondDifficultyAndConfirm(maniaImport, 2);
            presentSecondDifficultyAndConfirm(osuImport, 1);
            presentSecondDifficultyAndConfirm(catchImport, 3);
        }

        [Test]
        public void TestFromSongSelectDifferentRulesetWithConvertAllowed()
        {
            AddStep("Set converts allowed", () => Game.LocalConfig.SetValue(OsuSetting.ShowConvertedBeatmaps, true));

            var osuImport = importBeatmap(1);
            presentAndConfirm(osuImport);

            var maniaImport = importBeatmap(2, new ManiaRuleset().RulesetInfo);
            confirmBeatmapInSongSelect(maniaImport);
            presentAndConfirm(maniaImport);

            var catchImport = importBeatmap(3, new CatchRuleset().RulesetInfo);
            confirmBeatmapInSongSelect(catchImport);
            presentAndConfirm(catchImport);

            // force ruleset to osu!mania
            presentSecondDifficultyAndConfirm(maniaImport, 2);

            // ruleset is not changed as we can convert osu! beatmap.
            presentSecondDifficultyAndConfirm(osuImport, 1, expectedRulesetOnlineID: 3);

            // ruleset is changed as we cannot convert.
            presentSecondDifficultyAndConfirm(catchImport, 3);
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
                var metadata = new BeatmapMetadata
                {
                    Artist = "SomeArtist",
                    Author = { Username = "SomeAuthor" },
                    Title = $"import {i}"
                };

                imported = Game.BeatmapManager.Import(new BeatmapSetInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    OnlineID = i * 1024,
                    Beatmaps =
                    {
                        new BeatmapInfo
                        {
                            OnlineID = i * 1024 + 1,
                            Metadata = metadata,
                            Difficulty = new BeatmapDifficulty(),
                            Ruleset = ruleset ?? new OsuRuleset().RulesetInfo
                        },
                        new BeatmapInfo
                        {
                            OnlineID = i * 1024 + 2,
                            Metadata = metadata,
                            Difficulty = new BeatmapDifficulty(),
                            Ruleset = ruleset ?? new OsuRuleset().RulesetInfo
                        },
                    }
                })?.Value;
            });

            AddAssert($"import {i} succeeded", () => imported != null);

            return () => imported;
        }

        private void confirmBeatmapInSongSelect(Func<BeatmapSetInfo> getImport)
        {
            AddUntilStep("beatmap in song select", () =>
            {
                var songSelect = (Screens.Select.SongSelect)Game.ScreenStack.CurrentScreen;
                return songSelect.ChildrenOfType<BeatmapCarousel>().Single().BeatmapSets.Any(b => b.MatchesOnlineID(getImport()));
            });
        }

        private void presentAndConfirm(Func<BeatmapSetInfo> getImport)
        {
            AddStep("present beatmap", () => Game.PresentBeatmap(getImport()));

            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is Screens.Select.SongSelect songSelect && songSelect.IsLoaded);
            AddUntilStep("correct beatmap displayed", () => Game.Beatmap.Value.BeatmapSetInfo.OnlineID, () => Is.EqualTo(getImport().OnlineID));
            AddAssert("correct ruleset selected", () => Game.Ruleset.Value, () => Is.EqualTo(getImport().Beatmaps.First().Ruleset));
        }

        private void presentSecondDifficultyAndConfirm(Func<BeatmapSetInfo> getImport, int importedID, int? expectedRulesetOnlineID = null)
        {
            Predicate<BeatmapInfo> pred = b => b.OnlineID == importedID * 1024 + 2;
            AddStep("present difficulty", () => Game.PresentBeatmap(getImport(), pred));

            AddUntilStep("wait for song select", () => Game.ScreenStack.CurrentScreen is Screens.Select.SongSelect songSelect && songSelect.IsLoaded);
            AddUntilStep("correct beatmap displayed", () => Game.Beatmap.Value.BeatmapInfo.OnlineID, () => Is.EqualTo(importedID * 1024 + 2));
            AddAssert("correct ruleset selected", () => Game.Ruleset.Value.OnlineID, () => Is.EqualTo(expectedRulesetOnlineID ?? getImport().Beatmaps.First().Ruleset.OnlineID));
        }
    }
}
