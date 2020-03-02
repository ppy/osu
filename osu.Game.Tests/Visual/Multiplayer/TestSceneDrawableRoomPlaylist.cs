// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Testing;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.Multiplayer;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Mods;
using osu.Game.Screens.Multi;
using osu.Game.Tests.Beatmaps;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.Multiplayer
{
    public class TestSceneDrawableRoomPlaylist : ManualInputManagerTestScene
    {
        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(DrawableRoomPlaylist),
            typeof(DrawableRoomPlaylistItem)
        };

        private TestPlaylist playlist;

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

        private void moveToItem(int index, Vector2? offset = null)
            => AddStep($"move mouse to item {index}", () => InputManager.MoveMouseTo(playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>>().ElementAt(index), offset));

        private void moveToDragger(int index, Vector2? offset = null) => AddStep($"move mouse to dragger {index}", () =>
        {
            var item = playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>>().ElementAt(index);
            InputManager.MoveMouseTo(item.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>.PlaylistItemHandle>().Single(), offset);
        });

        private void moveToDeleteButton(int index, Vector2? offset = null) => AddStep($"move mouse to delete button {index}", () =>
        {
            var item = playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>>().ElementAt(index);
            InputManager.MoveMouseTo(item.ChildrenOfType<IconButton>().ElementAt(0), offset);
        });

        private void assertHandleVisibility(int index, bool visible)
            => AddAssert($"handle {index} {(visible ? "is" : "is not")} visible",
                () => (playlist.ChildrenOfType<OsuRearrangeableListItem<PlaylistItem>.PlaylistItemHandle>().ElementAt(index).Alpha > 0) == visible);

        private void assertDeleteButtonVisibility(int index, bool visible)
            => AddAssert($"delete button {index} {(visible ? "is" : "is not")} visible", () => (playlist.ChildrenOfType<IconButton>().ElementAt(2 + index * 2).Alpha > 0) == visible);

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
                        Beatmap = { Value = new TestBeatmap(new OsuRuleset().RulesetInfo).BeatmapInfo },
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
