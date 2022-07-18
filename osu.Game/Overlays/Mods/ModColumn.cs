// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Overlays.Mods.Input;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mods
{
    public class ModColumn : CompositeDrawable
    {
        public readonly Container TopLevelContent;

        public readonly ModType ModType;

        private IReadOnlyList<ModState> availableMods = Array.Empty<ModState>();

        /// <summary>
        /// Sets the list of mods to show in this column.
        /// </summary>
        public IReadOnlyList<ModState> AvailableMods
        {
            get => availableMods;
            set
            {
                Debug.Assert(value.All(mod => mod.Mod.Type == ModType));

                availableMods = value;

                foreach (var mod in availableMods)
                {
                    mod.Active.BindValueChanged(_ => updateState());
                    mod.Filtered.BindValueChanged(_ => updateState());
                }

                updateState();

                if (IsLoaded)
                    asyncLoadPanels();
            }
        }

        /// <summary>
        /// Determines whether this column should accept user input.
        /// </summary>
        public Bindable<bool> Active = new BindableBool(true);

        protected override bool ReceivePositionalInputAtSubTree(Vector2 screenSpacePos) => base.ReceivePositionalInputAtSubTree(screenSpacePos) && Active.Value;

        protected virtual ModPanel CreateModPanel(ModState mod) => new ModPanel(mod);

        private readonly bool allowIncompatibleSelection;

        private readonly TextFlowContainer headerText;
        private readonly Box headerBackground;
        private readonly Container contentContainer;
        private readonly Box contentBackground;
        private readonly FillFlowContainer<ModPanel> panelFlow;
        private readonly ToggleAllCheckbox? toggleAllCheckbox;

        private Colour4 accentColour;

        private Bindable<ModSelectHotkeyStyle> hotkeyStyle = null!;
        private IModHotkeyHandler hotkeyHandler = null!;

        private Task? latestLoadTask;
        internal bool ItemsLoaded => latestLoadTask == null;

        private const float header_height = 42;

        public ModColumn(ModType modType, bool allowIncompatibleSelection)
        {
            ModType = modType;
            this.allowIncompatibleSelection = allowIncompatibleSelection;

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

            if (allowIncompatibleSelection)
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
        private void load(OverlayColourProvider colourProvider, OsuColour colours, OsuConfigManager configManager)
        {
            headerBackground.Colour = accentColour = colours.ForModType(ModType);

            if (toggleAllCheckbox != null)
            {
                toggleAllCheckbox.AccentColour = accentColour;
                toggleAllCheckbox.AccentHoverColour = accentColour.Lighten(0.3f);
            }

            contentContainer.BorderColour = ColourInfo.GradientVertical(colourProvider.Background4, colourProvider.Background3);
            contentBackground.Colour = colourProvider.Background4;

            hotkeyStyle = configManager.GetBindable<ModSelectHotkeyStyle>(OsuSetting.ModSelectHotkeyStyle);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            toggleAllCheckbox?.Current.BindValueChanged(_ => updateToggleAllText(), true);
            hotkeyStyle.BindValueChanged(val => hotkeyHandler = createHotkeyHandler(val.NewValue), true);
            asyncLoadPanels();
        }

        private void updateToggleAllText()
        {
            Debug.Assert(toggleAllCheckbox != null);
            toggleAllCheckbox.LabelText = toggleAllCheckbox.Current.Value ? CommonStrings.DeselectAll : CommonStrings.SelectAll;
        }

        private CancellationTokenSource? cancellationTokenSource;

        private void asyncLoadPanels()
        {
            cancellationTokenSource?.Cancel();

            var panels = availableMods.Select(mod => CreateModPanel(mod).With(panel => panel.Shear = Vector2.Zero));

            Task? loadTask;

            latestLoadTask = loadTask = LoadComponentsAsync(panels, loaded =>
            {
                panelFlow.ChildrenEnumerable = loaded;
                updateState();
            }, (cancellationTokenSource = new CancellationTokenSource()).Token);
            loadTask.ContinueWith(_ =>
            {
                if (loadTask == latestLoadTask)
                    latestLoadTask = null;
            });
        }

        private void updateState()
        {
            Alpha = availableMods.All(mod => mod.Filtered.Value) ? 0 : 1;

            if (toggleAllCheckbox != null && !SelectionAnimationRunning)
            {
                toggleAllCheckbox.Alpha = availableMods.Any(panel => !panel.Filtered.Value) ? 1 : 0;
                toggleAllCheckbox.Current.Value = availableMods.Where(panel => !panel.Filtered.Value).All(panel => panel.Active.Value);
            }
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

            foreach (var button in availableMods.Where(b => !b.Active.Value && !b.Filtered.Value))
                pendingSelectionOperations.Enqueue(() => button.Active.Value = true);
        }

        /// <summary>
        /// Deselects all mods.
        /// </summary>
        public void DeselectAll()
        {
            pendingSelectionOperations.Clear();

            foreach (var button in availableMods.Where(b => b.Active.Value && !b.Filtered.Value))
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

        /// <summary>
        /// Creates an appropriate <see cref="IModHotkeyHandler"/> for this column's <see cref="ModType"/> and
        /// the supplied <paramref name="hotkeyStyle"/>.
        /// </summary>
        private IModHotkeyHandler createHotkeyHandler(ModSelectHotkeyStyle hotkeyStyle)
        {
            switch (ModType)
            {
                case ModType.DifficultyReduction:
                case ModType.DifficultyIncrease:
                case ModType.Automation:
                    return hotkeyStyle == ModSelectHotkeyStyle.Sequential
                        ? SequentialModHotkeyHandler.Create(ModType)
                        : new ClassicModHotkeyHandler(allowIncompatibleSelection);

                default:
                    return new NoopModHotkeyHandler();
            }
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ControlPressed || e.AltPressed || e.SuperPressed || e.Repeat)
                return false;

            return hotkeyHandler.HandleHotkeyPressed(e, availableMods);
        }

        #endregion
    }
}
