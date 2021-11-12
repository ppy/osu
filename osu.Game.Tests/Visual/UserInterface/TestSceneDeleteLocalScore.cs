// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneDeleteLocalScore : OsuManualInputManagerTestScene
    {
        private readonly ContextMenuContainer contextMenuContainer;
        private readonly BeatmapLeaderboard leaderboard;

        private RulesetStore rulesetStore;
        private BeatmapManager beatmapManager;
        private ScoreManager scoreManager;

        private readonly List<ScoreInfo> importedScores = new List<ScoreInfo>();

        private BeatmapInfo beatmapInfo;

        [Cached]
        private readonly DialogOverlay dialogOverlay;

        public TestSceneDeleteLocalScore()
        {
            Children = new Drawable[]
            {
                contextMenuContainer = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = leaderboard = new BeatmapLeaderboard
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        Size = new Vector2(550f, 450f),
                        Scope = BeatmapLeaderboardScope.Local,
                        BeatmapInfo = new BeatmapInfo
                        {
                            ID = 1,
                            Metadata = new BeatmapMetadata
                            {
                                ID = 1,
                                Title = "TestSong",
                                Artist = "TestArtist",
                                Author = new APIUser
                                {
                                    Username = "TestAuthor"
                                },
                            },
                            DifficultyName = "Insane"
                        },
                    }
                },
                dialogOverlay = new DialogOverlay()
            };
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.Cache(rulesetStore = new RulesetStore(ContextFactory));
            dependencies.Cache(beatmapManager = new BeatmapManager(LocalStorage, ContextFactory, rulesetStore, null, dependencies.Get<AudioManager>(), Resources, dependencies.Get<GameHost>(), Beatmap.Default));
            dependencies.Cache(scoreManager = new ScoreManager(rulesetStore, () => beatmapManager, LocalStorage, null, ContextFactory, Scheduler));

            beatmapInfo = beatmapManager.Import(new ImportTask(TestResources.GetQuickTestBeatmapForImport())).Result.Value.Beatmaps[0];

            for (int i = 0; i < 50; i++)
            {
                var score = new ScoreInfo
                {
                    OnlineScoreID = i,
                    BeatmapInfo = beatmapInfo,
                    BeatmapInfoID = beatmapInfo.ID,
                    Accuracy = RNG.NextDouble(),
                    TotalScore = RNG.Next(1, 1000000),
                    MaxCombo = RNG.Next(1, 1000),
                    Rank = ScoreRank.XH,
                    User = new APIUser { Username = "TestUser" },
                };

                importedScores.Add(scoreManager.Import(score).Result.Value);
            }

            return dependencies;
        }

        [SetUp]
        public void Setup() => Schedule(() =>
        {
            // Due to soft deletions, we can re-use deleted scores between test runs
            scoreManager.Undelete(scoreManager.QueryScores(s => s.DeletePending).ToList());

            leaderboard.Scores = null;
            leaderboard.FinishTransforms(true); // After setting scores, we may be waiting for transforms to expire drawables

            leaderboard.BeatmapInfo = beatmapInfo;
            leaderboard.RefreshScores(); // Required in the case that the beatmap hasn't changed
        });

        [SetUpSteps]
        public void SetupSteps()
        {
            // Ensure the leaderboard has finished async-loading drawables
            AddUntilStep("wait for drawables", () => leaderboard.ChildrenOfType<LeaderboardScore>().Any());

            // Ensure the leaderboard items have finished showing up
            AddStep("finish transforms", () => leaderboard.FinishTransforms(true));
        }

        [Test]
        public void TestDeleteViaRightClick()
        {
            ScoreInfo scoreBeingDeleted = null;
            AddStep("open menu for top score", () =>
            {
                var leaderboardScore = leaderboard.ChildrenOfType<LeaderboardScore>().First();

                scoreBeingDeleted = leaderboardScore.Score;

                InputManager.MoveMouseTo(leaderboardScore);
                InputManager.Click(MouseButton.Right);
            });

            // Ensure the context menu has finished showing
            AddStep("finish transforms", () => contextMenuContainer.FinishTransforms(true));

            AddStep("click delete option", () =>
            {
                InputManager.MoveMouseTo(contextMenuContainer.ChildrenOfType<DrawableOsuMenuItem>().First(i => i.Item.Text.Value.ToString().ToLowerInvariant() == "delete"));
                InputManager.Click(MouseButton.Left);
            });

            // Ensure the dialog has finished showing
            AddStep("finish transforms", () => dialogOverlay.FinishTransforms(true));

            AddStep("click delete button", () =>
            {
                InputManager.MoveMouseTo(dialogOverlay.ChildrenOfType<DialogButton>().First());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for fetch", () => leaderboard.Scores != null);
            AddUntilStep("score removed from leaderboard", () => leaderboard.Scores.All(s => s.OnlineScoreID != scoreBeingDeleted.OnlineScoreID));
        }

        [Test]
        public void TestDeleteViaDatabase()
        {
            AddStep("delete top score", () => scoreManager.Delete(importedScores[0]));
            AddUntilStep("wait for fetch", () => leaderboard.Scores != null);
            AddUntilStep("score removed from leaderboard", () => leaderboard.Scores.All(s => s.OnlineScoreID != importedScores[0].OnlineScoreID));
        }
    }
}
