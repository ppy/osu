// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Screens.Play.HUD;

namespace osu.Game.Skinning.Editor
{
    public class SkinBlueprintContainer : BlueprintContainer<ISkinnableComponent>
    {
        private readonly Drawable target;

        public SkinBlueprintContainer(Drawable target)
        {
            this.target = target;
        }

        [BackgroundDependencyLoader(true)]
        private void load(SkinEditor editor)
        {
            SelectedItems.BindTo(editor.SelectedComponents);
        }

        private readonly List<BindableList<ISkinnableComponent>> targetComponents = new List<BindableList<ISkinnableComponent>>();

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // track each target container on the current screen.
            foreach (var targetContainer in target.ChildrenOfType<SkinnableElementTargetContainer>())
            {
                var bindableList = new BindableList<ISkinnableComponent> { BindTarget = targetContainer.Components };
                bindableList.BindCollectionChanged(componentsChanged, true);

                targetComponents.Add(bindableList);
            }
        }

        private void componentsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<ISkinnableComponent>())
                        AddBlueprintFor(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in e.OldItems.Cast<ISkinnableComponent>())
                        RemoveBlueprintFor(item);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.OldItems.Cast<ISkinnableComponent>())
                        RemoveBlueprintFor(item);

                    foreach (var item in e.NewItems.Cast<ISkinnableComponent>())
                        AddBlueprintFor(item);
                    break;
            }
        }

        protected override SelectionHandler<ISkinnableComponent> CreateSelectionHandler() => new SkinSelectionHandler();

        protected override SelectionBlueprint<ISkinnableComponent> CreateBlueprintFor(ISkinnableComponent component)
            => new SkinBlueprint(component);
    }
}
