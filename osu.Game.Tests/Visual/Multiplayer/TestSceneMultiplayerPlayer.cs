// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.Play;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public partial class TestSceneMultiplayerPlayer : MultiplayerTestScene
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

            AddStep("Start track playing", () =>
            {
                Beatmap.Value.Track.Start();
            });

            AddStep("initialise gameplay", () =>
            {
                Stack.Push(player = new MultiplayerPlayer(MultiplayerClient.ServerAPIRoom, new PlaylistItem(Beatmap.Value.BeatmapInfo)
                {
                    RulesetID = Beatmap.Value.BeatmapInfo.Ruleset.OnlineID,
                }, MultiplayerClient.ServerRoom?.Users.ToArray()));
            });

            AddUntilStep("wait for player to be current", () => player.IsCurrentScreen() && player.IsLoaded);

            AddAssert("gameplay clock is paused", () => player.ChildrenOfType<GameplayClockContainer>().Single().IsPaused.Value);
            AddAssert("gameplay clock is not running", () => !player.ChildrenOfType<GameplayClockContainer>().Single().IsRunning);

            AddStep("start gameplay", () => ((IMultiplayerClient)MultiplayerClient).GameplayStarted());

            AddUntilStep("gameplay clock is not paused", () => !player.ChildrenOfType<GameplayClockContainer>().Single().IsPaused.Value);
            AddAssert("gameplay clock is running", () => player.ChildrenOfType<GameplayClockContainer>().Single().IsRunning);
        }

        [Test]
        public void TestGameplay()
        {
            AddUntilStep("wait for gameplay start", () => player.LocalUserPlaying.Value);
        }
    }
}
