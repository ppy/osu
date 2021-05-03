// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Graphics.UserInterface;
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
    public class TestSceneMultiplayerSpectateButton : MultiplayerTestScene
    {
        private MultiplayerSpectateButton spectateButton;
        private MultiplayerReadyButton readyButton;

        private readonly Bindable<PlaylistItem> selectedItem = new Bindable<PlaylistItem>();

        private BeatmapSetInfo importedSet;
        private BeatmapManager beatmaps;
        private RulesetStore rulesets;

        private IDisposable readyClickOperation;

        protected override Container<Drawable> Content => content;
        private readonly Container content;

        public TestSceneMultiplayerSpectateButton()
        {
            base.Content.Add(content = new Container
            {
                RelativeSizeAxes = Axes.Both
            });
        }

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            var dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            return dependencies;
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, host, Beatmap.Default));

            var beatmapTracker = new OnlinePlayBeatmapAvailabilityTracker { SelectedItem = { BindTarget = selectedItem } };
            base.Content.Add(beatmapTracker);
            Dependencies.Cache(beatmapTracker);

            beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
            Beatmap.Value = beatmaps.GetWorkingBeatmap(importedSet.Beatmaps.First());
            selectedItem.Value = new PlaylistItem
            {
                Beatmap = { Value = Beatmap.Value.BeatmapInfo },
                Ruleset = { Value = Beatmap.Value.BeatmapInfo.Ruleset },
            };

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
                        OnSpectateClick = async () =>
                        {
                            readyClickOperation = OngoingOperationTracker.BeginOperation();
                            await Client.ToggleSpectate();
                            readyClickOperation.Dispose();
                        }
                    },
                    readyButton = new MultiplayerReadyButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(200, 50),
                        OnReadyClick = async () =>
                        {
                            readyClickOperation = OngoingOperationTracker.BeginOperation();

                            if (Client.IsHost && Client.LocalUser?.State == MultiplayerUserState.Ready)
                            {
                                await Client.StartMatch();
                                return;
                            }

                            await Client.ToggleReady();
                            readyClickOperation.Dispose();
                        }
                    }
                }
            };
        });

        [Test]
        public void TestEnabledWhenRoomOpen()
        {
            assertSpectateButtonEnablement(true);
        }

        [TestCase(MultiplayerUserState.Idle)]
        [TestCase(MultiplayerUserState.Ready)]
        public void TestToggleWhenIdle(MultiplayerUserState initialState)
        {
            addClickSpectateButtonStep();
            AddAssert("user is spectating", () => Client.Room?.Users[0].State == MultiplayerUserState.Spectating);

            addClickSpectateButtonStep();
            AddAssert("user is idle", () => Client.Room?.Users[0].State == MultiplayerUserState.Idle);
        }

        [TestCase(MultiplayerRoomState.WaitingForLoad)]
        [TestCase(MultiplayerRoomState.Playing)]
        [TestCase(MultiplayerRoomState.Closed)]
        public void TestDisabledDuringGameplayOrClosed(MultiplayerRoomState roomState)
        {
            AddStep($"change user to {roomState}", () => Client.ChangeRoomState(roomState));
            assertSpectateButtonEnablement(false);
        }

        [Test]
        public void TestReadyButtonDisabledWhenHostAndNoReadyUsers()
        {
            addClickSpectateButtonStep();
            assertReadyButtonEnablement(false);
        }

        [Test]
        public void TestReadyButtonEnabledWhenHostAndUsersReady()
        {
            AddStep("add user", () => Client.AddUser(new User { Id = 55 }));
            AddStep("set user ready", () => Client.ChangeUserState(55, MultiplayerUserState.Ready));

            addClickSpectateButtonStep();
            assertReadyButtonEnablement(true);
        }

        [Test]
        public void TestReadyButtonDisabledWhenNotHostAndUsersReady()
        {
            AddStep("add user and transfer host", () =>
            {
                Client.AddUser(new User { Id = 55 });
                Client.TransferHost(55);
            });

            AddStep("set user ready", () => Client.ChangeUserState(55, MultiplayerUserState.Ready));

            addClickSpectateButtonStep();
            assertReadyButtonEnablement(false);
        }

        private void addClickSpectateButtonStep() => AddStep("click spectate button", () =>
        {
            InputManager.MoveMouseTo(spectateButton);
            InputManager.Click(MouseButton.Left);
        });

        private void assertSpectateButtonEnablement(bool shouldBeEnabled)
            => AddAssert($"spectate button {(shouldBeEnabled ? "is" : "is not")} enabled", () => spectateButton.ChildrenOfType<OsuButton>().Single().Enabled.Value == shouldBeEnabled);

        private void assertReadyButtonEnablement(bool shouldBeEnabled)
            => AddAssert($"ready button {(shouldBeEnabled ? "is" : "is not")} enabled", () => readyButton.ChildrenOfType<OsuButton>().Single().Enabled.Value == shouldBeEnabled);
    }
}
