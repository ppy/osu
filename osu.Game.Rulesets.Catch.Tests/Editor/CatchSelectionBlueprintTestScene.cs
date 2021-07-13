// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI.Scrolling;
using osu.Game.Tests.Visual;

namespace osu.Game.Rulesets.Catch.Tests.Editor
{
    public abstract class CatchSelectionBlueprintTestScene : SelectionBlueprintTestScene
    {
        protected ScrollingHitObjectContainer HitObjectContainer => contentContainer.Playfield.HitObjectContainer;

        protected override Container<Drawable> Content => contentContainer;

        private readonly CatchEditorTestSceneContainer contentContainer;

        protected CatchSelectionBlueprintTestScene()
        {
            base.Content.Add(contentContainer = new CatchEditorTestSceneContainer());
        }
    }
}
