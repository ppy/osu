// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Utils;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Footer;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Overlays.Mods
{
    public partial class ModSelectOverlay : ShearedOverlayContainer, ISamplePlaybackDisabler, IKeyBindingHandler<PlatformAction>
    {
        public const int BUTTON_WIDTH = 200;

        protected override string PopInSampleName => "";
        protected override string PopOutSampleName => @"SongSelect/mod-select-overlay-pop-out";

        [Cached]
        public Bindable<IReadOnlyList<Mod>> SelectedMods { get; private set; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// Contains a list of mods which <see cref="ModSelectOverlay"/> should read from to display effects on the selected beatmap.
        /// </summary>
        /// <remarks>
        /// This is different from <see cref="SelectedMods"/> in screens like online-play rooms, where there are required mods activated from the playlist.
        /// </remarks>
        public Bindable<IReadOnlyList<Mod>> ActiveMods { get; private set; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        /// <summary>
        /// Contains a dictionary with the current <see cref="ModState"/> of all mods applicable for the current ruleset.
        /// </summary>
        /// <remarks>
        /// Contrary to <see cref="OsuGameBase.AvailableMods"/> and <see cref="globalAvailableMods"/>, the <see cref="Mod"/> instances
        /// inside the <see cref="ModState"/> objects are owned solely by this <see cref="ModSelectOverlay"/> instance.
        /// </remarks>
        public Bindable<Dictionary<ModType, IReadOnlyList<ModState>>> AvailableMods { get; } =
            new Bindable<Dictionary<ModType, IReadOnlyList<ModState>>>(new Dictionary<ModType, IReadOnlyList<ModState>>());

        private Func<Mod, bool> isValidMod = _ => true;

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

        public string SearchTerm
        {
            get => SearchTextBox.Current.Value;
            set => SearchTextBox.Current.Value = value;
        }

        public ShearedSearchTextBox SearchTextBox { get; private set; } = null!;

        /// <summary>
        /// Whether per-mod customisation controls are visible.
        /// </summary>
        protected virtual bool AllowCustomisation => true;

        /// <summary>
        /// Whether the column with available mod presets should be shown.
        /// </summary>
        public bool ShowPresets { get; init; }

        protected virtual ModColumn CreateModColumn(ModType modType) => new ModColumn(modType, false);

        protected virtual IReadOnlyList<Mod> ComputeNewModsFromSelection(IReadOnlyList<Mod> oldSelection, IReadOnlyList<Mod> newSelection) => newSelection;

        protected virtual IReadOnlyList<Mod> ComputeActiveMods() => SelectedMods.Value;

        private readonly Bindable<Dictionary<ModType, IReadOnlyList<Mod>>> globalAvailableMods = new Bindable<Dictionary<ModType, IReadOnlyList<Mod>>>();

        public IEnumerable<ModState> AllAvailableMods => AvailableMods.Value.SelectMany(pair => pair.Value);

        private Bindable<bool> textSearchStartsActive = null!;

        private ColumnScrollContainer columnScroll = null!;
        private ColumnFlowContainer columnFlow = null!;

        private Container aboveColumnsContent = null!;
        private ModCustomisationPanel customisationPanel = null!;

        protected virtual SelectAllModsButton? SelectAllModsButton => null;

        private Sample? columnAppearSample;

        public readonly Bindable<WorkingBeatmap?> Beatmap = new Bindable<WorkingBeatmap?>();

        [Resolved]
        private ScreenFooter? footer { get; set; }

        public ModSelectOverlay(OverlayColourScheme colourScheme = OverlayColourScheme.Green)
            : base(colourScheme)
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuColour colours, AudioManager audio, OsuConfigManager configManager)
        {
            Header.Title = ModSelectOverlayStrings.ModSelectTitle;
            Header.Description = ModSelectOverlayStrings.ModSelectDescription;

            columnAppearSample = audio.Samples.Get(@"SongSelect/mod-column-pop-in");

            MainAreaContent.Add(new OsuContextMenuContainer
            {
                RelativeSizeAxes = Axes.Both,
                Child = new PopoverContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            Padding = new MarginPadding
                            {
                                Top = RankingInformationDisplay.HEIGHT + PADDING,
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
                                        Shear = new Vector2(OsuGame.SHEAR, 0),
                                        RelativeSizeAxes = Axes.Y,
                                        AutoSizeAxes = Axes.X,
                                        Margin = new MarginPadding { Horizontal = 70 },
                                        Padding = new MarginPadding { Bottom = 10 },
                                        ChildrenEnumerable = createColumns()
                                    }
                                }
                            }
                        },
                        aboveColumnsContent = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Horizontal = 100, Bottom = 15f },
                            Children = new Drawable[]
                            {
                                SearchTextBox = new ShearedSearchTextBox
                                {
                                    HoldFocus = false,
                                    Width = 300,
                                },
                                customisationPanel = new ModCustomisationPanel
                                {
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    Width = 400,
                                    State = { Value = Visibility.Visible },
                                }
                            }
                        }
                    },
                }
            });

            globalAvailableMods.BindTo(game.AvailableMods);

            textSearchStartsActive = configManager.GetBindable<bool>(OsuSetting.ModSelectTextSearchStartsActive);
        }

        public override void Hide()
        {
            base.Hide();

            // clear search for next user interaction with mod overlay
            SearchTextBox.Current.Value = string.Empty;
        }

        protected override void LoadComplete()
        {
            // this is called before base call so that the mod state is populated early, and the transition in `PopIn()` can play out properly.
            globalAvailableMods.BindValueChanged(_ => createLocalMods(), true);

            base.LoadComplete();

            State.BindValueChanged(_ => samplePlaybackDisabled.Value = State.Value == Visibility.Hidden, true);

            // This is an optimisation to prevent refreshing the available settings controls when it can be
            // reasonably assumed that the settings panel is never to be displayed (e.g. FreeModSelectOverlay).
            if (AllowCustomisation)
                ((IBindable<IReadOnlyList<Mod>>)customisationPanel.SelectedMods).BindTo(SelectedMods);

            SelectedMods.BindValueChanged(_ =>
            {
                updateFromExternalSelection();
                updateCustomisation();

                ActiveMods.Value = ComputeActiveMods();
            }, true);

            customisationPanel.ExpandedState.BindValueChanged(_ => updateCustomisationVisualState(), true);

            SearchTextBox.Current.BindValueChanged(query =>
            {
                foreach (var column in columnFlow.Columns)
                    column.SearchTerm = query.NewValue;

                if (SearchTextBox.HasFocus)
                    preselectMod();
            }, true);

            // Start scrolling from the end, to give the user a sense that
            // there is more horizontal content available.
            ScheduleAfterChildren(() =>
            {
                columnScroll.ScrollToEnd(false);
                columnScroll.ScrollTo(0);
            });
        }

        private void preselectMod()
        {
            var visibleMods = columnFlow.Columns.OfType<ModColumn>().Where(c => c.IsPresent).SelectMany(c => c.AvailableMods.Where(m => m.Visible));

            // Search for an exact acronym or name match, or otherwise default to the first visible mod.
            ModState? matchingMod =
                visibleMods.FirstOrDefault(m => m.Mod.Acronym.Equals(SearchTerm, StringComparison.OrdinalIgnoreCase) || m.Mod.Name.Equals(SearchTerm, StringComparison.OrdinalIgnoreCase))
                ?? visibleMods.FirstOrDefault();
            var preselectedMod = matchingMod;

            foreach (var mod in AllAvailableMods)
                mod.Preselected.Value = mod == preselectedMod && SearchTextBox.Current.Value.Length > 0;
        }

        private void clearPreselection()
        {
            foreach (var mod in AllAvailableMods)
                mod.Preselected.Value = false;
        }

        public new ModSelectFooterContent? DisplayedFooterContent => base.DisplayedFooterContent as ModSelectFooterContent;

        public override VisibilityContainer CreateFooterContent() => new ModSelectFooterContent(this)
        {
            Beatmap = { BindTarget = Beatmap },
            ActiveMods = { BindTarget = ActiveMods },
        };

        private static readonly LocalisableString input_search_placeholder = Resources.Localisation.Web.CommonStrings.InputSearch;
        private static readonly LocalisableString tab_to_search_placeholder = ModSelectOverlayStrings.TabToSearch;

        protected override void Update()
        {
            base.Update();

            SearchTextBox.PlaceholderText = SearchTextBox.HasFocus ? input_search_placeholder : tab_to_search_placeholder;
            aboveColumnsContent.Padding = aboveColumnsContent.Padding with { Bottom = DisplayedFooterContent?.DisplaysStackedVertically == true ? 75f : 15f };
        }

        /// <summary>
        /// Select all visible mods in all columns.
        /// </summary>
        public void SelectAll()
        {
            foreach (var column in columnFlow.Columns.OfType<ModColumn>())
                column.SelectAll();
        }

        /// <summary>
        /// Deselect all visible mods in all columns.
        /// </summary>
        public void DeselectAll()
        {
            foreach (var column in columnFlow.Columns.OfType<ModColumn>())
                column.DeselectAll();
        }

        private IEnumerable<ColumnDimContainer> createColumns()
        {
            if (ShowPresets)
            {
                yield return new ColumnDimContainer(new ModPresetColumn
                {
                    Margin = new MarginPadding { Right = 10 }
                });
            }

            yield return createModColumnContent(ModType.DifficultyReduction);
            yield return createModColumnContent(ModType.DifficultyIncrease);
            yield return createModColumnContent(ModType.Automation);
            yield return createModColumnContent(ModType.Conversion);
            yield return createModColumnContent(ModType.Fun);
        }

        private ColumnDimContainer createModColumnContent(ModType modType)
        {
            var column = CreateModColumn(modType).With(column =>
            {
                // spacing applied here rather than via `columnFlow.Spacing` to avoid uneven gaps when some of the columns are hidden.
                column.Margin = new MarginPadding { Right = 10 };
            });

            return new ColumnDimContainer(column);
        }

        private void createLocalMods()
        {
            var newLocalAvailableMods = new Dictionary<ModType, IReadOnlyList<ModState>>();

            foreach (var (modType, mods) in globalAvailableMods.Value)
            {
                var modStates = mods.SelectMany(ModUtils.FlattenMod)
                                    .Select(mod => new ModState(mod.DeepClone()))
                                    .ToArray();

                foreach (var modState in modStates)
                    modState.Active.BindValueChanged(_ => updateFromInternalSelection());

                newLocalAvailableMods[modType] = modStates;
            }

            AvailableMods.Value = newLocalAvailableMods;
            filterMods();

            foreach (var column in columnFlow.Columns.OfType<ModColumn>())
                column.AvailableMods = AvailableMods.Value.GetValueOrDefault(column.ModType, Array.Empty<ModState>());
        }

        private void filterMods()
        {
            foreach (var modState in AllAvailableMods)
                modState.ValidForSelection.Value = modState.Mod.Type != ModType.System && modState.Mod.HasImplementation && IsValidMod.Invoke(modState.Mod);
        }

        private void updateCustomisation()
        {
            if (!AllowCustomisation)
                return;

            bool anyCustomisableModActive = false;
            bool anyModPendingConfiguration = false;

            foreach (var modState in AllAvailableMods)
            {
                anyCustomisableModActive |= modState.Active.Value && modState.Mod.GetSettingsSourceProperties().Any();
                anyModPendingConfiguration |= modState.PendingConfiguration;
                modState.PendingConfiguration = false;
            }

            if (anyCustomisableModActive)
            {
                customisationPanel.Enabled.Value = true;

                if (anyModPendingConfiguration)
                    customisationPanel.ExpandedState.Value = ModCustomisationPanel.ModCustomisationPanelState.ExpandedByMod;
            }
            else
            {
                customisationPanel.ExpandedState.Value = ModCustomisationPanel.ModCustomisationPanelState.Collapsed;
                customisationPanel.Enabled.Value = false;
            }
        }

        private void updateCustomisationVisualState()
        {
            if (customisationPanel.ExpandedState.Value != ModCustomisationPanel.ModCustomisationPanelState.Collapsed)
            {
                columnScroll.FadeColour(OsuColour.Gray(0.5f), 400, Easing.OutQuint);
                SearchTextBox.FadeColour(OsuColour.Gray(0.5f), 400, Easing.OutQuint);
                setTextBoxFocus(false);
            }
            else
            {
                columnScroll.FadeColour(Color4.White, 400, Easing.OutQuint);
                SearchTextBox.FadeColour(Color4.White, 400, Easing.OutQuint);
                setTextBoxFocus(textSearchStartsActive.Value);
            }
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

            foreach (var modState in AllAvailableMods)
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

            var candidateSelection = AllAvailableMods.Where(modState => modState.Active.Value)
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

            aboveColumnsContent
                .FadeIn(fade_in_duration, Easing.OutQuint)
                .MoveToY(0, fade_in_duration, Easing.OutQuint);

            int nonFilteredColumnCount = 0;

            for (int i = 0; i < columnFlow.Count; i++)
            {
                var column = columnFlow[i].Column;

                bool allFiltered = column is ModColumn modColumn && modColumn.AvailableMods.All(modState => !modState.Visible);

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

                if (allFiltered)
                    continue;

                int columnNumber = nonFilteredColumnCount;
                Scheduler.AddDelayed(() =>
                {
                    var channel = columnAppearSample?.GetChannel();
                    if (channel == null) return;

                    // Still play sound effects for off-screen columns up to a certain point.
                    if (columnNumber > 5 && !column.Active.Value) return;

                    // use X position of the column on screen as a basis for panning the sample
                    float balance = column.Parent!.BoundingBox.Centre.X / RelativeToAbsoluteFactor.X;

                    // dip frequency and ramp volume of sample over the first 5 displayed columns
                    float progress = Math.Min(1, columnNumber / 5f);

                    channel.Frequency.Value = 1.3 - (progress * 0.3) + RNG.NextDouble(0.1);
                    channel.Volume.Value = Math.Max(progress, 0.2);
                    channel.Balance.Value = -1 + balance * 2;
                    channel.Play();
                }, delay);

                nonFilteredColumnCount += 1;
            }

            setTextBoxFocus(textSearchStartsActive.Value);
        }

        protected override void PopOut()
        {
            const double fade_out_duration = 500;

            base.PopOut();

            aboveColumnsContent
                .FadeOut(fade_out_duration / 2, Easing.OutQuint)
                .MoveToY(-distance, fade_out_duration / 2, Easing.OutQuint);

            int nonFilteredColumnCount = 0;

            for (int i = 0; i < columnFlow.Count; i++)
            {
                var column = columnFlow[i].Column;

                bool allFiltered = false;

                if (column is ModColumn modColumn)
                {
                    allFiltered = modColumn.AvailableMods.All(modState => !modState.Visible);
                    modColumn.FlushPendingSelections();
                }

                double duration = allFiltered ? 0 : fade_out_duration;
                float newYPosition = 0;
                if (!allFiltered)
                    newYPosition = nonFilteredColumnCount % 2 == 0 ? -distance : distance;

                column.TopLevelContent
                      .MoveToY(newYPosition, duration, Easing.OutQuint)
                      .FadeOut(duration, Easing.OutQuint);

                if (!allFiltered)
                    nonFilteredColumnCount += 1;
            }

            customisationPanel.ExpandedState.Value = ModCustomisationPanel.ModCustomisationPanelState.Collapsed;
        }

        #endregion

        #region Input handling

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            switch (e.Action)
            {
                // If the customisation panel is expanded, the back action will be handled by it first.
                case GlobalAction.Back:
                // This is handled locally here because this overlay is being registered at the game level
                // and therefore takes away keyboard focus from the screen stack.
                case GlobalAction.ToggleModSelection:
                    hideOverlay();
                    return true;

                // This is handled locally here due to conflicts in input handling between the search text box and the deselect all mods button.
                // Attempting to handle this action locally in both places leads to a possible scenario
                // wherein activating the binding will both change the contents of the search text box and deselect all mods.
                case GlobalAction.DeselectAllMods:
                {
                    if (!SearchTextBox.HasFocus && customisationPanel.ExpandedState.Value == ModCustomisationPanel.ModCustomisationPanelState.Collapsed)
                    {
                        DisplayedFooterContent?.DeselectAllModsButton?.TriggerClick();
                        return true;
                    }

                    break;
                }

                case GlobalAction.Select:
                {
                    // Pressing select should select first filtered mod if a search is in progress.
                    // If there is no search in progress, it should exit the dialog (a bit weird, but this is the expectation from stable).
                    if (string.IsNullOrEmpty(SearchTerm))
                    {
                        hideOverlay();
                        return true;
                    }

                    var matchingMod = AllAvailableMods.SingleOrDefault(m => m.Preselected.Value);

                    if (matchingMod is not null)
                    {
                        matchingMod.Active.Value = !matchingMod.Active.Value;
                        SearchTextBox.SelectAll();
                    }

                    return true;
                }
            }

            return base.OnPressed(e);

            void hideOverlay()
            {
                if (footer != null)
                    footer.BackButton.TriggerClick();
                else
                    Hide();
            }
        }

        /// <inheritdoc cref="IKeyBindingHandler{PlatformAction}"/>
        /// <remarks>
        /// This is handled locally here due to conflicts in input handling between the search text box and the select all mods button.
        /// Attempting to handle this action locally in both places leads to a possible scenario
        /// wherein activating the "select all" platform binding will both select all text in the search box and select all mods.
        /// </remarks>
        public bool OnPressed(KeyBindingPressEvent<PlatformAction> e)
        {
            if (e.Repeat || e.Action != PlatformAction.SelectAll || SelectAllModsButton == null)
                return false;

            SelectAllModsButton.TriggerClick();
            return true;
        }

        public void OnReleased(KeyBindingReleaseEvent<PlatformAction> e)
        {
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat || e.Key != Key.Tab)
                return false;

            if (customisationPanel.ExpandedState.Value != ModCustomisationPanel.ModCustomisationPanelState.Collapsed)
                return true;

            // TODO: should probably eventually support typical platform search shortcuts (`Ctrl-F`, `/`)
            setTextBoxFocus(!SearchTextBox.HasFocus);
            return true;
        }

        private void setTextBoxFocus(bool focus)
        {
            if (focus)
            {
                SearchTextBox.TakeFocus();
                preselectMod();
            }
            else
            {
                SearchTextBox.KillFocus();
                clearPreselection();
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
        [Cached]
        internal partial class ColumnScrollContainer : OsuScrollContainer<ColumnFlowContainer>
        {
            public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => true;

            public ColumnScrollContainer()
                : base(Direction.Horizontal)
            {
            }

            protected override void Update()
            {
                base.Update();

                // the bounds below represent the horizontal range of scroll items to be considered fully visible/active, in the scroll's internal coordinate space.
                // note that clamping is applied to the left scroll bound to ensure scrolling past extents does not change the set of active columns.
                double leftVisibleBound = Math.Clamp(Current, 0, ScrollableExtent);
                double rightVisibleBound = leftVisibleBound + DrawWidth;

                // if a movement is occurring at this time, the bounds below represent the full range of columns that the scroll movement will encompass.
                // this will be used to ensure that columns do not change state from active to inactive back and forth until they are fully scrolled past.
                double leftMovementBound = Math.Min(Current, Target);
                double rightMovementBound = Math.Max(Current, Target) + DrawWidth;

                foreach (var column in Child)
                {
                    // DrawWidth/DrawPosition do not include shear effects, and we want to know the full extents of the columns post-shear,
                    // so we have to manually compensate.
                    var topLeft = column.ToSpaceOfOtherDrawable(Vector2.Zero, ScrollContent);
                    var bottomRight = column.ToSpaceOfOtherDrawable(new Vector2(column.DrawWidth - column.DrawHeight * OsuGame.SHEAR, 0), ScrollContent);

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
        internal partial class ColumnFlowContainer : FillFlowContainer<ColumnDimContainer>
        {
            public IEnumerable<ModSelectColumn> Columns => Children.Select(dimWrapper => dimWrapper.Column);

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
        internal partial class ColumnDimContainer : Container
        {
            public ModSelectColumn Column { get; }

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

            public ColumnDimContainer(ModSelectColumn column)
            {
                AutoSizeAxes = Axes.X;
                RelativeSizeAxes = Axes.Y;

                Child = Column = column;
                column.Active.BindTo(Active);
            }

            [BackgroundDependencyLoader]
            private void load(ColumnScrollContainer columnScroll)
            {
                RequestScroll = col => columnScroll.ScrollIntoView(col, extraScroll: 140);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                Active.BindValueChanged(_ => updateState(), true);
                FinishTransforms();
            }

            protected override bool RequiresChildrenUpdate
            {
                get
                {
                    bool result = base.RequiresChildrenUpdate;

                    if (Column is ModColumn modColumn)
                        result |= !modColumn.ItemsLoaded || modColumn.SelectionAnimationRunning;

                    return result;
                }
            }

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

                // Killing focus is done here because it's the only feasible place on ModSelectOverlay you can click on without triggering any action.
                Scheduler.Add(() => GetContainingFocusManager()!.ChangeFocus(null));

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
    }
}
