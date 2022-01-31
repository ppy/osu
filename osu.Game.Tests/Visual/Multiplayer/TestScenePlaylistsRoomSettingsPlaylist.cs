// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics.Containers;
using osu.Game.Models;
using osu.Game.Online.Rooms;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.OnlinePlay;
using osu.Game.Screens.OnlinePlay.Playlists;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestScenePlaylistsRoomSettingsPlaylist : OsuManualInputManagerTestScene
    {
        private TestPlaylist playlist;

        [Cached(typeof(UserLookupCache))]
        private readonly TestUserLookupCache userLookupCache = new TestUserLookupCache();

        [Test]
        public void TestItemRemovedOnDeletion()
        {
            PlaylistItem selectedItem = null;

            createPlaylist();

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
            createPlaylist();

            moveToItem(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));

            moveToDeleteButton(0);
            AddStep("click delete button", () => InputManager.Click(MouseButton.Left));

            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);
        }

        [Test]
        public void TestLastItemSelectedAfterLastItemDeleted()
        {
            createPlaylist();

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
            createPlaylist();

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
            createPlaylist();

            moveToItem(0);
            AddStep("click", () => InputManager.Click(MouseButton.Left));
            AddStep("remove item 0", () => playlist.Items.RemoveAt(0));

            AddAssert("item 0 is selected", () => playlist.SelectedItem.Value == playlist.Items[0]);
        }

        [Test]
        public void TestChangeBeatmapAndRemove()
        {
            createPlaylist();

            AddStep("change beatmap of first item", () => playlist.Items[0].BeatmapID = 30);
            moveToDeleteButton(0);
            AddStep("click delete button", () => InputManager.Click(MouseButton.Left));
        }

        private void moveToItem(int index, Vector2? offset = null)
            => AddStep($"move mouse to item {index}", () => InputManager.MoveMouseTo(playlist.ChildrenOfType<DifficultyIcon>().ElementAt(index), offset));

        private void moveToDeleteButton(int index, Vector2? offset = null) => AddStep($"move mouse to delete button {index}", () =>
        {
            var item = playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>>().ElementAt(index);
            InputManager.MoveMouseTo(item.ChildrenOfType<DrawableRoomPlaylistItem.PlaylistRemoveButton>().ElementAt(0), offset);
        });

        private void createPlaylist()
        {
            AddStep("create playlist", () =>
            {
                Child = playlist = new TestPlaylist
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

        private class TestPlaylist : PlaylistsRoomSettingsPlaylist
        {
            public new IReadOnlyDictionary<PlaylistItem, RearrangeableListItem<PlaylistItem>> ItemMap => base.ItemMap;

            public TestPlaylist()
            {
                AllowSelection = true;
            }
        }
    }
}
