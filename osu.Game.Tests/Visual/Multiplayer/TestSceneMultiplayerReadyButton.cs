// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Resources;
using osu.Game.Users;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerReadyButton : MultiplayerTestScene
    {
        private MultiplayerReadyButton button;

        private BeatmapManager beatmaps;
        private RulesetStore rulesets;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, host, Beatmap.Default));
            beatmaps.Import(TestResources.GetTestBeatmapForImport(true)).Wait();
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            var beatmap = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First().Beatmaps.First();

            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap);

            Child = button = new MultiplayerReadyButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200, 50),
                SelectedItem =
                {
                    Value = new PlaylistItem
                    {
                        Beatmap = { Value = beatmap },
                        Ruleset = { Value = beatmap.Ruleset }
                    }
                }
            };
        });

        [Test]
        public void TestToggleStateWhenNotHost()
        {
            AddStep("add second user as host", () =>
            {
                Client.AddUser(new User { Id = 2, Username = "Another user" });
                Client.TransferHost(2);
            });

            addClickButtonStep();
            AddAssert("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            addClickButtonStep();
            AddAssert("user is idle", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);

                if (!allReady)
                    Client.AddUser(new User { Id = 2, Username = "Another user" });
            });

            addClickButtonStep();
            AddAssert("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            addClickButtonStep();
            AddAssert("match started", () => Client.Room?.Users[0].State == MultiplayerUserState.WaitingForLoad);
        }

        [Test]
        public void TestBecomeHostWhileReady()
        {
            AddStep("add host", () =>
            {
                Client.AddUser(new User { Id = 2, Username = "Another user" });
                Client.TransferHost(2);
            });

            addClickButtonStep();
            AddStep("make user host", () => Client.TransferHost(Client.Room?.Users[0].UserID ?? 0));

            addClickButtonStep();
            AddAssert("match started", () => Client.Room?.Users[0].State == MultiplayerUserState.WaitingForLoad);
        }

        [Test]
        public void TestLoseHostWhileReady()
        {
            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);
                Client.AddUser(new User { Id = 2, Username = "Another user" });
            });

            addClickButtonStep();
            AddStep("transfer host", () => Client.TransferHost(Client.Room?.Users[1].UserID ?? 0));

            addClickButtonStep();
            AddAssert("match not started", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestManyUsersChangingState(bool isHost)
        {
            const int users = 10;
            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);
                for (int i = 0; i < users; i++)
                    Client.AddUser(new User { Id = i, Username = "Another user" });
            });

            if (!isHost)
                AddStep("transfer host", () => Client.TransferHost(2));

            addClickButtonStep();

            AddRepeatStep("change user ready state", () =>
            {
                Client.ChangeUserState(RNG.Next(0, users), RNG.NextBool() ? MultiplayerUserState.Ready : MultiplayerUserState.Idle);
            }, 20);

            AddRepeatStep("ready all users", () =>
            {
                var nextUnready = Client.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    Client.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
            }, users);
        }

        private void addClickButtonStep() => AddStep("click button", () =>
        {
            InputManager.MoveMouseTo(button);
            InputManager.Click(MouseButton.Left);
        });
    }
}
