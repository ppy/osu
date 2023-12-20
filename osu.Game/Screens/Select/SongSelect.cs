// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Select.Details;
using osu.Game.Screens.Select.Options;
using osu.Game.Skinning;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.Select
{
    public abstract partial class SongSelect : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>
    {
        public static readonly float WEDGE_HEIGHT = 200;

        protected const float BACKGROUND_BLUR = 20;
        private const float left_area_padding = 20;

        public FilterControl FilterControl { get; private set; } = null!;

        /// <summary>
        /// Whether this song select instance should take control of the global track,
        /// applying looping and preview offsets.
        /// </summary>
        protected virtual bool ControlGlobalMusic => true;

        protected virtual bool ShowFooter => true;

        public override bool? ApplyModTrackAdjustments => true;

        /// <summary>
        /// Can be null if <see cref="ShowFooter"/> is false.
        /// </summary>
        protected BeatmapOptionsOverlay BeatmapOptions { get; private set; } = null!;

        /// <summary>
        /// Can be null if <see cref="ShowFooter"/> is false.
        /// </summary>
        protected Footer? Footer { get; private set; }

        /// <summary>
        /// Contains any panel which is triggered by a footer button.
        /// Helps keep them located beneath the footer itself.
        /// </summary>
        protected Container FooterPanels { get; private set; } = null!;

        /// <summary>
        /// Whether entering editor mode should be allowed.
        /// </summary>
        public virtual bool AllowEditing => true;

        public bool BeatmapSetsLoaded => IsLoaded && Carousel.BeatmapSetsLoaded;

        /// <summary>
        /// Creates any "action" menu items for the provided beatmap (ie. "Select", "Play", "Edit").
        /// These will always be placed at the top of the context menu, with common items added below them.
        /// </summary>
        /// <param name="beatmap">The beatmap to create items for.</param>
        /// <returns>The menu items.</returns>
        public virtual MenuItem[] CreateForwardNavigationMenuItemsForBeatmap(BeatmapInfo beatmap) => new MenuItem[]
        {
            new OsuMenuItem(@"Select", MenuItemType.Highlighted, () => FinaliseSelection(beatmap))
        };

        [Resolved]
        private Bindable<IReadOnlyList<Mod>> selectedMods { get; set; } = null!;

        protected BeatmapCarousel Carousel { get; private set; } = null!;

        private ParallaxContainer wedgeBackground = null!;

        protected Container LeftArea { get; private set; } = null!;

        private BeatmapInfoWedge beatmapInfoWedge = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        protected ModSelectOverlay ModSelect { get; private set; } = null!;

        protected Sample? SampleConfirm { get; private set; }

        private Sample sampleChangeDifficulty = null!;
        private Sample sampleChangeBeatmap = null!;

        private Container carouselContainer = null!;

        protected BeatmapDetailArea BeatmapDetails { get; private set; } = null!;

        private FooterButtonOptions beatmapOptionsButton = null!;

        private readonly Bindable<RulesetInfo> decoupledRuleset = new Bindable<RulesetInfo>();

        private double audioFeedbackLastPlaybackTime;

        private IDisposable? modSelectOverlayRegistration;

        private AdvancedStats advancedStats = null!;

        [Resolved]
        private MusicController music { get; set; } = null!;

        [Resolved]
        internal IOverlayManager? OverlayManager { get; private set; }

        private Bindable<bool> configBackgroundBlur = null!;

        [BackgroundDependencyLoader(true)]
        private void load(AudioManager audio, OsuColour colours, ManageCollectionsDialog? manageCollectionsDialog, DifficultyRecommender? recommender, OsuConfigManager config)
        {
            configBackgroundBlur = config.GetBindable<bool>(OsuSetting.SongSelectBackgroundBlur);
            configBackgroundBlur.BindValueChanged(e =>
            {
                if (!this.IsCurrentScreen())
                    return;

                ApplyToBackground(applyBlurToBackground);
            });

            LoadComponentAsync(Carousel = new BeatmapCarousel
            {
                AllowSelection = false, // delay any selection until our bindables are ready to make a good choice.
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                RelativeSizeAxes = Axes.Both,
                BleedTop = FilterControl.HEIGHT,
                BleedBottom = Footer.HEIGHT,
                SelectionChanged = updateSelectedBeatmap,
                BeatmapSetsChanged = carouselBeatmapsLoaded,
                FilterApplied = () => Scheduler.AddOnce(updateVisibleBeatmapCount),
                GetRecommendedBeatmap = s => recommender?.GetRecommendedBeatmap(s),
            }, c => carouselContainer.Child = c);

            // initial value transfer is required for FilterControl (it uses our re-cached bindables in its async load for the initial filter).
            transferRulesetValue();

            AddRangeInternal(new Drawable[]
            {
                new VerticalMaskingContainer
                {
                    Children = new Drawable[]
                    {
                        new GridContainer // used for max width implementation
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColumnDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Relative, 0.5f, maxSize: 850),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    wedgeBackground = new ParallaxContainer
                                    {
                                        ParallaxAmount = 0.005f,
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Child = new WedgeBackground
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Padding = new MarginPadding { Right = -150 },
                                        },
                                    },
                                    carouselContainer = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding
                                        {
                                            Top = FilterControl.HEIGHT,
                                            Bottom = Footer.HEIGHT
                                        },
                                        Child = new LoadingSpinner(true) { State = { Value = Visibility.Visible } }
                                    }
                                },
                            }
                        },
                        FilterControl = new FilterControl
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = FilterControl.HEIGHT,
                            FilterChanged = ApplyFilterToCarousel,
                        },
                        new GridContainer // used for max width implementation
                        {
                            RelativeSizeAxes = Axes.Both,
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.Relative, 0.5f, maxSize: 650),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    LeftArea = new Container
                                    {
                                        Origin = Anchor.BottomLeft,
                                        Anchor = Anchor.BottomLeft,
                                        RelativeSizeAxes = Axes.Both,
                                        Padding = new MarginPadding { Top = 5 },
                                        Children = new Drawable[]
                                        {
                                            new LeftSideInteractionContainer(() => Carousel.ScrollToSelected())
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                            },
                                            beatmapInfoWedge = new BeatmapInfoWedge
                                            {
                                                Height = WEDGE_HEIGHT,
                                                RelativeSizeAxes = Axes.X,
                                                Margin = new MarginPadding
                                                {
                                                    Right = left_area_padding,
                                                    Left = -BeatmapInfoWedge.BORDER_THICKNESS, // Hide the left border
                                                },
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 90,
                                                Padding = new MarginPadding(10)
                                                {
                                                    Left = left_area_padding,
                                                    Right = left_area_padding * 2 + 5,
                                                },
                                                Y = WEDGE_HEIGHT,
                                                Children = new Drawable[]
                                                {
                                                    new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Masking = true,
                                                        CornerRadius = 10,
                                                        Children = new Drawable[]
                                                        {
                                                            new Box
                                                            {
                                                                RelativeSizeAxes = Axes.Both,
                                                                Colour = Colour4.Black.Opacity(0.3f),
                                                            },
                                                            advancedStats = new AdvancedStats(2)
                                                            {
                                                                RelativeSizeAxes = Axes.X,
                                                                AutoSizeAxes = Axes.Y,
                                                                Anchor = Anchor.Centre,
                                                                Origin = Anchor.Centre,
                                                                Padding = new MarginPadding(10)
                                                            },
                                                        }
                                                    },
                                                }
                                            },
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding
                                                {
                                                    Bottom = Footer.HEIGHT,
                                                    Top = WEDGE_HEIGHT + 70,
                                                    Left = left_area_padding,
                                                    Right = left_area_padding * 2,
                                                },
                                                Child = BeatmapDetails = CreateBeatmapDetailArea().With(d =>
                                                {
                                                    d.RelativeSizeAxes = Axes.Both;
                                                    d.Padding = new MarginPadding { Top = 10, Right = 5 };
                                                })
                                            },
                                        }
                                    },
                                },
                            }
                        }
                    }
                },
                new SkinComponentsContainer(new SkinComponentsContainerLookup(SkinComponentsContainerLookup.TargetArea.SongSelect))
                {
                    RelativeSizeAxes = Axes.Both,
                },
            });

            if (ShowFooter)
            {
                AddRangeInternal(new Drawable[]
                {
                    FooterPanels = new Container
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Bottom = Footer.HEIGHT },
                        Children = new Drawable[]
                        {
                            BeatmapOptions = new BeatmapOptionsOverlay(),
                        }
                    },
                    Footer = new Footer()
                });
            }

            // preload the mod select overlay for later use in `LoadComplete()`.
            // therein it will be registered at the `OsuGame` level to properly function as a blocking overlay.
            LoadComponent(ModSelect = CreateModSelectOverlay());

            if (Footer != null)
            {
                foreach (var (button, overlay) in CreateFooterButtons())
                    Footer.AddButton(button, overlay);

                BeatmapOptions.AddButton(@"Manage", @"collections", FontAwesome.Solid.Book, colours.Green, () => manageCollectionsDialog?.Show());
                BeatmapOptions.AddButton(@"Delete", @"all difficulties", FontAwesome.Solid.Trash, colours.Pink, () => DeleteBeatmap(Beatmap.Value.BeatmapSetInfo));
                BeatmapOptions.AddButton(@"Remove", @"from unplayed", FontAwesome.Regular.TimesCircle, colours.Purple, null);
                BeatmapOptions.AddButton(@"Clear", @"local scores", FontAwesome.Solid.Eraser, colours.Purple, () => ClearScores(Beatmap.Value.BeatmapInfo));
            }

            sampleChangeDifficulty = audio.Samples.Get(@"SongSelect/select-difficulty");
            sampleChangeBeatmap = audio.Samples.Get(@"SongSelect/select-expand");
            SampleConfirm = audio.Samples.Get(@"SongSelect/confirm-selection");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            modSelectOverlayRegistration = OverlayManager?.RegisterBlockingOverlay(ModSelect);
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            // Match stable behaviour of only alt-scroll adjusting volume.
            // Supporting scroll adjust without a modifier key just feels bad, since there are so many scrollable elements on the screen.
            if (!e.CurrentState.Keyboard.AltPressed)
                return true;

            return base.OnScroll(e);
        }

        /// <summary>
        /// Creates the buttons to be displayed in the footer.
        /// </summary>
        /// <returns>A set of <see cref="FooterButton"/> and an optional <see cref="OverlayContainer"/> which the button opens when pressed.</returns>
        protected virtual IEnumerable<(FooterButton, OverlayContainer?)> CreateFooterButtons() => new (FooterButton, OverlayContainer?)[]
        {
            (new FooterButtonMods { Current = Mods }, ModSelect),
            (new FooterButtonRandom
            {
                NextRandom = () => Carousel.SelectNextRandom(),
                PreviousRandom = Carousel.SelectPreviousRandom
            }, null),
            (beatmapOptionsButton = new FooterButtonOptions(), BeatmapOptions)
        };

        protected virtual ModSelectOverlay CreateModSelectOverlay() => new SoloModSelectOverlay();

        protected virtual void ApplyFilterToCarousel(FilterCriteria criteria)
        {
            // if not the current screen, we want to get carousel in a good presentation state before displaying (resume or enter).
            bool shouldDebounce = this.IsCurrentScreen();

            Carousel.Filter(criteria, shouldDebounce);
        }

        private DependencyContainer dependencies = null!;

        protected override IReadOnlyDependencyContainer CreateChildDependencies(IReadOnlyDependencyContainer parent)
        {
            dependencies = new DependencyContainer(base.CreateChildDependencies(parent));

            dependencies.CacheAs(this);
            dependencies.CacheAs(decoupledRuleset);
            dependencies.CacheAs<IBindable<RulesetInfo>>(decoupledRuleset);

            return dependencies;
        }

        /// <summary>
        /// Creates the beatmap details to be displayed underneath the wedge.
        /// </summary>
        protected abstract BeatmapDetailArea CreateBeatmapDetailArea();

        public void Edit(BeatmapInfo? beatmapInfo = null)
        {
            if (!AllowEditing)
                throw new InvalidOperationException($"Attempted to edit when {nameof(AllowEditing)} is disabled");

            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo ?? beatmapInfoNoDebounce);
            this.Push(new EditorLoader());
        }

        /// <summary>
        /// Set the query to the search text box.
        /// </summary>
        /// <param name="query">The string to search.</param>
        public void Search(string query)
        {
            FilterControl.CurrentTextSearch.Value = query;
        }

        /// <summary>
        /// Call to make a selection and perform the default action for this SongSelect.
        /// </summary>
        /// <param name="beatmapInfo">An optional beatmap to override the current carousel selection.</param>
        /// <param name="ruleset">An optional ruleset to override the current carousel selection.</param>
        /// <param name="customStartAction">An optional custom action to perform instead of <see cref="OnStart"/>.</param>
        public void FinaliseSelection(BeatmapInfo? beatmapInfo = null, RulesetInfo? ruleset = null, Action? customStartAction = null)
        {
            // This is very important as we have not yet bound to screen-level bindables before the carousel load is completed.
            if (!Carousel.BeatmapSetsLoaded)
            {
                Logger.Log($"{nameof(FinaliseSelection)} aborted as carousel beatmaps are not yet loaded");
                return;
            }

            if (ruleset != null)
                Ruleset.Value = ruleset;

            transferRulesetValue();

            // while transferRulesetValue will flush, it only does so if the ruleset changes.
            // the user could have changed a filter, and we want to ensure we are 100% up-to-date and consistent here.
            Carousel.FlushPendingFilterOperations();

            // avoid attempting to continue before a selection has been obtained.
            // this could happen via a user interaction while the carousel is still in a loading state.
            if (Carousel.SelectedBeatmapInfo == null) return;

            if (beatmapInfo != null)
                Carousel.SelectBeatmap(beatmapInfo);

            if (selectionChangedDebounce?.Completed == false)
            {
                selectionChangedDebounce.RunTask();
                selectionChangedDebounce?.Cancel(); // cancel the already scheduled task.
                selectionChangedDebounce = null;
            }

            if (customStartAction != null)
            {
                customStartAction();
                Carousel.AllowSelection = false;
            }
            else if (OnStart())
                Carousel.AllowSelection = false;
        }

        /// <summary>
        /// Called when a selection is made.
        /// </summary>
        /// <returns>If a resultant action occurred that takes the user away from SongSelect.</returns>
        protected abstract bool OnStart();

        private ScheduledDelegate? selectionChangedDebounce;

        private void updateCarouselSelection(ValueChangedEvent<WorkingBeatmap>? e = null)
        {
            var beatmap = e?.NewValue ?? Beatmap.Value;
            if (beatmap is DummyWorkingBeatmap || !this.IsCurrentScreen()) return;

            Logger.Log($"Song select working beatmap updated to {beatmap}");

            if (!Carousel.SelectBeatmap(beatmap.BeatmapInfo, false))
            {
                // A selection may not have been possible with filters applied.

                // There was possibly a ruleset mismatch. This is a case we can help things along by updating the game-wide ruleset to match.
                if (!beatmap.BeatmapInfo.Ruleset.Equals(decoupledRuleset.Value))
                {
                    Ruleset.Value = beatmap.BeatmapInfo.Ruleset;
                    transferRulesetValue();
                }

                // Even if a ruleset mismatch was not the cause (ie. a text filter is applied),
                // we still want to temporarily show the new beatmap, bypassing filters.
                // This will be undone the next time the user changes the filter.
                var criteria = FilterControl.CreateCriteria();
                criteria.SelectedBeatmapSet = beatmap.BeatmapInfo.BeatmapSet;
                Carousel.Filter(criteria);

                Carousel.SelectBeatmap(beatmap.BeatmapInfo);
            }
        }

        // We need to keep track of the last selected beatmap ignoring debounce to play the correct selection sounds.
        private BeatmapInfo? beatmapInfoPrevious;
        private BeatmapInfo? beatmapInfoNoDebounce;
        private RulesetInfo? rulesetNoDebounce;

        private void updateSelectedBeatmap(BeatmapInfo? beatmapInfo)
        {
            if (beatmapInfo == null && beatmapInfoNoDebounce == null)
                return;

            if (beatmapInfo?.Equals(beatmapInfoNoDebounce) == true)
                return;

            beatmapInfoNoDebounce = beatmapInfo;
            performUpdateSelected();
        }

        private void updateSelectedRuleset(RulesetInfo? ruleset)
        {
            if (ruleset == null && rulesetNoDebounce == null)
                return;

            if (ruleset?.Equals(rulesetNoDebounce) == true)
                return;

            rulesetNoDebounce = ruleset;
            performUpdateSelected();
        }

        /// <summary>
        /// Selection has been changed as the result of a user interaction.
        /// </summary>
        private void performUpdateSelected()
        {
            var beatmap = beatmapInfoNoDebounce;
            RulesetInfo? ruleset = rulesetNoDebounce;

            selectionChangedDebounce?.Cancel();

            if (beatmapInfoNoDebounce == null)
                run();
            else
            {
                // Intentionally slightly higher than repeat_tick_rate to avoid loading songs when holding left / right arrows.
                // See https://github.com/ppy/osu-framework/blob/master/osu.Framework/Input/InputManager.cs#L44
                selectionChangedDebounce = Scheduler.AddDelayed(run, 80);
            }

            if (beatmap?.Equals(beatmapInfoPrevious) != true)
            {
                if (beatmap != null && beatmapInfoPrevious != null && Time.Current - audioFeedbackLastPlaybackTime >= 50)
                {
                    if (beatmap.BeatmapSet?.ID == beatmapInfoPrevious.BeatmapSet?.ID)
                        sampleChangeDifficulty.Play();
                    else
                        sampleChangeBeatmap.Play();

                    audioFeedbackLastPlaybackTime = Time.Current;
                }

                beatmapInfoPrevious = beatmap;
            }

            void run()
            {
                // clear pending task immediately to track any potential nested debounce operation.
                selectionChangedDebounce = null;

                Logger.Log($"Song select updating selection with beatmap:{beatmap?.ID.ToString() ?? "null"} ruleset:{ruleset?.ShortName ?? "null"}");

                if (transferRulesetValue())
                {
                    // transferRulesetValue() may trigger a re-filter. If the current selection does not match the new ruleset, we want to switch away from it.
                    // The default logic on WorkingBeatmap change is to switch to a matching ruleset (see workingBeatmapChanged()), but we don't want that here.
                    // We perform an early selection attempt and clear out the beatmap selection to avoid a second ruleset change (revert).
                    if (beatmap != null && !Carousel.SelectBeatmap(beatmap, false))
                        beatmap = null;
                }

                if (selectionChangedDebounce != null)
                {
                    // a new nested operation was started; switch to it for further selection.
                    // this avoids having two separate debounces trigger from the same source.
                    selectionChangedDebounce.RunTask();
                    return;
                }

                // We may be arriving here due to another component changing the bindable Beatmap.
                // In these cases, the other component has already loaded the beatmap, so we don't need to do so again.
                if (!EqualityComparer<BeatmapInfo>.Default.Equals(beatmap, Beatmap.Value.BeatmapInfo))
                {
                    Logger.Log($"Song select changing beatmap from \"{Beatmap.Value.BeatmapInfo}\" to \"{beatmap?.ToString() ?? "null"}\"");
                    Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap);
                }

                if (this.IsCurrentScreen())
                    ensurePlayingSelected();

                updateComponentFromBeatmap(Beatmap.Value);
            }
        }

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeInFromZero(250);
            FilterControl.Activate();

            ModSelect.SelectedMods.BindTo(selectedMods);

            beginLooping();
        }

        private const double logo_transition = 250;

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            Vector2 position = new Vector2(0.95f, 0.96f);

            if (logo.Alpha > 0.8f)
            {
                logo.MoveTo(position, 500, Easing.OutQuint);
            }
            else
            {
                logo.Hide();
                logo.ScaleTo(0.2f);
                logo.MoveTo(position);
            }

            logo.FadeIn(logo_transition, Easing.OutQuint);
            logo.ScaleTo(0.4f, logo_transition, Easing.OutQuint);

            logo.Action = () =>
            {
                FinaliseSelection();
                return false;
            };
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);
            logo.ScaleTo(0.2f, logo_transition / 2, Easing.Out);
            logo.FadeOut(logo_transition / 2, Easing.Out);
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            // required due to https://github.com/ppy/osu-framework/issues/3218
            ModSelect.SelectedMods.Disabled = false;
            ModSelect.SelectedMods.BindTo(selectedMods);

            Carousel.AllowSelection = true;

            BeatmapDetails.Refresh();

            beginLooping();

            if (!Beatmap.Value.BeatmapSetInfo.DeletePending)
            {
                updateCarouselSelection();

                updateComponentFromBeatmap(Beatmap.Value);

                if (ControlGlobalMusic)
                {
                    // restart playback on returning to song select, regardless.
                    // not sure this should be a permanent thing (we may want to leave a user pause paused even on returning)
                    music.ResetTrackAdjustments();
                    music.Play(requestedByUser: true);
                }
            }

            LeftArea.MoveToX(0, 400, Easing.OutQuint);
            LeftArea.FadeIn(100, Easing.OutQuint);

            FilterControl.MoveToY(0, 400, Easing.OutQuint);
            FilterControl.FadeIn(100, Easing.OutQuint);

            this.FadeIn(250, Easing.OutQuint);

            wedgeBackground.ScaleTo(1, 500, Easing.OutQuint);

            FilterControl.Activate();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            // Handle the case where FinaliseSelection is never called (ie. when a screen is pushed externally).
            // Without this, it's possible for a transfer to happen while we are not the current screen.
            transferRulesetValue();

            ModSelect.SelectedMods.UnbindFrom(selectedMods);

            playExitingTransition();
            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            if (base.OnExiting(e))
                return true;

            playExitingTransition();
            return false;
        }

        private void playExitingTransition()
        {
            ModSelect.Hide();

            BeatmapOptions.Hide();

            Carousel.AllowSelection = false;

            endLooping();

            FilterControl.MoveToY(-120, 500, Easing.OutQuint);
            FilterControl.FadeOut(200, Easing.OutQuint);

            LeftArea.MoveToX(-150, 1800, Easing.OutQuint);
            LeftArea.FadeOut(200, Easing.OutQuint);

            wedgeBackground.ScaleTo(2.4f, 400, Easing.OutQuint);

            this.FadeOut(400, Easing.OutQuint);

            FilterControl.Deactivate();
        }

        private bool isHandlingLooping;

        private void beginLooping()
        {
            if (!ControlGlobalMusic)
                return;

            Debug.Assert(!isHandlingLooping);

            isHandlingLooping = true;

            ensureTrackLooping(Beatmap.Value, TrackChangeDirection.None);

            music.TrackChanged += ensureTrackLooping;
        }

        private void endLooping()
        {
            // may be called multiple times during screen exit process.
            if (!isHandlingLooping)
                return;

            music.CurrentTrack.Looping = isHandlingLooping = false;

            music.TrackChanged -= ensureTrackLooping;
        }

        private void ensureTrackLooping(IWorkingBeatmap beatmap, TrackChangeDirection changeDirection)
            => beatmap.PrepareTrackForPreview(true);

        public override bool OnBackButton()
        {
            if (ModSelect.State.Value == Visibility.Visible)
            {
                ModSelect.Hide();
                return true;
            }

            return false;
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            decoupledRuleset.UnbindAll();

            if (music.IsNotNull())
                music.TrackChanged -= ensureTrackLooping;

            modSelectOverlayRegistration?.Dispose();
        }

        /// <summary>
        /// Allow components in SongSelect to update their loaded beatmap details.
        /// This is a debounced call (unlike directly binding to WorkingBeatmap.ValueChanged).
        /// </summary>
        /// <param name="beatmap">The working beatmap.</param>
        private void updateComponentFromBeatmap(WorkingBeatmap beatmap)
        {
            // If not the current screen, this will be applied in OnResuming.
            if (this.IsCurrentScreen())
            {
                ApplyToBackground(backgroundModeBeatmap =>
                {
                    backgroundModeBeatmap.Beatmap = beatmap;
                    backgroundModeBeatmap.IgnoreUserSettings.Value = true;
                    backgroundModeBeatmap.FadeColour(Color4.White, 250);

                    applyBlurToBackground(backgroundModeBeatmap);
                });
            }

            beatmapInfoWedge.Beatmap = beatmap;

            BeatmapDetails.Beatmap = beatmap;

            ModSelect.Beatmap = beatmap;

            advancedStats.BeatmapInfo = beatmap.BeatmapInfo;

            bool beatmapSelected = beatmap is not DummyWorkingBeatmap;

            if (beatmapSelected)
                beatmapOptionsButton.Enabled.Value = true;
            else
            {
                beatmapOptionsButton.Enabled.Value = false;
                BeatmapOptions.Hide();
            }
        }

        private void applyBlurToBackground(BackgroundScreenBeatmap backgroundModeBeatmap)
        {
            backgroundModeBeatmap.BlurAmount.Value = configBackgroundBlur.Value ? BACKGROUND_BLUR : 0f;
            backgroundModeBeatmap.DimWhenUserSettingsIgnored.Value = configBackgroundBlur.Value ? 0 : 0.4f;

            wedgeBackground.FadeTo(configBackgroundBlur.Value ? 0.5f : 0.2f, UserDimContainer.BACKGROUND_FADE_DURATION, Easing.OutQuint);
        }

        private readonly WeakReference<ITrack?> lastTrack = new WeakReference<ITrack?>(null);

        /// <summary>
        /// Ensures some music is playing for the current track.
        /// Will resume playback from a manual user pause if the track has changed.
        /// </summary>
        private void ensurePlayingSelected()
        {
            if (!ControlGlobalMusic)
                return;

            ITrack track = music.CurrentTrack;

            bool isNewTrack = !lastTrack.TryGetTarget(out var last) || last != track;

            if (!track.IsRunning && (music.UserPauseRequested != true || isNewTrack))
            {
                Logger.Log($"Song select decided to {nameof(ensurePlayingSelected)}");
                music.Play(true);
            }

            lastTrack.SetTarget(track);
        }

        private void carouselBeatmapsLoaded()
        {
            bindBindables();
            Scheduler.AddOnce(updateVisibleBeatmapCount);

            Carousel.AllowSelection = true;

            // If a selection was already obtained, do not attempt to update the selected beatmap.
            if (Carousel.SelectedBeatmapSet != null)
                return;

            // Attempt to select the current beatmap on the carousel, if it is valid to be selected.
            if (!Beatmap.IsDefault && Beatmap.Value.BeatmapSetInfo?.DeletePending == false && Beatmap.Value.BeatmapSetInfo?.Protected == false)
            {
                if (Carousel.SelectBeatmap(Beatmap.Value.BeatmapInfo, false))
                    return;

                // prefer not changing ruleset at this point, so look for another difficulty in the currently playing beatmap
                var found = Beatmap.Value.BeatmapSetInfo.Beatmaps.FirstOrDefault(b => b.Ruleset.Equals(decoupledRuleset.Value));

                if (found != null && Carousel.SelectBeatmap(found, false))
                    return;
            }

            // If the current active beatmap could not be selected, select a new random beatmap.
            if (!Carousel.SelectNextRandom())
            {
                // in the case random selection failed, we want to trigger selectionChanged
                // to show the dummy beatmap (we have nothing else to display).
                performUpdateSelected();
            }
        }

        private void updateVisibleBeatmapCount()
        {
            // Intentionally not localised until we have proper support for this (see https://github.com/ppy/osu-framework/pull/4918
            // but also in this case we want support for formatting a number within a string).
            int carouselCountDisplayed = Carousel.CountDisplayed;
            FilterControl.InformationalText = carouselCountDisplayed != 1 ? $"{carouselCountDisplayed:#,0} matches" : $"{carouselCountDisplayed:#,0} match";
        }

        private bool boundLocalBindables;

        private void bindBindables()
        {
            if (boundLocalBindables)
                return;

            // manual binding to parent ruleset to allow for delayed load in the incoming direction.
            transferRulesetValue();

            Ruleset.ValueChanged += r => updateSelectedRuleset(r.NewValue);

            decoupledRuleset.ValueChanged += r =>
            {
                bool wasDisabled = Ruleset.Disabled;

                // a sub-screen may have taken a lease on this decoupled ruleset bindable,
                // which would indirectly propagate to the game-global bindable via the `DisabledChanged` callback below.
                // to make sure changes sync without crashes, lift the disable for a short while to sync, and then restore the old value.
                Ruleset.Disabled = false;
                Ruleset.Value = r.NewValue;
                Ruleset.Disabled = wasDisabled;
            };
            decoupledRuleset.DisabledChanged += r => Ruleset.Disabled = r;

            Beatmap.BindValueChanged(updateCarouselSelection);

            boundLocalBindables = true;
        }

        /// <summary>
        /// Transfer the game-wide ruleset to the local decoupled ruleset.
        /// Will immediately run filter operations if required.
        /// </summary>
        /// <returns>Whether a transfer occurred.</returns>
        private bool transferRulesetValue()
        {
            if (decoupledRuleset.Value?.Equals(Ruleset.Value) == true)
                return false;

            Logger.Log($"decoupled ruleset transferred (\"{decoupledRuleset.Value}\" -> \"{Ruleset.Value}\")");
            rulesetNoDebounce = decoupledRuleset.Value = Ruleset.Value;

            // if we have a pending filter operation, we want to run it now.
            // it could change selection (ie. if the ruleset has been changed).
            Carousel.FlushPendingFilterOperations();

            return true;
        }

        /// <summary>
        /// Request to delete a specific beatmap.
        /// </summary>
        public void DeleteBeatmap(BeatmapSetInfo? beatmap)
        {
            if (beatmap == null) return;

            dialogOverlay?.Push(new BeatmapDeleteDialog(beatmap));
        }

        /// <summary>
        /// Request to clear the scores of a specific beatmap.
        /// </summary>
        public void ClearScores(BeatmapInfo? beatmapInfo)
        {
            if (beatmapInfo == null) return;

            dialogOverlay?.Push(new BeatmapClearScoresDialog(beatmapInfo, () =>
                // schedule done here rather than inside the dialog as the dialog may fade out and never callback.
                Schedule(() => BeatmapDetails.Refresh())));
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (!this.IsCurrentScreen()) return false;

            switch (e.Action)
            {
                case GlobalAction.Select:
                    FinaliseSelection();
                    return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.Repeat) return false;

            switch (e.Key)
            {
                case Key.Delete:
                    if (e.ShiftPressed)
                    {
                        if (!Beatmap.IsDefault)
                            DeleteBeatmap(Beatmap.Value.BeatmapSetInfo);
                        return true;
                    }

                    break;
            }

            return base.OnKeyDown(e);
        }

        private partial class VerticalMaskingContainer : Container
        {
            private const float panel_overflow = 1.2f;

            protected override Container<Drawable> Content { get; }

            public VerticalMaskingContainer()
            {
                RelativeSizeAxes = Axes.Both;
                Masking = true;
                Anchor = Anchor.Centre;
                Origin = Anchor.Centre;
                Width = panel_overflow; // avoid horizontal masking so the panels don't clip when screen stack is pushed.
                InternalChild = Content = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 1 / panel_overflow,
                };
            }
        }

        /// <summary>
        /// Handles mouse interactions required when moving away from the carousel.
        /// </summary>
        internal partial class LeftSideInteractionContainer : Container
        {
            private readonly Action? resetCarouselPosition;

            public LeftSideInteractionContainer(Action resetCarouselPosition)
            {
                this.resetCarouselPosition = resetCarouselPosition;
            }

            // we want to block plain scrolls on the left side so that they don't scroll the carousel,
            // but also we *don't* want to handle scrolls when they're combined with keyboard modifiers
            // as those will usually correspond to other interactions like adjusting volume.
            protected override bool OnScroll(ScrollEvent e) => !e.ControlPressed && !e.AltPressed && !e.ShiftPressed && !e.SuperPressed;

            protected override bool OnMouseDown(MouseDownEvent e) => true;

            protected override bool OnHover(HoverEvent e)
            {
                resetCarouselPosition?.Invoke();
                return base.OnHover(e);
            }
        }

        internal partial class SoloModSelectOverlay : UserModSelectOverlay
        {
            protected override bool ShowPresets => true;
        }
    }
}
