// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Skinning.Editor
{
    public class SkinBlueprintContainer : BlueprintContainer<ISkinnableDrawable>
    {
        private readonly Drawable target;

        private readonly List<BindableList<ISkinnableDrawable>> targetComponents = new List<BindableList<ISkinnableDrawable>>();

        [Resolved]
        private SkinEditor editor { get; set; }

        public SkinBlueprintContainer(Drawable target)
        {
            this.target = target;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItems.BindTo(editor.SelectedComponents);

            // track each target container on the current screen.
            var targetContainers = target.ChildrenOfType<ISkinnableTarget>().ToArray();

            if (targetContainers.Length == 0)
            {
                string targetScreen = target.ChildrenOfType<Screen>().LastOrDefault()?.GetType().Name ?? "this screen";

                AddInternal(new ScreenWhiteBox.UnderConstructionMessage(targetScreen, "doesn't support skin customisation just yet."));
                return;
            }

            foreach (var targetContainer in targetContainers)
            {
                var bindableList = new BindableList<ISkinnableDrawable> { BindTarget = targetContainer.Components };
                bindableList.BindCollectionChanged(componentsChanged, true);

                targetComponents.Add(bindableList);
            }
        }

        private void componentsChanged(object sender, NotifyCollectionChangedEventArgs e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in e.NewItems.Cast<ISkinnableDrawable>())
                        AddBlueprintFor(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    foreach (var item in e.OldItems.Cast<ISkinnableDrawable>())
                        RemoveBlueprintFor(item);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (var item in e.OldItems.Cast<ISkinnableDrawable>())
                        RemoveBlueprintFor(item);

                    foreach (var item in e.NewItems.Cast<ISkinnableDrawable>())
                        AddBlueprintFor(item);
                    break;
            }
        });

        protected override void AddBlueprintFor(ISkinnableDrawable item)
        {
            if (!item.IsEditable)
                return;

            base.AddBlueprintFor(item);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            switch (e.Key)
            {
                case Key.Left:
                    moveSelection(new Vector2(-1, 0));
                    return true;

                case Key.Right:
                    moveSelection(new Vector2(1, 0));
                    return true;

                case Key.Up:
                    moveSelection(new Vector2(0, -1));
                    return true;

                case Key.Down:
                    moveSelection(new Vector2(0, 1));
                    return true;
            }

            return false;
        }

        /// <summary>
        /// Move the current selection spatially by the specified delta, in screen coordinates (ie. the same coordinates as the blueprints).
        /// </summary>
        /// <param name="delta"></param>
        private void moveSelection(Vector2 delta)
        {
            var firstBlueprint = SelectionHandler.SelectedBlueprints.FirstOrDefault();

            if (firstBlueprint == null)
                return;

            // convert to game space coordinates
            delta = firstBlueprint.ToScreenSpace(delta) - firstBlueprint.ToScreenSpace(Vector2.Zero);

            SelectionHandler.HandleMovement(new MoveSelectionEvent<ISkinnableDrawable>(firstBlueprint, delta));
        }

        protected override SelectionHandler<ISkinnableDrawable> CreateSelectionHandler() => new SkinSelectionHandler();

        protected override SelectionBlueprint<ISkinnableDrawable> CreateBlueprintFor(ISkinnableDrawable component)
            => new SkinBlueprint(component);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var list in targetComponents)
                list.UnbindAll();
        }
    }
}
