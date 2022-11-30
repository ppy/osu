// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays;
using osu.Game.Overlays.OSD;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Components.Menus;
using osu.Game.Screens.Edit.List;
using osu.Game.Skinning.Components;
using osuTK;

namespace osu.Game.Skinning.Editor
{
    [Cached(typeof(SkinEditor))]
    public partial class SkinEditor : VisibilityContainer, ICanAcceptFiles, IKeyBindingHandler<PlatformAction>
    {
        public const double TRANSITION_DURATION = 500;

        public const float MENU_HEIGHT = 40;

        public readonly BindableList<ISkinnableDrawable> SelectedComponents = new BindableList<ISkinnableDrawable>();

        protected override bool StartHidden => true;

        private Drawable targetScreen = null!;

        private OsuTextFlowContainer headerText = null!;

        private Bindable<Skin> currentSkin = null!;

        [Resolved(canBeNull: true)]
        private OsuGame? game { get; set; }

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved(canBeNull: true)]
        private SkinEditorOverlay? skinEditorOverlay { get; set; }

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private bool hasBegunMutating;

        private readonly Container content = new Container
        {
            Depth = float.MaxValue,
            RelativeSizeAxes = Axes.Both,
        };

        private readonly EditorSidebar componentsSidebar = new EditorSidebar();
        private readonly EditorSidebar settingsSidebar = new EditorSidebar();
        private readonly EditorSidebarSection layerSidebarSection = new EditorSidebarSection(@"Layer Editor");

        public readonly DrawableMinimisableList<SelectionBlueprint<ISkinnableDrawable>> LayerSidebarList
            = new DrawableMinimisableList<SelectionBlueprint<ISkinnableDrawable>>(new SkinBlueprint(new BigBlackBox
            {
                Name = "DrawableMinimisableList"
            }));

        [Resolved(canBeNull: true)]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        public SkinEditor()
        {
        }

        public SkinEditor(Drawable targetScreen)
        {
            UpdateTargetScreen(targetScreen);
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            EditorSidebar layerSidebar;

            RelativeSizeAxes = Axes.Both;

            InternalChild = new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(),
                    },

                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new Container
                            {
                                Name = "Menu container",
                                RelativeSizeAxes = Axes.X,
                                Depth = float.MinValue,
                                Height = MENU_HEIGHT,
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
                                                    new EditorMenuItem("Exit", MenuItemType.Standard, () => skinEditorOverlay?.Hide()),
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
                        },
                        new Drawable[]
                        {
                            new SkinEditorSceneLibrary
                            {
                                RelativeSizeAxes = Axes.X,
                            },
                        },
                        new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.Both,
                                ColumnDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                    new Dimension(),
                                    new Dimension(GridSizeMode.AutoSize),
                                },
                                Content = new[]
                                {
                                    new Drawable[]
                                    {
                                        componentsSidebar,
                                        content,
                                        new GridContainer
                                        {
                                            RelativeSizeAxes = Axes.Y,
                                            Width = EditorSidebar.WIDTH,
                                            RowDimensions = new[]
                                            {
                                                new Dimension(),
                                                new Dimension(),
                                            },
                                            Content = new[]
                                            {
                                                new Drawable[]
                                                {
                                                    settingsSidebar,
                                                },
                                                new Drawable[]
                                                {
                                                    layerSidebar = new EditorSidebar()
                                                }
                                            },
                                        }
                                    }
                                }
                            }
                        },
                    }
                }
            };

            layerSidebar.Add(layerSidebarSection);
            layerSidebarSection.Clear();
            initLayerEditor();
        }

        private void initLayerEditor()
        {
            LayerSidebarList.List?.Items.RemoveAll(_ => true);
            layerSidebarSection.Child = LayerSidebarList;
            LayerSidebarList.GetName = t => IDrawableListItem<SelectionBlueprint<ISkinnableDrawable>>.GetDefaultText((Drawable)t.Item);
            LayerSidebarList.SetItemDepth = (blueprint, depth) =>
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
            LayerSidebarList.OnDragAction();
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Show();

            game?.RegisterImportHandler(this);

            // as long as the skin editor is loaded, let's make sure we can modify the current skin.
            currentSkin = skins.CurrentSkin.GetBoundCopy();

            // schedule ensures this only happens when the skin editor is visible.
            // also avoid some weird endless recursion / bindable feedback loop (something to do with tracking skins across three different bindable types).
            // probably something which will be factored out in a future database refactor so not too concerning for now.
            currentSkin.BindValueChanged(_ =>
            {
                hasBegunMutating = false;
                Scheduler.AddOnce(skinChanged);
            }, true);

            SelectedComponents.BindCollectionChanged((_, _) => Scheduler.AddOnce(populateSettings), true);
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.Save:
                    if (e.Repeat)
                        return false;

                    Save();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        public void UpdateTargetScreen(Drawable targetScreen)
        {
            this.targetScreen = targetScreen;

            SelectedComponents.Clear();

            // Immediately clear the previous blueprint container to ensure it doesn't try to interact with the old target.
            content.Clear();

            Scheduler.AddOnce(loadBlueprintContainer);
            Scheduler.AddOnce(populateSettings);

            void loadBlueprintContainer()
            {
                SkinBlueprintContainer blueprintContainer = new SkinBlueprintContainer(targetScreen);
                content.Child = blueprintContainer;

                bool open = LayerSidebarList.Enabled.Value;
                initLayerEditor();

                LayerSidebarList.Enabled.Value = open;
                LayerSidebarList.List?.Items.AddRange(
                    blueprintContainer.SelectionBlueprints
                                      .Children
                                      .Select(item =>
                                          new DrawableListRepresetedItem<SelectionBlueprint<ISkinnableDrawable>>(item)
                                      ));

                componentsSidebar.Child = new SkinComponentToolbox(getFirstTarget() as CompositeDrawable)
                {
                    RequestPlacement = placeComponent
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
            if (!(Activator.CreateInstance(type) is ISkinnableDrawable component))
                throw new InvalidOperationException($"Attempted to instantiate a component for placement which was not an {typeof(ISkinnableDrawable)}.");

            placeComponent(component);
        }

        private void placeComponent(ISkinnableDrawable component, bool applyDefaults = true)
        {
            var targetContainer = getFirstTarget();

            if (targetContainer == null)
                return;

            var drawableComponent = (Drawable)component;

            if (applyDefaults)
            {
                // give newly added components a sane starting location.
                drawableComponent.Origin = Anchor.TopCentre;
                drawableComponent.Anchor = Anchor.TopCentre;
                drawableComponent.Y = targetContainer.DrawSize.Y / 2;
            }

            targetContainer.Add(component);

            SelectedComponents.Clear();
            SelectedComponents.Add(component);
        }

        private void populateSettings()
        {
            settingsSidebar.Clear();

            foreach (var component in SelectedComponents.OfType<Drawable>())
            {
                settingsSidebar?.Add(new SkinSettingsToolbox(component));
            }
        }

        private IEnumerable<ISkinnableTarget> availableTargets => targetScreen.ChildrenOfType<ISkinnableTarget>();

        private ISkinnableTarget? getFirstTarget() => availableTargets.FirstOrDefault();

        private ISkinnableTarget? getTarget(GlobalSkinComponentLookup.LookupType target)
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
                getTarget(t.Target)?.Reload();
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
            onScreenDisplay?.Display(new SkinEditorToast(ToastStrings.SkinSaved, currentSkin.Value.SkinInfo.ToString()));
        }

        protected override bool OnHover(HoverEvent e) => true;

        protected override bool OnMouseDown(MouseDownEvent e) => true;

        public override void Hide()
        {
            base.Hide();
            SelectedComponents.Clear();
        }

        protected override void PopIn()
        {
            this
                // align animation to happen after the majority of the ScalingContainer animation completes.
                .Delay(ScalingContainer.TRANSITION_DURATION * 0.3f)
                .FadeIn(TRANSITION_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
        }

        public void DeleteItems(ISkinnableDrawable[] items)
        {
            foreach (var item in items)
            {
                availableTargets.FirstOrDefault(t => t.Components.Contains(item))?.Remove(item);
            }
        }

        #region Drag & drop import handling

        public Task Import(params string[] paths)
        {
            Schedule(() =>
            {
                var file = new FileInfo(paths.First());

                // import to skin
                currentSkin.Value.SkinInfo.PerformWrite(skinInfo =>
                {
                    using (var contents = file.OpenRead())
                        skins.AddFile(skinInfo, contents, file.Name);
                });

                // Even though we are 100% on an update thread, we need to wait for realm callbacks to fire (to correctly invalidate caches in RealmBackedResourceStore).
                // See https://github.com/realm/realm-dotnet/discussions/2634#discussioncomment-2483573 for further discussion.
                // This is the best we can do for now.
                realm.Run(r => r.Refresh());

                // place component
                var sprite = new SkinnableSprite
                {
                    SpriteName = { Value = file.Name },
                    Origin = Anchor.Centre,
                    Position = getFirstTarget()?.ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position) ?? Vector2.Zero,
                };

                placeComponent(sprite, false);

                SkinSelectionHandler.ApplyClosestAnchor(sprite);
            });

            return Task.CompletedTask;
        }

        public Task Import(params ImportTask[] tasks) => throw new NotImplementedException();

        public IEnumerable<string> HandledExtensions => new[] { ".jpg", ".jpeg", ".png" };

        #endregion

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            game?.UnregisterImportHandler(this);
        }

        private partial class SkinEditorToast : Toast
        {
            public SkinEditorToast(LocalisableString value, string skinDisplayName)
                : base(SkinSettingsStrings.SkinLayoutEditor, value, skinDisplayName)
            {
            }
        }
    }
}
