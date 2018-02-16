// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Music;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Overlays
{
    public class MusicController : OsuFocusedOverlayContainer
    {
        private const float player_height = 130;

        private const float transition_length = 800;

        private const float progress_height = 10;

        private const float bottom_black_area_height = 55;

        private Drawable background;
        private ProgressBar progressBar;

        private IconButton prevButton;
        private IconButton playButton;
        private IconButton nextButton;
        private IconButton playlistButton;

        private SpriteText title, artist;

        private PlaylistOverlay playlist;

        private LocalisationEngine localisation;

        private readonly Bindable<WorkingBeatmap> beatmapBacking = new Bindable<WorkingBeatmap>();

        private Container dragContainer;
        private Container playerContainer;

        public MusicController()
        {
            Width = 400;
            Margin = new MarginPadding(10);

            // required to let MusicController handle beatmap cycling.
            AlwaysPresent = true;
        }

        private Vector2 dragStart;

        protected override bool OnDragStart(InputState state)
        {
            base.OnDragStart(state);
            dragStart = state.Mouse.Position;
            return true;
        }

        protected override bool OnDrag(InputState state)
        {
            if (base.OnDrag(state)) return true;

            Vector2 change = state.Mouse.Position - dragStart;

            // Diminish the drag distance as we go further to simulate "rubber band" feeling.
            change *= change.Length <= 0 ? 0 : (float)Math.Pow(change.Length, 0.7f) / change.Length;

            dragContainer.MoveTo(change);
            return true;
        }

        protected override bool OnDragEnd(InputState state)
        {
            dragContainer.MoveTo(Vector2.Zero, 800, Easing.OutElastic);
            return base.OnDragEnd(state);
        }

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, OsuColour colours, LocalisationEngine localisation)
        {
            this.localisation = localisation;

            Children = new Drawable[]
            {
                dragContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        playlist = new PlaylistOverlay
                        {
                            RelativeSizeAxes = Axes.X,
                            Y = player_height + 10,
                        },
                        playerContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = player_height,
                            Masking = true,
                            CornerRadius = 5,
                            EdgeEffect = new EdgeEffectParameters
                            {
                                Type = EdgeEffectType.Shadow,
                                Colour = Color4.Black.Opacity(40),
                                Radius = 5,
                            },
                            Children = new[]
                            {
                                background = new Background(),
                                title = new OsuSpriteText
                                {
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.TopCentre,
                                    Position = new Vector2(0, 40),
                                    TextSize = 25,
                                    Colour = Color4.White,
                                    Text = @"Nothing to play",
                                    Font = @"Exo2.0-MediumItalic"
                                },
                                artist = new OsuSpriteText
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Position = new Vector2(0, 45),
                                    TextSize = 15,
                                    Colour = Color4.White,
                                    Text = @"Nothing to play",
                                    Font = @"Exo2.0-BoldItalic"
                                },
                                new Container
                                {
                                    Padding = new MarginPadding { Bottom = progress_height },
                                    Height = bottom_black_area_height,
                                    RelativeSizeAxes = Axes.X,
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.BottomCentre,
                                    Children = new Drawable[]
                                    {
                                        new FillFlowContainer<IconButton>
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Direction = FillDirection.Horizontal,
                                            Spacing = new Vector2(5),
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.Centre,
                                            Children = new[]
                                            {
                                                prevButton = new IconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Action = prev,
                                                    Icon = FontAwesome.fa_step_backward,
                                                },
                                                playButton = new IconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Scale = new Vector2(1.4f),
                                                    IconScale = new Vector2(1.4f),
                                                    Action = play,
                                                    Icon = FontAwesome.fa_play_circle_o,
                                                },
                                                nextButton = new IconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Action = next,
                                                    Icon = FontAwesome.fa_step_forward,
                                                },
                                            }
                                        },
                                        playlistButton = new IconButton
                                        {
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.CentreRight,
                                            Position = new Vector2(-bottom_black_area_height / 2, 0),
                                            Icon = FontAwesome.fa_bars,
                                            Action = () => playlist.ToggleVisibility(),
                                        },
                                    }
                                },
                                progressBar = new ProgressBar
                                {
                                    Origin = Anchor.BottomCentre,
                                    Anchor = Anchor.BottomCentre,
                                    Height = progress_height,
                                    FillColour = colours.Yellow,
                                    OnSeek = progress => current?.Track.Seek(progress)
                                }
                            },
                        },
                    }
                }
            };

            beatmapBacking.BindTo(game.Beatmap);

            playlist.StateChanged += s => playlistButton.FadeColour(s == Visibility.Visible ? colours.Yellow : Color4.White, 200, Easing.OutQuint);
        }

        protected override void LoadComplete()
        {
            beatmapBacking.ValueChanged += beatmapChanged;
            beatmapBacking.DisabledChanged += beatmapDisabledChanged;
            beatmapBacking.TriggerChange();

            base.LoadComplete();
        }

        private void beatmapDisabledChanged(bool disabled)
        {
            if (disabled)
                playlist.Hide();

            prevButton.Enabled.Value = !disabled;
            nextButton.Enabled.Value = !disabled;
            playlistButton.Enabled.Value = !disabled;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            Height = dragContainer.Height;
        }

        protected override void Update()
        {
            base.Update();

            if (current?.TrackLoaded ?? false)
            {
                var track = current.Track;

                progressBar.EndTime = track.Length;
                progressBar.CurrentTime = track.CurrentTime;

                playButton.Icon = track.IsRunning ? FontAwesome.fa_pause_circle_o : FontAwesome.fa_play_circle_o;

                if (track.HasCompleted && !track.Looping && !beatmapBacking.Disabled && playlist.BeatmapSets.Any())
                    next();
            }
            else
                playButton.Icon = FontAwesome.fa_play_circle_o;
        }

        private void play()
        {
            var track = current?.Track;

            if (track == null)
            {
                if (!beatmapBacking.Disabled)
                    playlist.PlayNext();
                return;
            }

            if (track.IsRunning)
                track.Stop();
            else
                track.Start();
        }

        private void prev()
        {
            queuedDirection = TransformDirection.Prev;
            playlist.PlayPrevious();
        }

        private void next()
        {
            queuedDirection = TransformDirection.Next;
            playlist.PlayNext();
        }

        private WorkingBeatmap current;
        private TransformDirection? queuedDirection;

        private void beatmapChanged(WorkingBeatmap beatmap)
        {
            TransformDirection direction = TransformDirection.None;

            if (current != null)
            {
                bool audioEquals = beatmap?.BeatmapInfo?.AudioEquals(current.BeatmapInfo) ?? false;

                if (audioEquals)
                    direction = TransformDirection.None;
                else if (queuedDirection.HasValue)
                {
                    direction = queuedDirection.Value;
                    queuedDirection = null;
                }
                else
                {
                    //figure out the best direction based on order in playlist.
                    var last = playlist.BeatmapSets.TakeWhile(b => b.ID != current.BeatmapSetInfo?.ID).Count();
                    var next = beatmap == null ? -1 : playlist.BeatmapSets.TakeWhile(b => b.ID != beatmap.BeatmapSetInfo?.ID).Count();

                    direction = last > next ? TransformDirection.Prev : TransformDirection.Next;
                }
            }

            current = beatmap;

            progressBar.CurrentTime = 0;

            updateDisplay(current, direction);

            queuedDirection = null;
        }

        private ScheduledDelegate pendingBeatmapSwitch;

        private void updateDisplay(WorkingBeatmap beatmap, TransformDirection direction)
        {
            //we might be off-screen when this update comes in.
            //rather than Scheduling, manually handle this to avoid possible memory contention.
            pendingBeatmapSwitch?.Cancel();

            pendingBeatmapSwitch = Schedule(delegate
            {
                // todo: this can likely be replaced with WorkingBeatmap.GetBeatmapAsync()
                Task.Run(() =>
                {
                    if (beatmap?.Beatmap == null) //this is not needed if a placeholder exists
                    {
                        title.Current = null;
                        title.Text = @"Nothing to play";

                        artist.Current = null;
                        artist.Text = @"Nothing to play";
                    }
                    else
                    {
                        BeatmapMetadata metadata = beatmap.Metadata;
                        title.Current = localisation.GetUnicodePreference(metadata.TitleUnicode, metadata.Title);
                        artist.Current = localisation.GetUnicodePreference(metadata.ArtistUnicode, metadata.Artist);
                    }
                });

                LoadComponentAsync(new Background(beatmap) { Depth = float.MaxValue }, newBackground =>
                {
                    switch (direction)
                    {
                        case TransformDirection.Next:
                            newBackground.Position = new Vector2(400, 0);
                            newBackground.MoveToX(0, 500, Easing.OutCubic);
                            background.MoveToX(-400, 500, Easing.OutCubic);
                            break;
                        case TransformDirection.Prev:
                            newBackground.Position = new Vector2(-400, 0);
                            newBackground.MoveToX(0, 500, Easing.OutCubic);
                            background.MoveToX(400, 500, Easing.OutCubic);
                            break;
                    }

                    background.Expire();
                    background = newBackground;

                    playerContainer.Add(newBackground);
                });
            });
        }

        protected override void PopIn()
        {
            base.PopIn();

            this.FadeIn(transition_length, Easing.OutQuint);
            dragContainer.ScaleTo(1, transition_length, Easing.OutElastic);
        }

        protected override void PopOut()
        {
            base.PopOut();

            this.FadeOut(transition_length, Easing.OutQuint);
            dragContainer.ScaleTo(0.9f, transition_length, Easing.OutQuint);
        }

        private enum TransformDirection
        {
            None,
            Next,
            Prev
        }

        private class Background : BufferedContainer
        {
            private readonly Sprite sprite;
            private readonly WorkingBeatmap beatmap;

            public Background(WorkingBeatmap beatmap = null)
            {
                this.beatmap = beatmap;
                CacheDrawnFrameBuffer = true;
                Depth = float.MaxValue;
                RelativeSizeAxes = Axes.Both;

                Children = new Drawable[]
                {
                    sprite = new Sprite
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = OsuColour.Gray(150),
                        FillMode = FillMode.Fill,
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.X,
                        Height = bottom_black_area_height,
                        Origin = Anchor.BottomCentre,
                        Anchor = Anchor.BottomCentre,
                        Colour = Color4.Black.Opacity(0.5f)
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                sprite.Texture = beatmap?.Background ?? textures.Get(@"Backgrounds/bg4");
            }
        }
    }
}
