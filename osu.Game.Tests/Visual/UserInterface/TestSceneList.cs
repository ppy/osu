// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.List;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osu.Game.Skinning.Editor;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneList : OsuManualInputManagerTestScene
    {
        protected Container SkinElements { get; set; } = null!;
        protected IDrawableListItem<SelectionBlueprint<ISkinnableDrawable>> BackingDrawableList = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            BackingDrawableList = CreateDrawableList();
            var list = (Drawable)BackingDrawableList;
            list.Width = 100;
            list.RelativeSizeAxes = Axes.None;
            list.Anchor = Anchor.CentreRight;
            BackingDrawableList.GetName = t => IDrawableListItem<SelectionBlueprint<ISkinnableDrawable>>.GetDefaultText((Drawable)t.Item);
            BackingDrawableList.SetItemDepth = (blueprint, depth) =>
            {
                if (blueprint.Parent is Container<Drawable> container)
                {
                    container.ChangeChildDepth(blueprint, depth);
                    container.Invalidate();
                }

                if (blueprint.Item.Parent is Container<Drawable> containerM)
                {
                    containerM.ChangeChildDepth((Drawable)blueprint.Item, depth);
                    containerM.Invalidate();
                }
            };

            Child = new Container
            {
                Size = new Vector2(500),
                Children = new[]
                {
                    skinElements = new Container
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    list
                },
            };
            DrawableList.Height = 500;
        });

        protected virtual IDrawableListItem<SelectionBlueprint<ISkinnableDrawable>> CreateDrawableList() => new DrawableList<SelectionBlueprint<ISkinnableDrawable>>();
        protected virtual DrawableList<SelectionBlueprint<ISkinnableDrawable>> DrawableList => (DrawableList<SelectionBlueprint<ISkinnableDrawable>>)BackingDrawableList;

        protected virtual void AddElement(ISkinnableDrawable skinnableDrawable)
        {
            var skinnable = (Drawable)skinnableDrawable;
            skinnable.Name = "Element" + skinElements.Count;

            //this makes sure I actually can only select a single element

            var skinBlueprint = new SkinBlueprint(skinnableDrawable);
            DrawableList.Items.Add(new DrawableListRepresetedItem<SelectionBlueprint<ISkinnableDrawable>>(skinBlueprint, DrawableListEntryType.Item));

            skinElements.Add(new Container
            {
                Children = new[]
                {
                    skinBlueprint,
                    skinnable
                }
            });
        }

        private bool applyToItems(Predicate<DrawableListItem<SelectionBlueprint<ISkinnableDrawable>>> predicate,
                                  IEnumerable<RearrangeableListItem<DrawableListRepresetedItem<SelectionBlueprint<ISkinnableDrawable>>>> items)
        {
            foreach (var value in items)
            {
                if (value is DrawableListItem<SelectionBlueprint<ISkinnableDrawable>> item)
                {
                    if (!predicate(item)) return false;
                }
                else if (value is DrawableMinimisableList<SelectionBlueprint<ISkinnableDrawable>> minimisableList)
                {
                    if (!applyToItems(predicate, minimisableList.List.ItemMaps.Values)) return false;
                }
            }

            return true;
        }

        private bool testElementSelected(int element) => ((DrawableListItem<SelectionBlueprint<ISkinnableDrawable>>)DrawableList.ItemMaps[DrawableList.Items[element]]).IsSelected;

        protected void ListAddItems()
        {
            int before = 0;
            AddStep("Get Item Count", () => before = DrawableList.Items.Count);
            AddRepeatStep("add item", () =>
            {
                float pos = skinElements.Count * (50 + 2);
                //make sure we can fit exactly the number of elements we want in a grid pattern
                float xwidth = (int)(skinElements.ChildSize.X - 100) / 52 * 52;
                AddElement(new BigBlackBox
                {
                    Size = Vector2.One * 50,
                    Position = new Vector2(pos % xwidth, (int)pos / (int)xwidth * (50 + 2)),
                });
            }, 10);
            AddAssert("Exactly 10 items were added", () => DrawableList.Items.Count == before + 10);
            checkDepth();
        }

        private void checkDepth()
        {
            AddAssert("item positions corrospond to depth", () =>
            {
                for (int i = 1; i < DrawableList.Items.Count; i++)
                {
                    if (DrawableList.Items[i - 1].RepresentedItem.Item is Drawable drawable1
                        && DrawableList.Items[i].RepresentedItem.Item is Drawable drawable2
                        && drawable1.Depth < drawable2.Depth) return false;
                }

                return true;
            });
        }

        [Test]
        public void TestListSelection()
        {
            ListAddItems();
            //start with regular clicks
            AddAssert("no Item is selected", () => applyToItems(t => !t.IsSelected, DrawableList.ItemMaps.Values));
            AddStep("select first item", () =>
            {
                InputManager.MoveMouseTo(DrawableList.ItemMaps[DrawableList.Items[0]]);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("first item is selected", () => testElementSelected(0));
            //pressing CTRL should allow selection of multiple items
            AddStep("Select second element too", () =>
            {
                InputManager.MoveMouseTo(DrawableList.ItemMaps[DrawableList.Items[1]]);
                InputManager.PressKey(Key.LControl);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.LControl);
            });
            AddAssert("second item is selected", () => testElementSelected(1));
            //pressing CTRL should also allow deselection of items
            AddStep("Deselect second element too", () =>
            {
                InputManager.MoveMouseTo(DrawableList.ItemMaps[DrawableList.Items[1]]);
                InputManager.PressKey(Key.LControl);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.LControl);
            });
            AddAssert("second item is not selected", () => !testElementSelected(1));
            //pressing CTRL and shift should should allow also allow toggeling the selection of multiple items
            AddStep("Unselect the first item, by clicking on it directly", () =>
            {
                DrawableList.Items[0].RepresentedItem.Deselect();
            });
            AddAssert("first item is not selected", () => !testElementSelected(0));
        }

        [Test]
        public void TestListDrag()
        {
            ListAddItems();
            AddStep("Mouse to first element", () =>
            {
                var first = DrawableList.ItemMaps[DrawableList.Items[0]];
                InputManager.MoveMouseTo(first);
            });
            AddStep("Mouse Down", () => InputManager.PressButton(MouseButton.Left));
            AddStep("Mouse Move", () =>
            {
                var last = DrawableList.ItemMaps[DrawableList.Items[^1]];
                InputManager.MoveMouseTo(last, Vector2.UnitY * last.LayoutSize.Y * 2 / 3);
            });
            AddStep("Mouse Up", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("Check Elements have been swapped", () =>
            {
                for (int i = 2; i < DrawableList.Items.Count; i++)
                {
                    var selectionBlueprint = DrawableList.ItemMaps[DrawableList.Items[i]].Model.RepresentedItem;

                    if (((Drawable)selectionBlueprint.Item).Name != $"Element{(i + 1) % DrawableList.Items.Count}") return false;
                }

                return true;
            });
            checkDepth();
        }
    }
}
