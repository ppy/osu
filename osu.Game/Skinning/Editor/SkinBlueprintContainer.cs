// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Skinning.Editor
{
    public class SkinBlueprintContainer : BlueprintContainer<ISkinnableComponent>
    {
        private readonly Drawable target;

        public SkinBlueprintContainer(Drawable target)
        {
            this.target = target;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            checkForComponents();
        }

        private void checkForComponents()
        {
            foreach (var c in target.ChildrenOfType<ISkinnableComponent>().ToArray()) AddBlueprintFor(c);

            Scheduler.AddDelayed(checkForComponents, 1000);
        }

        protected override SelectionHandler<ISkinnableComponent> CreateSelectionHandler() => new SkinSelectionHandler();

        protected override SelectionBlueprint<ISkinnableComponent> CreateBlueprintFor(ISkinnableComponent component)
            => new SkinBlueprint(component);
    }
}
