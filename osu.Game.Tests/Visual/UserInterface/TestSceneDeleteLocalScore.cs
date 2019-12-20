// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Scoring;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Users;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public class TestSceneDeleteLocalScore : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Placeholder),
            typeof(MessagePlaceholder),
            typeof(RetrievalFailurePlaceholder),
            typeof(UserTopScoreContainer),
            typeof(Leaderboard<BeatmapLeaderboardScope, ScoreInfo>),
            typeof(LeaderboardScore),
        };

        private readonly FailableLeaderboard leaderboard;

        private DialogOverlay dialogOverlay;

        public TestSceneDeleteLocalScore()
        {
            Add(dialogOverlay = new DialogOverlay()
            {
                Depth = -1
            });

            leaderboard = new FailableLeaderboard(dialogOverlay)
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Size = new Vector2(550f, 450f),
                Scope = BeatmapLeaderboardScope.Local,
                Beatmap = new BeatmapInfo
                {
                    ID = 1,
                    Metadata = new BeatmapMetadata
                    {
                        ID = 1,
                        Title = "TestSong",
                        Artist = "TestArtist",
                        Author = new User
                        {
                            Username = "TestAuthor"
                        },
                    },
                    Version = "Insane"
                },
            };

            AddStep("Insert Locacl Scores", null);

            TestConfirmDeleteLocalScore();
            TestCancelDeleteLocalScore();
        }

        private void TestConfirmDeleteLocalScore()
        {
            AddStep("Move to leaderboard", () => InputManager.MoveMouseTo(leaderboard));
            AddStep("Show ContextMenu", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("Wait for ContextMenu", () => typeof(OsuContextMenu) == InputManager.FocusedDrawable.GetType() && InputManager.FocusedDrawable.IsLoaded);
            AddStep("Move to Delete Context Menu", () => InputManager.MoveMouseTo(InputManager.FocusedDrawable));
            AddStep("Show Delete Score Dialog", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("Wait for DialogOverlay", () => dialogOverlay.CurrentDialog.IsLoaded);
            AddStep("Move to confirm button", () => InputManager.MoveMouseTo(((TestLocalScoreDeleteDialog)dialogOverlay.CurrentDialog).confirmButton));
            AddStep("Confirm Action", () => InputManager.Click(MouseButton.Left));
            AddAssert("Check Score Count", () => leaderboard.ScoreCount() == 49);
        }

        private void TestCancelDeleteLocalScore()
        {
            AddStep("Move to leaderboard", () => InputManager.MoveMouseTo(leaderboard));
            AddStep("Show ContextMenu", () => InputManager.Click(MouseButton.Right));
            AddUntilStep("Wait for ContextMenu", () => typeof(OsuContextMenu) == InputManager.FocusedDrawable.GetType() && InputManager.FocusedDrawable.IsLoaded);
            AddStep("Move to Delete Context Menu", () => InputManager.MoveMouseTo(InputManager.FocusedDrawable));
            AddStep("Show Delete Score Dialog", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("Wait for DialogOverlay", () => dialogOverlay.CurrentDialog.IsLoaded);
            AddStep("Move to cancel button", () => InputManager.MoveMouseTo(((TestLocalScoreDeleteDialog)dialogOverlay.CurrentDialog).cancelButton));
            AddStep("Cancel Action", () => InputManager.Click(MouseButton.Left));
            AddAssert("Check Score Count", () => leaderboard.ScoreCount() == 49);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Dependencies.Cache(dialogOverlay);
            Add(leaderboard);
        }

        private class FailableLeaderboard : BeatmapLeaderboard
        {
            private DialogOverlay dialogOverlay;

            private List<ScoreInfo> scoreList;

            private Random rnd;

            private bool initialLoad;

            public void DeleteScore(ScoreInfo score)
            {
                scoreList.Remove(score);
                RefreshScores();
            }

            public int ScoreCount()
            {
                return scoreList.Count;
            }

            public FailableLeaderboard(DialogOverlay dialogOverlay)
                : base()
            {
                this.dialogOverlay = dialogOverlay;
                initialLoad = true;
            }

            public void SetRetrievalState(PlaceholderState state)
            {
                PlaceholderState = state;
            }

            protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
            {
                if (initialLoad)
                {
                    rnd = new Random();

                    scoreList = Enumerable.Range(1, 50).Select(createScore).ToList();
                    Scores = scoreList.OrderByDescending(s => s.TotalScore).ToArray();

                    initialLoad = false;
                }
                else
                {
                    Scores = scoreList.OrderByDescending(s => s.TotalScore).ToArray();
                }

                return null;
            }

            private ScoreInfo createScore(int id) => new ScoreInfo
            {
                ID = id,
                Accuracy = rnd.NextDouble(),
                PP = rnd.Next(1, 1000000),
                TotalScore = rnd.Next(1, 1000000),
                MaxCombo = rnd.Next(1, 1000),
                Rank = ScoreRank.XH,
                User = new User { Username = "TestUser" },
            };

            protected override LeaderboardScore CreateDrawableScore(ScoreInfo model, int index)
            {
                model.Beatmap = Beatmap;
                return new TestLeaderboardScore(model, index, dialogOverlay, this, IsOnlineScope);
            }
        }

        private class TestLeaderboardScore : LeaderboardScore
        {
            private DialogOverlay dialogOverlay;

            private FailableLeaderboard leaderboard;

            public TestLeaderboardScore(ScoreInfo score, int rank, DialogOverlay dialogOverlay, FailableLeaderboard leaderboard, bool allowHighlight = true)
                : base(score, rank, allowHighlight)
            {
                this.dialogOverlay = dialogOverlay;
                this.leaderboard = leaderboard;
            }

            protected override void deleteLocalScore(ScoreInfo score)
            {
                dialogOverlay?.Push(new TestLocalScoreDeleteDialog(score, leaderboard));
            }
        }

        private class TestLocalScoreDeleteDialog : PopupDialog
        {
            public PopupDialogOkButton confirmButton;

            public PopupDialogCancelButton cancelButton;

            public TestLocalScoreDeleteDialog(ScoreInfo score, FailableLeaderboard leaderboard)
            {
                Debug.Assert(score != null);

                string accuracy = string.Format(score.Accuracy % 1 == 0 ? @"{0:P0}" : @"{0:P2}", score.Accuracy);

                BodyText = $@"{score} {Environment.NewLine} Rank: {score.Rank} - Max Combo: {score.MaxCombo} - {accuracy}";
                Icon = FontAwesome.Solid.Eraser;
                HeaderText = @"Deleting this local score. Are you sure?";
                Buttons = new PopupDialogButton[]
                {
                    confirmButton = new PopupDialogOkButton
                    {
                        Text = @"Yes. Please.",
                        Action = () => leaderboard.DeleteScore(score)
                    },
                    cancelButton = new PopupDialogCancelButton
                    {
                        Text = @"No, I'm still attached.",
                    },
                };
            }
        }
    }
}
