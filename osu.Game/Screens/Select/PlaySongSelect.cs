// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Screens;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using osu.Game.Screens.Play;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Transforms;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics;
using osu.Framework.Input;
using OpenTK.Input;
using System.Collections.Generic;
using osu.Framework.Threading;
using osu.Game.Overlays;
using osu.Game.Screens.Select.Options;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : OsuScreen
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

        private BeatmapOptionsOverlay beatmapOptions;
        private Footer footer;

        OsuScreen player;

        private FilterControl filter;
        public FilterControl Filter
        {
            get
            {
                return filter;
            }
            private set
            {
                if (filter != value)
                {
                    filter = value;
                    filterChanged();
                }
            }
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapDatabase beatmaps, AudioManager audio, DialogOverlay dialog, Framework.Game game,
            OsuGame osu, OsuColour colours)
        {
            const float carousel_width = 640;
            const float filter_height = 100;

            beatmapGroups = new List<BeatmapGroup>();
            Children = new Drawable[]
            {
                new ParallaxContainer
                {
                    Padding = new MarginPadding { Top = filter_height },
                    ParallaxAmount = 0.005f,
                    RelativeSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        new WedgeBackground
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding
                            {
                                Right = carousel_width * 0.76f
                            },
                        },
                    }
                },
                carousel = new CarouselContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(carousel_width, 1),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                },
                filter = new FilterControl
                {
                    RelativeSizeAxes = Axes.X,
                    Height = filter_height,
                    FilterChanged = () => filterChanged(),
                    Exit = Exit,
                },
                beatmapInfoWedge = new BeatmapInfoWedge
                {
                    Alpha = 0,
                    Size = wedged_container_size,
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding
                    {
                        Top = 20,
                        Right = 20,
                    },
                },
                beatmapOptions = new BeatmapOptionsOverlay
                {
                    OnRemoveFromUnplayed = null,
                    OnClearLocalScores = null,
                    OnEdit = null,
                    OnDelete = promptDelete,
                    Margin = new MarginPadding
                    {
                        Bottom = 50,
                    },
                },
                footer = new Footer
                {
                    OnBack = Exit,
                    OnStart = () =>
                    {
                        if (player != null || Beatmap == null)
                            return;

                        (player = new PlayerLoader(new Player
                        {
                            BeatmapInfo = carousel.SelectedGroup.SelectedPanel.Beatmap,
                            PreferredPlayMode = playMode.Value
                        })).LoadAsync(Game, l => Push(player));
                    }
                },
            };

            footer.AddButton(@"mods", colours.Yellow, null);
            footer.AddButton(@"random", colours.Green, carousel.SelectRandom);
            footer.AddButton(@"options", colours.Blue, beatmapOptions.ToggleVisibility);

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

        private ScheduledDelegate filterTask;

        private void filterChanged(bool debounce = true, bool eagerSelection = true)
        {
            filterTask?.Cancel();
            filterTask = Scheduler.AddDelayed(() =>
            {
                filterTask = null;
                var search = filter.Search;
                BeatmapGroup newSelection = null;
                carousel.Sort(filter.Sort);
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

            beatmapInfoWedge.MoveToX(-50);
            beatmapInfoWedge.MoveToX(0, 800, EasingTypes.OutQuint);

            filter.Activate();
        }

        protected override void OnResuming(Screen last)
        {
            player = null;

            changeBackground(Beatmap);
            ensurePlayingSelected();
            base.OnResuming(last);

            Content.FadeIn(250);

            Content.ScaleTo(1, 250, EasingTypes.OutSine);

            filter.Activate();
        }

        protected override void OnSuspending(Screen next)
        {
            Content.ScaleTo(1.1f, 250, EasingTypes.InSine);

            Content.FadeOut(250);

            filter.Deactivate();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(Screen next)
        {
            beatmapInfoWedge.MoveToX(-100, 800, EasingTypes.InQuint);
            beatmapInfoWedge.RotateTo(10, 800, EasingTypes.InQuint);

            Content.FadeOut(100);

            filter.Deactivate();
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

            if (beatmap != null)
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
                StartRequested = b => footer.StartButton.TriggerClick()
            };

            //for the time being, let's completely load the difficulty panels in the background.
            //this likely won't scale so well, but allows us to completely async the loading flow.
            Task.WhenAll(group.BeatmapPanels.Select(panel => panel.LoadAsync(game))).ContinueWith(task => Schedule(delegate
            {
                beatmapGroups.Add(group);

                carousel.AddGroup(group);

                filterChanged(false, false);

                if (Beatmap == null || select)
                    carousel.SelectBeatmap(beatmapSet.Beatmaps.First());
                else
                {
                    var panel = group.BeatmapPanels.FirstOrDefault(p => p.Beatmap.Equals(Beatmap.BeatmapInfo));
                    if (panel != null)
                        carousel.SelectGroup(group, panel);
                }
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
            switch (args.Key)
            {
                case Key.Enter:
                    footer.StartButton.TriggerClick();
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
