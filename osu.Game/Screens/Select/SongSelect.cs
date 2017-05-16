// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Threading;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Overlays;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Select.Options;

namespace osu.Game.Screens.Select
{
    public abstract class SongSelect : OsuScreen
    {
        private readonly Bindable<RulesetInfo> ruleset = new Bindable<RulesetInfo>();
        private BeatmapDatabase database;
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        private readonly BeatmapCarousel carousel;
        private TrackManager trackManager;
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
                SelectionChanged = selectionChanged,
                StartRequested = raiseSelect
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
                X = -50,
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
                    OnStart = raiseSelect,
                });

                FooterPanels.Add(BeatmapOptions = new BeatmapOptionsOverlay());
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapDatabase beatmaps, AudioManager audio, DialogOverlay dialog, OsuGame osu, OsuColour colours)
        {
            if (Footer != null)
            {
                Footer.AddButton(@"random", colours.Green, SelectRandom, Key.F2);
                Footer.AddButton(@"options", colours.Blue, BeatmapOptions.ToggleVisibility, Key.F3);

                BeatmapOptions.AddButton(@"Delete", @"Beatmap", FontAwesome.fa_trash, colours.Pink, promptDelete, Key.Number4, float.MaxValue);
            }

            if (database == null)
                database = beatmaps;

            if (osu != null)
                ruleset.BindTo(osu.Ruleset);

            database.BeatmapSetAdded += onBeatmapSetAdded;
            database.BeatmapSetRemoved += onBeatmapSetRemoved;

            trackManager = audio.Track;
            dialogOverlay = dialog;

            sampleChangeDifficulty = audio.Sample.Get(@"SongSelect/select-difficulty");
            sampleChangeBeatmap = audio.Sample.Get(@"SongSelect/select-expand");

            initialAddSetsTask = new CancellationTokenSource();

            carousel.BeatmapsChanged = beatmapsLoaded;
            carousel.Beatmaps = database.GetAllWithChildren<BeatmapSetInfo>(b => !b.DeletePending);
        }

        private void beatmapsLoaded()
        {
            if (Beatmap != null)
                carousel.SelectBeatmap(Beatmap.BeatmapInfo, false);
            else
                carousel.SelectNext();
        }

        private void raiseSelect()
        {
            var pendingSelection = selectionChangedDebounce;
            selectionChangedDebounce = null;

            pendingSelection?.RunTask();
            pendingSelection?.Cancel(); // cancel the already scheduled task.

            if (Beatmap == null) return;

            OnSelected();
        }

        public void SelectRandom() => carousel.SelectRandom();

        protected abstract void OnSelected();

        private void filterChanged(FilterCriteria criteria, bool debounce = true)
        {
            carousel.Filter(criteria, debounce);
        }

        private void onBeatmapSetAdded(BeatmapSetInfo s) => carousel.AddBeatmap(s);

        private void onBeatmapSetRemoved(BeatmapSetInfo s) => Schedule(() => removeBeatmapSet(s));

        protected override void OnEntering(Screen last)
        {
            base.OnEntering(last);
            ensurePlayingSelected();

            changeBackground(Beatmap);

            Content.FadeInFromZero(250);

            beatmapInfoWedge.State = Visibility.Visible;

            FilterControl.Activate();
        }

        protected override void OnResuming(Screen last)
        {
            changeBackground(Beatmap);
            ensurePlayingSelected();
            base.OnResuming(last);

            Content.FadeIn(250);

            Content.ScaleTo(1, 250, EasingTypes.OutSine);

            FilterControl.Activate();
        }

        protected override void OnSuspending(Screen next)
        {
            Content.ScaleTo(1.1f, 250, EasingTypes.InSine);

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

            database.BeatmapSetAdded -= onBeatmapSetAdded;
            database.BeatmapSetRemoved -= onBeatmapSetRemoved;

            initialAddSetsTask.Cancel();
        }

        private void changeBackground(WorkingBeatmap beatmap)
        {
            var backgroundModeBeatmap = Background as BackgroundScreenBeatmap;
            if (backgroundModeBeatmap != null)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurTo(background_blur, 1000);
                backgroundModeBeatmap.FadeTo(1, 250);
            }

            beatmapInfoWedge.UpdateBeatmap(beatmap);
        }

        /// <summary>
        /// The global Beatmap was changed.
        /// </summary>
        protected override void OnBeatmapChanged(WorkingBeatmap beatmap)
        {
            base.OnBeatmapChanged(beatmap);

            //todo: change background in selectionChanged instead; support per-difficulty backgrounds.
            changeBackground(beatmap);
            carousel.SelectBeatmap(beatmap?.BeatmapInfo);
        }

        private ScheduledDelegate selectionChangedDebounce;

        // We need to keep track of the last selected beatmap ignoring debounce to play the correct selection sounds.
        private BeatmapInfo selectionChangeNoBounce;

        /// <summary>
        /// selection has been changed as the result of interaction with the carousel.
        /// </summary>
        private void selectionChanged(BeatmapInfo beatmap)
        {
            bool beatmapSetChange = false;

            if (!beatmap.Equals(Beatmap?.BeatmapInfo))
            {
                if (beatmap.BeatmapSetInfoID == selectionChangeNoBounce?.BeatmapSetInfoID)
                    sampleChangeDifficulty.Play();
                else
                {
                    sampleChangeBeatmap.Play();
                    beatmapSetChange = true;
                }
            }

            selectionChangeNoBounce = beatmap;

            selectionChangedDebounce?.Cancel();
            selectionChangedDebounce = Scheduler.AddDelayed(delegate
            {
                Beatmap = database.GetWorkingBeatmap(beatmap, Beatmap);
                ensurePlayingSelected(beatmapSetChange);
            }, 100);
        }

        private void ensurePlayingSelected(bool preview = false)
        {
            Track track = Beatmap?.Track;

            if (track != null)
            {
                trackManager.SetExclusive(track);
                if (preview)
                    track.Seek(Beatmap.Metadata.PreviewTime);
                track.Start();
            }
        }

        private void removeBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            carousel.RemoveBeatmap(beatmapSet);
            if (carousel.SelectedBeatmap == null)
                Beatmap = null;
        }

        private void promptDelete()
        {
            if (Beatmap != null)
                dialogOverlay?.Push(new BeatmapDeleteDialog(Beatmap));
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            if (args.Repeat) return false;

            switch (args.Key)
            {
                case Key.Enter:
                    raiseSelect();
                    return true;
                case Key.Delete:
                    if (state.Keyboard.ShiftPressed)
                    {
                        promptDelete();
                        return true;
                    }
                    break;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
