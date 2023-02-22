// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Skinning;
using osuTK;
using osuTK.Input;

namespace osu.Game.Overlays.SkinEditor
{
    public partial class SkinBlueprintContainer : BlueprintContainer<ISerialisableDrawable>
    {
        private readonly Drawable target;

        private readonly List<BindableList<ISerialisableDrawable>> targetComponents = new List<BindableList<ISerialisableDrawable>>();

        [Resolved]
        private SkinEditor editor { get; set; } = null!;

        public SkinBlueprintContainer(Drawable target)
        {
            this.target = target;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            SelectedItems.BindTo(editor.SelectedComponents);

            // track each target container on the current screen.
            var targetContainers = target.ChildrenOfType<ISerialisableDrawableContainer>().ToArray();

            if (targetContainers.Length == 0)
            {
                AddInternal(new NonSkinnableScreenPlaceholder());
                return;
            }

            foreach (var targetContainer in targetContainers)
            {
                var bindableList = new BindableList<ISerialisableDrawable> { BindTarget = targetContainer.Components };
                bindableList.BindCollectionChanged(componentsChanged, true);

                targetComponents.Add(bindableList);
            }
        }

        private void componentsChanged(object? sender, NotifyCollectionChangedEventArgs e) => Schedule(() =>
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    Debug.Assert(e.NewItems != null);

                    foreach (var item in e.NewItems.Cast<ISerialisableDrawable>())
                        AddBlueprintFor(item);
                    break;

                case NotifyCollectionChangedAction.Remove:
                case NotifyCollectionChangedAction.Reset:
                    Debug.Assert(e.OldItems != null);

                    foreach (var item in e.OldItems.Cast<ISerialisableDrawable>())
                        RemoveBlueprintFor(item);
                    break;

                case NotifyCollectionChangedAction.Replace:
                    Debug.Assert(e.NewItems != null);
                    Debug.Assert(e.OldItems != null);

                    foreach (var item in e.OldItems.Cast<ISerialisableDrawable>())
                        RemoveBlueprintFor(item);

                    foreach (var item in e.NewItems.Cast<ISerialisableDrawable>())
                        AddBlueprintFor(item);
                    break;
            }
        });

        protected override void AddBlueprintFor(ISerialisableDrawable item)
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

        protected override void SelectAll()
        {
            SelectedItems.AddRange(targetComponents.SelectMany(list => list).Except(SelectedItems).ToArray());
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

            SelectionHandler.HandleMovement(new MoveSelectionEvent<ISerialisableDrawable>(firstBlueprint, delta));
        }

        protected override SelectionHandler<ISerialisableDrawable> CreateSelectionHandler() => new SkinSelectionHandler();

        protected override SelectionBlueprint<ISerialisableDrawable> CreateBlueprintFor(ISerialisableDrawable component)
            => new SkinBlueprint(component);

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            foreach (var list in targetComponents)
                list.UnbindAll();
        }

        public partial class NonSkinnableScreenPlaceholder : CompositeDrawable
        {
            [Resolved]
            private SkinEditorOverlay? skinEditorOverlay { get; set; }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                RelativeSizeAxes = Axes.Both;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        Colour = colourProvider.Dark6,
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0.95f,
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Spacing = new Vector2(0, 5),
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new SpriteIcon
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Icon = FontAwesome.Solid.ExclamationCircle,
                                Size = new Vector2(24),
                                Y = -5,
                            },
                            new OsuTextFlowContainer(t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold, size: 18))
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                TextAnchor = Anchor.Centre,
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Text = "Please navigate to a skinnable screen using the scene library",
                            },
                            new RoundedButton
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Width = 200,
                                Margin = new MarginPadding { Top = 20 },
                                Action = () => skinEditorOverlay?.Hide(),
                                Text = "Return to game"
                            }
                        }
                    },
                };
            }
        }
    }
}
