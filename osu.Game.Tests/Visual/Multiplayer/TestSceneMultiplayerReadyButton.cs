// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerReadyButton : MultiplayerTestScene
    {
        private MultiplayerReadyButton button;
        private BeatmapSetInfo importedSet;

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();

        private BeatmapManager beatmaps;
        private RulesetStore rulesets;

        private IDisposable readyClickOperation;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            AvailabilityTracker.SelectedItem.BindTo(selectedItem);

            importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
            Beatmap.Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First());
            selectedItem.Value = new PlaylistItem
            {
                Beatmap = { Value = Beatmap.Value.BeatmapInfo },
                Ruleset = { Value = Beatmap.Value.BeatmapInfo.Ruleset },
            };

            if (button != null)
                Remove(button);

            Add(button = new MultiplayerReadyButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200, 50),
                OnReadyClick = () =>
                {
                    readyClickOperation = OngoingOperationTracker.BeginOperation();

                    Task.Run(async () =>
                    {
                        if (Client.IsHost && Client.LocalUser?.State == MultiplayerUserState.Ready)
                        {
                            await Client.StartMatch();
                            return;
                        }

                        await Client.ToggleReady();

                        readyClickOperation.Dispose();
                    });
                }
            });
        });

        [Test]
        public void TestDeletedBeatmapDisableReady()
        {
            OsuButton readyButton = null;

            AddAssert("ensure ready button enabled", () =>
            {
                readyButton = button.ChildrenOfType<OsuButton>().Single();
                return readyButton.Enabled.Value;
            });

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));
            AddAssert("ready button disabled", () => !readyButton.Enabled.Value);
            AddStep("undelete beatmap", () => beatmaps.Undelete(importedSet));
            AddAssert("ready button enabled back", () => readyButton.Enabled.Value);
        }

        [Test]
        public void TestToggleStateWhenNotHost()
        {
            AddStep("add second user as host", () =>
            {
                Client.AddUser(new APIUser { Id = 2, Username = "Another user" });
                Client.TransferHost(2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);

                if (!allReady)
                    Client.AddUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestBecomeHostWhileReady()
        {
            AddStep("add host", () =>
            {
                Client.AddUser(new APIUser { Id = 2, Username = "Another user" });
                Client.TransferHost(2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddStep("make user host", () => Client.TransferHost(Client.Room?.Users[0].UserID ?? 0));

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestLoseHostWhileReady()
        {
            AddStep("setup", () =>
            {
                Client.TransferHost(Client.Room?.Users[0].UserID ?? 0);
                Client.AddUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);

            AddStep("transfer host", () => Client.TransferHost(Client.Room?.Users[1].UserID ?? 0));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle (match not started)", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
            AddAssert("ready button enabled", () => button.ChildrenOfType<OsuButton>().Single().Enabled.Value);
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
                    Client.AddUser(new APIUser { Id = i, Username = "Another user" });
            });

            if (!isHost)
                AddStep("transfer host", () => Client.TransferHost(2));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

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

        private void verifyGameplayStartFlow()
        {
            AddUntilStep("user is ready", () => Client.Room?.Users[0].State == MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user waiting for load", () => Client.Room?.Users[0].State == MultiplayerUserState.WaitingForLoad);

            AddAssert("ready button disabled", () => !button.ChildrenOfType<OsuButton>().Single().Enabled.Value);
            AddStep("transitioned to gameplay", () => readyClickOperation.Dispose());

            AddStep("finish gameplay", () =>
            {
                Client.ChangeUserState(Client.Room?.Users[0].UserID ?? 0, MultiplayerUserState.Loaded);
                Client.ChangeUserState(Client.Room?.Users[0].UserID ?? 0, MultiplayerUserState.FinishedPlay);
            });

            AddUntilStep("ready button enabled", () => button.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }
    }
}
