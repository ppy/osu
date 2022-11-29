// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Platform;
using osu.Framework.Testing;
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
    public partial class TestSceneMultiplayerSpectateButton : MultiplayerTestScene
    {
        private MultiplayerSpectateButton spectateButton;
        private MatchStartControl startControl;

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();

        private BeatmapSetInfo importedSet;
        private BeatmapManager beatmaps;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("create button", () =>
            {
                AvailabilityTracker.SelectedItem.BindTo(selectedItem);

                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
                Beatmap.Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First());
                selectedItem.Value = new PlaylistItem(Beatmap.Value.BeatmapInfo)
                {
                    RulesetID = Beatmap.Value.BeatmapInfo.Ruleset.OnlineID,
                };

                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            spectateButton = new MultiplayerSpectateButton
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(200, 50),
                            },
                            startControl = new MatchStartControl
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Size = new Vector2(200, 50),
                            }
                        }
                    }
                };
            });
        }

        [TestCase(MultiplayerRoomState.Open)]
        [TestCase(MultiplayerRoomState.WaitingForLoad)]
        [TestCase(MultiplayerRoomState.Playing)]
        public void TestEnabledWhenRoomOpenOrInGameplay(MultiplayerRoomState roomState)
        {
            AddStep($"change room to {roomState}", () => MultiplayerClient.ChangeRoomState(roomState));
            assertSpectateButtonEnablement(true);
        }

        [TestCase(MultiplayerUserState.Idle)]
        [TestCase(MultiplayerUserState.Ready)]
        public void TestToggleWhenIdle(MultiplayerUserState initialState)
        {
            ClickButtonWhenEnabled<MultiplayerSpectateButton>();
            AddUntilStep("user is spectating", () => MultiplayerClient.ClientRoom?.Users[0].State == MultiplayerUserState.Spectating);

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();
            AddUntilStep("user is idle", () => MultiplayerClient.ClientRoom?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(MultiplayerRoomState.Closed)]
        public void TestDisabledWhenClosed(MultiplayerRoomState roomState)
        {
            AddStep($"change room to {roomState}", () => MultiplayerClient.ChangeRoomState(roomState));
            assertSpectateButtonEnablement(false);
        }

        [Test]
        public void TestReadyButtonDisabledWhenHostAndNoReadyUsers()
        {
            ClickButtonWhenEnabled<MultiplayerSpectateButton>();
            assertReadyButtonEnablement(false);
        }

        [Test]
        public void TestReadyButtonEnabledWhenHostAndUsersReady()
        {
            AddStep("add user", () => MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID }));
            AddStep("set user ready", () => MultiplayerClient.ChangeUserState(PLAYER_1_ID, MultiplayerUserState.Ready));

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();
            assertReadyButtonEnablement(true);
        }

        [Test]
        public void TestReadyButtonDisabledWhenNotHostAndUsersReady()
        {
            AddStep("add user and transfer host", () =>
            {
                MultiplayerClient.AddUser(new APIUser { Id = PLAYER_1_ID });
                MultiplayerClient.TransferHost(PLAYER_1_ID);
            });

            AddStep("set user ready", () => MultiplayerClient.ChangeUserState(PLAYER_1_ID, MultiplayerUserState.Ready));

            ClickButtonWhenEnabled<MultiplayerSpectateButton>();
            assertReadyButtonEnablement(false);
        }

        private void assertSpectateButtonEnablement(bool shouldBeEnabled)
            => AddUntilStep($"spectate button {(shouldBeEnabled ? "is" : "is not")} enabled", () => spectateButton.ChildrenOfType<OsuButton>().Single().Enabled.Value == shouldBeEnabled);

        private void assertReadyButtonEnablement(bool shouldBeEnabled)
            => AddUntilStep($"ready button {(shouldBeEnabled ? "is" : "is not")} enabled", () => startControl.ChildrenOfType<MultiplayerReadyButton>().Single().Enabled.Value == shouldBeEnabled);
    }
}
