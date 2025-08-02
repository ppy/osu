// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Select.Leaderboards;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneGameplayLeaderboard : OsuTestScene
    {
        private TestDrawableGameplayLeaderboard leaderboard = null!;

        [Cached(typeof(IGameplayLeaderboardProvider))]
        private TestGameplayLeaderboardProvider leaderboardProvider = new TestGameplayLeaderboardProvider();

        private readonly BindableLong playerScore = new BindableLong();

        public TestSceneGameplayLeaderboard()
        {
            AddStep("toggle expanded", () =>
            {
                if (leaderboard.IsNotNull())
                    leaderboard.CollapseDuringGameplay.Value = !leaderboard.CollapseDuringGameplay.Value;
            });

            AddSliderStep("set player score", 50, 5000000, 1222333, v => playerScore.Value = v);
        }

        [Test]
        public void TestLayoutWithManyScores()
        {
            createLeaderboard();

            AddStep("add many scores in one go", () =>
            {
                for (int i = 0; i < 32; i++)
                    leaderboardProvider.CreateRandomScore(new APIUser { Username = $"Player {i + 1}" });

                // Add player at end to force an animation down the whole list.
                playerScore.Value = 0;
                leaderboardProvider.CreateLeaderboardScore(playerScore, new APIUser { Username = "You", Id = 3 }, true);
            });

            // Gameplay leaderboard has custom scroll logic, which when coupled with LayoutDuration
            // has caused layout to not work in the past.

            AddUntilStep("wait for fill flow layout",
                () => leaderboard.ChildrenOfType<FillFlowContainer<DrawableGameplayLeaderboardScore>>().First().ScreenSpaceDrawQuad.Intersects(leaderboard.ScreenSpaceDrawQuad));

            AddUntilStep("wait for some scores not masked away",
                () => leaderboard.ChildrenOfType<DrawableGameplayLeaderboardScore>().Any(s => leaderboard.ScreenSpaceDrawQuad.Contains(s.ScreenSpaceDrawQuad.Centre)));

            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));

            AddStep("change score to middle", () => playerScore.Value = 1000000);
            AddWaitStep("wait for movement", 5);
            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));

            AddStep("change score to first", () => playerScore.Value = 5000000);
            AddWaitStep("wait for movement", 5);
            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));
        }

        [Test]
        public void TestRandomScores()
        {
            createLeaderboard();
            addLocalPlayer();

            int playerNumber = 1;
            AddRepeatStep("add player with random score", () => leaderboardProvider.CreateRandomScore(new APIUser { Username = $"Player {playerNumber++}" }), 10);
        }

        [Test]
        public void TestExistingUsers()
        {
            createLeaderboard();
            addLocalPlayer();

            AddStep("add peppy", () => leaderboardProvider.CreateRandomScore(new APIUser { Username = "peppy", Id = 2 }));
            AddStep("add smoogipoo", () => leaderboardProvider.CreateRandomScore(new APIUser { Username = "smoogipoo", Id = 1040328 }));
            AddStep("add flyte", () => leaderboardProvider.CreateRandomScore(new APIUser { Username = "flyte", Id = 3103765 }));
            AddStep("add frenzibyte", () => leaderboardProvider.CreateRandomScore(new APIUser { Username = "frenzibyte", Id = 14210502 }));
        }

        [Test]
        public void TestFriendScore()
        {
            APIUser friend = new APIUser { Username = "my friend", Id = 10000 };

            createLeaderboard();
            addLocalPlayer();

            AddStep("Add friend to API", () =>
            {
                var api = (DummyAPIAccess)API;

                api.Friends.Clear();
                api.Friends.Add(new APIRelation
                {
                    Mutual = true,
                    RelationType = RelationType.Friend,
                    TargetID = friend.OnlineID,
                    TargetUser = friend
                });
            });

            int playerNumber = 1;

            AddRepeatStep("add 3 other players", () => leaderboardProvider.CreateRandomScore(new APIUser { Username = $"Player {playerNumber++}" }), 3);
            AddUntilStep("no pink color scores",
                () => leaderboard.ChildrenOfType<Box>().Select(b => ((Colour4)b.Colour).ToHex()),
                () => Does.Not.Contain("#FF549A"));

            AddRepeatStep("add 3 friend score", () => leaderboardProvider.CreateRandomScore(friend), 3);
            AddUntilStep("at least one friend score is pink",
                () => leaderboard.GetAllScoresForUsername("my friend")
                                 .SelectMany(score => score.ChildrenOfType<Box>())
                                 .Select(b => ((Colour4)b.Colour).ToHex()),
                () => Does.Contain("#FF549A"));
        }

        private void addLocalPlayer()
        {
            AddStep("add local player", () =>
            {
                playerScore.Value = 1222333;
                leaderboardProvider.CreateLeaderboardScore(playerScore, new APIUser { Username = "You", Id = 3 }, true);
            });
        }

        private void createLeaderboard()
        {
            AddStep("create leaderboard", () =>
            {
                leaderboardProvider.Scores.Clear();
                Child = leaderboard = new TestDrawableGameplayLeaderboard
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(2),
                };
            });
        }

        private partial class TestDrawableGameplayLeaderboard : DrawableGameplayLeaderboard
        {
            public float Spacing => Flow.Spacing.Y;

            public IEnumerable<DrawableGameplayLeaderboardScore> GetAllScoresForUsername(string username)
                => Flow.Where(i => i.User?.Username == username);
        }

        public class TestGameplayLeaderboardProvider : IGameplayLeaderboardProvider
        {
            public BindableList<GameplayLeaderboardScore> Scores { get; } = new BindableList<GameplayLeaderboardScore>();

            public GameplayLeaderboardScore CreateRandomScore(APIUser user) => CreateLeaderboardScore(new BindableLong(RNG.Next(0, 5_000_000)), user);

            public GameplayLeaderboardScore CreateLeaderboardScore(BindableLong totalScore, APIUser user, bool isTracked = false)
            {
                var score = new GameplayLeaderboardScore(user, isTracked, totalScore);
                Scores.Add(score);
                return score;
            }

            IBindableList<GameplayLeaderboardScore> IGameplayLeaderboardProvider.Scores => Scores;
        }
    }
}
