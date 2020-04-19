// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Chat;
using osu.Game.Rulesets;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [HeadlessTest]
    public class TestSceneNowPlayingCommand : OsuTestScene
    {
        [Cached(typeof(IChannelPostTarget))]
        private PostTarget postTarget { get; set; }

        public TestSceneNowPlayingCommand()
        {
            Add(postTarget = new PostTarget());
        }

        [Test]
        public void TestGenericActivity()
        {
            AddStep("Set activity", () => API.Activity.Value = new UserActivity.InLobby());

            AddStep("Run command", () => Add(new NowPlayingCommand()));

            AddAssert("Check correct response", () => postTarget.LastMessage.Contains("is listening"));
        }

        [Test]
        public void TestEditActivity()
        {
            AddStep("Set activity", () => API.Activity.Value = new UserActivity.Editing(new BeatmapInfo()));

            AddStep("Run command", () => Add(new NowPlayingCommand()));

            AddAssert("Check correct response", () => postTarget.LastMessage.Contains("is editing"));
        }

        [Test]
        public void TestPlayActivity()
        {
            AddStep("Set activity", () => API.Activity.Value = new UserActivity.SoloGame(new BeatmapInfo(), new RulesetInfo()));

            AddStep("Run command", () => Add(new NowPlayingCommand()));

            AddAssert("Check correct response", () => postTarget.LastMessage.Contains("is playing"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestLinkPresence(bool hasOnlineId)
        {
            AddStep("Set activity", () => API.Activity.Value = new UserActivity.InLobby());

            AddStep("Set beatmap", () => Beatmap.Value = new DummyWorkingBeatmap(null, null)
            {
                BeatmapInfo = { OnlineBeatmapID = hasOnlineId ? 1234 : (int?)null }
            });

            AddStep("Run command", () => Add(new NowPlayingCommand()));

            if (hasOnlineId)
                AddAssert("Check link presence", () => postTarget.LastMessage.Contains("https://osu.ppy.sh/b/1234"));
            else
                AddAssert("Check link not present", () => !postTarget.LastMessage.Contains("https://"));
        }

        public class PostTarget : Component, IChannelPostTarget
        {
            public void PostMessage(string text, bool isAction = false, Channel target = null)
            {
                LastMessage = text;
            }

            public string LastMessage { get; private set; }
        }
    }
}
