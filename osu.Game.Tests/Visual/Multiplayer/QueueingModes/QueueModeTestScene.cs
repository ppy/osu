// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Multiplayer.Queueing;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osu.Game.Screens.Play;
using osu.Game.Tests.Resources;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer.QueueingModes
{
    public abstract class QueueModeTestScene : ScreenTestScene
    {
        protected abstract QueueModes Mode { get; }

        protected BeatmapInfo InitialBeatmap { get; private set; }
        protected BeatmapInfo OtherBeatmap { get; private set; }

        protected IScreen CurrentScreen => multiplayerScreenStack.CurrentScreen;
        protected IScreen CurrentSubScreen => multiplayerScreenStack.MultiplayerScreen.CurrentSubScreen;

        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;

        private TestMultiplayerScreenStack multiplayerScreenStack;

        protected TestMultiplayerClient Client => multiplayerScreenStack.Client;
        protected TestMultiplayerRoomManager RoomManager => multiplayerScreenStack.RoomManager;

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        public override void SetUpSteps()
        {
            base.SetUpSteps();

            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
                InitialBeatmap = importedSet.Beatmaps.First(b => b.RulesetID == 0);
                OtherBeatmap = importedSet.Beatmaps.Last(b => b.RulesetID == 0);
            });

            AddStep("load multiplayer", () => LoadScreen(multiplayerScreenStack = new TestMultiplayerScreenStack()));
            AddUntilStep("wait for multiplayer to load", () => multiplayerScreenStack.IsLoaded);
            AddUntilStep("wait for lounge to load", () => this.ChildrenOfType<MultiplayerLoungeSubScreen>().FirstOrDefault()?.IsLoaded == true);

            AddUntilStep("wait for lounge", () => multiplayerScreenStack.ChildrenOfType<LoungeSubScreen>().SingleOrDefault()?.IsLoaded == true);
            AddStep("open room", () => multiplayerScreenStack.ChildrenOfType<LoungeSubScreen>().Single().Open(new Room
            {
                Name = { Value = "Test Room" },
                QueueMode = { Value = Mode },
                Playlist =
                {
                    new PlaylistItem
                    {
                        Beatmap = { Value = InitialBeatmap },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                    }
                }
            }));

            AddUntilStep("wait for room open", () => this.ChildrenOfType<MultiplayerMatchSubScreen>().FirstOrDefault()?.IsLoaded == true);
            AddWaitStep("wait for transition", 2);

            AddStep("create room", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerMatchSettingsOverlay.CreateOrUpdateButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for join", () => Client.Room != null);
        }

        [Test]
        public void TestCreatedWithCorrectMode()
        {
            AddAssert("room created with correct mode", () => Client.APIRoom?.QueueMode.Value == Mode);
        }

        protected void RunGameplay()
        {
            AddStep("click ready button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerReadyButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for ready", () => Client.LocalUser?.State == MultiplayerUserState.Ready);

            AddStep("click ready button", () =>
            {
                InputManager.MoveMouseTo(this.ChildrenOfType<MultiplayerReadyButton>().Single());
                InputManager.Click(MouseButton.Left);
            });

            AddUntilStep("wait for player", () => multiplayerScreenStack.CurrentScreen is Player player && player.IsLoaded);
            AddStep("exit player", () => multiplayerScreenStack.MultiplayerScreen.MakeCurrent());
        }
    }
}
