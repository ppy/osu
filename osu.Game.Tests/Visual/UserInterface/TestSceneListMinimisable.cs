// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Testing;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.List;
using osu.Game.Skinning;
using osu.Game.Skinning.Components;
using osu.Game.Skinning.Editor;

namespace osu.Game.Tests.Visual.UserInterface
{
    public partial class TestSceneListMinimisable : TestSceneList
    {
        protected override IDrawableListItem<SelectionBlueprint<ISkinnableDrawable>> CreateDrawableList()
        {
            var list = new DrawableMinimisableList<SelectionBlueprint<ISkinnableDrawable>>(
                new SkinBlueprint(
                    new BigBlackBox
                    {
                        Name = "DrawableMinimisableList"
                    }));
            AddStep("Expand List", () => list.ShowList());
            AddAssert("List is Expanded", () => list.Enabled.Value && (list.List?.IsPresent ?? false));
            return list;
        }

        protected override DrawableList<SelectionBlueprint<ISkinnableDrawable>> GetDrawableList(IDrawableListItem<SelectionBlueprint<ISkinnableDrawable>> list)
            => ((DrawableMinimisableList<SelectionBlueprint<ISkinnableDrawable>>)list).List ?? throw new NullReferenceException();
    }
}
