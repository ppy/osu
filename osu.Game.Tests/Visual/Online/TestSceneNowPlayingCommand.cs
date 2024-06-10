// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Osu;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Online
{
    [HeadlessTest]
    public partial class TestSceneNowPlayingCommand : OsuTestScene
    {
        [Cached(typeof(IChannelPostTarget))]
        private PostTarget postTarget { get; set; }

        private DummyAPIAccess api => (DummyAPIAccess)API;

        public TestSceneNowPlayingCommand()
        {
            Add(postTarget = new PostTarget());
        }

        [Test]
        public void TestGenericActivity()
        {
            AddStep("Set activity", () => api.Activity.Value = new UserActivity.InLobby(new Room()));

            AddStep("Run command", () => Add(new NowPlayingCommand(new Channel())));

            AddAssert("Check correct response", () => postTarget.LastMessage.Contains("is listening"));
        }

        [Test]
        public void TestEditActivity()
        {
            AddStep("Set activity", () => api.Activity.Value = new UserActivity.EditingBeatmap(new BeatmapInfo()));

            AddStep("Run command", () => Add(new NowPlayingCommand(new Channel())));

            AddAssert("Check correct response", () => postTarget.LastMessage.Contains("is editing"));
        }

        [Test]
        public void TestPlayActivity()
        {
            AddStep("Set activity", () => api.Activity.Value = new UserActivity.InSoloGame(new BeatmapInfo(), new OsuRuleset().RulesetInfo));

            AddStep("Run command", () => Add(new NowPlayingCommand(new Channel())));

            AddAssert("Check correct response", () => postTarget.LastMessage.Contains("is playing"));
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestLinkPresence(bool hasOnlineId)
        {
            AddStep("Set activity", () => api.Activity.Value = new UserActivity.InLobby(new Room()));

            AddStep("Set beatmap", () => Beatmap.Value = new DummyWorkingBeatmap(Audio, null)
            {
                BeatmapInfo = { OnlineID = hasOnlineId ? 1234 : -1 }
            });

            AddStep("Run command", () => Add(new NowPlayingCommand(new Channel())));

            if (hasOnlineId)
                AddAssert("Check link presence", () => postTarget.LastMessage.Contains("/b/1234"));
            else
                AddAssert("Check link not present", () => !postTarget.LastMessage.Contains("https://"));
        }

        [Test]
        public void TestModPresence()
        {
            AddStep("Set activity", () => api.Activity.Value = new UserActivity.InSoloGame(new BeatmapInfo(), new OsuRuleset().RulesetInfo));

            AddStep("Add Hidden mod", () => SelectedMods.Value = new[] { Ruleset.Value.CreateInstance().CreateMod<ModHidden>() });

            AddStep("Run command", () => Add(new NowPlayingCommand(new Channel())));

            AddAssert("Check mod is present", () => postTarget.LastMessage.Contains("+HD"));
        }

        public partial class PostTarget : Component, IChannelPostTarget
        {
            public void PostMessage(string text, bool isAction = false, Channel target = null)
            {
                LastMessage = text;
            }

            public string LastMessage { get; private set; }
        }
    }
}
