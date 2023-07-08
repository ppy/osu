// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Screens;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Spectator;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mania;
using osu.Game.Tests.Visual.Spectator;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneSpectatorHost : PlayerTestScene
    {
        protected override Ruleset CreatePlayerRuleset() => new ManiaRuleset();

        [Cached(typeof(SpectatorClient))]
        private TestSpectatorClient spectatorClient { get; } = new TestSpectatorClient();

        private DummyAPIAccess dummyAPIAccess => (DummyAPIAccess)API;
        private const int dummy_user_id = 42;

        public override void SetUpSteps()
        {
            AddStep("set dummy user", () => dummyAPIAccess.LocalUser.Value = new APIUser
            {
                Id = dummy_user_id,
                Username = "DummyUser"
            });
            AddStep("add test spectator client", () => Add(spectatorClient));
            AddStep("add watching user", () => spectatorClient.WatchUser(dummy_user_id));
            base.SetUpSteps();
        }

        [Test]
        public void TestClientSendsCorrectRuleset()
        {
            AddUntilStep("spectator client sending frames", () => spectatorClient.WatchedUserStates.ContainsKey(dummy_user_id));
            AddAssert("spectator client sent correct ruleset", () => spectatorClient.WatchedUserStates[dummy_user_id].RulesetID == Ruleset.Value.OnlineID);
        }

        [Test]
        public void TestRestart()
        {
            AddAssert("spectator client sees playing state", () => spectatorClient.WatchedUserStates[dummy_user_id].State == SpectatedUserState.Playing);

            AddStep("exit player", () => Player.Exit());
            AddStep("reload player", LoadPlayer);
            AddUntilStep("wait for player load", () => Player.IsLoaded && Player.Alpha == 1);

            AddAssert("spectator client sees playing state", () => spectatorClient.WatchedUserStates[dummy_user_id].State == SpectatedUserState.Playing);

            AddWaitStep("wait", 5);
            AddUntilStep("spectator client still sees playing state", () => spectatorClient.WatchedUserStates[dummy_user_id].State == SpectatedUserState.Playing);
        }

        public override void TearDownSteps()
        {
            base.TearDownSteps();
            AddStep("stop watching user", () => spectatorClient.StopWatchingUser(dummy_user_id));
            AddStep("remove test spectator client", () => Remove(spectatorClient, false));
        }
    }
}
