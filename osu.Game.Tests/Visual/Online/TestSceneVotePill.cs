// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Game.Overlays.Comments;
using osu.Game.Online.API.Requests.Responses;
using osu.Framework.Allocation;
using osu.Game.Overlays;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Online.API;

namespace osu.Game.Tests.Visual.Online
{
    [TestFixture]
    public partial class TestSceneVotePill : OsuTestScene
    {
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Cached]
        private LoginOverlay login;

        private TestPill votePill;
        private readonly Container pillContainer;

        public TestSceneVotePill()
        {
            AddRange(new Drawable[]
            {
                pillContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both
                },
                login = new LoginOverlay()
            });
        }

        [Test]
        public void TestUserCommentPill()
        {
            AddStep("Hide login overlay", () => login.Hide());
            AddStep("Log in", logIn);
            AddStep("User comment", () => addVotePill(getUserComment()));
            AddAssert("Background is transparent", () => votePill.Background.Alpha == 0);
            AddStep("Click", () => votePill.TriggerClick());
            AddAssert("Not loading", () => !votePill.IsLoading);
        }

        [Test]
        public void TestRandomCommentPill()
        {
            AddStep("Hide login overlay", () => login.Hide());
            AddStep("Log in", logIn);
            AddStep("Random comment", () => addVotePill(getRandomComment()));
            AddAssert("Background is visible", () => votePill.Background.Alpha == 1);
            AddStep("Click", () => votePill.TriggerClick());
            AddAssert("Loading", () => votePill.IsLoading);
        }

        [Test]
        public void TestOfflineRandomCommentPill()
        {
            AddStep("Hide login overlay", () => login.Hide());
            AddStep("Log out", API.Logout);
            AddStep("Random comment", () => addVotePill(getRandomComment()));
            AddStep("Click", () => votePill.TriggerClick());
            AddAssert("Login overlay is visible", () => login.State.Value == Visibility.Visible);
        }

        private void logIn()
        {
            API.Login("localUser", "password");
            ((DummyAPIAccess)API).AuthenticateSecondFactor("abcdefgh");
        }

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
            pillContainer.Clear();
            pillContainer.Child = votePill = new TestPill(comment)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
        }

        private partial class TestPill : VotePill
        {
            public new Box Background => base.Background;

            public TestPill(Comment comment)
                : base(comment)
            {
            }
        }
    }
}
