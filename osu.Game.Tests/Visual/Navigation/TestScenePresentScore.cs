// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Navigation
{
    public class TestScenePresentScore : OsuGameTestScene
    {
        private BeatmapSetInfo beatmap;

        [SetUpSteps]
        public new void SetUpSteps()
        {
            AddStep("import beatmap", () =>
            {
                var difficulty = new BeatmapDifficulty();
                var metadata = new BeatmapMetadata
                {
                    Artist = "SomeArtist",
                    AuthorString = "SomeAuthor",
                    Title = "import"
                };

                beatmap = Game.BeatmapManager.Import(new BeatmapSetInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    OnlineBeatmapSetID = 1,
                    Metadata = metadata,
                    Beatmaps = new List<BeatmapInfo>
                    {
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = 1 * 1024,
                            Metadata = metadata,
                            BaseDifficulty = difficulty,
                            Ruleset = new OsuRuleset().RulesetInfo
                        },
                        new BeatmapInfo
                        {
                            OnlineBeatmapID = 1 * 2048,
                            Metadata = metadata,
                            BaseDifficulty = difficulty,
                            Ruleset = new OsuRuleset().RulesetInfo
                        },
                    }
                }).Result;
            });
        }

        [Test]
        public void TestFromMainMenu([Values] ScorePresentType type)
        {
            var firstImport = importScore(1);
            var secondimport = importScore(3);

            presentAndConfirm(firstImport, type);
            returnToMenu();
            presentAndConfirm(secondimport, type);
            returnToMenu();
            returnToMenu();
        }

        [Test]
        public void TestFromMainMenuDifferentRuleset([Values] ScorePresentType type)
        {
            var firstImport = importScore(1);
            var secondimport = importScore(3, new ManiaRuleset().RulesetInfo);

            presentAndConfirm(firstImport, type);
            returnToMenu();
            presentAndConfirm(secondimport, type);
            returnToMenu();
            returnToMenu();
        }

        [Test]
        public void TestFromSongSelect([Values] ScorePresentType type)
        {
            var firstImport = importScore(1);
            presentAndConfirm(firstImport, type);

            var secondimport = importScore(3);
            presentAndConfirm(secondimport, type);
        }

        [Test]
        public void TestFromSongSelectDifferentRuleset([Values] ScorePresentType type)
        {
            var firstImport = importScore(1);
            presentAndConfirm(firstImport, type);

            var secondimport = importScore(3, new ManiaRuleset().RulesetInfo);
            presentAndConfirm(secondimport, type);
        }

        private void returnToMenu()
        {
            AddStep("return to menu", () => Game.ScreenStack.CurrentScreen.Exit());
            AddUntilStep("wait for menu", () => Game.ScreenStack.CurrentScreen is MainMenu);
        }

        private Func<ScoreInfo> importScore(int i, RulesetInfo ruleset = null)
        {
            ScoreInfo imported = null;
            AddStep($"import score {i}", () =>
            {
                imported = Game.ScoreManager.Import(new ScoreInfo
                {
                    Hash = Guid.NewGuid().ToString(),
                    OnlineScoreID = i,
                    Beatmap = beatmap.Beatmaps.First(),
                    Ruleset = ruleset ?? new OsuRuleset().RulesetInfo
                }).Result;
            });

            AddAssert($"import {i} succeeded", () => imported != null);

            return () => imported;
        }

        private void presentAndConfirm(Func<ScoreInfo> getImport, ScorePresentType type)
        {
            AddStep("present score", () => Game.PresentScore(getImport(), type));

            switch (type)
            {
                case ScorePresentType.Results:
                    AddUntilStep("wait for results", () => Game.ScreenStack.CurrentScreen is ResultsScreen);
                    AddUntilStep("correct score displayed", () => ((ResultsScreen)Game.ScreenStack.CurrentScreen).Score.ID == getImport().ID);
                    AddAssert("correct ruleset selected", () => Game.Ruleset.Value.ID == getImport().Ruleset.ID);
                    break;

                case ScorePresentType.Gameplay:
                    AddUntilStep("wait for player loader", () => Game.ScreenStack.CurrentScreen is ReplayPlayerLoader);
                    AddUntilStep("correct score displayed", () => ((ReplayPlayerLoader)Game.ScreenStack.CurrentScreen).Score.ID == getImport().ID);
                    AddAssert("correct ruleset selected", () => Game.Ruleset.Value.ID == getImport().Ruleset.ID);
                    break;
            }
        }
    }
}
