// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning.Editor
{
    public class SkinBlueprintContainer : BlueprintContainer<SkinnableHUDComponent>
    {
        private readonly Drawable target;

        public SkinBlueprintContainer(Drawable target)
        {
            this.target = target;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SkinnableHUDComponent[] components = target.ChildrenOfType<SkinnableHUDComponent>().ToArray();

            foreach (var c in components) AddBlueprintFor(c);
        }

        protected override SelectionHandler<SkinnableHUDComponent> CreateSelectionHandler() => new SkinSelectionHandler();

        protected override SelectionBlueprint<SkinnableHUDComponent> CreateBlueprintFor(SkinnableHUDComponent component)
            => new SkinBlueprint(component);
    }
}
