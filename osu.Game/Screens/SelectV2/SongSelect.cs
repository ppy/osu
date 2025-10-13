// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Bindables;
using osu.Framework.Development;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Input.StateChanges;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Volume;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;
using osu.Game.Screens.Footer;
using osu.Game.Screens.Menu;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Select;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;

namespace osu.Game.Screens.SelectV2
{
    /// <summary>
    /// This screen is intended to house all components introduced in the new song select design to add transitions and examine the overall look.
    /// This will be gradually built upon and ultimately replace <see cref="Select.SongSelect"/> once everything is in place.
    /// </summary>
    [Cached(typeof(ISongSelect))]
    public abstract partial class SongSelect : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>, ISongSelect, IHandlePresentBeatmap
    {
        /// <summary>
        /// A debounce that governs how long after a panel is selected before the rest of song select (and the game at large)
        /// updates to show that selection.
        ///
        /// This is intentionally slightly higher than key repeat, but low enough to not impede user experience.
        /// </summary>
        public const int SELECTION_DEBOUNCE = 150;

        /// <summary>
        /// A general "global" debounce to be applied to anything aggressive difficulty calculation at song select,
        /// either after selection or after a panel comes on screen. Value should be low enough that users don't complain,
        /// but otherwise as high as possible to reduce overheads.
        /// </summary>
        public const int DIFFICULTY_CALCULATION_DEBOUNCE = 150;

        private const float logo_scale = 0.4f;
        private const double fade_duration = 300;

        public const float WEDGE_CONTENT_MARGIN = CORNER_RADIUS_HIDE_OFFSET + OsuGame.SCREEN_EDGE_MARGIN;
        public const float CORNER_RADIUS_HIDE_OFFSET = 20f;
        public const float ENTER_DURATION = 600;

        /// <summary>
        /// Whether this song select instance should take control of the global track,
        /// applying looping and preview offsets.
        /// </summary>
        protected bool ControlGlobalMusic { get; init; } = true;

        // Colour scheme for mod overlay is left as default (green) to match mods button.
        // Not sure about this, but we'll iterate based on feedback.
        private readonly ModSelectOverlay modSelectOverlay = new UserModSelectOverlay
        {
            ShowPresets = true,
        };

        private ModSpeedHotkeyHandler modSpeedHotkeyHandler = null!;

        // Blue is the most neutral choice, so I'm using that for now.
        // Purple makes the most sense to match the "gameplay" flow, but it's a bit too strong for the current design.
        // TODO: Colour scheme choice should probably be customisable by the user.
        [Cached]
        private readonly OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        private BeatmapCarousel carousel = null!;

        private FilterControl filterControl = null!;
        private BeatmapTitleWedge titleWedge = null!;
        private BeatmapDetailsArea detailsArea = null!;
        private FillFlowContainer wedgesContainer = null!;
        private Box rightGradientBackground = null!;
        private Container mainContent = null!;
        private SkinnableContainer skinnableContent = null!;

        private NoResultsPlaceholder noResultsPlaceholder = null!;

        public override bool? ApplyModTrackAdjustments => true;

        public override bool ShowFooter => true;

        private Sample? errorSample;

        [Resolved]
        private OsuGameBase? game { get; set; }

        [Resolved]
        private OsuLogo? logo { get; set; }

        [Resolved]
        private BeatmapSetOverlay? beatmapOverlay { get; set; }

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private ManageCollectionsDialog? collectionsDialog { get; set; }

        [Resolved]
        private DifficultyRecommender? difficultyRecommender { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        private InputManager inputManager = null!;

        private readonly RealmPopulatingOnlineLookupSource onlineLookupSource = new RealmPopulatingOnlineLookupSource();

        private Bindable<bool> configBackgroundBlur = null!;
        private Bindable<bool> showConvertedBeatmaps = null!;

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            errorSample = audio.Samples.Get(@"UI/generic-error");

            AddRangeInternal(new Drawable[]
            {
                new GlobalScrollAdjustsVolume(),
                onlineLookupSource,
                mainContent = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Bottom = ScreenFooter.HEIGHT },
                    Child = new OsuContextMenuContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Child = new PopoverContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Width = 0.6f,
                                    Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.3f), Color4.Black.Opacity(0f)),
                                },
                                mainGridContainer = new GridContainer // used for max width implementation
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Content = new[]
                                    {
                                        new[]
                                        {
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                // Ensure the left components are on top of the carousel both visually (although they should never overlay)
                                                // but more importantly, for input purposes to allow the scroll-to-selection logic to override carousel's
                                                // screen-wide scroll handling.
                                                Depth = float.MinValue,
                                                Shear = OsuGame.SHEAR,
                                                Padding = new MarginPadding
                                                {
                                                    Top = -CORNER_RADIUS_HIDE_OFFSET,
                                                    Left = -CORNER_RADIUS_HIDE_OFFSET,
                                                },
                                                Children = new Drawable[]
                                                {
                                                    new Container
                                                    {
                                                        // Pad enough to only reset scroll when well into the left wedge areas.
                                                        Padding = new MarginPadding { Right = 40 },
                                                        RelativeSizeAxes = Axes.Both,
                                                        Child = new Select.SongSelect.LeftSideInteractionContainer(() =>
                                                        {
                                                            carousel.ExpandGroupForCurrentSelection();
                                                            carousel.ScrollToSelection();
                                                        })
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                        },
                                                    },
                                                    wedgesContainer = new FillFlowContainer
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Spacing = new Vector2(0f, 4f),
                                                        Direction = FillDirection.Vertical,
                                                        Children = new Drawable[]
                                                        {
                                                            new ShearAligningWrapper(titleWedge = new BeatmapTitleWedge()),
                                                            new ShearAligningWrapper(detailsArea = new BeatmapDetailsArea()),
                                                        },
                                                    },
                                                }
                                            },
                                            Empty(),
                                            new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    rightGradientBackground = new Box
                                                    {
                                                        Anchor = Anchor.TopRight,
                                                        Origin = Anchor.TopRight,
                                                        Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.0f), Color4.Black.Opacity(0.5f)),
                                                        RelativeSizeAxes = Axes.Both,
                                                    },
                                                    new Container
                                                    {
                                                        RelativeSizeAxes = Axes.Both,
                                                        Padding = new MarginPadding
                                                        {
                                                            Top = FilterControl.HEIGHT_FROM_SCREEN_TOP + 5,
                                                            Bottom = 5,
                                                        },
                                                        Children = new Drawable[]
                                                        {
                                                            carousel = new BeatmapCarousel
                                                            {
                                                                BleedTop = FilterControl.HEIGHT_FROM_SCREEN_TOP + 5,
                                                                BleedBottom = ScreenFooter.HEIGHT + 5,
                                                                RelativeSizeAxes = Axes.Both,
                                                                RequestPresentBeatmap = b => SelectAndRun(b, OnStart),
                                                                RequestSelection = queueBeatmapSelection,
                                                                RequestRecommendedSelection = requestRecommendedSelection,
                                                                NewItemsPresented = newItemsPresented,
                                                            },
                                                            noResultsPlaceholder = new NoResultsPlaceholder
                                                            {
                                                                RequestClearFilterText = () => filterControl.Search(string.Empty)
                                                            }
                                                        }
                                                    },
                                                    filterControl = new FilterControl
                                                    {
                                                        Anchor = Anchor.TopRight,
                                                        Origin = Anchor.TopRight,
                                                        RelativeSizeAxes = Axes.X,
                                                    },
                                                }
                                            },
                                        },
                                    }
                                },
                            }
                        },
                    }
                },
                skinnableContent = new SkinnableContainer(new GlobalSkinnableContainerLookup(GlobalSkinnableContainers.SongSelect))
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                },
                modSpeedHotkeyHandler = new ModSpeedHotkeyHandler(),
                modSelectOverlay,
            });

            configBackgroundBlur = config.GetBindable<bool>(OsuSetting.SongSelectBackgroundBlur);
            configBackgroundBlur.BindValueChanged(e =>
            {
                if (!this.IsCurrentScreen())
                    return;

                updateBackgroundDim();
            });

            showConvertedBeatmaps = config.GetBindable<bool>(OsuSetting.ShowConvertedBeatmaps);
        }

        private void requestRecommendedSelection(IEnumerable<GroupedBeatmap> groupedBeatmaps)
        {
            var recommendedBeatmap = difficultyRecommender?.GetRecommendedBeatmap(groupedBeatmaps.Select(gb => gb.Beatmap)) ?? groupedBeatmaps.First().Beatmap;
            queueBeatmapSelection(groupedBeatmaps.First(bug => bug.Beatmap.Equals(recommendedBeatmap)));
        }

        /// <summary>
        /// Called when a selection is made to progress away from the song select screen.
        ///
        /// This is the default action which should be provided to <see cref="SelectAndRun"/>.
        /// </summary>
        protected abstract void OnStart();

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new ScreenFooterButton[]
        {
            new FooterButtonMods(modSelectOverlay)
            {
                Hotkey = GlobalAction.ToggleModSelection,
                Current = Mods,
                RequestDeselectAllMods = () =>
                {
                    if (modSelectOverlay.State.Value == Visibility.Visible)
                        modSelectOverlay.DeselectAll();
                    else
                        Mods.Value = Array.Empty<Mod>();
                }
            },
            new FooterButtonRandom
            {
                NextRandom = () =>
                {
                    if (!carousel.NextRandom())
                        errorSample?.Play();
                },
                PreviousRandom = () =>
                {
                    if (!carousel.PreviousRandom())
                        errorSample?.Play();
                }
            },
            new FooterButtonOptions
            {
                Hotkey = GlobalAction.ToggleBeatmapOptions,
            }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            inputManager = GetContainingInputManager()!;

            filterControl.CriteriaChanged += criteriaChanged;

            modSelectOverlay.State.BindValueChanged(v =>
            {
                if (!this.IsCurrentScreen())
                    return;

                logo?.FadeTo(v.NewValue == Visibility.Visible ? 0f : 1f, 200, Easing.OutQuint);
            });

            Beatmap.BindValueChanged(_ =>
            {
                if (!this.IsCurrentScreen())
                    return;

                ensureGlobalBeatmapValid();

                ensurePlayingSelected();
                updateBackgroundDim();
                updateWedgeVisibility();
                fetchOnlineInfo();
            });
        }

        protected override void Update()
        {
            base.Update();

            detailsArea.Height = wedgesContainer.DrawHeight - titleWedge.LayoutSize.Y - 4;

            float widescreenBonusWidth = Math.Max(0, DrawWidth / DrawHeight - 2f);

            mainGridContainer.ColumnDimensions = new[]
            {
                new Dimension(GridSizeMode.Relative, 0.5f, maxSize: 700 + widescreenBonusWidth * 100),
                new Dimension(),
                new Dimension(GridSizeMode.Relative, 0.5f, minSize: 500, maxSize: 700 + widescreenBonusWidth * 300),
            };

            if (this.IsCurrentScreen())
                updateDebounce();
        }

        #region Selection debounce

        private BeatmapInfo? debounceQueuedSelection;
        private double debounceElapsedTime;

        private void debounceQueueSelection(BeatmapInfo beatmap)
        {
            debounceQueuedSelection = beatmap;
            debounceElapsedTime = 0;
        }

        private void updateDebounce()
        {
            if (debounceQueuedSelection == null) return;

            double elapsed = Clock.ElapsedFrameTime;

            // When a key is being held, assume the user is traversing the carousel using key repeat.
            // We want to change panels less often in this state (basically making debounce longer than initial key repeat, at least).
            double debounceInterval = inputManager.CurrentState.Keyboard.Keys.HasAnyButtonPressed ? SELECTION_DEBOUNCE * 2 : SELECTION_DEBOUNCE;

            // avoid debounce running early if there's a single long frame.
            if (!DebugUtils.IsNUnitRunning && Clock.FramesPerSecond > 0)
                elapsed = Math.Min(1000 / Clock.FramesPerSecond, elapsed);

            debounceElapsedTime += elapsed;

            if (debounceElapsedTime >= debounceInterval)
                performDebounceSelection();
        }

        private void performDebounceSelection()
        {
            if (debounceQueuedSelection == null) return;

            try
            {
                if (Beatmap.Value.BeatmapInfo.Equals(debounceQueuedSelection))
                    return;

                Beatmap.Value = beatmaps.GetWorkingBeatmap(debounceQueuedSelection);
            }
            finally
            {
                cancelDebounceSelection();
            }
        }

        private void cancelDebounceSelection()
        {
            debounceQueuedSelection = null;
            debounceElapsedTime = 0;
        }

        #endregion

        #region Audio

        [Resolved]
        private MusicController music { get; set; } = null!;

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

                // Only restart playback if a new track.
                // This is important so that when exiting gameplay, the track is not restarted back to the preview point.
                music.Play(isNewTrack);
            }

            lastTrack.SetTarget(track);
        }

        private bool isHandlingLooping;

        private void beginLooping()
        {
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

        #endregion

        #region Selection handling

        /// <summary>
        /// Finalises selection on the given <see cref="BeatmapInfo"/> and runs the provided action if possible.
        /// </summary>
        /// <param name="beatmap">The beatmap which should be selected. If not provided, the current globally selected beatmap will be used.</param>
        /// <param name="startAction">The action to perform if conditions are met to be able to proceed. May not be invoked if in an invalid state.</param>
        public void SelectAndRun(BeatmapInfo beatmap, Action startAction)
        {
            if (!this.IsCurrentScreen())
                return;

            if (!checkBeatmapValidForSelection(beatmap))
                return;

            // To ensure sanity, cancel any pending selection as we are about to force a selection.
            // Carousel selection will update to the forced selection via a call of `ensureGlobalBeatmapValid` below, or when song select becomes current again.
            cancelDebounceSelection();

            // Forced refetch is important here to guarantee correct invalidation across all difficulties (editor specific).
            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap, true);

            if (Beatmap.IsDefault)
                return;

            startAction();
        }

        /// <summary>
        /// Prepares the proposed beatmap for global selection based on a carousel user-performed action.
        /// </summary>
        /// <remarks>
        /// Calling this method will:
        /// - Immediately update the selection the carousel.
        /// - After <see cref="SELECTION_DEBOUNCE"/>, update the global beatmap. This in turn causes song select visuals (title, details, leaderboard) to update.
        ///   This debounce is intended to avoid high overheads from churning lookups while a user is changing selection via rapid keyboard operations.
        /// </remarks>
        /// <param name="groupedBeatmap">The beatmap to be selected.</param>
        private void queueBeatmapSelection(GroupedBeatmap groupedBeatmap)
        {
            if (!this.IsCurrentScreen())
                return;

            carousel.CurrentGroupedBeatmap = groupedBeatmap;

            // Debounce consideration is to avoid beatmap churn on key repeat selection.
            debounceQueueSelection(groupedBeatmap.Beatmap);
        }

        private bool ensureGlobalBeatmapValid()
        {
            if (!this.IsCurrentScreen())
                return false;

            performDebounceSelection();

            // While filtering, let's not ever attempt to change selection.
            // This will be resolved after the filter completes, see `newItemsPresented`.
            if (IsFiltering)
                return false;

            // Refetch to be confident that the current selection is still valid. It may have been deleted or hidden.
            var currentBeatmap = beatmaps.GetWorkingBeatmap(Beatmap.Value.BeatmapInfo, true);
            bool validSelection = checkBeatmapValidForSelection(currentBeatmap.BeatmapInfo);

            if (validSelection)
            {
                carousel.CurrentBeatmap = currentBeatmap.BeatmapInfo;
                return true;
            }

            // If there was no beatmap selected, pick a random one.
            if (Beatmap.IsDefault)
            {
                validSelection = carousel.NextRandom();
                performDebounceSelection();
                return validSelection;
            }

            // If a previous non-default selection became non-valid, it was likely hidden or deleted.
            if (!validSelection)
            {
                // In the case a difficulty was hidden or removed, prefer selecting another difficulty from the same set.
                var activeSet = currentBeatmap.BeatmapSetInfo;

                var validBeatmaps = activeSet.Beatmaps.Where(checkBeatmapValidForSelection).ToArray();

                if (validBeatmaps.Any())
                {
                    var beatmap = difficultyRecommender?.GetRecommendedBeatmap(validBeatmaps) ?? validBeatmaps.First();
                    carousel.CurrentBeatmap = beatmap;
                    debounceQueueSelection(beatmap);
                    return true;
                }
            }

            // If all else fails, use the default beatmap.
            Beatmap.SetDefault();
            performDebounceSelection();

            return validSelection;
        }

        private bool checkBeatmapValidForSelection(BeatmapInfo beatmap)
        {
            if (!beatmap.AllowGameplayWithRuleset(Ruleset.Value, showConvertedBeatmaps.Value))
                return false;

            if (beatmap.Hidden)
                return false;

            if (beatmap.BeatmapSet == null)
                return false;

            if (beatmap.BeatmapSet.Protected || beatmap.BeatmapSet.DeletePending)
                return false;

            return true;
        }

        #endregion

        #region Transitions

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeIn();
            onArrivingAtScreen();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            this.FadeIn(fade_duration, Easing.OutQuint);
            onArrivingAtScreen();

            ensureGlobalBeatmapValid();

            detailsArea.Refresh();

            if (ControlGlobalMusic)
            {
                // restart playback on returning to song select, regardless.
                // not sure this should be a permanent thing (we may want to leave a user pause paused even on returning)
                music.ResetTrackAdjustments();
                music.Play(requestedByUser: true);
            }
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            carousel.VisuallyFocusSelected = true;

            this.FadeOut(fade_duration, Easing.OutQuint);
            onLeavingScreen();

            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);
            onLeavingScreen();

            return base.OnExiting(e);
        }

        private void onArrivingAtScreen()
        {
            modSelectOverlay.Beatmap.BindTo(Beatmap);
            // required due to https://github.com/ppy/osu-framework/issues/3218
            modSelectOverlay.SelectedMods.Disabled = false;
            modSelectOverlay.SelectedMods.BindTo(Mods);

            carousel.VisuallyFocusSelected = false;

            updateWedgeVisibility();

            if (ControlGlobalMusic)
            {
                // Avoid abruptly starting playback at preview point.
                // Importantly, this should be done before looping is setup to ensure we get the correct imminent `IsPlaying` state.
                if (!music.IsPlaying)
                {
                    music.DuckMomentarily(0, new DuckParameters
                    {
                        DuckDuration = 0,
                        DuckVolumeTo = 0,
                        RestoreDuration = 800,
                        RestoreEasing = Easing.OutQuint
                    });
                }

                beginLooping();
            }

            ensureGlobalBeatmapValid();

            ensurePlayingSelected();
            updateBackgroundDim();
            fetchOnlineInfo(force: true);
        }

        private void onLeavingScreen()
        {
            restoreBackground();

            modSelectOverlay.SelectedMods.UnbindFrom(Mods);
            modSelectOverlay.Beatmap.UnbindFrom(Beatmap);

            updateWedgeVisibility();

            endLooping();
        }

        protected override void LogoArriving(OsuLogo logo, bool resuming)
        {
            base.LogoArriving(logo, resuming);

            if (logo.Alpha > 0.8f && resuming)
                Footer?.StartTrackingLogo(logo, 400, Easing.OutQuint);
            else
            {
                logo.Hide();
                logo.ScaleTo(0.2f);
                Footer?.StartTrackingLogo(logo);
            }

            logo.FadeIn(240, Easing.OutQuint);
            logo.ScaleTo(logo_scale, 240, Easing.OutQuint);

            logo.Action = () =>
            {
                SelectAndRun(Beatmap.Value.BeatmapInfo, OnStart);
                return false;
            };
        }

        protected override void LogoSuspending(OsuLogo logo)
        {
            base.LogoSuspending(logo);
            Footer?.StopTrackingLogo();
        }

        protected override void LogoExiting(OsuLogo logo)
        {
            base.LogoExiting(logo);

            Footer?.StopTrackingLogo();

            logo.ScaleTo(0.2f, 120, Easing.Out);
            logo.FadeOut(120, Easing.Out);
        }

        private void updateWedgeVisibility()
        {
            // Ensure we don't show an invalid selection before the carousel has finished initially filtering.
            // This avoids a flicker of a placeholder or invalid beatmap before a proper selection.
            //
            // After the carousel finishes filtering, it will attempt a selection then call this method again.
            if (!CarouselItemsPresented && !checkBeatmapValidForSelection(Beatmap.Value.BeatmapInfo))
                return;

            if (carousel.VisuallyFocusSelected)
            {
                titleWedge.Hide();
                detailsArea.Hide();
                filterControl.Hide();
            }
            else
            {
                titleWedge.Show();
                detailsArea.Show();
                filterControl.Show();
            }
        }

        private void updateBackgroundDim() => ApplyToBackground(backgroundModeBeatmap =>
        {
            backgroundModeBeatmap.Beatmap = Beatmap.Value;
            backgroundModeBeatmap.IgnoreUserSettings.Value = true;

            backgroundModeBeatmap.DimWhenUserSettingsIgnored.Value = 0.1f;

            // Required to undo results screen dimming the background.
            // Probably needs more thought because this needs to be in every `ApplyToBackground` currently to restore sane defaults.
            backgroundModeBeatmap.FadeColour(Color4.White, 250);

            backgroundModeBeatmap.BlurAmount.Value = revealingBackground == null && configBackgroundBlur.Value ? 20 : 0f;
        });

        #endregion

        #region Filtering

        /// <summary>
        /// Whether the carousel has finished initial presentation of beatmap panels.
        /// </summary>
        public bool CarouselItemsPresented { get; private set; }

        /// <summary>
        /// Whether the carousel is or will be undergoing a filter operation.
        /// </summary>
        public bool IsFiltering => carousel.IsFiltering || filterDebounce?.State == ScheduledDelegate.RunState.Waiting;

        private const double filter_delay = 250;

        private ScheduledDelegate? filterDebounce;

        private void criteriaChanged(FilterCriteria criteria)
        {
            filterDebounce?.Cancel();

            // The first filter needs to be applied immediately as this triggers the initial carousel load.
            bool isFirstFilter = filterDebounce == null;

            // Criteria change may have included a ruleset change which made the current selection invalid.
            bool isSelectionValid = checkBeatmapValidForSelection(Beatmap.Value.BeatmapInfo);

            filterDebounce = Scheduler.AddDelayed(() => carousel.Filter(criteria, !isSelectionValid), isFirstFilter || !isSelectionValid ? 0 : filter_delay);
        }

        private void newItemsPresented(IEnumerable<CarouselItem> carouselItems)
        {
            if (carousel.Criteria == null)
                return;

            CarouselItemsPresented = true;

            int count = carousel.MatchedBeatmapsCount;

            updateNoResultsPlaceholder();

            // Intentionally not localised until we have proper support for this (see https://github.com/ppy/osu-framework/pull/4918
            // but also in this case we want support for formatting a number within a string).
            filterControl.StatusText = count != 1 ? $"{count:#,0} matches" : $"{count:#,0} match";

            // If there's already a selection update in progress, let's not interrupt it.
            // Interrupting could cause the debounce interval to be reduced.
            //
            // `ensureGlobalBeatmapValid` is run post-selection which will resolve any pending incompatibilities (see `Beatmap` bindable callback).
            if (debounceQueuedSelection == null)
                ensureGlobalBeatmapValid();

            updateWedgeVisibility();
        }

        private void updateNoResultsPlaceholder()
        {
            int count = carousel.MatchedBeatmapsCount;

            if (count == 0)
            {
                if (noResultsPlaceholder.State.Value == Visibility.Hidden)
                {
                    // Duck audio temporarily when the no results placeholder becomes visible.
                    //
                    // Temporary ducking makes it easier to avoid scenarios where the ducking interacts badly
                    // with other global UI components (like overlays).
                    music.DuckMomentarily(400, new DuckParameters
                    {
                        DuckVolumeTo = 1,
                        DuckCutoffTo = 500,
                        DuckDuration = 250,
                        RestoreDuration = 2000,
                    });
                }

                noResultsPlaceholder.Show();
                noResultsPlaceholder.Filter = carousel.Criteria!;

                rightGradientBackground.ResizeWidthTo(3, 1000, Easing.OutPow10);
            }
            else
            {
                noResultsPlaceholder.Hide();

                rightGradientBackground.ResizeWidthTo(1, 400, Easing.OutPow10);
            }
        }

        #endregion

        #region Input

        private ScheduledDelegate? revealingBackground;

        private GridContainer mainGridContainer = null!;

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            var containingInputManager = GetContainingInputManager();

            // I don't know why this works, but it does.
            // If the carousel panels are hovered, hovered no longer contains the screen.
            // Maybe there's a better way of doing this, but I couldn't immediately find a good setup.
            bool mouseDownPriority = containingInputManager!.HoveredDrawables.Contains(this);

            // Touch input synthesises right clicks, which allow absolute scroll of the carousel.
            // For simplicity, disable this functionality on mobile.
            bool isTouchInput = e.CurrentState.Mouse.LastSource is ISourcedFromTouch;

            if (!carousel.AbsoluteScrolling && !isTouchInput && mouseDownPriority && revealingBackground == null)
            {
                revealingBackground = Scheduler.AddDelayed(() =>
                {
                    if (containingInputManager.DraggedDrawable != null)
                    {
                        revealingBackground = null;
                        return;
                    }

                    mainContent.ResizeWidthTo(1.2f, 600, Easing.OutQuint);
                    mainContent.ScaleTo(1.2f, 600, Easing.OutQuint);
                    mainContent.FadeOut(200, Easing.OutQuint);

                    skinnableContent.ResizeWidthTo(1.2f, 600, Easing.OutQuint);
                    skinnableContent.ScaleTo(1.2f, 600, Easing.OutQuint);
                    skinnableContent.FadeOut(200, Easing.OutQuint);

                    updateBackgroundDim();

                    Footer?.Hide();
                }, 200);
            }

            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            restoreBackground();
            base.OnMouseUp(e);
        }

        private void restoreBackground()
        {
            if (revealingBackground == null)
                return;

            if (revealingBackground.State == ScheduledDelegate.RunState.Complete)
            {
                mainContent.ResizeWidthTo(1f, 500, Easing.OutQuint);
                mainContent.ScaleTo(1, 500, Easing.OutQuint);
                mainContent.FadeIn(500, Easing.OutQuint);

                skinnableContent.ResizeWidthTo(1f, 500, Easing.OutQuint);
                skinnableContent.ScaleTo(1, 500, Easing.OutQuint);
                skinnableContent.FadeIn(500, Easing.OutQuint);

                Footer?.Show();
            }

            revealingBackground.Cancel();
            revealingBackground = null;

            updateBackgroundDim();
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (!this.IsCurrentScreen()) return false;

            if (game == null)
                return false;

            var flattenedMods = ModUtils.FlattenMods(game.AvailableMods.Value.SelectMany(kv => kv.Value));

            switch (e.Action)
            {
                case GlobalAction.Select:
                    // in most circumstances this is handled already by the carousel itself, but there are cases where it will not be.
                    // one of which is filtering out all visible beatmaps and attempting to start gameplay.
                    // in that case, users still expect a `Select` press to advance to gameplay anyway, using the ambient selected beatmap if there is one,
                    // which matches the behaviour resulting from clicking the osu! cookie in that scenario.
                    SelectAndRun(Beatmap.Value.BeatmapInfo, OnStart);
                    return true;

                case GlobalAction.IncreaseModSpeed:
                    return modSpeedHotkeyHandler.ChangeSpeed(0.05, flattenedMods);

                case GlobalAction.DecreaseModSpeed:
                    return modSpeedHotkeyHandler.ChangeSpeed(-0.05, flattenedMods);
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
                            Delete(Beatmap.Value.BeatmapSetInfo);
                        return true;
                    }

                    break;
            }

            return base.OnKeyDown(e);
        }

        #endregion

        #region Online lookups

        public enum BeatmapSetLookupStatus
        {
            InProgress,
            Completed,
        }

        public class BeatmapSetLookupResult
        {
            public BeatmapSetLookupStatus Status { get; }
            public APIBeatmapSet? Result { get; }

            private BeatmapSetLookupResult(BeatmapSetLookupStatus status, APIBeatmapSet? result)
            {
                Status = status;
                Result = result;
            }

            public static BeatmapSetLookupResult InProgress() => new BeatmapSetLookupResult(BeatmapSetLookupStatus.InProgress, null);
            public static BeatmapSetLookupResult Completed(APIBeatmapSet? beatmapSet) => new BeatmapSetLookupResult(BeatmapSetLookupStatus.Completed, beatmapSet);
        }

        /// <summary>
        /// Result of the latest online beatmap set lookup.
        /// Note that this being <see langword="null"/> or <see cref="BeatmapSetLookupResult.InProgress"/> is different from
        /// being a <see cref="BeatmapSetLookupResult.Completed"/> with a <see cref="BeatmapSetLookupResult.Result"/> of null.
        /// The former indicates a lookup never occurring or being in progress, while the latter indicates a completed lookup with no result.
        /// </summary>
        [Cached(typeof(IBindable<BeatmapSetLookupResult?>))]
        private readonly Bindable<BeatmapSetLookupResult?> lastLookupResult = new Bindable<BeatmapSetLookupResult?>();

        private CancellationTokenSource? onlineLookupCancellation;
        private Task<APIBeatmapSet?>? currentOnlineLookup;

        private void fetchOnlineInfo(bool force = false)
        {
            var beatmapSetInfo = Beatmap.Value.BeatmapSetInfo;

            if (lastLookupResult.Value?.Result?.OnlineID == beatmapSetInfo.OnlineID && !force)
                return;

            onlineLookupCancellation?.Cancel();
            onlineLookupCancellation = null;

            if (beatmapSetInfo.OnlineID < 0)
            {
                lastLookupResult.Value = BeatmapSetLookupResult.Completed(null);
                return;
            }

            lastLookupResult.Value = BeatmapSetLookupResult.InProgress();
            onlineLookupCancellation = new CancellationTokenSource();
            currentOnlineLookup = onlineLookupSource.GetBeatmapSetAsync(beatmapSetInfo.OnlineID, onlineLookupCancellation.Token);
            currentOnlineLookup.ContinueWith(t =>
            {
                if (t.IsCompletedSuccessfully)
                    Schedule(() => lastLookupResult.Value = BeatmapSetLookupResult.Completed(t.GetResultSafely()));

                if (t.Exception != null)
                {
                    Logger.Log($"Error when fetching online beatmap set: {t.Exception}", LoggingTarget.Network);
                    Schedule(() => lastLookupResult.Value = BeatmapSetLookupResult.Completed(null));
                }
            });
        }

        #endregion

        #region Implementation of ISongSelect

        void ISongSelect.Search(string query) => filterControl.Search(query);

        void ISongSelect.PresentScore(ScoreInfo score)
        {
            Debug.Assert(Beatmap.Value.BeatmapInfo.Equals(score.BeatmapInfo));
            Debug.Assert(Ruleset.Value.Equals(score.Ruleset));

            this.Push(new SoloResultsScreen(score));
        }

        #endregion

        #region IHandlePresentBeatmap

        void IHandlePresentBeatmap.PresentBeatmap(WorkingBeatmap workingBeatmap, RulesetInfo ruleset)
        {
            cancelDebounceSelection();

            var beatmapInfo = workingBeatmap.BeatmapInfo;

            // Don't change the local ruleset if the user is on another ruleset and is showing converted beatmaps.
            // Eventually we probably want to check whether conversion is actually possible for the current ruleset.
            bool requiresRulesetSwitch = !beatmapInfo.Ruleset.Equals(Ruleset.Value)
                                         && (beatmapInfo.Ruleset.OnlineID > 0 || !showConvertedBeatmaps.Value);

            if (requiresRulesetSwitch)
            {
                Ruleset.Value = beatmapInfo.Ruleset;
                Beatmap.Value = workingBeatmap;

                Logger.Log($"Completing {nameof(IHandlePresentBeatmap.PresentBeatmap)} with beatmap {workingBeatmap} ruleset {beatmapInfo.Ruleset}");
            }
            else
            {
                Beatmap.Value = workingBeatmap;

                Logger.Log($"Completing {nameof(IHandlePresentBeatmap.PresentBeatmap)} with beatmap {workingBeatmap} (maintaining ruleset)");
            }
        }

        #endregion

        #region Beatmap management

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public virtual IEnumerable<OsuMenuItem> GetForwardActions(BeatmapInfo beatmap)
        {
            yield return new OsuMenuItem(GlobalActionKeyBindingStrings.Select, MenuItemType.Highlighted, () => SelectAndRun(beatmap, OnStart))
            {
                Icon = FontAwesome.Solid.Check
            };

            yield return new OsuMenuItemSpacer();

            if (beatmap.OnlineID > 0)
            {
                yield return new OsuMenuItem(CommonStrings.Details, MenuItemType.Standard, () => beatmapOverlay?.FetchAndShowBeatmap(beatmap.OnlineID));

                if (beatmap.GetOnlineURL(api, Ruleset.Value) is string url)
                    yield return new OsuMenuItem(CommonStrings.CopyLink, MenuItemType.Standard, () => (game as OsuGame)?.CopyToClipboard(url));
            }

            yield return new OsuMenuItemSpacer();

            foreach (var i in CreateCollectionMenuActions(beatmap))
                yield return i;
        }

        protected IEnumerable<OsuMenuItem> CreateCollectionMenuActions(BeatmapInfo beatmap)
        {
            var collectionItems = realm.Realm.All<BeatmapCollection>()
                                       .OrderBy(c => c.Name)
                                       .AsEnumerable()
                                       .Select(c => new CollectionToggleMenuItem(c.ToLive(realm), beatmap)).Cast<OsuMenuItem>().ToList();

            collectionItems.Add(new OsuMenuItem(CommonStrings.Manage, MenuItemType.Standard, () => manageCollectionsDialog?.Show()));

            yield return new OsuMenuItem(CommonStrings.Collections) { Items = collectionItems };
        }

        public void ManageCollections() => collectionsDialog?.Show();

        public void Delete(BeatmapSetInfo beatmapSet) => dialogOverlay?.Push(new BeatmapDeleteDialog(beatmapSet));

        public void RestoreAllHidden(BeatmapSetInfo beatmapSet)
        {
            foreach (var b in beatmapSet.Beatmaps)
                beatmaps.Restore(b);
        }

        #endregion
    }
}
