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
using osu.Game.Overlays.PopUpDialogs;
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

        private Box modeLight;

        private PlaySongSelectButtonContainer playSongSelectButtonContainer;
        private PlaySongSelectButton beatmapModsButton;
        private PlaySongSelectButton beatmapRandomButton;
        private PlaySongSelectButton beatmapOptionsButton;
        private const float play_song_select_button_width = 100;
        private const float play_song_select_button_height = 50;
        private const int mode_light_transition_time = 200;

        private SongSelectOptionsContainer songSelectOptionsContainer;
        private const float options_button_width = 140;
        private const float options_button_height = 125;

        private DeleteBeatmapDialog deleteBeatmapDialog;

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
                };
            }
        }

        public PlaySongSelect()
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
                deleteBeatmapDialog = new DeleteBeatmapDialog(),
                songSelectOptionsContainer = new SongSelectOptionsContainer
                {
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Position = new Vector2(0, -53),
                    Depth = -10,
                    Children = new SongSelectOptionsButton[]
                    {
                        new SongSelectOptionsButton
                        {
                            Icon = FontAwesome.fa_osu_cross_o,
                            TextLineA = "Remove",
                            TextLineB = "from Unplayed",
                            Size = new Vector2(options_button_width, options_button_height),
                            Colour = new Color4(103, 83, 196, 255),
                        },
                        new SongSelectOptionsButton
                        {
                            Icon = FontAwesome.fa_eraser,
                            TextLineA = "Clear",
                            TextLineB = "local scores",
                            Size = new Vector2(options_button_width, options_button_height),
                            Colour = new Color4(103, 83, 196, 255),
                        },
                        new SongSelectOptionsButton
                        {
                            Icon = FontAwesome.fa_pencil,
                            TextLineA = "Edit",
                            TextLineB = "Beatmap",
                            Size = new Vector2(options_button_width, options_button_height),
                            Colour = new Color4(227, 159, 12, 255),
                        },
                        new SongSelectOptionsButton
                        {
                            Icon = FontAwesome.fa_trash,
                            TextLineA = "Delete",
                            TextLineB = "Beatmap",
                            Size = new Vector2(options_button_width, options_button_height),
                            Colour = new Color4(238, 51, 153, 255),
                            Action = deleteBeatmapDialog.ToggleVisibility,
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
                            Colour = new Color4(0, 0, 0, 0.5f),
                        },
                        modeLight = new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 3,
                            Alpha = 0f,
                            Position = new Vector2(0, -3),
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
                        new FlowContainer
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.Y,
                            AutoSizeAxes = Axes.X,
                            Direction = FlowDirection.HorizontalOnly,
                            Spacing = new Vector2(80, 0),
                            Children = new Drawable[]
                            {
                                new BackButton
                                {
                                    Anchor = Anchor.BottomLeft,
                                    Origin = Anchor.BottomLeft,
                                    Action = Exit,
                                },
                                playSongSelectButtonContainer = new PlaySongSelectButtonContainer
                                {
                                    Children = new Drawable[]
                                    {
                                        beatmapModsButton = new PlaySongSelectButton
                                        {
                                            Text = "mods",
                                            Height = play_song_select_button_height,
                                            Width = play_song_select_button_width,
                                            SelectedColour = new Color4(238, 51, 153, 255),
                                            DeselectedColour = new Color4(125, 54, 82, 255),
                                            Action = deleteBeatmapDialog.ToggleVisibility,
                                        },
                                        beatmapRandomButton = new PlaySongSelectButton
                                        {
                                            Text = "random",
                                            Height = play_song_select_button_height,
                                            Width = play_song_select_button_width,
                                            SelectedColour = new Color4(165, 204, 0, 225),
                                            DeselectedColour = new Color4(79, 99, 8, 255),
                                            Action = carousel.SelectRandom,
                                        },
                                        beatmapOptionsButton = new PlaySongSelectButton
                                        {
                                            Text = "options",
                                            Height = play_song_select_button_height,
                                            Width = play_song_select_button_width,
                                            SelectedColour = new Color4(68, 170, 221, 225),
                                            DeselectedColour = new Color4(14, 116, 145, 255),
                                            Action = songSelectOptionsContainer.ToggleState,
                                        },
                                    }
                                }
                            }
                        }
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

            sampleChangeDifficulty = audio.Sample.Get(@"SongSelect/select-difficulty");
            sampleChangeBeatmap = audio.Sample.Get(@"SongSelect/select-expand");

            playSongSelectButtonContainer.On_HoveredChanged += updateModeLight;

            initialAddSetsTask = new CancellationTokenSource();

            Task.Factory.StartNew(() => addBeatmapSets(game, initialAddSetsTask.Token), initialAddSetsTask.Token);
        }

        private void updateModeLight()
        {
            modeLight.FadeIn(mode_light_transition_time);
            modeLight.FadeColour(playSongSelectButtonContainer.HoveredButton.SelectedColour, mode_light_transition_time);
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

            beatmapInfoWedge.MoveTo(wedged_container_start_position + new Vector2(-100, 50));
            beatmapInfoWedge.RotateTo(10);

            beatmapInfoWedge.MoveTo(wedged_container_start_position, 800, EasingTypes.OutQuint);
            beatmapInfoWedge.RotateTo(0, 800, EasingTypes.OutQuint);
        }

        protected override void OnResuming(GameMode last)
        {
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
            deleteBeatmapDialog.UpdateSelectedBeatmap(beatmap);
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
            {
                if (beatmap.BeatmapSetID == Beatmap?.BeatmapInfo.BeatmapSetID)
                    sampleChangeDifficulty.Play();
                else
                    sampleChangeBeatmap.Play();

                Beatmap = database.GetWorkingBeatmap(beatmap, Beatmap);
            }

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
