//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Screens.Play;
using osu.Framework;
using osu.Game.Beatmaps.Drawables;

namespace osu.Game.Screens.Select
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase database;
        protected override BackgroundMode CreateBackground() => new BackgroundModeBeatmap(Beatmap);

        private CarouselContainer carousel;
        private TrackManager trackManager;
        private Container backgroundWedgesContainer;

        private static readonly Vector2 wedged_container_size = new Vector2(0.5f, 225);
        private static readonly Vector2 wedged_container_shear = new Vector2(0.15f, 0);
        private static readonly Vector2 wedged_container_start_position = new Vector2(0, 50);
        private BeatmapInfoOverlay wedgedBeatmapInfoOverlay;

        private static readonly Vector2 BACKGROUND_BLUR = new Vector2(20);
        private CancellationTokenSource initialAddSetsTask;

        /// <param name="database">Optionally provide a database to use instead of the OsuGame one.</param>
        public PlaySongSelect(BeatmapDatabase database = null)
        {
            this.database = database;

            const float scrollWidth = 640;
            const float bottomToolHeight = 50;
            Children = new Drawable[]
            {
                backgroundWedgesContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                    Padding = new MarginPadding { Right = scrollWidth },
                    Children = new[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = new Vector2(1, 0.5f),
                            Colour = new Color4(0, 0, 0, 0.5f),
                            Shear = new Vector2(0.15f, 0),
                            EdgeSmoothness = new Vector2(2, 0),
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            RelativePositionAxes = Axes.Y,
                            Size = new Vector2(1, -0.5f),
                            Position = new Vector2(0, 1),
                            Colour = new Color4(0, 0, 0, 0.5f),
                            Shear = new Vector2(-0.15f, 0),
                            EdgeSmoothness = new Vector2(2, 0),
                        },
                    }
                },
                carousel = new CarouselContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(scrollWidth, 1),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                },
                wedgedBeatmapInfoOverlay = new BeatmapInfoOverlay
                {
                    Alpha = 0,
                    Position = wedged_container_start_position,
                    Size = wedged_container_size,
                    RelativeSizeAxes = Axes.X,
                    Shear = wedged_container_shear,
                    Margin = new MarginPadding { Top = 20, Right = 20, },
                    Masking = true,
                    BorderColour = new Color4(221, 255, 255, 255),
                    BorderThickness = 2.5f,
                    EdgeEffect = new EdgeEffect
                    {
                        Type = EdgeEffectType.Glow,
                        Colour = new Color4(130, 204, 255, 150),
                        Radius = 20,
                        Roundness = 15,
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = bottomToolHeight,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                            Colour = new Color4(0, 0, 0, 0.5f),
                        },
                        new Button
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 100,
                            Text = "Play",
                            Colour = new Color4(238, 51, 153, 255),
                            Action = () => Push(new Player
                            {
                                BeatmapInfo = carousel.SelectedGroup.SelectedPanel.Beatmap,
                                PreferredPlayMode = playMode.Value
                            })
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapDatabase beatmaps, AudioManager audio, BaseGame game, OsuGame osuGame)
        {
            if (osuGame != null)
            {
                playMode = osuGame.PlayMode;
                playMode.ValueChanged += playMode_ValueChanged;
            }

            if (database == null)
                database = beatmaps;

            database.BeatmapSetAdded += onDatabaseOnBeatmapSetAdded;

            trackManager = audio.Track;

            initialAddSetsTask = new CancellationTokenSource();

            Task.Factory.StartNew(() => addBeatmapSets(game, initialAddSetsTask.Token), initialAddSetsTask.Token);
        }

        private void onDatabaseOnBeatmapSetAdded(BeatmapSetInfo s)
        {
            Schedule(() => addBeatmapSet(s, Game));
        }

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            ensurePlayingSelected();

            changeBackground(Beatmap);

            Content.FadeInFromZero(250);
        }

        protected override void OnResuming(GameMode last)
        {
            changeBackground(Beatmap);
            ensurePlayingSelected();
            base.OnResuming(last);

            Content.FadeIn(250);
        }

        protected override void OnSuspending(GameMode next)
        {
            Content.FadeOut(250);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(GameMode next)
        {
            Content.FadeOut(100);
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
            if (beatmap == null)
                return;

            var backgroundModeBeatmap = Background as BackgroundModeBeatmap;
            if (backgroundModeBeatmap != null)
            {
                backgroundModeBeatmap.Beatmap = beatmap;
                // TODO: Remove this once we have non-nullable Beatmap
                (Background as BackgroundModeBeatmap)?.BlurTo(BACKGROUND_BLUR, 1000);
            }

            wedgedBeatmapInfoOverlay.UpdateBeatmap(beatmap);
        }

        /// <summary>
        /// The global Beatmap was changed.
        /// </summary>
        protected override void OnBeatmapChanged(WorkingBeatmap beatmap)
        {
            base.OnBeatmapChanged(beatmap);

            //todo: change background in selectionChanged instead; support per-difficulty backgrounds.
            changeBackground(beatmap);

            selectBeatmap(beatmap.BeatmapInfo);
        }

        private void selectBeatmap(BeatmapInfo beatmap)
        {
            carousel.SelectBeatmap(beatmap);
        }

        /// <summary>
        /// selection has been changed as the result of interaction with the carousel.
        /// </summary>
        private void selectionChanged(BeatmapGroup group, BeatmapInfo beatmap)
        {
            if (!beatmap.Equals(Beatmap?.BeatmapInfo))
                Beatmap = database.GetWorkingBeatmap(beatmap, Beatmap);

            ensurePlayingSelected();
        }

        private async Task ensurePlayingSelected()
        {
            AudioTrack track = null;

            await Task.Run(() => track = Beatmap?.Track);

            Schedule(delegate
            {
                if (track != null)
                {
                    trackManager.SetExclusive(track);
                    track.Start();
                }
            });
        }

        private void addBeatmapSet(BeatmapSetInfo beatmapSet, BaseGame game)
        {
            beatmapSet = database.GetWithChildren<BeatmapSetInfo>(beatmapSet.BeatmapSetID);
            beatmapSet.Beatmaps.ForEach(b => database.GetChildren(b));
            beatmapSet.Beatmaps = beatmapSet.Beatmaps.OrderBy(b => b.BaseDifficulty.OverallDifficulty).ToList();

            var beatmap = database.GetWorkingBeatmap(beatmapSet.Beatmaps.FirstOrDefault());

            var group = new BeatmapGroup(beatmap) { SelectionChanged = selectionChanged };

            //for the time being, let's completely load the difficulty panels in the background.
            //this likely won't scale so well, but allows us to completely async the loading flow.
            Task.WhenAll(group.BeatmapPanels.Select(panel => panel.Preload(game))).ContinueWith(task => Schedule(delegate
            {
                carousel.AddGroup(group);

                if (Beatmap == null)
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
    }
}
