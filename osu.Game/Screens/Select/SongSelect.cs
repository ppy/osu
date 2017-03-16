// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Transforms;
using osu.Framework.Input;
using osu.Framework.Screens;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Modes;
using osu.Game.Overlays;
using osu.Game.Screens.Backgrounds;
using osu.Game.Screens.Select.Options;

namespace osu.Game.Screens.Select
{
    public abstract class SongSelect : OsuScreen
    {
        private Bindable<PlayMode> playMode = new Bindable<PlayMode>();
        private BeatmapDatabase database;
        protected override BackgroundScreen CreateBackground() => new BackgroundScreenBeatmap(Beatmap);

        private CarouselContainer carousel;
        private TrackManager trackManager;
        private DialogOverlay dialogOverlay;

        private static readonly Vector2 wedged_container_size = new Vector2(0.5f, 225);
        private BeatmapInfoWedge beatmapInfoWedge;

        private static readonly Vector2 background_blur = new Vector2(20);
        private CancellationTokenSource initialAddSetsTask;

        private SampleChannel sampleChangeDifficulty;
        private SampleChannel sampleChangeBeatmap;

        private List<BeatmapGroup> beatmapGroups;

        protected virtual bool ShowFooter => true;

        /// <summary>
        /// Can be null if <see cref="ShowFooter"/> is false.
        /// </summary>
        protected readonly BeatmapOptionsOverlay BeatmapOptions;

        /// <summary>
        /// Can be null if <see cref="ShowFooter"/> is false.
        /// </summary>
        protected readonly Footer Footer;

        public readonly FilterControl FilterControl;

        protected SongSelect()
        {
            const float carousel_width = 640;
            const float filter_height = 100;

            beatmapGroups = new List<BeatmapGroup>();
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
            Add(carousel = new CarouselContainer
            {
                RelativeSizeAxes = Axes.Y,
                Size = new Vector2(carousel_width, 1),
                Anchor = Anchor.CentreRight,
                Origin = Anchor.CentreRight,
            });
            Add(FilterControl = new FilterControl
            {
                RelativeSizeAxes = Axes.X,
                Height = filter_height,
                FilterChanged = () => filterChanged(),
                Exit = Exit,
            });
            Add(beatmapInfoWedge = new BeatmapInfoWedge
            {
                Alpha = 0,
                Size = wedged_container_size,
                RelativeSizeAxes = Axes.X,
                Margin = new MarginPadding
                {
                    Top = 20,
                    Right = 20,
                },
                X = -50,
            });

            if (ShowFooter)
            {
                Add(BeatmapOptions = new BeatmapOptionsOverlay
                {
                    Margin = new MarginPadding
                    {
                        Bottom = 50,
                    },
                });
                Add(Footer = new Footer
                {
                    OnBack = Exit,
                    OnStart = raiseSelect,
                });
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapDatabase beatmaps, AudioManager audio, DialogOverlay dialog, Framework.Game game,
            OsuGame osu, OsuColour colours)
        {
            if (Footer != null)
            {
                Footer.AddButton(@"random", colours.Green, SelectRandom, Key.F2);
                Footer.AddButton(@"options", colours.Blue, BeatmapOptions.ToggleVisibility, Key.F3);

                BeatmapOptions.AddButton(@"Delete", @"Beatmap", FontAwesome.fa_trash, colours.Pink, promptDelete, Key.Number4, float.MaxValue);
            }

            if (osu != null)
                playMode.BindTo(osu.PlayMode);
            playMode.ValueChanged += playMode_ValueChanged;

            if (database == null)
                database = beatmaps;

            database.BeatmapSetAdded += onBeatmapSetAdded;
            database.BeatmapSetRemoved += onBeatmapSetRemoved;

            trackManager = audio.Track;
            dialogOverlay = dialog;

            sampleChangeDifficulty = audio.Sample.Get(@"SongSelect/select-difficulty");
            sampleChangeBeatmap = audio.Sample.Get(@"SongSelect/select-expand");

            initialAddSetsTask = new CancellationTokenSource();

            Task.Factory.StartNew(() => addBeatmapSets(game, initialAddSetsTask.Token), initialAddSetsTask.Token);
        }

        private void raiseSelect()
        {
            if (Beatmap == null) return;

            Beatmap.PreferredPlayMode = playMode.Value;
            OnSelected();
        }

        public void SelectRandom() => carousel.SelectRandom();
        protected abstract void OnSelected();

        private ScheduledDelegate filterTask;

        private void filterChanged(bool debounce = true, bool eagerSelection = true)
        {
            filterTask?.Cancel();
            filterTask = Scheduler.AddDelayed(() =>
            {
                filterTask = null;
                var search = FilterControl.Search;
                BeatmapGroup newSelection = null;
                carousel.Sort(FilterControl.Sort);
                foreach (var beatmapGroup in carousel)
                {
                    var set = beatmapGroup.BeatmapSet;

                    bool hasCurrentMode = set.Beatmaps.Any(bm => bm.Mode == playMode);

                    bool match = hasCurrentMode;

                    match &= string.IsNullOrEmpty(search)
                        || (set.Metadata.Artist ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (set.Metadata.ArtistUnicode ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (set.Metadata.Title ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (set.Metadata.TitleUnicode ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1;

                    if (match)
                    {
                        if (newSelection == null || beatmapGroup.BeatmapSet.OnlineBeatmapSetID == Beatmap.BeatmapSetInfo.OnlineBeatmapSetID)
                        {
                            if (newSelection != null)
                                newSelection.State = BeatmapGroupState.Collapsed;
                            newSelection = beatmapGroup;
                        }
                        else
                            beatmapGroup.State = BeatmapGroupState.Collapsed;
                    }
                    else
                    {
                        beatmapGroup.State = BeatmapGroupState.Hidden;
                    }
                }

                if (newSelection != null)
                {
                    if (newSelection.BeatmapPanels.Any(b => b.Beatmap.ID == Beatmap.BeatmapInfo.ID))
                        carousel.SelectBeatmap(Beatmap.BeatmapInfo, false);
                    else if (eagerSelection)
                        carousel.SelectBeatmap(newSelection.BeatmapSet.Beatmaps[0], false);
                }
            }, debounce ? 250 : 0);
        }

        private void onBeatmapSetAdded(BeatmapSetInfo s) => Schedule(() => addBeatmapSet(s, Game, true));

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

        private void playMode_ValueChanged(object sender, EventArgs e)
        {
            filterChanged(false);
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

        /// <summary>
        /// selection has been changed as the result of interaction with the carousel.
        /// </summary>
        private void selectionChanged(BeatmapGroup group, BeatmapInfo beatmap)
        {
            bool beatmapSetChange = false;

            if (!beatmap.Equals(Beatmap?.BeatmapInfo))
            {
                if (beatmap.BeatmapSetInfoID == Beatmap?.BeatmapInfo.BeatmapSetInfoID)
                    sampleChangeDifficulty.Play();
                else
                {
                    sampleChangeBeatmap.Play();
                    beatmapSetChange = true;
                }
                Beatmap = database.GetWorkingBeatmap(beatmap, Beatmap);
            }
            ensurePlayingSelected(beatmapSetChange);
        }

        private void ensurePlayingSelected(bool preview = false)
        {
            Track track = Beatmap?.Track;

            if (track != null)
            {
                trackManager.SetExclusive(track);
                if (preview)
                    track.Seek(Beatmap.Beatmap.Metadata.PreviewTime);
                track.Start();
            }
        }

        private void addBeatmapSet(BeatmapSetInfo beatmapSet, Framework.Game game, bool select = false)
        {
            beatmapSet = database.GetWithChildren<BeatmapSetInfo>(beatmapSet.ID);
            beatmapSet.Beatmaps.ForEach(b =>
            {
                database.GetChildren(b);
                if (b.Metadata == null) b.Metadata = beatmapSet.Metadata;
            });

            var group = new BeatmapGroup(beatmapSet, database)
            {
                SelectionChanged = selectionChanged,
                StartRequested = b => raiseSelect()
            };

            //for the time being, let's completely load the difficulty panels in the background.
            //this likely won't scale so well, but allows us to completely async the loading flow.
            Task.WhenAll(group.BeatmapPanels.Select(panel => panel.LoadAsync(game))).ContinueWith(task => Schedule(delegate
            {
                beatmapGroups.Add(group);

                group.State = BeatmapGroupState.Collapsed;
                carousel.AddGroup(group);

                filterChanged(false, false);

                if (Beatmap == null || select)
                    carousel.SelectBeatmap(beatmapSet.Beatmaps.First());
                else
                    carousel.SelectBeatmap(Beatmap.BeatmapInfo);
            }));
        }

        private void removeBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            var group = beatmapGroups.Find(b => b.BeatmapSet.ID == beatmapSet.ID);
            if (group == null) return;

            if (carousel.SelectedGroup == group)
                carousel.SelectNext();

            beatmapGroups.Remove(group);
            carousel.RemoveGroup(group);

            if (beatmapGroups.Count == 0)
                Beatmap = null;
        }

        private void addBeatmapSets(Framework.Game game, CancellationToken token)
        {
            foreach (var beatmapSet in database.Query<BeatmapSetInfo>().Where(b => !b.DeletePending))
            {
                if (token.IsCancellationRequested) return;
                addBeatmapSet(beatmapSet, game);
            }
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
