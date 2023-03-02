// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Logging;
using osu.Game.Extensions;
using osu.Game.Overlays.SkinEditor;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.List;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    [TestFixture]
    public partial class TestSceneList : OsuManualInputManagerTestScene
    {
        protected Container SkinElements { get; set; } = null!;
        protected DrawableList<SelectionBlueprint<ISerialisableDrawable>> BackingDrawableList = null!;

        [SetUp]
        public void SetUp()
        {
            CreateDrawableList();
            var drawable = GetContent();
            drawable.Width = 100;
            drawable.RelativeSizeAxes = Axes.None;
            drawable.Anchor = Anchor.CentreRight;
            ((IDrawableListItem<SelectionBlueprint<ISerialisableDrawable>>)drawable).Properties.GetName = static t => IDrawableListItem<SelectionBlueprint<ISerialisableDrawable>>.GetDefaultText((Drawable)t.Item);
            ((IDrawableListItem<SelectionBlueprint<ISerialisableDrawable>>)drawable).Properties.SetItemDepth = static (blueprint, depth) =>
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

            Scheduler.Add(() =>
            {
                Child = new Container
                {
                    Size = new Vector2(500),
                    Children = new[]
                    {
                        SkinElements = new Container
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        GetContent()
                    },
                };
            });
        }

        protected virtual Drawable GetContent() => BackingDrawableList;
        protected virtual void CreateDrawableList() => BackingDrawableList = new DrawableList<SelectionBlueprint<ISerialisableDrawable>>();
        protected virtual DrawableList<SelectionBlueprint<ISerialisableDrawable>> DrawableList => BackingDrawableList;

        protected void AddElement(IEnumerable<ISerialisableDrawable> skinnableDrawable, IList<DrawableListRepresetedItem<SelectionBlueprint<ISerialisableDrawable>>> list)
        {
            string getName() => "Element" + list.Count;
            AddElement(skinnableDrawable, list, getName);
        }

        protected void AddElement(IEnumerable<ISerialisableDrawable> skinnableDrawable, IList<DrawableListRepresetedItem<SelectionBlueprint<ISerialisableDrawable>>> list, Func<string> producer)
            => AddElement(skinnableDrawable, list, producer, DrawableListEntryType.Item, true);

        protected virtual void AddElement(IEnumerable<ISerialisableDrawable> skinnableDrawable, IList<DrawableListRepresetedItem<SelectionBlueprint<ISerialisableDrawable>>> list, Func<string> producer, DrawableListEntryType type, bool addToSkinElements)
        {
            var represetedItems = skinnableDrawable.Select(e =>
            {
                ((Drawable)e).Name = producer();
                return new SkinBlueprint(e);
            }).Select(e => new DrawableListRepresetedItem<SelectionBlueprint<ISerialisableDrawable>>(e, type));

            //this makes sure I actually can only select a single element

            list.AddRange(represetedItems);

            if (!addToSkinElements) return;

            Drawable getElement(Drawable drawable)
            {
                float pos = SkinElements.Children.Count / 2 * (50 + 2);
                //make sure we can fit exactly the number of elements we want in a grid pattern
                float xwidth = (int)(SkinElements.ChildSize.X - 100) / 52 * 52;
                drawable.Position = new Vector2(pos % xwidth, (int)pos / (int)xwidth * (50 + 2));
                return drawable;
            }

            var containers = represetedItems.Select(e => new Container
            {
                Children = new[]
                {
                    getElement(e.RepresentedItem),
                    getElement((Drawable)e.RepresentedItem.Item),
                }
            });

            SkinElements.AddRange(containers);
        }

        private bool applyToItems(Predicate<DrawableListItem<SelectionBlueprint<ISerialisableDrawable>>> predicate,
                                  IEnumerable<AbstractListItem<SelectionBlueprint<ISerialisableDrawable>>> items)
        {
            foreach (var value in items)
            {
                if (value is DrawableListItem<SelectionBlueprint<ISerialisableDrawable>> item)
                {
                    if (!predicate(item)) return false;
                }
                else if (value is DrawableMinimisableList<SelectionBlueprint<ISerialisableDrawable>> minimisableList)
                {
                    if (!applyToItems(predicate, minimisableList.List.ItemMaps.Values)) return false;
                }
            }

            return true;
        }

        private bool testElementSelected(int element) => ((DrawableListItem<SelectionBlueprint<ISerialisableDrawable>>)DrawableList.ItemMaps[DrawableList.Items[element]]).IsSelected;

        protected void ListAddItems(Func<DrawableList<SelectionBlueprint<ISerialisableDrawable>>> listSupplier)
        {
            const int item_number = 10;

            int before = 0;
            AddStep("Get Item Count", () => before = listSupplier().Items.Count);
            AddStep($"add {item_number} items", () =>
            {
                int items = 0;
                AddElement(Enumerable.Range(0, item_number)
                                     .Select(static _ => new TextElement()),
                    listSupplier().Items,
                    () => "Element" + (item_number - ++items)
                );
            });
            AddAssert($"Exactly {item_number} items were added", () => listSupplier().Items.Count == before + item_number);
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
                        && drawable1.Depth > drawable2.Depth) return false;
                }

                return true;
            });
        }

        [Test, Order(2)]
        public void TestListSelection()
        {
            ListAddItems(() => DrawableList);
            //start with regular clicks
            AddAssert("no Item is selected", () => applyToItems(static t => !t.IsSelected, DrawableList.ItemMaps.Values));
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

        [Test, Order(1)]
        public void TestListDrag()
        {
            ListAddItems(() => DrawableList);
            AddStep("Mouse to first element", () =>
            {
                var first = DrawableList.ItemMaps[DrawableList.Items[0]];
                InputManager.MoveMouseTo(first);
            });
            AddStep("Mouse Down", () => InputManager.PressButton(MouseButton.Left));
            AddStep("Mouse Move", () =>
            {
                var last = DrawableList.ItemMaps[DrawableList.Items[^1]];
                InputManager.MoveMouseTo(last.ToScreenSpace(Vector2.UnitY * (last.BoundingBox.Height / 2 + 1)));
            });
            AddStep("Mouse Up", () => InputManager.ReleaseButton(MouseButton.Left));
            AddAssert("Check Elements have been swapped", () =>
            {
                for (int i = 2; i < DrawableList.Items.Count; i++)
                {
                    var selectionBlueprint = DrawableList.ItemMaps[DrawableList.Items[i]].Model.RepresentedItem;

                    int expectedNum = (i + 1) % DrawableList.Items.Count;
                    string name = ((Drawable)selectionBlueprint.Item).Name;

                    if (name != $"Element{expectedNum}")
                    {
                        Logger.Log($"Element {i} doesn't have the correct name, Expected 'Element{expectedNum}' but got '{name}'");
                        return false;
                    }
                }

                return true;
            });
            checkDepth();
        }
    }
}
