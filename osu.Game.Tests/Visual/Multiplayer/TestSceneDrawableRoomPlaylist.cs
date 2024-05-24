// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Platform;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Models;
using osu.Game.Online.API;
using osu.Game.Online.Chat;
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
    public partial class TestSceneDrawableRoomPlaylist : MultiplayerTestScene
    {
        private TestPlaylist playlist;

        private BeatmapManager manager;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(new RealmRulesetStore(Realm));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, Realm, null, audio, Resources, host, Beatmap.Default));
            Dependencies.Cache(Realm);
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

            AddStep("press down", () => InputManager.Key(Key.Down));
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

            AddStep("press down", () => InputManager.Key(Key.Down));
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

            AddStep("press down", () => InputManager.Key(Key.Down));
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
        public void TestKeyboardSelection()
        {
            createPlaylist(p => p.AllowSelection = true);

            AddStep("press down", () => InputManager.Key(Key.Down));
            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);

            AddStep("press down", () => InputManager.Key(Key.Down));
            AddAssert("item 1 is selected", () => playlist.SelectedItem.Value == playlist.Items[1]);

            AddStep("press up", () => InputManager.Key(Key.Up));
            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);

            AddUntilStep("navigate to last item via keyboard", () =>
            {
                InputManager.Key(Key.Down);
                return playlist.SelectedItem.Value == playlist.Items.Last();
            });
            AddAssert("last item is selected", () => playlist.SelectedItem.Value == playlist.Items.Last());
            AddUntilStep("last item is scrolled into view", () =>
            {
                var drawableItem = playlist.ItemMap[playlist.Items.Last()];
                return playlist.ScreenSpaceDrawQuad.Contains(drawableItem.ScreenSpaceDrawQuad.TopLeft)
                       && playlist.ScreenSpaceDrawQuad.Contains(drawableItem.ScreenSpaceDrawQuad.BottomRight);
            });

            AddStep("press down", () => InputManager.Key(Key.Down));
            AddAssert("last item is selected", () => playlist.SelectedItem.Value == playlist.Items.Last());

            AddStep("press up", () => InputManager.Key(Key.Up));
            AddAssert("second last item is selected", () => playlist.SelectedItem.Value == playlist.Items.Reverse().ElementAt(1));
        }

        [Test]
        public void TestDownloadButtonHiddenWhenBeatmapExists()
        {
            Live<BeatmapSetInfo> imported = null;

            AddStep("import beatmap", () =>
            {
                var beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo;

                Debug.Assert(beatmap.BeatmapSet != null);
                imported = manager.Import(beatmap.BeatmapSet);
            });

            createPlaylistWithBeatmaps(() => imported.PerformRead(s => s.Beatmaps.Detach()));

            assertDownloadButtonVisible(false);

            AddStep("delete beatmap set", () => imported.PerformWrite(s => s.DeletePending = true));
            assertDownloadButtonVisible(true);

            AddStep("undelete beatmap set", () => imported.PerformWrite(s => s.DeletePending = false));
            assertDownloadButtonVisible(false);

            void assertDownloadButtonVisible(bool visible) => AddUntilStep($"download button {(visible ? "shown" : "hidden")}",
                () => playlist.ChildrenOfType<BeatmapDownloadButton>().SingleOrDefault()?.Alpha == (visible ? 1 : 0));
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
            createPlaylist(p =>
            {
                p.Items.Clear();
                p.Items.AddRange(new[]
                {
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        ID = 0,
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        Expired = true,
                        RequiredMods = new[]
                        {
                            new APIMod(new OsuModHardRock()),
                            new APIMod(new OsuModDoubleTime()),
                            new APIMod(new OsuModAutoplay())
                        }
                    },
                    new PlaylistItem(new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo)
                    {
                        ID = 1,
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        RequiredMods = new[]
                        {
                            new APIMod(new OsuModHardRock()),
                            new APIMod(new OsuModDoubleTime()),
                            new APIMod(new OsuModAutoplay())
                        }
                    }
                });
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

        [Test]
        public void TestSelectableMouseHandling()
        {
            bool resultsRequested = false;

            AddStep("reset flag", () => resultsRequested = false);
            createPlaylist(p =>
            {
                p.AllowSelection = true;
                p.AllowShowingResults = true;
                p.RequestResults = _ => resultsRequested = true;
            });

            AddStep("move mouse to first item title", () =>
            {
                var drawQuad = playlist.ChildrenOfType<LinkFlowContainer>().First().ScreenSpaceDrawQuad;
                var location = (drawQuad.TopLeft + drawQuad.BottomLeft) / 2 + new Vector2(drawQuad.Width * 0.2f, 0);
                InputManager.MoveMouseTo(location);
            });
            AddUntilStep("wait for text load", () => playlist.ChildrenOfType<DrawableLinkCompiler>().Any());
            AddAssert("first item title not hovered", () => playlist.ChildrenOfType<DrawableLinkCompiler>().First().IsHovered, () => Is.False);
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("first item selected", () => playlist.ChildrenOfType<DrawableRoomPlaylistItem>().First().IsSelectedItem, () => Is.True);
            // implies being clickable.
            AddUntilStep("first item title hovered", () => playlist.ChildrenOfType<DrawableLinkCompiler>().First().IsHovered, () => Is.True);

            AddStep("move mouse to second item results button", () => InputManager.MoveMouseTo(playlist.ChildrenOfType<GrayButton>().ElementAt(5)));
            AddStep("click left mouse", () => InputManager.Click(MouseButton.Left));
            AddUntilStep("results requested", () => resultsRequested);
        }

        private void moveToItem(int index, Vector2? offset = null)
            => AddStep($"move mouse to item {index}", () => InputManager.MoveMouseTo(playlist.ChildrenOfType<DrawableRoomPlaylistItem>().ElementAt(index), offset));

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

        private void createPlaylistWithBeatmaps(Func<IEnumerable<IBeatmapInfo>> beatmaps) => createPlaylist(p =>
        {
            int index = 0;

            p.Items.Clear();

            foreach (var b in beatmaps())
            {
                p.Items.Add(new PlaylistItem(b)
                {
                    ID = index++,
                    OwnerID = 2,
                    RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                    RequiredMods = new[]
                    {
                        new APIMod(new OsuModHardRock()),
                        new APIMod(new OsuModDoubleTime()),
                        new APIMod(new OsuModAutoplay())
                    }
                });
            }
        });

        private void createPlaylist(Action<TestPlaylist> setupPlaylist = null)
        {
            AddStep("create playlist", () =>
            {
                Child = new OsuContextMenuContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = playlist = new TestPlaylist
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(500, 300)
                    }
                };

                for (int i = 0; i < 20; i++)
                {
                    playlist.Items.Add(new PlaylistItem(i % 2 == 1
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
                        })
                    {
                        ID = i,
                        OwnerID = 2,
                        RulesetID = new OsuRuleset().RulesetInfo.OnlineID,
                        RequiredMods = new[]
                        {
                            new APIMod(new OsuModHardRock()),
                            new APIMod(new OsuModDoubleTime()),
                            new APIMod(new OsuModAutoplay())
                        }
                    });
                }

                setupPlaylist?.Invoke(playlist);
            });

            AddUntilStep("wait for items to load", () => playlist.ItemMap.Values.All(i => i.IsLoaded));
        }

        private partial class TestPlaylist : DrawableRoomPlaylist
        {
            public new IReadOnlyDictionary<PlaylistItem, RearrangeableListItem<PlaylistItem>> ItemMap => base.ItemMap;
        }
    }
}
