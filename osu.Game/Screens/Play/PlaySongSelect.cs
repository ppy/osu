//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
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
using osu.Game.Beatmaps.Drawable;
using osu.Game.Database;
using osu.Game.Modes;
using osu.Game.Screens.Backgrounds;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Screens.Play
{
    public class PlaySongSelect : OsuGameMode
    {
        private Bindable<PlayMode> playMode;
        private BeatmapDatabase database;
        private BeatmapGroup selectedBeatmapGroup;
        private BeatmapInfo selectedBeatmapInfo;

        protected override BackgroundMode CreateBackground() => new BackgroundModeBeatmap(Beatmap);

        private ScrollContainer scrollContainer;
        private FlowContainer beatmapSetFlow;
        private TrackManager trackManager;
        private Container backgroundWedgesContainer;

        private static readonly Vector2 wedged_container_size = new Vector2(600, 300);
        private static readonly Vector2 wedged_container_shear = new Vector2(0.15f, 0);
        private static readonly Vector2 wedged_container_start_position = new Vector2(0, 50);
        private Container wedgedContainer;
        private Container wedgedBeatmapInfo;

        private static readonly Vector2 BACKGROUND_BLUR = new Vector2(20);

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
                scrollContainer = new ScrollContainer
                {
                    RelativeSizeAxes = Axes.Y,
                    Size = new Vector2(scrollWidth, 1),
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Children = new Drawable[]
                    {
                        beatmapSetFlow = new FlowContainer
                        {
                            Padding = new MarginPadding { Left = 25, Top = 25, Bottom = 25 + bottomToolHeight },
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FlowDirection.VerticalOnly,
                            Spacing = new Vector2(0, 5),
                        }
                    }
                },
                wedgedContainer = new Container
                {
                    Position = wedged_container_start_position,
                    Size = wedged_container_size,
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
                    Shear = wedged_container_shear,
                    Children = new Drawable[]
                    {
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
                                BeatmapInfo = selectedBeatmapGroup.SelectedPanel.Beatmap,
                                PreferredPlayMode = playMode.Value
                            })
                        },
                    }
                }
            };
        }

        [BackgroundDependencyLoader(permitNulls: true)]
        private void load(BeatmapDatabase beatmaps, AudioManager audio, OsuGame game)
        {
            if (game != null)
            {
                playMode = game.PlayMode;
                playMode.ValueChanged += playMode_ValueChanged;
                // Temporary:
                scrollContainer.Padding = new MarginPadding { Top = ToolbarPadding };
            }

            if (database == null)
                database = beatmaps;

            database.BeatmapSetAdded += s => Schedule(() => addBeatmapSet(s));

            trackManager = audio.Track;

            Task.Factory.StartNew(addBeatmapSets);
        }

        protected override void OnEntering(GameMode last)
        {
            base.OnEntering(last);
            ensurePlayingSelected();
            backgroundWedgesContainer.FadeInFromZero(250);

            changeBackground(Beatmap);
        }

        protected override void OnResuming(GameMode last)
        {
            changeBackground(Beatmap);

            ensurePlayingSelected();
            base.OnResuming(last);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            if (playMode != null)
                playMode.ValueChanged -= playMode_ValueChanged;
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

            refreshWedgedBeatmapInfo(beatmap);
        }

        private void refreshWedgedBeatmapInfo(WorkingBeatmap beatmap)
        {
            if (beatmap == null)
                return;

            if (wedgedBeatmapInfo != null)
            {
                Drawable oldWedgedBeatmapInfo = wedgedBeatmapInfo;
                oldWedgedBeatmapInfo.Depth = 1;
                oldWedgedBeatmapInfo.FadeOut(250);
                oldWedgedBeatmapInfo.Expire();
            }

            BeatmapSetInfo beatmapSetInfo = beatmap.BeatmapSetInfo;
            BeatmapInfo beatmapInfo = beatmap.BeatmapInfo;
            wedgedContainer.Add(wedgedBeatmapInfo = new BufferedContainer
            {
                PixelSnapping = true,
                CacheDrawnFrameBuffer = true,
                Shear = -wedged_container_shear,
                RelativeSizeAxes = Axes.Both,
                Children = new Drawable[]
                {
                    // We will create the white-to-black gradient by modulating transparency and having
                    // a black backdrop. This results in an sRGB-space gradient and not linear space,
                    // transitioning from white to black more perceptually uniformly.
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                    },
                    // We use a container, such that we can set the colour gradient to go across the
                    // vertices of the masked container instead of the vertices of the (larger) sprite.
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        ColourInfo = ColourInfo.GradientVertical(Color4.White, new Color4(1f, 1f, 1f, 0.3f)),
                        Children = new []
                        {
                            // Zoomed-in and cropped beatmap background
                            new Sprite
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Texture = beatmap.Background,
                                Scale = new Vector2(1366 / beatmap.Background.Width * 0.6f),
                            },
                        },
                    },
                    // Text for beatmap info
                    new FlowContainer
                    {
                        Anchor = Anchor.BottomLeft,
                        Origin = Anchor.BottomLeft,
                        Direction = FlowDirection.VerticalOnly,
                        Margin = new MarginPadding { Top = 10, Left = 25, Right = 10, Bottom = 40 },
                        AutoSizeAxes = Axes.Both,
                        Children = new[]
                        {
                            new SpriteText
                            {
                                Font = @"Exo2.0-MediumItalic",
                                Text = beatmapSetInfo.Metadata.Artist + " -- " + beatmapSetInfo.Metadata.Title,
                                TextSize = 28,
                                Shadow = true,
                            },
                            new SpriteText
                            {
                                Font = @"Exo2.0-MediumItalic",
                                Text = beatmapInfo.Version,
                                TextSize = 17,
                                Shadow = true,
                            },
                            new FlowContainer
                            {
                                Margin = new MarginPadding { Top = 10 },
                                Direction = FlowDirection.HorizontalOnly,
                                AutoSizeAxes = Axes.Both,
                                Children = new []
                                {
                                    new SpriteText
                                    {
                                        Font = @"Exo2.0-Medium",
                                        Text = "mapped by ",
                                        TextSize = 15,
                                        Shadow = true,
                                    },
                                    new SpriteText
                                    {
                                        Font = @"Exo2.0-Bold",
                                        Text = beatmapSetInfo.Metadata.Author,
                                        TextSize = 15,
                                        Shadow = true,
                                    },
                                }
                            }
                        }
                    }
                }
            });
        }

        /// <summary>
        /// The global Beatmap was changed.
        /// </summary>
        protected override void OnBeatmapChanged(WorkingBeatmap beatmap)
        {
            base.OnBeatmapChanged(beatmap);

            changeBackground(beatmap);

            selectBeatmap(beatmap.BeatmapInfo);
        }

        private void selectBeatmap(BeatmapInfo beatmap)
        {
            if (beatmap.Equals(selectedBeatmapInfo))
                return;

            //this is VERY temporary logic.
            beatmapSetFlow.Children.Cast<BeatmapGroup>().Any(b =>
            {
                var panel = b.BeatmapPanels.FirstOrDefault(p => p.Beatmap.Equals(beatmap));
                if (panel != null)
                {
                    panel.State = PanelSelectedState.Selected;
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// selection has been changed as the result of interaction with the carousel.
        /// </summary>
        private void selectionChanged(BeatmapGroup group, BeatmapInfo beatmap)
        {
            selectedBeatmapInfo = beatmap;

            if (!beatmap.Equals(Beatmap?.BeatmapInfo))
            {
                Beatmap = database.GetWorkingBeatmap(beatmap, Beatmap);
            }

            ensurePlayingSelected();

            if (selectedBeatmapGroup == group)
                return;

            if (selectedBeatmapGroup != null)
                selectedBeatmapGroup.State = BeatmapGroupState.Collapsed;

            selectedBeatmapGroup = group;
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

        private void addBeatmapSet(BeatmapSetInfo beatmapSet)
        {
            beatmapSet = database.GetWithChildren<BeatmapSetInfo>(beatmapSet.BeatmapSetID);
            beatmapSet.Beatmaps.ForEach(b => database.GetChildren(b));
            beatmapSet.Beatmaps = beatmapSet.Beatmaps.OrderBy(b => b.BaseDifficulty.OverallDifficulty).ToList();

            var working = database.GetWorkingBeatmap(beatmapSet.Beatmaps.FirstOrDefault());

            var group = new BeatmapGroup(beatmapSet, working) { SelectionChanged = selectionChanged };

            group.Preload(Game, g =>
            {
                beatmapSetFlow.Add(group);

                if (Beatmap == null)
                {
                    if (beatmapSetFlow.Children.Count() == 1)
                    {
                        group.State = BeatmapGroupState.Expanded;
                        return;
                    }
                }
                else
                {
                    if (selectedBeatmapInfo?.Equals(Beatmap.BeatmapInfo) != true)
                    {
                        var panel = group.BeatmapPanels.FirstOrDefault(p => p.Beatmap.Equals(Beatmap.BeatmapInfo));
                        if (panel != null)
                        {
                            panel.State = PanelSelectedState.Selected;
                            return;
                        }
                    }
                }

                group.State = BeatmapGroupState.Collapsed;
            });
        }

        private void addBeatmapSets()
        {
            foreach (var beatmapSet in database.Query<BeatmapSetInfo>())
                addBeatmapSet(beatmapSet);
        }
    }
}
