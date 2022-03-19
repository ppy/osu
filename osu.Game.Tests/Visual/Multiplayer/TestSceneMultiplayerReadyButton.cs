// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
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

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, rulesets, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            AvailabilityTracker.SelectedItem.BindTo(selectedItem);

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
            importedSet = beatmaps.GetAllUsableBeatmapSets().First();
            Beatmap.Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First());

            selectedItem.Value = new PlaylistItem(Beatmap.Value.BeatmapInfo)
            {
                RulesetID = Beatmap.Value.BeatmapInfo.Ruleset.OnlineID
            };

            if (button != null)
                Remove(button);

            Add(button = new MultiplayerReadyButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Size = new Vector2(200, 50),
            });
        });

        [Test]
        public void TestDeletedBeatmapDisableReady()
        {
            OsuButton readyButton = null;

            AddUntilStep("ensure ready button enabled", () =>
            {
                readyButton = button.ChildrenOfType<OsuButton>().Single();
                return readyButton.Enabled.Value;
            });

            AddStep("delete beatmap", () => beatmaps.Delete(importedSet));
            AddUntilStep("ready button disabled", () => !readyButton.Enabled.Value);
            AddStep("undelete beatmap", () => beatmaps.Undelete(importedSet));
            AddUntilStep("ready button enabled back", () => readyButton.Enabled.Value);
        }

        [Test]
        public void TestToggleStateWhenNotHost()
        {
            AddStep("add second user as host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestToggleStateWhenHost(bool allReady)
        {
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);

                if (!allReady)
                    MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestBecomeHostWhileReady()
        {
            AddStep("add host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
                MultiplayerClient.TransferHost(2);
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddStep("make user host", () => MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0));

            verifyGameplayStartFlow();
        }

        [Test]
        public void TestLoseHostWhileReady()
        {
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);
                MultiplayerClient.AddUser(new APIUser { Id = 2, Username = "Another user" });
            });

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);

            AddStep("transfer host", () => MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[1].UserID ?? 0));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user is idle (match not started)", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Idle);
            AddUntilStep("ready button enabled", () => button.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestManyUsersChangingState(bool isHost)
        {
            const int users = 10;
            AddStep("setup", () =>
            {
                MultiplayerClient.TransferHost(MultiplayerClient.Room?.Users[0].UserID ?? 0);
                for (int i = 0; i < users; i++)
                    MultiplayerClient.AddUser(new APIUser { Id = i, Username = "Another user" });
            });

            if (!isHost)
                AddStep("transfer host", () => MultiplayerClient.TransferHost(2));

            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddRepeatStep("change user ready state", () =>
            {
                MultiplayerClient.ChangeUserState(RNG.Next(0, users), RNG.NextBool() ? MultiplayerUserState.Ready : MultiplayerUserState.Idle);
            }, 20);

            AddRepeatStep("ready all users", () =>
            {
                var nextUnready = MultiplayerClient.Room?.Users.FirstOrDefault(c => c.State == MultiplayerUserState.Idle);
                if (nextUnready != null)
                    MultiplayerClient.ChangeUserState(nextUnready.UserID, MultiplayerUserState.Ready);
            }, users);
        }

        private void verifyGameplayStartFlow()
        {
            AddUntilStep("user is ready", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();
            AddUntilStep("user waiting for load", () => MultiplayerClient.Room?.Users[0].State == MultiplayerUserState.WaitingForLoad);

            AddStep("finish gameplay", () =>
            {
                MultiplayerClient.ChangeUserState(MultiplayerClient.Room?.Users[0].UserID ?? 0, MultiplayerUserState.Loaded);
                MultiplayerClient.ChangeUserState(MultiplayerClient.Room?.Users[0].UserID ?? 0, MultiplayerUserState.FinishedPlay);
            });

            AddUntilStep("ready button enabled", () => button.ChildrenOfType<OsuButton>().Single().Enabled.Value);
        }
    }
}
