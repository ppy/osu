// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public class ModColumn : CompositeDrawable
    {
        public readonly Container TopLevelContent;

        public readonly ModType ModType;

        private Func<Mod, bool>? filter;

        /// <summary>
        /// A function determining whether each mod in the column should be displayed.
        /// A return value of <see langword="true"/> means that the mod is not filtered and therefore its corresponding panel should be displayed.
        /// A return value of <see langword="false"/> means that the mod is filtered out and therefore its corresponding panel should be hidden.
        /// </summary>
        public Func<Mod, bool>? Filter
        {
            get => filter;
            set
            {
                filter = value;
                updateState();
            }
        }

        /// <summary>
        /// Determines whether this column should accept user input.
        /// </summary>
        public Bindable<bool> Active = new BindableBool(true);

        private readonly Bindable<bool> allFiltered = new BindableBool();

        /// <summary>
        /// True if all of the panels in this column have been filtered out by the current <see cref="Filter"/>.
        /// </summary>
        public IBindable<bool> AllFiltered => allFiltered;

        /// <summary>
        /// List of mods marked as selected in this column.
        /// </summary>
        /// <remarks>
        /// Note that the mod instances returned by this property are owned solely by this column
        /// (as in, they are locally-managed clones, to ensure proper isolation from any other external instances).
        /// </remarks>
        public IReadOnlyList<Mod> SelectedMods { get; private set; } = Array.Empty<Mod>();

        /// <summary>
        /// Invoked when a mod panel has been selected interactively by the user.
        /// </summary>
        public event Action? SelectionChangedByUser;

        protected override bool ReceivePositionalInputAtSubTree(Vector2 screenSpacePos) => base.ReceivePositionalInputAtSubTree(screenSpacePos) && Active.Value;

        protected virtual ModPanel CreateModPanel(Mod mod) => new ModPanel(mod);

        private readonly Key[]? toggleKeys;

        private readonly Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> availableMods = new Bindable<Dictionary<ModType, IReadOnlyList<Mod>>>();

        /// <summary>
        /// All mods that are available for the current ruleset in this particular column.
        /// </summary>
        /// <remarks>
        /// Note that the mod instances in this list are owned solely by this column
        /// (as in, they are locally-managed clones, to ensure proper isolation from any other external instances).
        /// </remarks>
        private IReadOnlyList<Mod> localAvailableMods = Array.Empty<Mod>();

        private readonly TextFlowContainer headerText;
        private readonly Box headerBackground;
        private readonly Container contentContainer;
        private readonly Box contentBackground;
        private readonly FillFlowContainer<ModPanel> panelFlow;
        private readonly ToggleAllCheckbox? toggleAllCheckbox;

        private Colour4 accentColour;

        private Task? latestLoadTask;
        internal bool ItemsLoaded => latestLoadTask == null;

        private const float header_height = 42;

        public ModColumn(ModType modType, bool allowBulkSelection, Key[]? toggleKeys = null)
        {
            ModType = modType;
            this.toggleKeys = toggleKeys;

            Width = 320;
            RelativeSizeAxes = Axes.Y;
            Shear = new Vector2(ShearedOverlayContainer.SHEAR, 0);

            Container controlContainer;
            InternalChildren = new Drawable[]
            {
                TopLevelContent = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    CornerRadius = ModPanel.CORNER_RADIUS,
                    Masking = true,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = header_height + ModPanel.CORNER_RADIUS,
                            Children = new Drawable[]
                            {
                                headerBackground = new Box
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Height = header_height + ModPanel.CORNER_RADIUS
                                },
                                headerText = new OsuTextFlowContainer(t =>
                                {
                                    t.Font = OsuFont.TorusAlternate.With(size: 17);
                                    t.Shadow = false;
                                    t.Colour = Colour4.Black;
                                })
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0),
                                    Padding = new MarginPadding
                                    {
                                        Horizontal = 17,
                                        Bottom = ModPanel.CORNER_RADIUS
                                    }
                                }
                            }
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Top = header_height },
                            Child = contentContainer = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Masking = true,
                                CornerRadius = ModPanel.CORNER_RADIUS,
                                BorderThickness = 3,
                                Children = new Drawable[]
                                {
                                    contentBackground = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both
                                    },
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        RowDimensions = new[]
                                        {
                                            new Dimension(GridSizeMode.AutoSize),
                                            new Dimension()
                                        },
                                        Content = new[]
                                        {
                                            new Drawable[]
                                            {
                                                controlContainer = new Container
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    Padding = new MarginPadding { Horizontal = 14 }
                                                }
                                            },
                                            new Drawable[]
                                            {
                                                new OsuScrollContainer(Direction.Vertical)
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    ClampExtension = 100,
                                                    ScrollbarOverlapsContent = false,
                                                    Child = panelFlow = new FillFlowContainer<ModPanel>
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        AutoSizeAxes = Axes.Y,
                                                        Spacing = new Vector2(0, 7),
                                                        Padding = new MarginPadding(7)
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            createHeaderText();

            if (allowBulkSelection)
            {
                controlContainer.Height = 35;
                controlContainer.Add(toggleAllCheckbox = new ToggleAllCheckbox(this)
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Scale = new Vector2(0.8f),
                    RelativeSizeAxes = Axes.X,
                    Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0)
                });
                panelFlow.Padding = new MarginPadding
                {
                    Top = 0,
                    Bottom = 7,
                    Horizontal = 7
                };
            }
        }

        private void createHeaderText()
        {
            IEnumerable<string> headerTextWords = ModType.Humanize(LetterCasing.Title).Split(' ');

            if (headerTextWords.Count() > 1)
            {
                headerText.AddText($"{headerTextWords.First()} ", t => t.Font = t.Font.With(weight: FontWeight.SemiBold));
                headerTextWords = headerTextWords.Skip(1);
            }

            headerText.AddText(string.Join(' ', headerTextWords));
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OverlayColourProvider colourProvider, OsuColour colours)
        {
            availableMods.BindTo(game.AvailableMods);
            updateLocalAvailableMods(asyncLoadContent: false);
            availableMods.BindValueChanged(_ => updateLocalAvailableMods(asyncLoadContent: true));

            headerBackground.Colour = accentColour = colours.ForModType(ModType);

            if (toggleAllCheckbox != null)
            {
                toggleAllCheckbox.AccentColour = accentColour;
                toggleAllCheckbox.AccentHoverColour = accentColour.Lighten(0.3f);
            }

            contentContainer.BorderColour = ColourInfo.GradientVertical(colourProvider.Background4, colourProvider.Background3);
            contentBackground.Colour = colourProvider.Background4;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            toggleAllCheckbox?.Current.BindValueChanged(_ => updateToggleAllText(), true);
        }

        private void updateToggleAllText()
        {
            Debug.Assert(toggleAllCheckbox != null);
            toggleAllCheckbox.LabelText = toggleAllCheckbox.Current.Value ? CommonStrings.DeselectAll : CommonStrings.SelectAll;
        }

        private void updateLocalAvailableMods(bool asyncLoadContent)
        {
            var newMods = ModUtils.FlattenMods(availableMods.Value.GetValueOrDefault(ModType) ?? Array.Empty<Mod>())
                                  .Select(m => m.DeepClone())
                                  .ToList();

            if (newMods.SequenceEqual(localAvailableMods))
                return;

            localAvailableMods = newMods;

            if (asyncLoadContent)
                asyncLoadPanels();
            else
                onPanelsLoaded(createPanels());
        }

        private CancellationTokenSource? cancellationTokenSource;

        private void asyncLoadPanels()
        {
            cancellationTokenSource?.Cancel();

            var panels = createPanels();

            Task? loadTask;

            latestLoadTask = loadTask = LoadComponentsAsync(panels, onPanelsLoaded, (cancellationTokenSource = new CancellationTokenSource()).Token);
            loadTask.ContinueWith(_ =>
            {
                if (loadTask == latestLoadTask)
                    latestLoadTask = null;
            });
        }

        private IEnumerable<ModPanel> createPanels()
        {
            var panels = localAvailableMods.Select(mod => CreateModPanel(mod).With(panel => panel.Shear = new Vector2(-ShearedOverlayContainer.SHEAR, 0)));
            return panels;
        }

        private void onPanelsLoaded(IEnumerable<ModPanel> loaded)
        {
            panelFlow.ChildrenEnumerable = loaded;

            updateState();

            foreach (var panel in panelFlow)
            {
                panel.Active.BindValueChanged(_ => panelStateChanged(panel));
            }
        }

        private void updateState()
        {
            foreach (var panel in panelFlow)
            {
                panel.Active.Value = SelectedMods.Contains(panel.Mod);
                panel.ApplyFilter(Filter);
            }

            allFiltered.Value = panelFlow.All(panel => panel.Filtered.Value);

            if (toggleAllCheckbox != null && !SelectionAnimationRunning)
            {
                toggleAllCheckbox.Alpha = panelFlow.Any(panel => !panel.Filtered.Value) ? 1 : 0;
                toggleAllCheckbox.Current.Value = panelFlow.Where(panel => !panel.Filtered.Value).All(panel => panel.Active.Value);
            }
        }

        /// <summary>
        /// This flag helps to determine the source of changes to <see cref="SelectedMods"/>.
        /// If the value is false, then <see cref="SelectedMods"/> are changing due to a user selection on the UI.
        /// If the value is true, then <see cref="SelectedMods"/> are changing due to an external <see cref="SetSelection"/> call.
        /// </summary>
        private bool externalSelectionUpdateInProgress;

        private void panelStateChanged(ModPanel panel)
        {
            if (externalSelectionUpdateInProgress)
                return;

            var newSelectedMods = panel.Active.Value
                ? SelectedMods.Append(panel.Mod)
                : SelectedMods.Except(panel.Mod.Yield());

            SelectedMods = newSelectedMods.ToArray();
            updateState();
            SelectionChangedByUser?.Invoke();
        }

        /// <summary>
        /// Adjusts the set of selected mods in this column to match the passed in <paramref name="mods"/>.
        /// </summary>
        /// <remarks>
        /// This method exists to be able to receive mod instances that come from potentially-external sources and to copy the changes across to this column's state.
        /// <see cref="ModSelectScreen"/> uses this to substitute any external mod references in <see cref="ModSelectScreen.SelectedMods"/>
        /// to references that are owned by this column.
        /// </remarks>
        internal void SetSelection(IReadOnlyList<Mod> mods)
        {
            externalSelectionUpdateInProgress = true;

            var newSelection = new List<Mod>();

            foreach (var mod in localAvailableMods)
            {
                var matchingSelectedMod = mods.SingleOrDefault(selected => selected.GetType() == mod.GetType());

                if (matchingSelectedMod != null)
                {
                    mod.CopyFrom(matchingSelectedMod);
                    newSelection.Add(mod);
                }
                else
                {
                    mod.ResetSettingsToDefaults();
                }
            }

            SelectedMods = newSelection;
            updateState();

            externalSelectionUpdateInProgress = false;
        }

        #region Bulk select / deselect

        private const double initial_multiple_selection_delay = 120;

        private double selectionDelay = initial_multiple_selection_delay;
        private double lastSelection;

        private readonly Queue<Action> pendingSelectionOperations = new Queue<Action>();

        internal bool SelectionAnimationRunning => pendingSelectionOperations.Count > 0;

        protected override void Update()
        {
            base.Update();

            if (selectionDelay == initial_multiple_selection_delay || Time.Current - lastSelection >= selectionDelay)
            {
                if (pendingSelectionOperations.TryDequeue(out var dequeuedAction))
                {
                    dequeuedAction();

                    // each time we play an animation, we decrease the time until the next animation (to ramp the visual and audible elements).
                    selectionDelay = Math.Max(30, selectionDelay * 0.8f);
                    lastSelection = Time.Current;
                }
                else
                {
                    // reset the selection delay after all animations have been completed.
                    // this will cause the next action to be immediately performed.
                    selectionDelay = initial_multiple_selection_delay;
                }
            }
        }

        /// <summary>
        /// Selects all mods.
        /// </summary>
        public void SelectAll()
        {
            pendingSelectionOperations.Clear();

            foreach (var button in panelFlow.Where(b => !b.Active.Value && !b.Filtered.Value))
                pendingSelectionOperations.Enqueue(() => button.Active.Value = true);
        }

        /// <summary>
        /// Deselects all mods.
        /// </summary>
        public void DeselectAll()
        {
            pendingSelectionOperations.Clear();

            foreach (var button in panelFlow.Where(b => b.Active.Value && !b.Filtered.Value))
                pendingSelectionOperations.Enqueue(() => button.Active.Value = false);
        }

        /// <summary>
        /// Run any delayed selections (due to animation) immediately to leave mods in a good (final) state.
        /// </summary>
        public void FlushPendingSelections()
        {
            while (pendingSelectionOperations.TryDequeue(out var dequeuedAction))
                dequeuedAction();
        }

        private class ToggleAllCheckbox : OsuCheckbox
        {
            private Color4 accentColour;

            public Color4 AccentColour
            {
                get => accentColour;
                set
                {
                    accentColour = value;
                    updateState();
                }
            }

            private Color4 accentHoverColour;

            public Color4 AccentHoverColour
            {
                get => accentHoverColour;
                set
                {
                    accentHoverColour = value;
                    updateState();
                }
            }

            private readonly ModColumn column;

            public ToggleAllCheckbox(ModColumn column)
                : base(false)
            {
                this.column = column;
            }

            protected override void ApplyLabelParameters(SpriteText text)
            {
                base.ApplyLabelParameters(text);
                text.Font = text.Font.With(weight: FontWeight.SemiBold);
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                updateState();
            }

            private void updateState()
            {
                Nub.AccentColour = AccentColour;
                Nub.GlowingAccentColour = AccentHoverColour;
                Nub.GlowColour = AccentHoverColour.Opacity(0.2f);
            }

            protected override void OnUserChange(bool value)
            {
                if (value)
                    column.SelectAll();
                else
                    column.DeselectAll();
            }
        }

        #endregion

        #region Keyboard selection support

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed || e.AltPressed || e.SuperPressed) return false;
            if (toggleKeys == null) return false;

            int index = Array.IndexOf(toggleKeys, e.Key);
            if (index < 0) return false;

            var panel = panelFlow.ElementAtOrDefault(index);
            if (panel == null || panel.Filtered.Value) return false;

            panel.Active.Toggle();
            return true;
        }

        #endregion
    }
}
