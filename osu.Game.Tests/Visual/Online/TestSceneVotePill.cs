// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Overlays.Comments;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Overlays;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public class TestSceneVotePill : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private VotePill votePill;

        [Test]
        public void TestUserCommentPill()
        {
            AddStep("Log in", logIn);
            AddStep("User comment", () => addVotePill(getUserComment()));
            AddStep("Click", () => votePill.Click());
            AddAssert("Not loading", () => !votePill.IsLoading);
        }

        [Test]
        public void TestRandomCommentPill()
        {
            AddStep("Log in", logIn);
            AddStep("Random comment", () => addVotePill(getRandomComment()));
            AddStep("Click", () => votePill.Click());
            AddAssert("Loading", () => votePill.IsLoading);
        }

        [Test]
        public void TestOfflineRandomCommentPill()
        {
            AddStep("Log out", API.Logout);
            AddStep("Random comment", () => addVotePill(getRandomComment()));
            AddStep("Click", () => votePill.Click());
            AddAssert("Not loading", () => !votePill.IsLoading);
        }

        private void logIn() => API.Login("localUser", "password");

        private Comment getUserComment() => new Comment
        {
            IsVoted = false,
            UserId = API.LocalUser.Value.Id,
            VotesCount = 10,
        };

        private Comment getRandomComment() => new Comment
        {
            IsVoted = false,
            UserId = 4444,
            VotesCount = 2,
        };

        private void addVotePill(Comment comment)
        {
            Clear();
            Add(votePill = new VotePill(comment)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            });
        }
    }
}
