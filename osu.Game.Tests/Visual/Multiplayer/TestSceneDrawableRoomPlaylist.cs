// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Rooms;
using osu.Game.Overlays.BeatmapListing.Panels;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneDrawableRoomPlaylist : OsuManualInputManagerTestScene
    {
        private TestPlaylist playlist;

        private BeatmapManager manager;
        private RulesetStore rulesets;

        [BackgroundDependencyLoader]
        private void load(GameHost host, AudioManager audio)
        {
            Dependencies.Cache(rulesets = new RulesetStore(ContextFactory));
            Dependencies.Cache(manager = new BeatmapManager(LocalStorage, ContextFactory, rulesets, null, audio, Resources, host, Beatmap.Default));
        }

        [Test]
        public void TestNonEditableNonSelectable()
        {
            createPlaylist(false, false);

            moveToItem(0);
            assertHandleVisibility(0, false);
            assertDeleteButtonVisibility(0, false);

            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddAssert("no item selected", () => playlist.SelectedItem.Value == null);
        }

        [Test]
        public void TestEditable()
        {
            createPlaylist(true, false);

            moveToItem(0);
            assertHandleVisibility(0, true);
            assertDeleteButtonVisibility(0, true);

            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddAssert("no item selected", () => playlist.SelectedItem.Value == null);
        }

        [Test]
        public void TestSelectable()
        {
            createPlaylist(false, true);

            moveToItem(0);
            assertHandleVisibility(0, false);
            assertDeleteButtonVisibility(0, false);

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);
        }

        [Test]
        public void TestEditableSelectable()
        {
            createPlaylist(true, true);

            moveToItem(0);
            assertHandleVisibility(0, true);
            assertDeleteButtonVisibility(0, true);

            AddStep("click", () => InputManager.Click(MouseButton.Left));

            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);
        }

        [Test]
        public void TestSelectionNotLostAfterRearrangement()
        {
            createPlaylist(true, true);

            moveToItem(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            moveToDragger(0);
            AddStep("begin drag", () => InputManager.PressButton(MouseButton.Left));
            moveToDragger(1, new Vector2(0, 5));
            AddStep("end drag", () => InputManager.ReleaseButton(MouseButton.Left));

            AddAssert("item 1 is selected", () => playlist.SelectedItem.Value == playlist.Items[1]);
        }

        [Test]
        public void TestItemRemovedOnDeletion()
        {
            PlaylistItem selectedItem = null;

            createPlaylist(true, true);

            moveToItem(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("retrieve selection", () => selectedItem = playlist.SelectedItem.Value);

            moveToDeleteButton(0);
            AddStep("click delete button", () => InputManager.Click(MouseButton.Left));

            AddAssert("item removed", () => !playlist.Items.Contains(selectedItem));
        }

        [Test]
        public void TestNextItemSelectedAfterDeletion()
        {
            createPlaylist(true, true);

            moveToItem(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            moveToDeleteButton(0);
            AddStep("click delete button", () => InputManager.Click(MouseButton.Left));

            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);
        }

        [Test]
        public void TestLastItemSelectedAfterLastItemDeleted()
        {
            createPlaylist(true, true);

            AddWaitStep("wait for flow", 5); // Items may take 1 update frame to flow. A wait count of 5 is guaranteed to result in the flow being updated as desired.
            AddStep("scroll to bottom", () => playlist.ChildrenOfType<ScrollContainer<Drawable>>().First().ScrollToEnd(false));

            moveToItem(19);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            moveToDeleteButton(19);
            AddStep("click delete button", () => InputManager.Click(MouseButton.Left));

            AddAssert("item 18 is selected", () => playlist.SelectedItem.Value == playlist.Items[18]);
        }

        [Test]
        public void TestSelectionResetWhenAllItemsDeleted()
        {
            createPlaylist(true, true);

            AddStep("remove all but one item", () =>
            {
                playlist.Items.RemoveRange(1, playlist.Items.Count - 1);
            });

            moveToItem(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            moveToDeleteButton(0);
            AddStep("click delete button", () => InputManager.Click(MouseButton.Left));

            AddAssert("no item selected", () => playlist.SelectedItem.Value == null);
        }

        // Todo: currently not possible due to bindable list shortcomings (https://github.com/ppy/osu-framework/issues/3081)
        // [Test]
        public void TestNextItemSelectedAfterExternalDeletion()
        {
            createPlaylist(true, true);

            moveToItem(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("remove item 0", () => playlist.Items.RemoveAt(0));

            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);
        }

        [Test]
        public void TestChangeBeatmapAndRemove()
        {
            createPlaylist(true, true);

            AddStep("change beatmap of first item", () => playlist.Items[0].BeatmapID = 30);
            moveToDeleteButton(0);
            AddStep("click delete button", () => InputManager.Click(MouseButton.Left));
        }

        [Test]
        public void TestDownloadButtonHiddenWhenBeatmapExists()
        {
            var beatmap = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo;

            AddStep("import beatmap", () => manager.Import(beatmap.BeatmapSet).Wait());

            createPlaylist(beatmap);

            assertDownloadButtonVisible(false);

            AddStep("delete beatmap set", () => manager.Delete(manager.QueryBeatmapSets(_ => true).Single()));
            assertDownloadButtonVisible(true);

            AddStep("undelete beatmap set", () => manager.Undelete(manager.QueryBeatmapSets(_ => true).Single()));
            assertDownloadButtonVisible(false);

            void assertDownloadButtonVisible(bool visible) => AddUntilStep($"download button {(visible ? "shown" : "hidden")}",
                () => playlist.ChildrenOfType<BeatmapPanelDownloadButton>().Single().Alpha == (visible ? 1 : 0));
        }

        [Test]
        public void TestDownloadButtonVisibleInitiallyWhenBeatmapDoesNotExist()
        {
            var byOnlineId = CreateAPIBeatmap();
            byOnlineId.OnlineID = 1337; // Some random ID that does not exist locally.

            var byChecksum = CreateAPIBeatmap();
            byChecksum.Checksum = "1337"; // Some random checksum that does not exist locally.

            createPlaylist(byOnlineId, byChecksum);

            AddAssert("download buttons shown", () => playlist.ChildrenOfType<BeatmapPanelDownloadButton>().All(d => d.IsPresent));
        }

        [Test]
        public void TestExplicitBeatmapItem()
        {
            var beatmap = CreateAPIBeatmap();

            Debug.Assert(beatmap.BeatmapSet != null);

            beatmap.BeatmapSet.HasExplicitContent = true;

            createPlaylist(beatmap);
        }

        private void moveToItem(int index, Vector2? offset = null)
            => AddStep($"move mouse to item {index}", () => InputManager.MoveMouseTo(playlist.ChildrenOfType<DifficultyIcon>().ElementAt(index), offset));

        private void moveToDragger(int index, Vector2? offset = null) => AddStep($"move mouse to dragger {index}", () =>
        {
            var item = playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>>().ElementAt(index);
            InputManager.MoveMouseTo(item.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>.PlaylistItemHandle>().Single(), offset);
        });

        private void moveToDeleteButton(int index, Vector2? offset = null) => AddStep($"move mouse to delete button {index}", () =>
        {
            var item = playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>>().ElementAt(index);
            InputManager.MoveMouseTo(item.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistRemoveButton>().ElementAt(0), offset);
        });

        private void assertHandleVisibility(int index, bool visible)
            => AddAssert($"handle {index} {(visible ? "is" : "is not")} visible",
                () => (playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>.PlaylistItemHandle>().ElementAt(index).Alpha > 0) == visible);

        private void assertDeleteButtonVisibility(int index, bool visible)
            => AddAssert($"delete button {index} {(visible ? "is" : "is not")} visible", () => (playlist.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistRemoveButton>().ElementAt(2 + index * 2).Alpha > 0) == visible);

        private void createPlaylist(bool allowEdit, bool allowSelection)
        {
            AddStep("create playlist", () =>
            {
                Child = playlist = new TestPlaylist(allowEdit, allowSelection)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 300)
                };

                for (int i = 0; i < 20; i++)
                {
                    playlist.Items.Add(new PlaylistItem
                    {
                        ID = i,
                        Beatmap =
                        {
                            Value = i % 2 == 1
                                ? new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo
                                : new BeatmapInfo
                                {
                                    Metadata = new BeatmapMetadata
                                    {
                                        Artist = "Artist",
                                        Author = new APIUser { Username = "Creator name here" },
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

        private void createPlaylist(params IBeatmapInfo[] beatmaps)
        {
            AddStep("create playlist", () =>
            {
                Child = playlist = new TestPlaylist(false, false)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(500, 300)
                };

                int index = 0;

                foreach (var b in beatmaps)
                {
                    playlist.Items.Add(new PlaylistItem
                    {
                        ID = index++,
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

            public TestPlaylist(bool allowEdit, bool allowSelection)
                : base(allowEdit, allowSelection)
            {
            }
        }
    }
}
