// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using Web = osu.Game.Resources.Localisation.Web;
using osu.Framework.Testing;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.OSD;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Components;
using osu.Game.Screens.Edit.Components.Menus;
using osu.Game.Skinning;

namespace osu.Game.Overlays.SkinEditor
{
    [Cached(typeof(SkinEditor))]
    public partial class SkinEditor : VisibilityContainer, ICanAcceptFiles, IKeyBindingHandler<PlatformAction>, IEditorChangeHandler
    {
        public const double TRANSITION_DURATION = 300;

        public const float MENU_HEIGHT = 40;

        public readonly BindableList<ISerialisableDrawable> SelectedComponents = new BindableList<ISerialisableDrawable>();

        protected override bool StartHidden => true;

        private Drawable? targetScreen;

        private OsuTextFlowContainer headerText = null!;

        private Bindable<Skin> currentSkin = null!;

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private EditorClipboard clipboard { get; set; } = null!;

        [Resolved]
        private SkinEditorOverlay? skinEditorOverlay { get; set; }

        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private readonly Bindable<ISkinComponentLookup?> selectedTarget = new Bindable<ISkinComponentLookup?>();

        private bool hasBegunMutating;

        private Container? content;

        private EditorSidebar componentsSidebar = null!;
        private EditorSidebar settingsSidebar = null!;

        private SkinEditorChangeHandler? changeHandler;

        private EditorMenuItem undoMenuItem = null!;
        private EditorMenuItem redoMenuItem = null!;

        private EditorMenuItem cutMenuItem = null!;
        private EditorMenuItem copyMenuItem = null!;
        private EditorMenuItem cloneMenuItem = null!;
        private EditorMenuItem pasteMenuItem = null!;

        private readonly BindableWithCurrent<bool> canCut = new BindableWithCurrent<bool>();
        private readonly BindableWithCurrent<bool> canCopy = new BindableWithCurrent<bool>();
        private readonly BindableWithCurrent<bool> canPaste = new BindableWithCurrent<bool>();

        [Resolved]
        private OnScreenDisplay? onScreenDisplay { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

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
                                Name = @"Menu container",
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
                                            new MenuItem(CommonStrings.MenuBarFile)
                                            {
                                                Items = new OsuMenuItem[]
                                                {
                                                    new EditorMenuItem(Web.CommonStrings.ButtonsSave, MenuItemType.Standard, () => Save()),
                                                    new EditorMenuItem(CommonStrings.Export, MenuItemType.Standard, () => skins.ExportCurrentSkin()) { Action = { Disabled = !RuntimeInfo.IsDesktop } },
                                                    new OsuMenuItemSpacer(),
                                                    new EditorMenuItem(CommonStrings.RevertToDefault, MenuItemType.Destructive, () => dialogOverlay?.Push(new RevertConfirmDialog(revert))),
                                                    new OsuMenuItemSpacer(),
                                                    new EditorMenuItem(CommonStrings.Exit, MenuItemType.Standard, () => skinEditorOverlay?.Hide()),
                                                },
                                            },
                                            new MenuItem(CommonStrings.MenuBarEdit)
                                            {
                                                Items = new OsuMenuItem[]
                                                {
                                                    undoMenuItem = new EditorMenuItem(CommonStrings.Undo, MenuItemType.Standard, Undo),
                                                    redoMenuItem = new EditorMenuItem(CommonStrings.Redo, MenuItemType.Standard, Redo),
                                                    new OsuMenuItemSpacer(),
                                                    cutMenuItem = new EditorMenuItem(CommonStrings.Cut, MenuItemType.Standard, Cut),
                                                    copyMenuItem = new EditorMenuItem(CommonStrings.Copy, MenuItemType.Standard, Copy),
                                                    pasteMenuItem = new EditorMenuItem(CommonStrings.Paste, MenuItemType.Standard, Paste),
                                                    cloneMenuItem = new EditorMenuItem(CommonStrings.Clone, MenuItemType.Standard, Clone),
                                                }
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
                                        componentsSidebar = new EditorSidebar(),
                                        content = new Container
                                        {
                                            Depth = float.MaxValue,
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        settingsSidebar = new EditorSidebar(),
                                    }
                                }
                            }
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            canCut.Current.BindValueChanged(cut => cutMenuItem.Action.Disabled = !cut.NewValue, true);
            canCopy.Current.BindValueChanged(copy =>
            {
                copyMenuItem.Action.Disabled = !copy.NewValue;
                cloneMenuItem.Action.Disabled = !copy.NewValue;
            }, true);
            canPaste.Current.BindValueChanged(paste => pasteMenuItem.Action.Disabled = !paste.NewValue, true);

            SelectedComponents.BindCollectionChanged((_, _) =>
            {
                canCopy.Value = canCut.Value = SelectedComponents.Any();
            }, true);

            clipboard.Content.BindValueChanged(content => canPaste.Value = !string.IsNullOrEmpty(content.NewValue), true);

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

            selectedTarget.BindValueChanged(targetChanged, true);
        }

        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            switch (e.Action)
            {
                case PlatformAction.Cut:
                    Cut();
                    return true;

                case PlatformAction.Copy:
                    Copy();
                    return true;

                case PlatformAction.Paste:
                    Paste();
                    return true;

                case PlatformAction.Undo:
                    Undo();
                    return true;

                case PlatformAction.Redo:
                    Redo();
                    return true;

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

            changeHandler?.Dispose();

            // Immediately clear the previous blueprint container to ensure it doesn't try to interact with the old target.
            if (content?.Child is SkinBlueprintContainer)
                content.Clear();

            Scheduler.AddOnce(loadBlueprintContainer);
            Scheduler.AddOnce(populateSettings);

            void loadBlueprintContainer()
            {
                selectedTarget.Default = getFirstTarget()?.Lookup;

                if (!availableTargets.Any(t => EqualityComparer<object>.Default.Equals(t.Lookup, selectedTarget.Value)))
                    selectedTarget.SetDefault();
            }
        }

        private void targetChanged(ValueChangedEvent<ISkinComponentLookup?> target)
        {
            foreach (var toolbox in componentsSidebar.OfType<SkinComponentToolbox>())
                toolbox.Expire();

            componentsSidebar.Clear();
            SelectedComponents.Clear();

            Debug.Assert(content != null);

            var skinComponentsContainer = getTarget(target.NewValue);

            if (target.NewValue == null || skinComponentsContainer == null)
            {
                content.Child = new NonSkinnableScreenPlaceholder();
                return;
            }

            changeHandler = new SkinEditorChangeHandler((Drawable)skinComponentsContainer);
            changeHandler.CanUndo.BindValueChanged(v => undoMenuItem.Action.Disabled = !v.NewValue, true);
            changeHandler.CanRedo.BindValueChanged(v => redoMenuItem.Action.Disabled = !v.NewValue, true);

            content.Child = new SkinBlueprintContainer(skinComponentsContainer);

            componentsSidebar.Children = new[]
            {
                new EditorSidebarSection("Current working layer")
                {
                    Children = new Drawable[]
                    {
                        new SettingsDropdown<ISkinComponentLookup?>
                        {
                            Items = availableTargets.Select(t => t.Lookup).Distinct(),
                            Current = selectedTarget,
                        }
                    }
                },
            };

            // If the new target has a ruleset, let's show ruleset-specific items at the top, and the rest below.
            if (target.NewValue.Ruleset != null)
            {
                componentsSidebar.Add(new SkinComponentToolbox(skinComponentsContainer, target.NewValue.Ruleset)
                {
                    RequestPlacement = requestPlacement
                });
            }

            // Remove the ruleset from the lookup to get base components.
            componentsSidebar.Add(new SkinComponentToolbox(skinComponentsContainer, null)
            {
                RequestPlacement = requestPlacement
            });

            void requestPlacement(Type type)
            {
                if (!(Activator.CreateInstance(type) is ISerialisableDrawable component))
                    throw new InvalidOperationException($"Attempted to instantiate a component for placement which was not an {typeof(ISerialisableDrawable)}.");

                SelectedComponents.Clear();
                placeComponent(component);
            }
        }

        private void skinChanged()
        {
            headerText.Clear();

            headerText.AddParagraph(SkinEditorStrings.SkinEditor, cp => cp.Font = OsuFont.Default.With(size: 16));
            headerText.NewParagraph();
            headerText.AddText(SkinEditorStrings.CurrentlyEditing, cp =>
            {
                cp.Font = OsuFont.Default.With(size: 12);
                cp.Colour = colours.Yellow;
            });

            headerText.AddText($" {currentSkin.Value.SkinInfo}", cp =>
            {
                cp.Font = OsuFont.Default.With(size: 12, weight: FontWeight.Bold);
                cp.Colour = colours.Yellow;
            });

            changeHandler?.Dispose();

            skins.EnsureMutableSkin();

            var targetContainer = getTarget(selectedTarget.Value);

            if (targetContainer != null)
                changeHandler = new SkinEditorChangeHandler((Drawable)targetContainer);
            hasBegunMutating = true;
        }

        /// <summary>
        /// Attempt to place a given component in the current target. If successful, the new component will be added to <see cref="SelectedComponents"/>.
        /// </summary>
        /// <param name="component">The component to be placed.</param>
        /// <param name="applyDefaults">Whether to apply default anchor / origin / position values.</param>
        /// <returns>Whether placement succeeded. Could fail if no target is available, or if the current target has missing dependency requirements for the component.</returns>
        private bool placeComponent(ISerialisableDrawable component, bool applyDefaults = true)
        {
            var targetContainer = getTarget(selectedTarget.Value);

            if (targetContainer == null)
                return false;

            var drawableComponent = (Drawable)component;

            if (applyDefaults)
            {
                // give newly added components a sane starting location.
                drawableComponent.Origin = Anchor.TopCentre;
                drawableComponent.Anchor = Anchor.TopCentre;
                drawableComponent.Y = targetContainer.DrawSize.Y / 2;
            }

            try
            {
                targetContainer.Add(component);
            }
            catch
            {
                // May fail if dependencies are not available, for instance.
                return false;
            }

            SelectedComponents.Add(component);
            return true;
        }

        private void populateSettings()
        {
            settingsSidebar.Clear();

            foreach (var component in SelectedComponents.OfType<Drawable>())
                settingsSidebar.Add(new SkinSettingsToolbox(component));
        }

        private IEnumerable<ISerialisableDrawableContainer> availableTargets => targetScreen.ChildrenOfType<ISerialisableDrawableContainer>();

        private ISerialisableDrawableContainer? getFirstTarget() => availableTargets.FirstOrDefault();

        private ISerialisableDrawableContainer? getTarget(ISkinComponentLookup? target)
        {
            if (target is not ISkinComponentLookup lookup)
                return null;

            return availableTargets.FirstOrDefault(c => c.Lookup.Equals(lookup));
        }

        private void revert()
        {
            ISerialisableDrawableContainer[] targetContainers = availableTargets.ToArray();

            foreach (var t in targetContainers)
            {
                currentSkin.Value.ResetDrawableTarget(t);

                // add back default components
                getTarget(t.Lookup)?.Reload();
            }
        }

        protected void Cut()
        {
            Copy();
            DeleteItems(SelectedComponents.ToArray());
        }

        protected void Copy()
        {
            clipboard.Content.Value = JsonConvert.SerializeObject(SelectedComponents.Cast<Drawable>().Select(s => s.CreateSerialisedInfo()).ToArray());
        }

        protected void Clone()
        {
            // Avoid attempting to clone if copying is not available (as it may result in pasting something unexpected).
            if (!canCopy.Value)
                return;

            Copy();
            Paste();
        }

        protected void Paste()
        {
            if (!canPaste.Value)
                return;

            changeHandler?.BeginChange();

            var drawableInfo = JsonConvert.DeserializeObject<SerialisedDrawableInfo[]>(clipboard.Content.Value);

            if (drawableInfo == null)
                return;

            var instances = drawableInfo.Select(d => d.CreateInstance())
                                        .OfType<ISerialisableDrawable>()
                                        .ToArray();

            SelectedComponents.Clear();

            foreach (var i in instances)
                placeComponent(i, false);

            changeHandler?.EndChange();
        }

        protected void Undo() => changeHandler?.RestoreState(-1);

        protected void Redo() => changeHandler?.RestoreState(1);

        public void Save(bool userTriggered = true)
        {
            if (!hasBegunMutating)
                return;

            if (targetScreen?.IsLoaded != true)
                return;

            ISerialisableDrawableContainer[] targetContainers = availableTargets.ToArray();

            if (!targetContainers.All(c => c.ComponentsLoaded))
                return;

            foreach (var t in targetContainers)
                currentSkin.Value.UpdateDrawableTarget(t);

            // In the case the save was user triggered, always show the save message to make them feel confident.
            if (skins.Save(skins.CurrentSkin.Value) || userTriggered)
                onScreenDisplay?.Display(new SkinEditorToast(ToastStrings.SkinSaved, currentSkin.Value.SkinInfo.ToString() ?? "Unknown"));
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
            this.FadeIn(TRANSITION_DURATION, Easing.OutQuint);
        }

        protected override void PopOut()
        {
            this.FadeOut(TRANSITION_DURATION, Easing.OutQuint);
        }

        public void DeleteItems(ISerialisableDrawable[] items)
        {
            changeHandler?.BeginChange();

            foreach (var item in items)
                availableTargets.FirstOrDefault(t => t.Components.Contains(item))?.Remove(item, true);

            changeHandler?.EndChange();
        }

        public void BringSelectionToFront()
        {
            if (getTarget(selectedTarget.Value) is not SkinComponentsContainer target)
                return;

            changeHandler?.BeginChange();

            // Iterating by target components order ensures we maintain the same order across selected components, regardless
            // of the order they were selected in.
            foreach (var d in target.Components.ToArray())
            {
                if (!SelectedComponents.Contains(d))
                    continue;

                target.Remove(d, false);

                // Selection would be reset by the remove.
                SelectedComponents.Add(d);
                target.Add(d);
            }

            changeHandler?.EndChange();
        }

        public void SendSelectionToBack()
        {
            if (getTarget(selectedTarget.Value) is not SkinComponentsContainer target)
                return;

            changeHandler?.BeginChange();

            foreach (var d in target.Components.ToArray())
            {
                if (SelectedComponents.Contains(d))
                    continue;

                target.Remove(d, false);
                target.Add(d);
            }

            changeHandler?.EndChange();
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

                var skinnableTarget = getFirstTarget();

                // Import still should happen for now, even if not placeable (as it allows a user to import skin resources that would apply to legacy gameplay skins).
                if (skinnableTarget == null)
                    return;

                // place component
                var sprite = new SkinnableSprite
                {
                    SpriteName = { Value = file.Name },
                    Origin = Anchor.Centre,
                    Position = skinnableTarget.ToLocalSpace(GetContainingInputManager().CurrentState.Mouse.Position),
                };

                SelectedComponents.Clear();
                placeComponent(sprite, false);

                SkinSelectionHandler.ApplyClosestAnchor(sprite);
            });

            return Task.CompletedTask;
        }

        Task ICanAcceptFiles.Import(ImportTask[] tasks, ImportParameters parameters) => throw new NotImplementedException();

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

        public partial class RevertConfirmDialog : DangerousActionDialog
        {
            public RevertConfirmDialog(Action revert)
            {
                HeaderText = CommonStrings.RevertToDefault;
                BodyText = SkinEditorStrings.RevertToDefaultDescription;
                DangerousAction = revert;
            }
        }

        #region Delegation of IEditorChangeHandler

        public event Action? OnStateChange
        {
            add => throw new NotImplementedException();
            remove => throw new NotImplementedException();
        }

        private IEditorChangeHandler? beginChangeHandler;

        public void BeginChange()
        {
            // Change handler may change between begin and end, which can cause unbalanced operations.
            // Let's track the one that was used when beginning the change so we can call EndChange on it specifically.
            (beginChangeHandler = changeHandler)?.BeginChange();
        }

        public void EndChange() => beginChangeHandler?.EndChange();
        public void SaveState() => changeHandler?.SaveState();

        #endregion
    }
}
