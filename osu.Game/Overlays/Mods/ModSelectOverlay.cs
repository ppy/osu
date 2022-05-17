// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public abstract class ModSelectOverlay : ShearedOverlayContainer, ISamplePlaybackDisabler
    {
        protected const int BUTTON_WIDTH = 200;

        [Cached]
        public Bindable<IReadOnlyList<Mod>> SelectedMods { get; private set; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        private Func<Mod, bool> isValidMod = m => true;

        /// <summary>
        /// A function determining whether each mod in the column should be displayed.
        /// A return value of <see langword="true"/> means that the mod is not filtered and therefore its corresponding panel should be displayed.
        /// A return value of <see langword="false"/> means that the mod is filtered out and therefore its corresponding panel should be hidden.
        /// </summary>
        public Func<Mod, bool> IsValidMod
        {
            get => isValidMod;
            set
            {
                isValidMod = value ?? throw new ArgumentNullException(nameof(value));
                filterMods();
            }
        }

        /// <summary>
        /// Whether the total score multiplier calculated from the current selected set of mods should be shown.
        /// </summary>
        protected virtual bool ShowTotalMultiplier => true;

        /// <summary>
        /// Whether per-mod customisation controls are visible.
        /// </summary>
        protected virtual bool AllowCustomisation => true;

        protected virtual ModColumn CreateModColumn(ModType modType, Key[]? toggleKeys = null) => new ModColumn(modType, false, toggleKeys);

        protected virtual IReadOnlyList<Mod> ComputeNewModsFromSelection(IReadOnlyList<Mod> oldSelection, IReadOnlyList<Mod> newSelection) => newSelection;

        protected virtual IEnumerable<ShearedButton> CreateFooterButtons()
        {
            if (AllowCustomisation)
            {
                yield return customisationButton = new ShearedToggleButton(BUTTON_WIDTH)
                {
                    Text = ModSelectOverlayStrings.ModCustomisation,
                    Active = { BindTarget = customisationVisible }
                };
            }

            yield return deselectAllButton = new ShearedButton(BUTTON_WIDTH)
            {
                Text = CommonStrings.DeselectAll,
                Action = DeselectAll
            };
        }

        private readonly Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> availableMods = new Bindable<Dictionary<ModType, IReadOnlyList<Mod>>>();
        private readonly Dictionary<ModType, IReadOnlyList<ModState>> localAvailableMods = new Dictionary<ModType, IReadOnlyList<ModState>>();
        private IEnumerable<ModState> allLocalAvailableMods => localAvailableMods.SelectMany(pair => pair.Value);

        private readonly BindableBool customisationVisible = new BindableBool();

        private ModSettingsArea modSettingsArea = null!;
        private ColumnScrollContainer columnScroll = null!;
        private ColumnFlowContainer columnFlow = null!;
        private FillFlowContainer<ShearedButton> footerButtonFlow = null!;
        private ShearedButton backButton = null!;

        private DifficultyMultiplierDisplay? multiplierDisplay;

        private ShearedToggleButton? customisationButton;
        private ShearedButton? deselectAllButton;

        protected ModSelectOverlay(OverlayColourScheme colourScheme = OverlayColourScheme.Green)
            : base(colourScheme)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuColour colours)
        {
            Header.Title = ModSelectOverlayStrings.ModSelectTitle;
            Header.Description = ModSelectOverlayStrings.ModSelectDescription;

            AddRange(new Drawable[]
            {
                new ClickToReturnContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    HandleMouse = { BindTarget = customisationVisible },
                    OnClicked = () => customisationVisible.Value = false
                },
                modSettingsArea = new ModSettingsArea
                {
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Height = 0
                }
            });

            MainAreaContent.AddRange(new Drawable[]
            {
                new Container
                {
                    Padding = new MarginPadding
                    {
                        Top = (ShowTotalMultiplier ? DifficultyMultiplierDisplay.HEIGHT : 0) + PADDING,
                        Bottom = PADDING
                    },
                    RelativeSizeAxes = Axes.Both,
                    RelativePositionAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        columnScroll = new ColumnScrollContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = false,
                            ClampExtension = 100,
                            ScrollbarOverlapsContent = false,
                            Child = columnFlow = new ColumnFlowContainer
                            {
                                Anchor = Anchor.BottomLeft,
                                Origin = Anchor.BottomLeft,
                                Direction = FillDirection.Horizontal,
                                Shear = new Vector2(SHEAR, 0),
                                RelativeSizeAxes = Axes.Y,
                                AutoSizeAxes = Axes.X,
                                Margin = new MarginPadding { Horizontal = 70 },
                                Padding = new MarginPadding { Bottom = 10 },
                                Children = new[]
                                {
                                    createModColumnContent(ModType.DifficultyReduction, new[] { Key.Q, Key.W, Key.E, Key.R, Key.T, Key.Y, Key.U, Key.I, Key.O, Key.P }),
                                    createModColumnContent(ModType.DifficultyIncrease, new[] { Key.A, Key.S, Key.D, Key.F, Key.G, Key.H, Key.J, Key.K, Key.L }),
                                    createModColumnContent(ModType.Automation, new[] { Key.Z, Key.X, Key.C, Key.V, Key.B, Key.N, Key.M }),
                                    createModColumnContent(ModType.Conversion),
                                    createModColumnContent(ModType.Fun)
                                }
                            }
                        }
                    }
                }
            });

            if (ShowTotalMultiplier)
            {
                MainAreaContent.Add(new Container
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    AutoSizeAxes = Axes.X,
                    Height = DifficultyMultiplierDisplay.HEIGHT,
                    Margin = new MarginPadding { Horizontal = 100 },
                    Child = multiplierDisplay = new DifficultyMultiplierDisplay
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre
                    },
                });
            }

            FooterContent.Child = footerButtonFlow = new FillFlowContainer<ShearedButton>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Horizontal,
                Anchor = Anchor.BottomLeft,
                Origin = Anchor.BottomLeft,
                Padding = new MarginPadding
                {
                    Vertical = PADDING,
                    Horizontal = 70
                },
                Spacing = new Vector2(10),
                ChildrenEnumerable = CreateFooterButtons().Prepend(backButton = new ShearedButton(BUTTON_WIDTH)
                {
                    Text = CommonStrings.Back,
                    Action = Hide,
                    DarkerColour = colours.Pink2,
                    LighterColour = colours.Pink1
                })
            };

            availableMods.BindTo(game.AvailableMods);
        }

        protected override void LoadComplete()
        {
            // this is called before base call so that the mod state is populated early, and the transition in `PopIn()` can play out properly.
            availableMods.BindValueChanged(_ => createLocalMods(), true);

            base.LoadComplete();

            State.BindValueChanged(_ => samplePlaybackDisabled.Value = State.Value == Visibility.Hidden, true);

            // This is an optimisation to prevent refreshing the available settings controls when it can be
            // reasonably assumed that the settings panel is never to be displayed (e.g. FreeModSelectOverlay).
            if (AllowCustomisation)
                ((IBindable<IReadOnlyList<Mod>>)modSettingsArea.SelectedMods).BindTo(SelectedMods);

            SelectedMods.BindValueChanged(val =>
            {
                updateMultiplier();
                updateCustomisation(val);
                updateFromExternalSelection();
            }, true);

            customisationVisible.BindValueChanged(_ => updateCustomisationVisualState(), true);

            // Start scrolled slightly to the right to give the user a sense that
            // there is more horizontal content available.
            ScheduleAfterChildren(() =>
            {
                columnScroll.ScrollTo(200, false);
                columnScroll.ScrollToStart();
            });
        }

        /// <summary>
        /// Select all visible mods in all columns.
        /// </summary>
        protected void SelectAll()
        {
            foreach (var column in columnFlow.Columns)
                column.SelectAll();
        }

        /// <summary>
        /// Deselect all visible mods in all columns.
        /// </summary>
        protected void DeselectAll()
        {
            foreach (var column in columnFlow.Columns)
                column.DeselectAll();
        }

        private ColumnDimContainer createModColumnContent(ModType modType, Key[]? toggleKeys = null)
        {
            var column = CreateModColumn(modType, toggleKeys).With(column =>
            {
                // spacing applied here rather than via `columnFlow.Spacing` to avoid uneven gaps when some of the columns are hidden.
                column.Margin = new MarginPadding { Right = 10 };
            });

            return new ColumnDimContainer(column)
            {
                AutoSizeAxes = Axes.X,
                RelativeSizeAxes = Axes.Y,
                RequestScroll = col => columnScroll.ScrollIntoView(col, extraScroll: 140),
            };
        }

        private void createLocalMods()
        {
            localAvailableMods.Clear();

            foreach (var (modType, mods) in availableMods.Value)
            {
                var modStates = mods.SelectMany(ModUtils.FlattenMod)
                                    .Select(mod => new ModState(mod.DeepClone()))
                                    .ToArray();

                foreach (var modState in modStates)
                    modState.Active.BindValueChanged(_ => updateFromInternalSelection());

                localAvailableMods[modType] = modStates;
            }

            filterMods();

            foreach (var column in columnFlow.Columns)
                column.AvailableMods = localAvailableMods.GetValueOrDefault(column.ModType, Array.Empty<ModState>());
        }

        private void filterMods()
        {
            foreach (var modState in allLocalAvailableMods)
                modState.Filtered.Value = !modState.Mod.HasImplementation || !IsValidMod.Invoke(modState.Mod);
        }

        private void updateMultiplier()
        {
            if (multiplierDisplay == null)
                return;

            double multiplier = 1.0;

            foreach (var mod in SelectedMods.Value)
                multiplier *= mod.ScoreMultiplier;

            multiplierDisplay.Current.Value = multiplier;
        }

        private void updateCustomisation(ValueChangedEvent<IReadOnlyList<Mod>> valueChangedEvent)
        {
            if (customisationButton == null)
                return;

            bool anyCustomisableMod = false;
            bool anyModWithRequiredCustomisationAdded = false;

            foreach (var mod in SelectedMods.Value)
            {
                anyCustomisableMod |= mod.GetSettingsSourceProperties().Any();
                anyModWithRequiredCustomisationAdded |= valueChangedEvent.OldValue.All(m => m.GetType() != mod.GetType()) && mod.RequiresConfiguration;
            }

            if (anyCustomisableMod)
            {
                customisationVisible.Disabled = false;

                if (anyModWithRequiredCustomisationAdded && !customisationVisible.Value)
                    customisationVisible.Value = true;
            }
            else
            {
                if (customisationVisible.Value)
                    customisationVisible.Value = false;

                customisationVisible.Disabled = true;
            }
        }

        private void updateCustomisationVisualState()
        {
            const double transition_duration = 300;

            MainAreaContent.FadeColour(customisationVisible.Value ? Colour4.Gray : Colour4.White, transition_duration, Easing.InOutCubic);

            foreach (var button in footerButtonFlow)
            {
                if (button != customisationButton)
                    button.Enabled.Value = !customisationVisible.Value;
            }

            float modAreaHeight = customisationVisible.Value ? ModSettingsArea.HEIGHT : 0;

            modSettingsArea.ResizeHeightTo(modAreaHeight, transition_duration, Easing.InOutCubic);
            TopLevelContent.MoveToY(-modAreaHeight, transition_duration, Easing.InOutCubic);
        }

        /// <summary>
        /// This flag helps to determine the source of changes to <see cref="SelectedMods"/>.
        /// If the value is false, then <see cref="SelectedMods"/> are changing due to a user selection on the UI.
        /// If the value is true, then <see cref="SelectedMods"/> are changing due to an external <see cref="SelectedMods"/> change.
        /// </summary>
        private bool externalSelectionUpdateInProgress;

        private void updateFromExternalSelection()
        {
            if (externalSelectionUpdateInProgress)
                return;

            externalSelectionUpdateInProgress = true;

            var newSelection = new List<Mod>();

            foreach (var modState in allLocalAvailableMods)
            {
                var matchingSelectedMod = SelectedMods.Value.SingleOrDefault(selected => selected.GetType() == modState.Mod.GetType());

                if (matchingSelectedMod != null)
                {
                    modState.Mod.CopyFrom(matchingSelectedMod);
                    modState.Active.Value = true;
                    newSelection.Add(modState.Mod);
                }
                else
                {
                    modState.Mod.ResetSettingsToDefaults();
                    modState.Active.Value = false;
                }
            }

            SelectedMods.Value = newSelection;

            externalSelectionUpdateInProgress = false;
        }

        private void updateFromInternalSelection()
        {
            if (externalSelectionUpdateInProgress)
                return;

            var candidateSelection = allLocalAvailableMods.Where(modState => modState.Active.Value)
                                                          .Select(modState => modState.Mod)
                                                          .ToArray();

            SelectedMods.Value = ComputeNewModsFromSelection(SelectedMods.Value, candidateSelection);
        }

        #region Transition handling

        private const float distance = 700;

        protected override void PopIn()
        {
            const double fade_in_duration = 400;

            base.PopIn();

            multiplierDisplay?
                .FadeIn(fade_in_duration, Easing.OutQuint)
                .MoveToY(0, fade_in_duration, Easing.OutQuint);

            int nonFilteredColumnCount = 0;

            for (int i = 0; i < columnFlow.Count; i++)
            {
                var column = columnFlow[i].Column;

                bool allFiltered = column.AvailableMods.All(modState => modState.Filtered.Value);

                double delay = allFiltered ? 0 : nonFilteredColumnCount * 30;
                double duration = allFiltered ? 0 : fade_in_duration;
                float startingYPosition = 0;
                if (!allFiltered)
                    startingYPosition = nonFilteredColumnCount % 2 == 0 ? -distance : distance;

                column.TopLevelContent
                      .MoveToY(startingYPosition)
                      .Delay(delay)
                      .MoveToY(0, duration, Easing.OutQuint)
                      .FadeIn(duration, Easing.OutQuint);

                if (!allFiltered)
                    nonFilteredColumnCount += 1;
            }
        }

        protected override void PopOut()
        {
            const double fade_out_duration = 500;

            base.PopOut();

            multiplierDisplay?
                .FadeOut(fade_out_duration / 2, Easing.OutQuint)
                .MoveToY(-distance, fade_out_duration / 2, Easing.OutQuint);

            int nonFilteredColumnCount = 0;

            for (int i = 0; i < columnFlow.Count; i++)
            {
                var column = columnFlow[i].Column;

                bool allFiltered = column.AvailableMods.All(modState => modState.Filtered.Value);

                double duration = allFiltered ? 0 : fade_out_duration;
                float newYPosition = 0;
                if (!allFiltered)
                    newYPosition = nonFilteredColumnCount % 2 == 0 ? -distance : distance;

                column.FlushPendingSelections();
                column.TopLevelContent
                      .MoveToY(newYPosition, duration, Easing.OutQuint)
                      .FadeOut(duration, Easing.OutQuint);

                if (!allFiltered)
                    nonFilteredColumnCount += 1;
            }
        }

        #endregion

        #region Input handling

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                case GlobalAction.Back:
                    // Pressing the back binding should only go back one step at a time.
                    hideOverlay(false);
                    return true;

                // This is handled locally here because this overlay is being registered at the game level
                // and therefore takes away keyboard focus from the screen stack.
                case GlobalAction.ToggleModSelection:
                case GlobalAction.Select:
                {
                    // Pressing toggle or select should completely hide the overlay in one shot.
                    hideOverlay(true);
                    return true;
                }

                case GlobalAction.DeselectAllMods:
                    deselectAllButton?.TriggerClick();
                    return true;
            }

            return base.OnPressed(e);

            void hideOverlay(bool immediate)
            {
                if (customisationVisible.Value)
                {
                    Debug.Assert(customisationButton != null);
                    customisationButton.TriggerClick();

                    if (!immediate)
                        return;
                }

                backButton.TriggerClick();
            }
        }

        #endregion

        #region Sample playback control

        private readonly Bindable<bool> samplePlaybackDisabled = new BindableBool(true);
        IBindable<bool> ISamplePlaybackDisabler.SamplePlaybackDisabled => samplePlaybackDisabled;

        #endregion

        /// <summary>
        /// Manages horizontal scrolling of mod columns, along with the "active" states of each column based on visibility.
        /// </summary>
        internal class ColumnScrollContainer : OsuScrollContainer<ColumnFlowContainer>
        {
            public ColumnScrollContainer()
                : base(Direction.Horizontal)
            {
            }

            protected override void Update()
            {
                base.Update();

                // the bounds below represent the horizontal range of scroll items to be considered fully visible/active, in the scroll's internal coordinate space.
                // note that clamping is applied to the left scroll bound to ensure scrolling past extents does not change the set of active columns.
                float leftVisibleBound = Math.Clamp(Current, 0, ScrollableExtent);
                float rightVisibleBound = leftVisibleBound + DrawWidth;

                // if a movement is occurring at this time, the bounds below represent the full range of columns that the scroll movement will encompass.
                // this will be used to ensure that columns do not change state from active to inactive back and forth until they are fully scrolled past.
                float leftMovementBound = Math.Min(Current, Target);
                float rightMovementBound = Math.Max(Current, Target) + DrawWidth;

                foreach (var column in Child)
                {
                    // DrawWidth/DrawPosition do not include shear effects, and we want to know the full extents of the columns post-shear,
                    // so we have to manually compensate.
                    var topLeft = column.ToSpaceOfOtherDrawable(Vector2.Zero, ScrollContent);
                    var bottomRight = column.ToSpaceOfOtherDrawable(new Vector2(column.DrawWidth - column.DrawHeight * SHEAR, 0), ScrollContent);

                    bool isCurrentlyVisible = Precision.AlmostBigger(topLeft.X, leftVisibleBound)
                                              && Precision.DefinitelyBigger(rightVisibleBound, bottomRight.X);
                    bool isBeingScrolledToward = Precision.AlmostBigger(topLeft.X, leftMovementBound)
                                                 && Precision.DefinitelyBigger(rightMovementBound, bottomRight.X);

                    column.Active.Value = isCurrentlyVisible || isBeingScrolledToward;
                }
            }
        }

        /// <summary>
        /// Manages layout of mod columns.
        /// </summary>
        internal class ColumnFlowContainer : FillFlowContainer<ColumnDimContainer>
        {
            public IEnumerable<ModColumn> Columns => Children.Select(dimWrapper => dimWrapper.Column);

            public override void Add(ColumnDimContainer dimContainer)
            {
                base.Add(dimContainer);

                Debug.Assert(dimContainer != null);
                dimContainer.Column.Shear = Vector2.Zero;
            }
        }

        /// <summary>
        /// Encapsulates a column and provides dim and input blocking based on an externally managed "active" state.
        /// </summary>
        internal class ColumnDimContainer : Container
        {
            public ModColumn Column { get; }

            /// <summary>
            /// Tracks whether this column is in an interactive state. Generally only the case when the column is on-screen.
            /// </summary>
            public readonly Bindable<bool> Active = new BindableBool();

            /// <summary>
            /// Invoked when the column is clicked while not active, requesting a scroll to be performed to bring it on-screen.
            /// </summary>
            public Action<ColumnDimContainer>? RequestScroll { get; set; }

            [Resolved]
            private OsuColour colours { get; set; } = null!;

            public ColumnDimContainer(ModColumn column)
            {
                Child = Column = column;
                column.Active.BindTo(Active);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Active.BindValueChanged(_ => updateState(), true);
                FinishTransforms();
            }

            protected override bool RequiresChildrenUpdate => base.RequiresChildrenUpdate || Column.SelectionAnimationRunning;

            private void updateState()
            {
                Colour4 targetColour;

                if (Column.Active.Value)
                    targetColour = Colour4.White;
                else
                    targetColour = IsHovered ? colours.GrayC : colours.Gray8;

                this.FadeColour(targetColour, 800, Easing.OutQuint);
            }

            protected override bool OnClick(ClickEvent e)
            {
                if (!Active.Value)
                    RequestScroll?.Invoke(this);

                return true;
            }

            protected override bool OnHover(HoverEvent e)
            {
                base.OnHover(e);
                updateState();
                return Active.Value;
            }

            protected override void OnHoverLost(HoverLostEvent e)
            {
                base.OnHoverLost(e);
                updateState();
            }
        }

        /// <summary>
        /// A container which blocks and handles input, managing the "return from customisation" state change.
        /// </summary>
        private class ClickToReturnContainer : Container
        {
            public BindableBool HandleMouse { get; } = new BindableBool();

            public Action? OnClicked { get; set; }

            public override bool HandlePositionalInput => base.HandlePositionalInput && HandleMouse.Value;

            protected override bool Handle(UIEvent e)
            {
                if (!HandleMouse.Value)
                    return base.Handle(e);

                switch (e)
                {
                    case ClickEvent _:
                        OnClicked?.Invoke();
                        return true;

                    case MouseEvent _:
                        return true;
                }

                return base.Handle(e);
            }
        }
    }
}
