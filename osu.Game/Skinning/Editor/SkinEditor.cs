// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Events;
using osu.Framework.Testing;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Screens.Edit.Components.Menus;

namespace osu.Game.Skinning.Editor
{
    [Cached(typeof(SkinEditor))]
    public class SkinEditor : VisibilityContainer
    {
        public const double TRANSITION_DURATION = 500;

        public readonly BindableList<ISkinnableDrawable> SelectedComponents = new BindableList<ISkinnableDrawable>();

        protected override bool StartHidden => true;

        private Drawable targetScreen;

        private OsuTextFlowContainer headerText;

        private Bindable<Skin> currentSkin;

        [Resolved]
        private SkinManager skins { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        private bool hasBegunMutating;

        private Container content;

        public SkinEditor(Drawable targetScreen)
        {
            RelativeSizeAxes = Axes.Both;

            UpdateTargetScreen(targetScreen);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    new Container
                    {
                        Name = "Top bar",
                        RelativeSizeAxes = Axes.X,
                        Depth = float.MinValue,
                        Height = 40,
                        Children = new Drawable[]
                        {
                            new EditorMenuBar
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.Both,
                                Items = new[]
                                {
                                    new MenuItem("File")
                                    {
                                        Items = new[]
                                        {
                                            new EditorMenuItem("Save", MenuItemType.Standard, Save),
                                            new EditorMenuItem("Revert to default", MenuItemType.Destructive, revert),
                                            new EditorMenuItemSpacer(),
                                            new EditorMenuItem("Exit", MenuItemType.Standard, Hide),
                                        },
                                    },
                                }
                            },
                            headerText = new OsuTextFlowContainer
                            {
                                TextAnchor = Anchor.TopRight,
                                Padding = new MarginPadding(5),
                                Anchor = Anchor.TopRight,
                                Origin = Anchor.TopRight,
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                            },
                        },
                    },
                    new GridContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColumnDimensions = new[]
                        {
                            new Dimension(GridSizeMode.AutoSize),
                            new Dimension()
                        },
                        Content = new[]
                        {
                            new Drawable[]
                            {
                                new SkinComponentToolbox(600)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    RequestPlacement = placeComponent
                                },
                                content = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Show();

            // as long as the skin editor is loaded, let's make sure we can modify the current skin.
            currentSkin = skins.CurrentSkin.GetBoundCopy();

            // schedule ensures this only happens when the skin editor is visible.
            // also avoid some weird endless recursion / bindable feedback loop (something to do with tracking skins across three different bindable types).
            // probably something which will be factored out in a future database refactor so not too concerning for now.
            currentSkin.BindValueChanged(skin =>
            {
                hasBegunMutating = false;
                Scheduler.AddOnce(skinChanged);
            }, true);
        }

        public void UpdateTargetScreen(Drawable targetScreen)
        {
            this.targetScreen = targetScreen;

            Scheduler.AddOnce(loadBlueprintContainer);

            void loadBlueprintContainer()
            {
                content.Children = new Drawable[]
                {
                    new SkinBlueprintContainer(targetScreen),
                };
            }
        }

        private void skinChanged()
        {
            headerText.Clear();

            headerText.AddParagraph("Skin editor", cp => cp.Font = OsuFont.Default.With(size: 16));
            headerText.NewParagraph();
            headerText.AddText("Currently editing ", cp =>
            {
                cp.Font = OsuFont.Default.With(size: 12);
                cp.Colour = colours.Yellow;
            });

            headerText.AddText($"{currentSkin.Value.SkinInfo}", cp =>
            {
                cp.Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold);
                cp.Colour = colours.Yellow;
            });

            skins.EnsureMutableSkin();
            hasBegunMutating = true;
        }

        private void placeComponent(Type type)
        {
            var target = availableTargets.FirstOrDefault()?.Target;

            if (target == null)
                return;

            var targetContainer = getTarget(target.Value);

            if (targetContainer == null)
                return;

            if (!(Activator.CreateInstance(type) is ISkinnableDrawable component))
                throw new InvalidOperationException($"Attempted to instantiate a component for placement which was not an {typeof(ISkinnableDrawable)}.");

            var drawableComponent = (Drawable)component;

            // give newly added components a sane starting location.
            drawableComponent.Origin = Anchor.TopCentre;
            drawableComponent.Anchor = Anchor.TopCentre;
            drawableComponent.Y = targetContainer.DrawSize.Y / 2;

            targetContainer.Add(component);

            SelectedComponents.Clear();
            SelectedComponents.Add(component);
        }

        private IEnumerable<ISkinnableTarget> availableTargets => targetScreen.ChildrenOfType<ISkinnableTarget>();

        private ISkinnableTarget getTarget(SkinnableTarget target)
        {
            return availableTargets.FirstOrDefault(c => c.Target == target);
        }

        private void revert()
        {
            ISkinnableTarget[] targetContainers = availableTargets.ToArray();

            foreach (var t in targetContainers)
            {
                currentSkin.Value.ResetDrawableTarget(t);

                // add back default components
                getTarget(t.Target).Reload();
            }
        }

        public void Save()
        {
            if (!hasBegunMutating)
                return;

            ISkinnableTarget[] targetContainers = availableTargets.ToArray();

            foreach (var t in targetContainers)
                currentSkin.Value.UpdateDrawableTarget(t);

            skins.Save(skins.CurrentSkin.Value);
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        protected override void PopIn()
        {
            this.FadeIn(TRANSITION_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
        }

        public void DeleteItems(ISkinnableDrawable[] items)
        {
            foreach (var item in items)
                availableTargets.FirstOrDefault(t => t.Components.Contains(item))?.Remove(item);
        }
    }
}
