// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.Play;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public abstract partial class QueueModeTestScene : ScreenTestScene
    {
        protected abstract QueueMode Mode { get; }

        protected BeatmapInfo InitialBeatmap { get; private set; }
        protected BeatmapInfo OtherBeatmap { get; private set; }

        protected IScreen CurrentScreen => multiplayerComponents.CurrentScreen;
        protected IScreen CurrentSubScreen => multiplayerComponents.MultiplayerScreen.CurrentSubScreen;

        private BeatmapManager beatmaps;
        private BeatmapSetInfo importedSet;

        private TestMultiplayerComponents multiplayerComponents;

        protected TestMultiplayerClient MultiplayerClient => multiplayerComponents.MultiplayerClient;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                importedSet = beatmaps.GetAllUsableBeatmapSets().First();
                InitialBeatmap = importedSet.Beatmaps.First(b => b.Ruleset.OnlineID == 0);
                OtherBeatmap = importedSet.Beatmaps.Last(b => b.Ruleset.OnlineID == 0);
            });

            AddStep("load multiplayer", () => LoadScreen(multiplayerComponents = new TestMultiplayerComponents()));
            AddUntilStep("wait for multiplayer to load", () => multiplayerComponents.IsLoaded);
            AddUntilStep("wait for lounge to load", () => this.ChildrenOfType<MultiplayerLoungeSubScreen>().FirstOrDefault()?.IsLoaded == true);

            AddUntilStep("wait for lounge", () => multiplayerComponents.ChildrenOfType<LoungeSubScreen>().SingleOrDefault()?.IsLoaded == true);
            AddStep("open room", () => multiplayerComponents.ChildrenOfType<LoungeSubScreen>().Single().Open(new Room
            {
                Name = { Value = "Test Room" },
                QueueMode = { Value = Mode },
                Playlist =
                {
                    new PlaylistItem(InitialBeatmap)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                }
            }));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddWaitStep("wait for transition", 2);

            ClickButtonWhenEnabled<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>();

            AddUntilStep("wait for join", () => MultiplayerClient.RoomJoined);
            AddUntilStep("wait for ongoing operation to complete", () => !(CurrentScreen as OnlinePlayScreen).ChildrenOfType<OngoingOperationTracker>().Single().InProgress.Value);
        }

        [Test]
        public void TestCreatedWithCorrectMode()
        {
            AddUntilStep("room created with correct mode", () => MultiplayerClient.ClientAPIRoom?.QueueMode.Value == Mode);
        }

        protected void RunGameplay()
        {
            AddUntilStep("wait for idle", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Idle);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddUntilStep("wait for ready", () => MultiplayerClient.LocalUser?.State == MultiplayerUserState.Ready);
            ClickButtonWhenEnabled<MultiplayerReadyButton>();

            AddUntilStep("wait for player", () => multiplayerComponents.CurrentScreen is Player player && player.IsLoaded);
            AddStep("exit player", () => multiplayerComponents.MultiplayerScreen.MakeCurrent());
        }
    }
}
