// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

#nullable enable

namespace osu.Game.Overlays.Mods
{
    public class ModColumn : CompositeDrawable
    {
        public readonly Container TopLevelContent;

        public readonly ModType ModType;

        private Func<Mod, bool>? filter;

        /// <summary>
        /// Function determining whether each mod in the column should be displayed.
        /// A return value of <see langword="true"/> means that the mod is not filtered and therefore its corresponding panel should be displayed.
        /// A return value of <see langword="false"/> means that the mod is filtered out and therefore its corresponding panel should be hidden.
        /// </summary>
        public Func<Mod, bool>? Filter
        {
            get => filter;
            set
            {
                filter = value;
                updateFilter();
            }
        }

        public Bindable<IReadOnlyList<Mod>> SelectedMods = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private readonly Key[]? toggleKeys;

        private readonly Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> availableMods = new Bindable<Dictionary<ModType, IReadOnlyList<Mod>>>();

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
            Shear = new Vector2(ModPanel.SHEAR_X, 0);

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
                                    Shear = new Vector2(-ModPanel.SHEAR_X, 0),
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
                                                new NestedVerticalScrollContainer
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
                    LabelText = "Enable All",
                    Shear = new Vector2(-ModPanel.SHEAR_X, 0)
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
            availableMods.BindValueChanged(_ => Scheduler.AddOnce(updateMods));
            SelectedMods.BindValueChanged(_ =>
            {
                // if a load is in progress, don't try to update the selection - the load flow will do so.
                if (latestLoadTask == null)
                    updateActiveState();
            });
            updateMods();
        }

        private CancellationTokenSource? cancellationTokenSource;

        private void updateMods()
        {
            var newMods = ModUtils.FlattenMods(availableMods.Value.GetValueOrDefault(ModType) ?? Array.Empty<Mod>()).ToList();

            if (newMods.SequenceEqual(panelFlow.Children.Select(p => p.Mod)))
                return;

            cancellationTokenSource?.Cancel();

            var panels = newMods.Select(mod => new ModPanel(mod)
            {
                Shear = new Vector2(-ModPanel.SHEAR_X, 0)
            });

            Task? loadTask;

            latestLoadTask = loadTask = LoadComponentsAsync(panels, loaded =>
            {
                panelFlow.ChildrenEnumerable = loaded;

                updateActiveState();
                updateToggleAllState();
                updateFilter();

                foreach (var panel in panelFlow)
                {
                    panel.Active.BindValueChanged(_ =>
                    {
                        updateToggleAllState();
                        SelectedMods.Value = panel.Active.Value
                            ? SelectedMods.Value.Append(panel.Mod).ToArray()
                            : SelectedMods.Value.Except(new[] { panel.Mod }).ToArray();
                    });
                }
            }, (cancellationTokenSource = new CancellationTokenSource()).Token);
            loadTask.ContinueWith(_ =>
            {
                if (loadTask == latestLoadTask)
                    latestLoadTask = null;
            });
        }

        private void updateActiveState()
        {
            foreach (var panel in panelFlow)
                panel.Active.Value = SelectedMods.Value.Contains(panel.Mod, EqualityComparer<Mod>.Default);
        }

        #region Bulk select / deselect

        private const double initial_multiple_selection_delay = 120;

        private double selectionDelay = initial_multiple_selection_delay;
        private double lastSelection;

        private readonly Queue<Action> pendingSelectionOperations = new Queue<Action>();

        protected bool SelectionAnimationRunning => pendingSelectionOperations.Count > 0;

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

        private void updateToggleAllState()
        {
            if (toggleAllCheckbox != null && !SelectionAnimationRunning)
            {
                toggleAllCheckbox.Alpha = panelFlow.Any(panel => !panel.Filtered.Value) ? 1 : 0;
                toggleAllCheckbox.Current.Value = panelFlow.Where(panel => !panel.Filtered.Value).All(panel => panel.Active.Value);
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

        #region Filtering support

        private void updateFilter()
        {
            foreach (var modPanel in panelFlow)
                modPanel.ApplyFilter(Filter);

            updateToggleAllState();
        }

        #endregion

        #region Keyboard selection support

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed || e.AltPressed) return false;
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
