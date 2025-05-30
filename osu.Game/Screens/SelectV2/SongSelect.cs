// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Audio.Track;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Collections;
using osu.Game.Database;
using osu.Game.Graphics.Carousel;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Cursor;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input.Bindings;
using osu.Game.Localisation;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Overlays.Volume;
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
    public abstract partial class SongSelect : ScreenWithBeatmapBackground, IKeyBindingHandler<GlobalAction>, ISongSelect
    {
        // this is intentionally slightly higher than key repeat, but low enough to not impede user experience.
        // this avoids rapid churn loading when iterating the carousel using keyboard.
        public const int SELECTION_DEBOUNCE = 100;

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

        private NoResultsPlaceholder noResultsPlaceholder = null!;

        public override bool? ApplyModTrackAdjustments => true;

        public override bool ShowFooter => true;

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

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(new Drawable[]
            {
                new GlobalScrollAdjustsVolume(),
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Width = 0.5f,
                    Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.5f), Color4.Black.Opacity(0f)),
                },
                new Container
                {
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
                                new GridContainer // used for max width implementation
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    ColumnDimensions = new[]
                                    {
                                        new Dimension(GridSizeMode.Relative, 0.5f, maxSize: 660),
                                        new Dimension(),
                                        new Dimension(GridSizeMode.Relative, 0.5f, maxSize: 620),
                                    },
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
                                                        Child = new Select.SongSelect.LeftSideInteractionContainer(() => carousel.ScrollToSelection())
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
                                                    new Box
                                                    {
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
                                                                RequestPresentBeatmap = _ => OnStart(),
                                                                RequestSelection = selectBeatmap,
                                                                RequestRecommendedSelection = selectRecommendedBeatmap,
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
                new SkinnableContainer(new GlobalSkinnableContainerLookup(GlobalSkinnableContainers.SongSelect))
                {
                    RelativeSizeAxes = Axes.Both,
                },
                modSpeedHotkeyHandler = new ModSpeedHotkeyHandler(),
                modSelectOverlay,
            });
        }

        /// <summary>
        /// Called when a selection is made.
        /// </summary>
        /// <returns>If a resultant action occurred that takes the user away from SongSelect.</returns>
        protected abstract bool OnStart();

        public override IReadOnlyList<ScreenFooterButton> CreateFooterButtons() => new ScreenFooterButton[]
        {
            new FooterButtonMods(modSelectOverlay)
            {
                Hotkey = GlobalAction.ToggleModSelection,
                Current = Mods,
                RequestDeselectAllMods = () => Mods.Value = Array.Empty<Mod>()
            },
            new FooterButtonRandom
            {
                NextRandom = () => carousel.NextRandom(),
                PreviousRandom = () => carousel.PreviousRandom()
            },
            new FooterButtonOptions
            {
                Hotkey = GlobalAction.ToggleBeatmapOptions,
            }
        };

        protected override void LoadComplete()
        {
            base.LoadComplete();

            filterControl.CriteriaChanged += criteriaChanged;

            modSelectOverlay.State.BindValueChanged(v =>
            {
                Debug.Assert(this.IsCurrentScreen());

                logo?.ScaleTo(v.NewValue == Visibility.Visible ? 0f : logo_scale, 400, Easing.OutQuint)
                    .FadeTo(v.NewValue == Visibility.Visible ? 0f : 1f, 200, Easing.OutQuint);
            });

            Beatmap.BindValueChanged(_ => updateSelection());
        }

        protected override void Update()
        {
            base.Update();

            detailsArea.Height = wedgesContainer.DrawHeight - titleWedge.LayoutSize.Y - 4;
        }

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
                music.Play(true);
            }

            lastTrack.SetTarget(track);
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

        #endregion

        #region Selection handling

        /// <summary>
        /// Immediately flush any pending selection. Should be run before performing final actions such as leaving the screen.
        /// </summary>
        protected void FinaliseSelection()
        {
            if (selectionDebounce?.State == ScheduledDelegate.RunState.Waiting)
                selectionDebounce.RunTask();
        }

        private ScheduledDelegate? selectionDebounce;

        private void selectRecommendedBeatmap(IEnumerable<BeatmapInfo> beatmaps)
        {
            selectBeatmap(difficultyRecommender?.GetRecommendedBeatmap(beatmaps) ?? beatmaps.First());
        }

        private void selectBeatmap(BeatmapInfo beatmap)
        {
            if (beatmap.BeatmapSet!.Protected)
                return;

            carousel.CurrentSelection = beatmap;

            selectionDebounce?.Cancel();
            selectionDebounce = Scheduler.AddDelayed(() => Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap), SELECTION_DEBOUNCE);
        }

        private void updateSelection() => Scheduler.AddOnce(() =>
        {
            var beatmap = Beatmap.Value;

            carousel.CurrentSelection = beatmap.BeatmapInfo;

            if (this.IsCurrentScreen())
                ensurePlayingSelected();

            // If not the current screen, this will be applied in OnResuming.
            if (this.IsCurrentScreen())
            {
                ApplyToBackground(backgroundModeBeatmap =>
                {
                    backgroundModeBeatmap.BlurAmount.Value = 0;
                    backgroundModeBeatmap.Beatmap = beatmap;
                    backgroundModeBeatmap.IgnoreUserSettings.Value = true;
                    backgroundModeBeatmap.DimWhenUserSettingsIgnored.Value = 0.1f;
                    backgroundModeBeatmap.FadeColour(Color4.White, 250);
                });
            }
        });

        #endregion

        #region Transitions

        public override void OnEntering(ScreenTransitionEvent e)
        {
            base.OnEntering(e);

            this.FadeIn();

            titleWedge.Show();
            detailsArea.Show();
            filterControl.Show();

            modSelectOverlay.Beatmap.BindTo(Beatmap);
            modSelectOverlay.SelectedMods.BindTo(Mods);

            beginLooping();

            // force reselection if entering song select with a protected beatmap
            if (Beatmap.Value.BeatmapInfo.BeatmapSet!.Protected)
            {
                if (!carousel.NextRandom())
                    Beatmap.SetDefault();
            }
            else
                updateSelection();
        }

        public override void OnResuming(ScreenTransitionEvent e)
        {
            base.OnResuming(e);

            this.FadeIn(fade_duration, Easing.OutQuint);

            carousel.VisuallyFocusSelected = false;

            titleWedge.Show();
            detailsArea.Show();
            filterControl.Show();

            modSelectOverlay.Beatmap.BindTo(Beatmap);

            // required due to https://github.com/ppy/osu-framework/issues/3218
            modSelectOverlay.SelectedMods.Disabled = false;
            modSelectOverlay.SelectedMods.BindTo(Mods);

            beginLooping();

            if (Beatmap.Value.BeatmapInfo.BeatmapSet!.Protected)
                Beatmap.SetDefault();
            else
                updateSelection();
        }

        public override void OnSuspending(ScreenTransitionEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);

            modSelectOverlay.SelectedMods.UnbindFrom(Mods);
            modSelectOverlay.Beatmap.UnbindFrom(Beatmap);

            titleWedge.Hide();
            detailsArea.Hide();
            filterControl.Hide();

            carousel.VisuallyFocusSelected = true;

            endLooping();

            base.OnSuspending(e);
        }

        public override bool OnExiting(ScreenExitEvent e)
        {
            this.FadeOut(fade_duration, Easing.OutQuint);

            titleWedge.Hide();
            detailsArea.Hide();
            filterControl.Hide();

            endLooping();

            return base.OnExiting(e);
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
                OnStart();
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

        #endregion

        #region Filtering

        private const double filter_delay = 250;

        private ScheduledDelegate? filterDebounce;

        /// <summary>
        /// Set the query to the search text box.
        /// </summary>
        /// <param name="query">The string to search.</param>
        public void Search(string query) => filterControl.Search(query);

        private void criteriaChanged(FilterCriteria criteria)
        {
            // The first filter needs to be applied immediately as this triggers the initial carousel load.
            double filterDelay = filterDebounce == null ? 0 : filter_delay;

            filterDebounce?.Cancel();
            filterDebounce = Scheduler.AddDelayed(() => { carousel.Filter(criteria); }, filterDelay);
        }

        private void newItemsPresented(IEnumerable<CarouselItem> carouselItems)
        {
            if (carousel.Criteria == null)
                return;

            int count = carousel.MatchedBeatmapsCount;

            if (count == 0)
            {
                noResultsPlaceholder.Show();
                noResultsPlaceholder.Filter = carousel.Criteria;
            }
            else
                noResultsPlaceholder.Hide();

            // Intentionally not localised until we have proper support for this (see https://github.com/ppy/osu-framework/pull/4918
            // but also in this case we want support for formatting a number within a string).
            filterControl.StatusText = count != 1 ? $"{count:#,0} matches" : $"{count:#,0} match";

            // Refetch to be confident that the current selection is still valid. It may have been deleted or hidden.
            var currentBeatmap = beatmaps.GetWorkingBeatmap(Beatmap.Value.BeatmapInfo, true);
            bool currentBeatmapNotValid = currentBeatmap.BeatmapInfo.Hidden || currentBeatmap.BeatmapSetInfo?.DeletePending == true;

            // If all results are filtered away don't deselect the current global beatmap selection...
            if (!carouselItems.Any())
            {
                // ...unless it has been deleted or hidden
                if (currentBeatmapNotValid)
                    Beatmap.SetDefault();
                return;
            }

            if (Beatmap.IsDefault || currentBeatmapNotValid)
                carousel.NextRandom();
        }

        #endregion

        #region Hotkeys

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (!this.IsCurrentScreen()) return false;

            if (game == null)
                return false;

            var flattenedMods = ModUtils.FlattenMods(game.AvailableMods.Value.SelectMany(kv => kv.Value));

            switch (e.Action)
            {
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

        /// <summary>
        /// Opens results screen with the given score.
        /// This assumes active beatmap and ruleset selection matches the score.
        /// </summary>
        public void PresentScore(ScoreInfo score)
        {
            Debug.Assert(Beatmap.Value.BeatmapInfo.Equals(score.BeatmapInfo));
            Debug.Assert(Ruleset.Value.Equals(score.Ruleset));

            this.Push(new SoloResultsScreen(score));
        }

        /// <summary>
        /// Finalises selection on the given <see cref="BeatmapInfo"/>.
        /// </summary>
        public void SelectAndStart(BeatmapInfo beatmap)
        {
            Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmap);
            OnStart();
        }

        #region Beatmap management

        [Resolved]
        private ManageCollectionsDialog? manageCollectionsDialog { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        public virtual IEnumerable<OsuMenuItem> GetForwardActions(BeatmapInfo beatmap)
        {
            yield return new OsuMenuItem("Select", MenuItemType.Highlighted, () => SelectAndStart(beatmap))
            {
                Icon = FontAwesome.Solid.Check
            };

            yield return new OsuMenuItemSpacer();

            if (beatmap.OnlineID > 0)
            {
                yield return new OsuMenuItem("Details...", MenuItemType.Standard, () => beatmapOverlay?.FetchAndShowBeatmap(beatmap.OnlineID));

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

            collectionItems.Add(new OsuMenuItem("Manage...", MenuItemType.Standard, () => manageCollectionsDialog?.Show()));

            yield return new OsuMenuItem("Collections") { Items = collectionItems };
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
