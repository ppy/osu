// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
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

        protected BeatmapInfo InitialBeatmap { get; private set; } = null!;
        protected BeatmapInfo OtherBeatmap { get; private set; } = null!;

        protected IScreen CurrentScreen => multiplayerComponents.CurrentScreen;
        protected IScreen CurrentSubScreen => multiplayerComponents.MultiplayerScreen.CurrentSubScreen;

        private BeatmapManager beatmaps = null!;
        private BeatmapSetInfo importedSet = null!;
        private RulesetStore rulesets = null!;

        private TestMultiplayerComponents multiplayerComponents = null!;

        protected TestMultiplayerClient MultiplayerClient => multiplayerComponents.MultiplayerClient;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            BeatmapStore beatmapStore;

            Dependencies.Cache(rulesets = new RealmRulesetStore(Realm));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.CacheAs(beatmapStore = new RealmDetachedBeatmapStore());
            Dependencies.Cache(Realm);

            Add(beatmapStore);
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).WaitSafely();
                Realm.Write(r =>
                {
                    foreach (var beatmapInfo in r.All<BeatmapInfo>())
                        beatmapInfo.OnlineMD5Hash = beatmapInfo.MD5Hash;
                });
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
                Name = "Test Room",
                QueueMode = Mode,
                Playlist =
                [
                    new PlaylistItem(InitialBeatmap)
                    {
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID
                    }
                ]
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
            AddUntilStep("room created with correct mode", () => MultiplayerClient.ClientAPIRoom?.QueueMode == Mode);
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

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (rulesets.IsNotNull())
                rulesets.Dispose();
        }
    }
}
