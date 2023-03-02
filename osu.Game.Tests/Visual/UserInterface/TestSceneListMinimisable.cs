// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.List;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneListMinimisable : TestSceneList
    {
        protected override void CreateDrawableList()
        {
            BackingDrawable = new DrawableMinimisableList<SelectionBlueprint<ISerialisableDrawable>>(
                new SkinBlueprint(
                    new BigBlackBox
                    {
                        Name = "DrawableMinimisableList"
                    }));
        }

        protected override Drawable GetContent() => BackingDrawable;

        protected DrawableMinimisableList<SelectionBlueprint<ISerialisableDrawable>> BackingDrawable = null!;

        private void expandList(Func<DrawableMinimisableList<SelectionBlueprint<ISerialisableDrawable>>> minimisableList)
        {
            AddStep("Expand List", () =>
            {
                // InputManager.MoveMouseTo(minimisableList.ToScreenSpace(minimisableList.LayoutRectangle.TopLeft + Vector2.One * 8));
                // InputManager.Click(MouseButton.Left);
                minimisableList().ShowList();
            });
            AddAssert("List is Expanded", () => minimisableList().Enabled.Value && minimisableList().IsPresent);
        }

        protected override DrawableList<SelectionBlueprint<ISerialisableDrawable>> DrawableList => BackingDrawable.List;

        [SetUpSteps]
        public void SetUpList()
        {
            expandList(() => BackingDrawable);
        }

        [Test, Order(3)]
        public void TestListinListDrag()
        {
            const int item_count = 3;
            ListAddItems(() => DrawableList);
            AddStep("Add Lists", () =>
            {
                int items = 0;
                AddElement(Enumerable.Range(0, item_count)
                                     .Select(static _ => new TextElement()),
                    BackingDrawable.List.Items,
                    () => "List" + (item_count - ++items),
                    DrawableListEntryType.MinimisableList,
                    false);
            });
            AddAssert("13 elements in list", () => DrawableList.Items.Count == 13);
            AddStep("Move mouse to first list", () =>
            {
                var drawableItem = DrawableList.ItemMaps[DrawableList.Items[0]];
                InputManager.MoveMouseTo(drawableItem.ToScreenSpace(drawableItem.LayoutRectangle.TopLeft) + Vector2.One * 8);
            });
            AddStep("Mouse Down", () => InputManager.PressButton(MouseButton.Left));
            AddStep("Mouse Move to last item", () =>
            {
                var drawableItem = DrawableList.ItemMaps[DrawableList.Items[^1]];
                InputManager.MoveMouseTo(drawableItem, Vector2.UnitY * drawableItem.LayoutSize.Y / 2);
            });
            AddStep("Mouse Up", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("last item \"List0\"", () =>
            {
                var lastItem = DrawableList.Items[^1];
                return lastItem.Type == DrawableListEntryType.MinimisableList && ((Drawable)lastItem.RepresentedItem.Item).Name == "List0";
            });
        }

        [Test, Order(4)]
        public void TestDragItemOutOfList()
        {
            const int item_count = 10;

            List<DrawableMinimisableList<SelectionBlueprint<ISerialisableDrawable>>> lists = new List<DrawableMinimisableList<SelectionBlueprint<ISerialisableDrawable>>>(item_count);
            AddStep("add lists", () =>
            {
                int items = 0;
                List<DrawableListRepresetedItem<SelectionBlueprint<ISerialisableDrawable>>> list = new List<DrawableListRepresetedItem<SelectionBlueprint<ISerialisableDrawable>>>(item_count);
                AddElement(Enumerable.Range(0, item_count)
                                     .Select(static _ => new TextElement()),
                    list,
                    () => "List" + (item_count - ++items),
                    DrawableListEntryType.MinimisableList,
                    false);
                BackingDrawable.List.Items.AddRange(list);

                AbstractListItem<SelectionBlueprint<ISerialisableDrawable>> getItem(DrawableListRepresetedItem<SelectionBlueprint<ISerialisableDrawable>> item) => BackingDrawable.List.ItemMaps[item];
                lists.AddRange(list.Select(getItem).Cast<DrawableMinimisableList<SelectionBlueprint<ISerialisableDrawable>>>().Reverse());
            });
            AddAssert("lists were all added", () => lists.Count == item_count);
            // ReSharper disable once HeuristicUnreachableCode
            expandList(() => lists[0]);
            ListAddItems(() => lists[0].List);
            ListAddItems(() => lists[1].List);
            DrawableListRepresetedItem<SelectionBlueprint<ISerialisableDrawable>> movedItem = null!;
            AbstractListItem<SelectionBlueprint<ISerialisableDrawable>> movedDrawableItem = null!;
            AddStep("Position Mouse over 1st list 1st item", () =>
            {
                movedItem = lists[0].List.Items[0];
                movedDrawableItem = lists[0].List.ItemMaps[movedItem];
                InputManager.MoveMouseTo(movedDrawableItem);
            });
            AddStep("Press mouse button", () => InputManager.PressButton(MouseButton.Left));
            AddStep("Drag item out of list", () =>
                InputManager.MoveMouseTo(
                    lists[0].ToScreenSpace(Vector2.Zero)
                    - new Vector2(0, BackingDrawable.ListHeadBoundingBox.Height / 2f + 1f + 2.5f)
                ));
            AddAssert("Item was moved out of list", () =>
                !lists[0].List.Items.Contains(movedItem)
                && BackingDrawable.List.Items.Contains(movedItem)
                && BackingDrawable.List.ItemMaps.TryGetValue(movedItem, out var test)
                && test == movedDrawableItem);
            AddStep("Move item back into the first list", () =>
            {
                var lastItem = lists[0].List.ItemMaps[lists[0].List.Items[0]];
                InputManager.MoveMouseTo(lastItem.ToScreenSpace(Vector2.UnitY * (lastItem.BoundingBox.Height / 2 + 1)));
            });
            AddAssert("Check item is in first List again", () =>
                lists[0].List.Items.Contains(movedItem)
                && lists[0].List.ItemMaps.TryGetValue(movedItem, out var test)
                && test == movedDrawableItem
                && !BackingDrawable.List.Items.Contains(movedItem)
                && !BackingDrawable.List.ItemMaps.TryGetValue(movedItem, out var tmp)
                && tmp is null
            );
            AddStep("Move item to end of first list", () =>
            {
                var lastItem = lists[0].List.Items[^1];
                var lastDrawableItem = lists[0].List.ItemMaps[lastItem];
                InputManager.MoveMouseTo(lastDrawableItem.ToScreenSpace(Vector2.UnitY * (lastDrawableItem.BoundingBox.Height - 1)));
            });
            AddAssert("Check item is last in first List", () =>
                lists[0].List.Items[^1] == movedItem
                && lists[0].List.ItemMaps.TryGetValue(movedItem, out var test)
                && test == movedDrawableItem
            );
            AddStep("Move item below the second List", () => InputManager.MoveMouseTo(lists[1].Parent.ToScreenSpace(lists[1].BoundingBox).BottomLeft));
            AddAssert("Item was actually moved to 3rd spot", () => BackingDrawable.List.Items[2] == movedItem);
        }
    }
}
