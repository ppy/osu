// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.List;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osu.Game.Skinning.Editor;
using osuTK;
using osuTK.Input;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneListMinimisable : TestSceneList
    {
        protected override IDrawableListItem<SelectionBlueprint<ISkinnableDrawable>> CreateDrawableList()
        {
            return new DrawableMinimisableList<SelectionBlueprint<ISkinnableDrawable>>(
                new SkinBlueprint(
                    new BigBlackBox
                    {
                        Name = "DrawableMinimisableList"
                    }));
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("Expand List", () =>
            {
                var backingDrawable = ((Drawable)BackingDrawableList);
                InputManager.MoveMouseTo(backingDrawable.ToScreenSpace(backingDrawable.LayoutRectangle.TopLeft));
                InputManager.Click(MouseButton.Left);
            });
            AddAssert("List is Expanded", () => ((DrawableMinimisableList<SelectionBlueprint<ISkinnableDrawable>>)BackingDrawableList).Enabled.Value && DrawableList.IsPresent);
        }

        protected override DrawableList<SelectionBlueprint<ISkinnableDrawable>> DrawableList => ((DrawableMinimisableList<SelectionBlueprint<ISkinnableDrawable>>)BackingDrawableList).List;

        [Test]
        public void TestListinListDrag()
        {
            AddRepeatStep("Add Lists", () =>
            {
                DrawableList
                    .Items
                    .Add(
                        new DrawableListRepresetedItem<SelectionBlueprint<ISkinnableDrawable>>(
                            new SkinBlueprint(
                                new BigBlackBox
                                {
                                    Name = "List" + DrawableList.Items.Count
                                }),
                            DrawableListEntryType.MinimisableList)
                    );
            }, 3);
            ListAddItems(DrawableList);
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
    }
}
