//

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
        private Container skinElements = null!;
        private DrawableList<SelectionBlueprint<ISkinnableDrawable>> drawableList = null!;

        [SetUp]
        public void SetUp() => Schedule(() =>
        {
            Child = new Container
            {
                Size = new Vector2(500),
                Children = new Drawable[]
                {
                    skinElements = new Container
                    {
                        RelativeSizeAxes = Axes.Both
                    },
                    drawableList = new DrawableList<SelectionBlueprint<ISkinnableDrawable>>
                    {
                        Width = 100,
                        RelativeSizeAxes = Axes.Y,
                        Anchor = Anchor.CentreRight,
                        GetName = t => IDrawableListItem<SelectionBlueprint<ISkinnableDrawable>>.GetDefaultText((Drawable)t.Item),
                        SetItemDepth = (blueprint, depth) =>
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
                        }
                    }
                }
            };
        });

        private void addElement(ISkinnableDrawable skinnableDrawable)
        {
            var skinnable = (Drawable)skinnableDrawable;
            skinnable.Name = "Element" + skinElements.Count;

            //this makes sure I actually can only select a single element

            var skinBlueprint = new SkinBlueprint(skinnableDrawable);
            drawableList.Items.Add(new DrawableListRepresetedItem<SelectionBlueprint<ISkinnableDrawable>>(skinBlueprint));

            skinElements.Add(new Container
                {
                    Children = new[]
                    {
                        skinBlueprint,
                        skinnable
                    }
                }
            );
        }

        private bool applyToItems(Predicate<DrawableListItem<SelectionBlueprint<ISkinnableDrawable>>> predicate,
                                  IEnumerable<RearrangeableListItem<IDrawableListRepresetedItem<SelectionBlueprint<ISkinnableDrawable>>>> items)
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

        private bool testElementSelected(int element) => ((DrawableListItem<SelectionBlueprint<ISkinnableDrawable>>)drawableList.ItemMaps[drawableList.Items[element]]).Selected;

        private void listAddItems()
        {
            AddAssert("no Items", () => drawableList.Items.Count == 0);
            AddRepeatStep("add item", () =>
            {
                float pos = skinElements.Count * (50 + 2);
                //make sure we can fit exactly the number of elements we want in a grid pattern
                float xwidth = (int)(skinElements.ChildSize.X - 100) / 52 * 52;
                addElement(new BigBlackBox
                {
                    Size = Vector2.One * 50,
                    Position = new Vector2(pos % xwidth, (int)pos / (int)xwidth * (50 + 2)),
                });
            }, 10);
            AddAssert("10x items in List", () => drawableList.Items.Count == 10);
            checkDepth();
        }

        private void checkDepth()
        {
            AddAssert("item positions corrospond to depth", () =>
            {
                for (int i = 1; i < drawableList.Items.Count; i++)
                {
                    if (drawableList.Items[i - 1].RepresentedItem?.Item is Drawable drawable1
                        && drawableList.Items[i].RepresentedItem?.Item is Drawable drawable2
                        && drawable1.Depth < drawable2.Depth) return false;
                }

                return true;
            });
        }

        [Test]
        public void TestListSelection()
        {
            listAddItems();
            //start with regular clicks
            AddAssert("no Item is selected", () => applyToItems(t => !t.Selected, drawableList.ItemMaps.Values));
            AddStep("select first item", () =>
            {
                InputManager.MoveMouseTo(drawableList.ItemMaps[drawableList.Items[0]]);
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("first item is selected", () => testElementSelected(0));
            //pressing CTRL should allow selection of multiple items
            AddStep("Select second element too", () =>
            {
                InputManager.MoveMouseTo(drawableList.ItemMaps[drawableList.Items[1]]);
                InputManager.PressKey(Key.LControl);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.LControl);
            });
            AddAssert("second item is selected", () => testElementSelected(1));
            //pressing CTRL should also allow deselection of items
            AddStep("Deselect second element too", () =>
            {
                InputManager.MoveMouseTo(drawableList.ItemMaps[drawableList.Items[1]]);
                InputManager.PressKey(Key.LControl);
                InputManager.Click(MouseButton.Left);
                InputManager.ReleaseKey(Key.LControl);
            });
            AddAssert("second item is not selected", () => !testElementSelected(1));
            //pressing CTRL and shift should should allow also allow toggeling the selection of multiple items
            AddStep("Unselect the first item, by clicking on it directly", () =>
            {
                drawableList.Items[0].RepresentedItem?.Deselect();
            });
            AddAssert("first item is not selected", () => !testElementSelected(0));
        }

        [Test]
        public void TestListDrag()
        {
            listAddItems();
            AddStep("Mouse to first element", () =>
            {
                var first = drawableList.ItemMaps[drawableList.Items[0]];
                InputManager.MoveMouseTo(first);
            });
            AddStep("Mouse Down", () => InputManager.PressButton(MouseButton.Left));
            AddStep("Mouse Move", () =>
            {
                var last = drawableList.ItemMaps[drawableList.Items[^1]];
                InputManager.MoveMouseTo(last, Vector2.UnitY * last.LayoutSize.Y * 2 / 3);
            });
            AddStep("Mouse Up", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("Check Elements have been swapped", () =>
            {
                for (int i = 2; i < drawableList.Items.Count; i++)
                {
                    var selectionBlueprint = drawableList.ItemMaps[drawableList.Items[i]].Model.RepresentedItem;

                    if (selectionBlueprint is null) return false;

                    if (((Drawable)selectionBlueprint.Item).Name != $"Element{(i + 1) % drawableList.Items.Count}") return false;
                }

                return true;
            });
            checkDepth();
        }
    }
}
