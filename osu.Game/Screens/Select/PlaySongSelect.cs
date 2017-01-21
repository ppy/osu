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
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using osu.Game.Screens.Play;
using osu.Framework;
using osu.Framework.Audio.Sample;
using osu.Framework.Graphics.Transformations;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics.Containers;
using osu.Framework.Input;
using OpenTK.Input;
using osu.Game.Graphics;

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
        private static readonly Vector2 wedged_container_start_position = new Vector2(0, 50);
        private BeatmapInfoWedge beatmapInfoWedge;

        private static readonly Vector2 BACKGROUND_BLUR = new Vector2(20);
        private CancellationTokenSource initialAddSetsTask;

        private AudioSample sampleChangeDifficulty;
        private AudioSample sampleChangeBeatmap;

        class WedgeBackground : Container
        {
            public WedgeBackground()
            {
                Children = new[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(1, 0.5f),
                        Colour = Color4.Black.Opacity(0.5f),
                        Shear = new Vector2(0.15f, 0),
                        EdgeSmoothness = new Vector2(2, 0),
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        RelativePositionAxes = Axes.Y,
                        Size = new Vector2(1, -0.5f),
                        Position = new Vector2(0, 1),
                        Colour = Color4.Black.Opacity(0.5f),
                        Shear = new Vector2(-0.15f, 0),
                        EdgeSmoothness = new Vector2(2, 0),
                    },
                };
            }
        }

        Player player;

        private void start()
        {
            if (player != null)
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
            const float carouselWidth = 640;
            const float bottomToolHeight = 50;
            Children = new Drawable[]
            {
                new ParallaxContainer
                {
                    ParallaxAmount = 0.005f,
                    RelativeSizeAxes = Axes.Both,
                    Children = new []
                    {
                        new WedgeBackground
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding { Right = carouselWidth * 0.76f },
                        },
                    }
                },
                carousel = new CarouselContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(carouselWidth, 1),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                },
                beatmapInfoWedge = new BeatmapInfoWedge
                {
                    Alpha = 0,
                    Position = wedged_container_start_position,
                    Size = wedged_container_size,
                    RelativeSizeAxes = Axes.X,
                    Margin = new MarginPadding { Top = 20, Right = 20, },
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
                            Colour = Color4.Black.Opacity(0.5f),
                        },
                        new BackButton
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            //RelativeSizeAxes = Axes.Y,
                            Action = () => Exit()
                        },
                        new Button
                        {
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                            RelativeSizeAxes = Axes.Y,
                            Width = 100,
                            Text = "Play",
                            Colour = colours.Pink,
                            Action = start
                        },
                    }
                }
            };
        
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

            beatmapInfoWedge.MoveToX(wedged_container_start_position.X - 50);
            beatmapInfoWedge.MoveToX(wedged_container_start_position.X, 800, EasingTypes.OutQuint);
        }

        protected override void OnResuming(GameMode last)
        {
            player = null;

            changeBackground(Beatmap);
            ensurePlayingSelected();
            base.OnResuming(last);

            Content.FadeIn(250);

            Content.ScaleTo(1, 250, EasingTypes.OutSine);
        }

        protected override void OnSuspending(GameMode next)
        {
            Content.ScaleTo(1.1f, 250, EasingTypes.InSine);

            Content.FadeOut(250);
            base.OnSuspending(next);
        }

        protected override bool OnExiting(GameMode next)
        {
            beatmapInfoWedge.MoveTo(wedged_container_start_position + new Vector2(-100, 50), 800, EasingTypes.InQuint);
            beatmapInfoWedge.RotateTo(10, 800, EasingTypes.InQuint);

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

        private void addBeatmapSet(BeatmapSetInfo beatmapSet, BaseGame game)
        {
            beatmapSet = database.GetWithChildren<BeatmapSetInfo>(beatmapSet.ID);
            beatmapSet.Beatmaps.ForEach(b =>
            {
                database.GetChildren(b);
                if (b.Metadata == null) b.Metadata = beatmapSet.Metadata;
            });

            beatmapSet.Beatmaps = beatmapSet.Beatmaps.OrderBy(b => b.BaseDifficulty.OverallDifficulty).ToList();

            var beatmap = new WorkingBeatmap(beatmapSet.Beatmaps.FirstOrDefault(), beatmapSet, database);

            var group = new BeatmapGroup(beatmap)
            {
                SelectionChanged = selectionChanged,
                StartRequested = b => start()
            };

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
