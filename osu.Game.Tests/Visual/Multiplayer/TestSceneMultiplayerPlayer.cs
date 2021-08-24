// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Testing;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerPlayer : MultiplayerTestScene
    {
        private MultiplayerPlayer player;

        [SetUpSteps]
        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("set beatmap", () =>
            {
                Beatmap.Value = CreateWorkingBeatmap(new OsuRuleset().RulesetInfo);
            });

            AddStep("initialise gameplay", () =>
            {
                Stack.Push(player = new MultiplayerPlayer(Client.CurrentMatchPlayingItem.Value, Client.Room?.Users.ToArray()));
            });
        }

        [Test]
        public void TestGameplay()
        {
            AddUntilStep("wait for gameplay start", () => player.LocalUserPlaying.Value);
        }
    }
}
