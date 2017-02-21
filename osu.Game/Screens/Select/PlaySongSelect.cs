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
using osu.Framework.GameModes;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Primitives;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using osu.Game.Screens.Play;
using osu.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Transformations;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics;
using osu.Framework.Input;
using OpenTK.Input;
using System.Collections.Generic;
using osu.Framework.Graphics.Containers;
using osu.Framework.Threading;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase database;
        protected override BackgroundMode CreateBackground() => new BackgroundModeBeatmap(Beatmap);

        private CarouselContainer carousel;
        private TrackManager trackManager;

        private static readonly Vector2 wedged_container_size = new Vector2(0.5f, 225);
        private BeatmapInfoWedge beatmapInfoWedge;

        private static readonly Vector2 background_blur = new Vector2(20);
        private CancellationTokenSource initialAddSetsTask;

        private AudioSample sampleChangeDifficulty;
        private AudioSample sampleChangeBeatmap;

        private List<BeatmapGroup> beatmapGroups;

        private Footer footer;

        Player player;
        FilterControl filter;

        private void start()
        {
            if (player != null || Beatmap == null)
                return;

            //in the future we may want to move this logic to a PlayerLoader gamemode or similar, so we can rely on the SongSelect transition
            //and provide a better loading experience (at the moment song select is still accepting input during preload).
            player = new Player
            {
                BeatmapInfo = carousel.SelectedGroup.SelectedPanel.Beatmap,
                PreferredPlayMode = playMode.Value
            };

            player.Preload(Game, delegate
            {
                if (!Push(player))
                {
                    player = null;
                    //error occured?
                }
            });
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapDatabase beatmaps, AudioManager audio, BaseGame game,
            OsuGame osuGame, OsuColour colours)
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
                    FilterChanged = filterChanged,
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
                footer = new Footer()
                {
                    OnBack = Exit,
                    OnStart = start,
                }
            };

            footer.AddButton(@"mods", colours.Yellow, null);
            footer.AddButton(@"random", colours.Green, carousel.SelectRandom);
            footer.AddButton(@"options", colours.Blue, null);

            if (osuGame != null)
            {
                playMode = osuGame.PlayMode;
                playMode.ValueChanged += playMode_ValueChanged;
            }

            if (database == null)
                database = beatmaps;

            database.BeatmapSetAdded += onDatabaseOnBeatmapSetAdded;

            trackManager = audio.Track;

            sampleChangeDifficulty = audio.Sample.Get(@"SongSelect/select-difficulty");
            sampleChangeBeatmap = audio.Sample.Get(@"SongSelect/select-expand");

            initialAddSetsTask = new CancellationTokenSource();

            Task.Factory.StartNew(() => addBeatmapSets(game, initialAddSetsTask.Token), initialAddSetsTask.Token);
        }

        private ScheduledDelegate filterTask;

        private void filterChanged()
        {
            filterTask?.Cancel();
            filterTask = Scheduler.AddDelayed(() =>
            {
                filterTask = null;
                var search = filter.Search;
                BeatmapGroup newSelection = null;
                foreach (var beatmapGroup in carousel)
                {
                    var set = beatmapGroup.BeatmapSet;
                    bool match = string.IsNullOrEmpty(search)
                        || (set.Metadata.Artist ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (set.Metadata.ArtistUnicode ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (set.Metadata.Title ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1
                        || (set.Metadata.TitleUnicode ?? "").IndexOf(search, StringComparison.InvariantCultureIgnoreCase) != -1;
                    if (match)
                    {
                        beatmapGroup.State = BeatmapGroupState.Collapsed;
                        if (newSelection == null || beatmapGroup.BeatmapSet.OnlineBeatmapSetID == Beatmap.BeatmapSetInfo.OnlineBeatmapSetID)
                            newSelection = beatmapGroup;
                    }
                    else
                    {
                        beatmapGroup.State = BeatmapGroupState.Hidden;
                    }
                }
                if (newSelection != null)
                    carousel.SelectBeatmap(newSelection.BeatmapSet.Beatmaps[0], false);
            }, 250);
        }

        private void onDatabaseOnBeatmapSetAdded(BeatmapSetInfo s)
        {
            Schedule(() => addBeatmapSet(s, Game, true));
        }

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            ensurePlayingSelected();

            changeBackground(Beatmap);

            Content.FadeInFromZero(250);

            beatmapInfoWedge.MoveToX(-50);
            beatmapInfoWedge.MoveToX(0, 800, EasingTypes.OutQuint);

            filter.Activate();
        }

        protected override void OnResuming(GameMode last)
        {
            player = null;

            changeBackground(Beatmap);
            ensurePlayingSelected();
            base.OnResuming(last);

            Content.FadeIn(250);

            Content.ScaleTo(1, 250, EasingTypes.OutSine);

            filter.Activate();
        }

        protected override void OnSuspending(GameMode next)
        {
            Content.ScaleTo(1.1f, 250, EasingTypes.InSine);

            Content.FadeOut(250);

            filter.Deactivate();
            base.OnSuspending(next);
        }

        protected override bool OnExiting(GameMode next)
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
            if (playMode != null)
                playMode.ValueChanged -= playMode_ValueChanged;

            database.BeatmapSetAdded -= onDatabaseOnBeatmapSetAdded;

            initialAddSetsTask.Cancel();
        }

        private void playMode_ValueChanged(object sender, EventArgs e)
        {
        }

        private void changeBackground(WorkingBeatmap beatmap)
        {
            var backgroundModeBeatmap = Background as BackgroundModeBeatmap;
            if (backgroundModeBeatmap != null)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                backgroundModeBeatmap.BlurTo(background_blur, 1000);
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
            carousel.SelectBeatmap(beatmap.BeatmapInfo);
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
            AudioTrack track = Beatmap?.Track;

            if (track != null)
            {
                trackManager.SetExclusive(track);
                if (preview)
                    track.Seek(Beatmap.Beatmap.Metadata.PreviewTime);
                track.Start();
            }
        }

        private void addBeatmapSet(BeatmapSetInfo beatmapSet, BaseGame game, bool select = false)
        {
            beatmapSet = database.GetWithChildren<BeatmapSetInfo>(beatmapSet.ID);
            beatmapSet.Beatmaps.ForEach(b =>
            {
                database.GetChildren(b);
                if (b.Metadata == null) b.Metadata = beatmapSet.Metadata;
            });

            beatmapSet.Beatmaps = beatmapSet.Beatmaps.OrderBy(b => b.StarDifficulty).ToList();

            var beatmap = new WorkingBeatmap(beatmapSet.Beatmaps.FirstOrDefault(), beatmapSet, database);

            var group = new BeatmapGroup(beatmap, beatmapSet)
            {
                SelectionChanged = selectionChanged,
                StartRequested = b => start()
            };

            //for the time being, let's completely load the difficulty panels in the background.
            //this likely won't scale so well, but allows us to completely async the loading flow.
            Task.WhenAll(group.BeatmapPanels.Select(panel => panel.Preload(game))).ContinueWith(task => Schedule(delegate
            {
                beatmapGroups.Add(group);

                carousel.AddGroup(group);

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

        private void addBeatmapSets(BaseGame game, CancellationToken token)
        {
            foreach (var beatmapSet in database.Query<BeatmapSetInfo>())
            {
                if (token.IsCancellationRequested) return;
                addBeatmapSet(beatmapSet, game);
            }
        }

        protected override bool OnKeyDown(InputState state, KeyDownEventArgs args)
        {
            switch (args.Key)
            {
                case Key.Enter:
                    start();
                    return true;
            }

            return base.OnKeyDown(state, args);
        }
    }
}
