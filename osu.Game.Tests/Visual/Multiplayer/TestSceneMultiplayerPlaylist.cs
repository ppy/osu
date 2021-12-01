// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Online.Multiplayer;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Multiplayer;
using osu.Game.Screens.OnlinePlay.Multiplayer.Match.Playlist;
using osu.Game.Tests.Resources;
using osuTK;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneMultiplayerPlaylist : MultiplayerTestScene
    {
        private BeatmapManager beatmaps;
        private RulesetStore rulesets;
        private BeatmapSetInfo importedSet;
        private BeatmapInfo importedBeatmap;

        [Cached(typeof(UserLookupCache))]
        private UserLookupCache lookupCache = new TestUserLookupCache();

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(beatmaps = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        [SetUp]
        public new void Setup() => Schedule(() =>
        {
            Child = new MultiplayerPlaylist
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(0.4f, 0.8f)
            };
        });

        [SetUpSteps]
        public new void SetUpSteps()
        {
            AddStep("import beatmap", () =>
            {
                beatmaps.Import(TestResources.GetQuickTestBeatmapForImport()).Wait();
                importedSet = beatmaps.GetAllUsableBeatmapSetsEnumerable(IncludedDetails.All).First();
                importedBeatmap = importedSet.Beatmaps.First(b => b.RulesetID == 0);
            });
        }

        [Test]
        public void DoTest()
        {
            AddStep("change to round robin mode", () => Client.ChangeSettings(new MultiplayerRoomSettings { QueueMode = QueueMode.AllPlayersRoundRobin }));
            AddStep("add playlist item for user 1", () => Client.AddPlaylistItem(new MultiplayerPlaylistItem
            {
                BeatmapID = importedBeatmap.OnlineID!.Value
            }));
        }

        public class MultiplayerPlaylist : MultiplayerRoomComposite
        {
            private MultiplayerQueueList multiplayerQueueList;
            private DrawableRoomPlaylist historyList;
            private bool firstPopulation = true;

            [BackgroundDependencyLoader]
            private void load()
            {
                InternalChild = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            multiplayerQueueList = new MultiplayerQueueList(false, false, true)
                            {
                                RelativeSizeAxes = Axes.Both
                            },
                            historyList = new DrawableRoomPlaylist(false, false, true)
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                };
            }

            protected override void OnRoomUpdated()
            {
                base.OnRoomUpdated();

                if (Room == null)
                    return;

                if (!firstPopulation) return;

                foreach (var item in Room.Playlist)
                    PlaylistItemAdded(item);

                firstPopulation = false;
            }

            protected override void PlaylistItemAdded(MultiplayerPlaylistItem item)
            {
                base.PlaylistItemAdded(item);

                if (item.Expired)
                    historyList.Items.Add(getPlaylistItem(item));
                else
                    multiplayerQueueList.Items.Add(getPlaylistItem(item));
            }

            protected override void PlaylistItemRemoved(long item)
            {
                base.PlaylistItemRemoved(item);

                multiplayerQueueList.Items.RemoveAll(i => i.ID == item);
            }

            protected override void PlaylistItemChanged(MultiplayerPlaylistItem item)
            {
                base.PlaylistItemChanged(item);

                PlaylistItemRemoved(item.ID);
                PlaylistItemAdded(item);
            }

            private PlaylistItem getPlaylistItem(MultiplayerPlaylistItem item) => Playlist.Single(i => i.ID == item.ID);
        }
    }
}
