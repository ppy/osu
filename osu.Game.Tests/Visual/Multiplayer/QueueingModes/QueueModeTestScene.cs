// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Multiplayer.Queueing;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Screens.OnlinePlay.Lounge;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer.QueueingModes
{
    public abstract class QueueModeTestScene : ScreenTestScene
    {
        protected abstract QueueModes Mode { get; }

        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private ILive<BeatmapSetInfo> importedBeatmap;

        private TestMultiplayerScreenStack multiplayerScreenStack;

        private TestMultiplayerClient client => multiplayerScreenStack.Client;
        private TestMultiplayerRoomManager roomManager => multiplayerScreenStack.RoomManager;

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
                var beatmap1 = CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo;
                beatmap1.Version = "1";

                var beatmap2 = CreateBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo;
                beatmap2.Version = "2";

                // Move beatmap2 to beatmap1's set.
                var beatmapSet = beatmap1.BeatmapSet;
                beatmapSet.Beatmaps.Add(beatmap2);
                beatmap2.BeatmapSet = beatmapSet;

                importedBeatmap = beatmaps.Import(beatmapSet).Result;
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
                        Beatmap = { Value = importedBeatmap.Value.Beatmaps.First() },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo }
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

            AddUntilStep("wait for join", () => client.Room != null);
        }

        [Test]
        public void TestCreatedWithCorrectMode()
        {
            AddAssert("room created with correct mode", () => client.APIRoom?.QueueMode.Value == Mode);
        }
    }
}
