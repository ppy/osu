// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Models;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Beatmaps;
using osu.Game.Users.Drawables;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneDrawableRoomPlaylist : OsuManualInputManagerTestScene
    {
        private TestPlaylist playlist;

        private BeatmapManager manager;
        private RulesetStore rulesets;

        [Cached(typeof(UserLookupCache))]
        private readonly TestUserLookupCache userLookupCache = new TestUserLookupCache();

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(ContextFactory);
        }

        [Test]
        public void TestNonEditableNonSelectable()
        {
            createPlaylist();

            moveToItem(0);
            assertHandleVisibility(0, false);
            assertDeleteButtonVisibility(0, false);

            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddAssert("no item selected", () => playlist.SelectedItem.Value == null);
        }

        [Test]
        public void TestEditable()
        {
            createPlaylist(p =>
            {
                p.AllowReordering = true;
                p.AllowDeletion = true;
            });

            moveToItem(0);
            assertHandleVisibility(0, true);
            assertDeleteButtonVisibility(0, true);

            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddAssert("no item selected", () => playlist.SelectedItem.Value == null);
        }

        [Test]
        public void TestMarkInvalid()
        {
            createPlaylist(p =>
            {
                p.AllowReordering = true;
                p.AllowDeletion = true;
                p.AllowSelection = true;
            });

            AddStep("mark item 0 as invalid", () => playlist.Items[0].MarkInvalid());

            moveToItem(0);

            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddAssert("no item selected", () => playlist.SelectedItem.Value == null);
        }

        [Test]
        public void TestSelectable()
        {
            createPlaylist(p => p.AllowSelection = true);

            moveToItem(0);
            assertHandleVisibility(0, false);
            assertDeleteButtonVisibility(0, false);

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);
        }

        [Test]
        public void TestEditableSelectable()
        {
            createPlaylist(p =>
            {
                p.AllowReordering = true;
                p.AllowDeletion = true;
                p.AllowSelection = true;
            });

            moveToItem(0);
            assertHandleVisibility(0, true);
            assertDeleteButtonVisibility(0, true);

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);
        }

        [Test]
        public void TestSelectionNotLostAfterRearrangement()
        {
            createPlaylist(p =>
            {
                p.AllowReordering = true;
                p.AllowDeletion = true;
                p.AllowSelection = true;
            });

            moveToItem(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            moveToDragger(0);
            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));
            moveToDragger(1, new Vector2(0, 5));
            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("item 1 is selected", () => playlist.SelectedItem.Value == playlist.Items[1]);
        }

        [Test]
        public void TestDownloadButtonHiddenWhenBeatmapExists()
        {
            var beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo;
            ILive<BeatmapSetInfo> imported = null;

            Debug.Assert(beatmap.BeatmapSet != null);

            AddStep("import beatmap", () => imported = manager.Import(beatmap.BeatmapSet).GetResultSafely());

            createPlaylistWithBeatmaps(() => imported.PerformRead(s => s.Beatmaps.Detach()));

            assertDownloadButtonVisible(false);

            AddStep("delete beatmap set", () => imported.PerformWrite(s => s.DeletePending = true));
            assertDownloadButtonVisible(true);

            AddStep("undelete beatmap set", () => imported.PerformWrite(s => s.DeletePending = false));
            assertDownloadButtonVisible(false);

            void assertDownloadButtonVisible(bool visible) => AddUntilStep($"download button {(visible ? "shown" : "hidden")}",
                () => playlist.ChildrenOfType<BeatmapDownloadButton>().Single().Alpha == (visible ? 1 : 0));
        }

        [Test]
        public void TestDownloadButtonVisibleInitiallyWhenBeatmapDoesNotExist()
        {
            var byOnlineId = CreateAPIBeatmap();
            byOnlineId.OnlineID = 1337; // Some random ID that does not exist locally.

            var byChecksum = CreateAPIBeatmap();
            byChecksum.Checksum = "1337"; // Some random checksum that does not exist locally.

            createPlaylistWithBeatmaps(() => new[] { byOnlineId, byChecksum });

            AddAssert("download buttons shown", () => playlist.ChildrenOfType<BeatmapDownloadButton>().All(d => d.IsPresent));
        }

        [Test]
        public void TestExplicitBeatmapItem()
        {
            var beatmap = CreateAPIBeatmap();

            Debug.Assert(beatmap.BeatmapSet != null);

            beatmap.BeatmapSet.HasExplicitContent = true;

            createPlaylistWithBeatmaps(() => new[] { beatmap });
        }

        [Test]
        public void TestExpiredItems()
        {
            AddStep("create playlist", () =>
            {
                Child = playlist = new TestPlaylist
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 300),
                    Items =
                    {
                        new PlaylistItem
                        {
                            ID = 0,
                            Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                            Expired = true,
                            RequiredMods =
                            {
                                new OsuModHardRock(),
                                new OsuModDoubleTime(),
                                new OsuModAutoplay()
                            }
                        },
                        new PlaylistItem
                        {
                            ID = 1,
                            Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
                            Ruleset = { Value = new OsuRuleset().RulesetInfo },
                            RequiredMods =
                            {
                                new OsuModHardRock(),
                                new OsuModDoubleTime(),
                                new OsuModAutoplay()
                            }
                        }
                    }
                };
            });

            AddUntilStep("wait for items to load", () => playlist.ItemMap.Values.All(i => i.IsLoaded));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void TestWithOwner(bool withOwner)
        {
            createPlaylist(p => p.ShowItemOwners = withOwner);

            AddAssert("owner visible", () => playlist.ChildrenOfType<UpdateableAvatar>().All(a => a.IsPresent == withOwner));
        }

        [Test]
        public void TestWithAllButtonsEnabled()
        {
            createPlaylist(p =>
            {
                p.AllowDeletion = true;
                p.AllowShowingResults = true;
                p.AllowEditing = true;
            });
        }

        private void moveToItem(int index, Vector2? offset = null)
            => AddStep($"move mouse to item {index}", () => InputManager.MoveMouseTo(playlist.ChildrenOfType<DifficultyIcon>().ElementAt(index), offset));

        private void moveToDragger(int index, Vector2? offset = null) => AddStep($"move mouse to dragger {index}", () =>
        {
            var item = playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>>().ElementAt(index);
            InputManager.MoveMouseTo(item.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>.PlaylistItemHandle>().Single(), offset);
        });

        private void assertHandleVisibility(int index, bool visible)
            => AddAssert($"handle {index} {(visible ? "is" : "is not")} visible",
                () => (playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>.PlaylistItemHandle>().ElementAt(index).Alpha > 0) == visible);

        private void assertDeleteButtonVisibility(int index, bool visible)
            => AddAssert($"delete button {index} {(visible ? "is" : "is not")} visible",
                () => (playlist.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistRemoveButton>().ElementAt(2 + index * 2).Alpha > 0) == visible);

        private void createPlaylist(Action<TestPlaylist> setupPlaylist = null)
        {
            AddStep("create playlist", () =>
            {
                Child = playlist = new TestPlaylist
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 300)
                };

                setupPlaylist?.Invoke(playlist);

                for (int i = 0; i < 20; i++)
                {
                    playlist.Items.Add(new PlaylistItem
                    {
                        ID = i,
                        OwnerID = 2,
                        Beatmap =
                        {
                            Value = i % 2 == 1
                                ? new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo
                                : new BeatmapInfo
                                {
                                    Metadata = new BeatmapMetadata
                                    {
                                        Artist = "Artist",
                                        Author = new RealmUser { Username = "Creator name here" },
                                        Title = "Long title used to check background colour",
                                    },
                                    BeatmapSet = new BeatmapSetInfo()
                                }
                        },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        RequiredMods =
                        {
                            new OsuModHardRock(),
                            new OsuModDoubleTime(),
                            new OsuModAutoplay()
                        }
                    });
                }
            });

            AddUntilStep("wait for items to load", () => playlist.ItemMap.Values.All(i => i.IsLoaded));
        }

        private void createPlaylistWithBeatmaps(Func<IEnumerable<IBeatmapInfo>> beatmaps)
        {
            AddStep("create playlist", () =>
            {
                Child = playlist = new TestPlaylist
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 300)
                };

                int index = 0;

                foreach (var b in beatmaps())
                {
                    playlist.Items.Add(new PlaylistItem
                    {
                        ID = index++,
                        OwnerID = 2,
                        Beatmap = { Value = b },
                        Ruleset = { Value = new OsuRuleset().RulesetInfo },
                        RequiredMods =
                        {
                            new OsuModHardRock(),
                            new OsuModDoubleTime(),
                            new OsuModAutoplay()
                        }
                    });
                }
            });

            AddUntilStep("wait for items to load", () => playlist.ItemMap.Values.All(i => i.IsLoaded));
        }

        private class TestPlaylist : DrawableRoomPlaylist
        {
            public new IReadOnlyDictionary<PlaylistItem, RearrangeableListItem<PlaylistItem>> ItemMap => base.ItemMap;
        }
    }
}
