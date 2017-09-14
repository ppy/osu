// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Threading;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Select.Options;

namespace osu.Game.Screens.Select
{
    public abstract class SongSelect : OsuScreen
    {
        private BeatmapManager manager;
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap();

        private readonly BeatmapCarousel carousel;
        private DialogOverlay dialogOverlay;

        private static readonly Vector2 wedged_container_size = new Vector2(0.5f, 245);

        private const float left_area_padding = 20;

        private readonly BeatmapInfoWedge beatmapInfoWedge;

        protected Container LeftContent;

        private static readonly Vector2 background_blur = new Vector2(20);
        private CancellationTokenSource initialAddSetsTask;

        private SampleChannel sampleChangeDifficulty;
        private SampleChannel sampleChangeBeatmap;

        protected virtual bool ShowFooter => true;

        /// <summary>
        /// Can be null if <see cref="ShowFooter"/> is false.
        /// </summary>
        protected readonly BeatmapOptionsOverlay BeatmapOptions;

        /// <summary>
        /// Can be null if <see cref="ShowFooter"/> is false.
        /// </summary>
        protected readonly Footer Footer;

        /// <summary>
        /// Contains any panel which is triggered by a footer button.
        /// Helps keep them located beneath the footer itself.
        /// </summary>
        protected readonly Container FooterPanels;

        public readonly FilterControl FilterControl;

        protected SongSelect()
        {
            const float carousel_width = 640;
            const float filter_height = 100;

            Add(new ParallaxContainer
            {
                Padding = new MarginPadding { Top = filter_height },
                ParallaxAmount = 0.005f,
                RelativeSizeAxes = Axes.Both,
                Children = new[]
                {
                    new WedgeBackground
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Right = carousel_width * 0.76f },
                    }
                }
            });
            Add(LeftContent = new Container
            {
                Origin = Anchor.BottomLeft,
                Anchor = Anchor.BottomLeft,
                RelativeSizeAxes = Axes.Both,
                Size = new Vector2(wedged_container_size.X, 1),
                Padding = new MarginPadding
                {
                    Bottom = 50,
                    Top = wedged_container_size.Y + left_area_padding,
                    Left = left_area_padding,
                    Right = left_area_padding * 2,
                }
            });
            Add(carousel = new BeatmapCarousel
            {
                RelativeSizeAxes = Axes.Y,
                Size = new Vector2(carousel_width, 1),
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
                SelectionChanged = carouselSelectionChanged,
                BeatmapsChanged = carouselBeatmapsLoaded,
                DeleteRequested = promptDelete,
                RestoreRequested = s => { foreach (var b in s.Beatmaps) manager.Restore(b); },
                HideDifficultyRequested = b => manager.Hide(b),
                StartRequested = () => carouselRaisedStart(),
            });
            Add(FilterControl = new FilterControl
            {
                RelativeSizeAxes = Axes.X,
                Height = filter_height,
                FilterChanged = criteria => filterChanged(criteria),
                Exit = Exit,
            });
            Add(beatmapInfoWedge = new BeatmapInfoWedge
            {
                Alpha = 0,
                Size = wedged_container_size,
                RelativeSizeAxes = Axes.X,
                Margin = new MarginPadding
                {
                    Top = left_area_padding,
                    Right = left_area_padding,
                },
            });
            Add(new ResetScrollContainer(() => carousel.ScrollToSelected())
            {
                RelativeSizeAxes = Axes.Y,
                Width = 250,
            });

            if (ShowFooter)
            {
                Add(FooterPanels = new Container
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Margin = new MarginPadding
                    {
                        Bottom = Footer.HEIGHT,
                    },
                });
                Add(Footer = new Footer
                {
                    OnBack = Exit,
                    OnStart = () => carouselRaisedStart(),
                });

                FooterPanels.Add(BeatmapOptions = new BeatmapOptionsOverlay());
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapManager beatmaps, AudioManager audio, DialogOverlay dialog, OsuGame osu, OsuColour colours)
        {
            if (Footer != null)
            {
                Footer.AddButton(@"random", colours.Green, triggerRandom, Key.F2);
                Footer.AddButton(@"options", colours.Blue, BeatmapOptions.ToggleVisibility, Key.F3);

                BeatmapOptions.AddButton(@"Delete", @"Beatmap", FontAwesome.fa_trash, colours.Pink, () => promptDelete(Beatmap.Value.BeatmapSetInfo), Key.Number4, float.MaxValue);
            }

            if (manager == null)
                manager = beatmaps;

            if (osu != null)
                Ruleset.BindTo(osu.Ruleset);

            manager.BeatmapSetAdded += onBeatmapSetAdded;
            manager.BeatmapSetRemoved += onBeatmapSetRemoved;
            manager.BeatmapHidden += onBeatmapHidden;
            manager.BeatmapRestored += onBeatmapRestored;

            dialogOverlay = dialog;

            sampleChangeDifficulty = audio.Sample.Get(@"SongSelect/select-difficulty");
            sampleChangeBeatmap = audio.Sample.Get(@"SongSelect/select-expand");

            initialAddSetsTask = new CancellationTokenSource();

            carousel.Beatmaps = manager.GetAllUsableBeatmapSets();

            Beatmap.ValueChanged += beatmap_ValueChanged;

            Beatmap.DisabledChanged += disabled => carousel.AllowSelection = !disabled;
            carousel.AllowSelection = !Beatmap.Disabled;
        }

        private void onBeatmapRestored(BeatmapInfo b) => carousel.UpdateBeatmap(b);
        private void onBeatmapHidden(BeatmapInfo b) => carousel.UpdateBeatmap(b);

        private void carouselBeatmapsLoaded()
        {
            if (Beatmap.Value.BeatmapSetInfo?.DeletePending == false)
                carousel.SelectBeatmap(Beatmap.Value.BeatmapInfo, false);
            else
                carousel.SelectNext();
        }

        private void carouselRaisedStart(InputState state = null)
        {
            // if we have a pending filter operation, we want to run it now.
            // it could change selection (ie. if the ruleset has been changed).
            carousel.FlushPendingFilters();

            if (selectionChangedDebounce?.Completed == false)
            {
                selectionChangedDebounce.RunTask();
                selectionChangedDebounce.Cancel(); // cancel the already scheduled task.
                selectionChangedDebounce = null;
            }

            OnSelected(state);
        }

        private ScheduledDelegate selectionChangedDebounce;

        // We need to keep track of the last selected beatmap ignoring debounce to play the correct selection sounds.
        private BeatmapInfo beatmapNoDebounce;

        /// <summary>
        /// selection has been changed as the result of interaction with the carousel.
        /// </summary>
        private void carouselSelectionChanged(BeatmapInfo beatmap)
        {
            Action performLoad = delegate
            {
                // We may be arriving here due to another component changing the bindable Beatmap.
                // In these cases, the other component has already loaded the beatmap, so we don't need to do so again.
                if (beatmap?.Equals(Beatmap.Value.BeatmapInfo) != true)
                {
                    bool preview = beatmap?.BeatmapSetInfoID != Beatmap.Value.BeatmapInfo.BeatmapSetInfoID;

                    Beatmap.Value = manager.GetWorkingBeatmap(beatmap, Beatmap);
                    ensurePlayingSelected(preview);
                }

                UpdateBeatmap(Beatmap.Value);
            };

            selectionChangedDebounce?.Cancel();

            if (beatmap?.Equals(beatmapNoDebounce) == true)
                return;

            beatmapNoDebounce = beatmap;

            if (beatmap == null)
            {
                if (!Beatmap.IsDefault)
                    performLoad();
            }
            else
            {
                if (beatmap.BeatmapSetInfoID == beatmapNoDebounce?.BeatmapSetInfoID)
                    sampleChangeDifficulty.Play();
                else
                    sampleChangeBeatmap.Play();

                if (beatmap == Beatmap.Value.BeatmapInfo)
                    performLoad();
                else
                    selectionChangedDebounce = Scheduler.AddDelayed(performLoad, 100);
            }
        }

        private void triggerRandom()
        {
            if (GetContainingInputManager().CurrentState.Keyboard.ShiftPressed)
                carousel.SelectPreviousRandom();
            else
                carousel.SelectNextRandom();
        }

        protected abstract void OnSelected(InputState state);

        private void filterChanged(FilterCriteria criteria, bool debounce = true)
        {
            carousel.Filter(criteria, debounce);
        }

        private void onBeatmapSetAdded(BeatmapSetInfo s) => Schedule(() => addBeatmapSet(s));

        private void onBeatmapSetRemoved(BeatmapSetInfo s) => Schedule(() => removeBeatmapSet(s));

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);

            Content.FadeInFromZero(250);

            FilterControl.Activate();
        }

        private void beatmap_ValueChanged(WorkingBeatmap beatmap)
        {
            if (!IsCurrentScreen) return;

            carousel.SelectBeatmap(beatmap?.BeatmapInfo);
        }

        protected override void OnResuming(Screen last)
        {
            if (Beatmap != null && !Beatmap.Value.BeatmapSetInfo.DeletePending)
            {
                UpdateBeatmap(Beatmap);
                ensurePlayingSelected();
            }

            base.OnResuming(last);

            Content.FadeIn(250);

            Content.ScaleTo(1, 250, Easing.OutSine);

            FilterControl.Activate();
        }

        protected override void OnSuspending(Screen next)
        {
            Content.ScaleTo(1.1f, 250, Easing.InSine);

            Content.FadeOut(250);

            FilterControl.Deactivate();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            beatmapInfoWedge.State = Visibility.Hidden;

            Content.FadeOut(100);

            FilterControl.Deactivate();
            return base.OnExiting(next);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (manager != null)
            {
                manager.BeatmapSetAdded -= onBeatmapSetAdded;
                manager.BeatmapSetRemoved -= onBeatmapSetRemoved;
                manager.BeatmapHidden -= onBeatmapHidden;
                manager.BeatmapRestored -= onBeatmapRestored;
            }

            initialAddSetsTask?.Cancel();
        }

        /// <summary>
        /// Allow components in SongSelect to update their loaded beatmap details.
        /// This is a debounced call (unlike directly binding to WorkingBeatmap.ValueChanged).
        /// </summary>
        /// <param name="beatmap">The working beatmap.</param>
        protected virtual void UpdateBeatmap(WorkingBeatmap beatmap)
        {
            var backgroundModeBeatmap = Background as BackgroundScreenBeatmap;
            if (backgroundModeBeatmap != null)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurTo(background_blur, 1000);
                backgroundModeBeatmap.FadeTo(1, 250);
            }

            beatmapInfoWedge.State = Visibility.Visible;
            beatmapInfoWedge.UpdateBeatmap(beatmap);
        }

        private void ensurePlayingSelected(bool preview = false)
        {
            Track track = Beatmap.Value.Track;

            if (!track.IsRunning)
            {
                // Ensure the track is added to the TrackManager, since it is removed after the player finishes the map.
                // Using AddItemToList rather than AddItem so that it doesn't attempt to register adjustment dependencies more than once.
                Game.Audio.Track.AddItemToList(track);
                if (preview) track.Seek(Beatmap.Value.Metadata.PreviewTime);
                track.Start();
            }
        }

        private void addBeatmapSet(BeatmapSetInfo beatmapSet) => carousel.AddBeatmap(beatmapSet);

        private void removeBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            carousel.RemoveBeatmap(beatmapSet);
            if (carousel.SelectedBeatmap == null)
                Beatmap.SetDefault();
        }

        private void promptDelete(BeatmapSetInfo beatmap)
        {
            if (beatmap == null)
                return;

            dialogOverlay?.Push(new BeatmapDeleteDialog(beatmap));
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.KeypadEnter:
                case Key.Enter:
                    carouselRaisedStart(state);
                    return true;
                case Key.Delete:
                    if (state.Keyboard.ShiftPressed)
                    {
                        if (!Beatmap.IsDefault)
                            promptDelete(Beatmap.Value.BeatmapSetInfo);
                        return true;
                    }
                    break;
            }

            return base.OnKeyDown(state, args);
        }

        private class ResetScrollContainer : Container
        {
            private readonly Action onHoverAction;

            public ResetScrollContainer(Action onHoverAction)
            {
                this.onHoverAction = onHoverAction;
            }

            protected override bool OnHover(InputState state)
            {
                onHoverAction?.Invoke();
                return base.OnHover(state);
            }
        }
    }
}
