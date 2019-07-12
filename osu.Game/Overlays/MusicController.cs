﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.Music;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

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

        private BeatmapManager beatmaps;

        private List<BeatmapSetInfo> beatmapSets;

        private Container dragContainer;
        private Container playerContainer;

        public bool IsUserPaused { get; private set; }

        [Resolved]
        private Bindable<WorkingBeatmap> beatmap { get; set; }

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        /// <summary>
        /// Provide a source for the toolbar height.
        /// </summary>
        public Func<float> GetToolbarHeight;

        public MusicController()
        {
            Width = 400;
            Margin = new MarginPadding(10);
        }

        [BackgroundDependencyLoader]
        private void load(Bindable<WorkingBeatmap> beatmap, BeatmapManager beatmaps, OsuColour colours)
        {
            this.beatmaps = beatmaps;

            Children = new Drawable[]
            {
                dragContainer = new DragContainer
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
                            OrderChanged = playlistOrderChanged
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
                                    Font = OsuFont.GetFont(size: 25, italics: true),
                                    Colour = Color4.White,
                                    Text = @"Nothing to play",
                                },
                                artist = new OsuSpriteText
                                {
                                    Origin = Anchor.TopCentre,
                                    Anchor = Anchor.TopCentre,
                                    Position = new Vector2(0, 45),
                                    Font = OsuFont.GetFont(size: 15, weight: FontWeight.Bold, italics: true),
                                    Colour = Color4.White,
                                    Text = @"Nothing to play",
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
                                                prevButton = new MusicIconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Action = prev,
                                                    Icon = FontAwesome.Solid.StepBackward,
                                                },
                                                playButton = new MusicIconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Scale = new Vector2(1.4f),
                                                    IconScale = new Vector2(1.4f),
                                                    Action = togglePause,
                                                    Icon = FontAwesome.Regular.PlayCircle,
                                                },
                                                nextButton = new MusicIconButton
                                                {
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                    Action = () => next(),
                                                    Icon = FontAwesome.Solid.StepForward,
                                                },
                                            }
                                        },
                                        playlistButton = new MusicIconButton
                                        {
                                            Origin = Anchor.Centre,
                                            Anchor = Anchor.CentreRight,
                                            Position = new Vector2(-bottom_black_area_height / 2, 0),
                                            Icon = FontAwesome.Solid.Bars,
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
                                    OnSeek = attemptSeek
                                }
                            },
                        },
                    }
                }
            };

            beatmapSets = beatmaps.GetAllUsableBeatmapSets();
            beatmaps.ItemAdded += handleBeatmapAdded;
            beatmaps.ItemRemoved += handleBeatmapRemoved;

            playlist.State.ValueChanged += s => playlistButton.FadeColour(s.NewValue == Visibility.Visible ? colours.Yellow : Color4.White, 200, Easing.OutQuint);
        }

        private ScheduledDelegate seekDelegate;

        private void attemptSeek(double progress)
        {
            seekDelegate?.Cancel();
            seekDelegate = Schedule(() =>
            {
                if (!beatmap.Disabled)
                    current?.Track.Seek(progress);
            });
        }

        private void playlistOrderChanged(BeatmapSetInfo beatmapSetInfo, int index)
        {
            beatmapSets.Remove(beatmapSetInfo);
            beatmapSets.Insert(index, beatmapSetInfo);
        }

        private void handleBeatmapAdded(BeatmapSetInfo set) => Schedule(() => beatmapSets.Add(set));

        private void handleBeatmapRemoved(BeatmapSetInfo set) => Schedule(() => beatmapSets.RemoveAll(s => s.ID == set.ID));

        protected override void LoadComplete()
        {
            beatmap.BindValueChanged(beatmapChanged, true);
            beatmap.BindDisabledChanged(beatmapDisabledChanged, true);
            mods.BindValueChanged(_ => updateAudioAdjustments(), true);
            base.LoadComplete();
        }

        private void beatmapDisabledChanged(bool disabled)
        {
            if (disabled)
                playlist.Hide();

            playButton.Enabled.Value = !disabled;
            prevButton.Enabled.Value = !disabled;
            nextButton.Enabled.Value = !disabled;
            playlistButton.Enabled.Value = !disabled;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();
            Height = dragContainer.Height;

            dragContainer.Padding = new MarginPadding { Top = GetToolbarHeight?.Invoke() ?? 0 };
        }

        protected override void Update()
        {
            base.Update();

            if (pendingBeatmapSwitch != null)
            {
                pendingBeatmapSwitch();
                pendingBeatmapSwitch = null;
            }

            var track = current?.TrackLoaded ?? false ? current.Track : null;

            if (track?.IsDummyDevice == false)
            {
                progressBar.EndTime = track.Length;
                progressBar.CurrentTime = track.CurrentTime;

                playButton.Icon = track.IsRunning ? FontAwesome.Regular.PauseCircle : FontAwesome.Regular.PlayCircle;
            }
            else
            {
                progressBar.CurrentTime = 0;
                progressBar.EndTime = 1;
                playButton.Icon = FontAwesome.Regular.PlayCircle;
            }
        }

        private void togglePause()
        {
            var track = current?.Track;

            if (track == null)
            {
                if (!beatmap.Disabled)
                    next(true);
                return;
            }

            if (track.IsRunning)
            {
                IsUserPaused = true;
                track.Stop();
            }
            else
            {
                track.Start();
                IsUserPaused = false;
            }
        }

        private void prev()
        {
            queuedDirection = TransformDirection.Prev;

            var playable = beatmapSets.TakeWhile(i => i.ID != current.BeatmapSetInfo.ID).LastOrDefault() ?? beatmapSets.LastOrDefault();

            if (playable != null)
            {
                beatmap.Value = beatmaps.GetWorkingBeatmap(playable.Beatmaps.First(), beatmap.Value);
                beatmap.Value.Track.Restart();
            }
        }

        private void next(bool instant = false)
        {
            if (!instant)
                queuedDirection = TransformDirection.Next;

            var playable = beatmapSets.SkipWhile(i => i.ID != current.BeatmapSetInfo.ID).Skip(1).FirstOrDefault() ?? beatmapSets.FirstOrDefault();

            if (playable != null)
            {
                beatmap.Value = beatmaps.GetWorkingBeatmap(playable.Beatmaps.First(), beatmap.Value);
                beatmap.Value.Track.Restart();
            }
        }

        private WorkingBeatmap current;
        private TransformDirection? queuedDirection;

        private void beatmapChanged(ValueChangedEvent<WorkingBeatmap> beatmap)
        {
            TransformDirection direction = TransformDirection.None;

            if (current != null)
            {
                bool audioEquals = beatmap.NewValue?.BeatmapInfo?.AudioEquals(current.BeatmapInfo) ?? false;

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
                    var last = beatmapSets.TakeWhile(b => b.ID != current.BeatmapSetInfo?.ID).Count();
                    var next = beatmap.NewValue == null ? -1 : beatmapSets.TakeWhile(b => b.ID != beatmap.NewValue.BeatmapSetInfo?.ID).Count();

                    direction = last > next ? TransformDirection.Prev : TransformDirection.Next;
                }
            }

            progressBar.CurrentTime = 0;

            updateDisplay(current = beatmap.NewValue, direction);
            updateAudioAdjustments();

            queuedDirection = null;
        }

        private void updateAudioAdjustments()
        {
            var track = current?.Track;
            if (track == null)
                return;

            track.ResetSpeedAdjustments();

            foreach (var mod in mods.Value.OfType<IApplicableToClock>())
                mod.ApplyToClock(track);
        }

        private Action pendingBeatmapSwitch;

        private void updateDisplay(WorkingBeatmap beatmap, TransformDirection direction)
        {
            // avoid using scheduler as our scheduler may not be run for a long time, holding references to beatmaps.
            pendingBeatmapSwitch = delegate
            {
                // todo: this can likely be replaced with WorkingBeatmap.GetBeatmapAsync()
                Task.Run(() =>
                {
                    if (beatmap?.Beatmap == null) //this is not needed if a placeholder exists
                    {
                        title.Text = @"Nothing to play";
                        artist.Text = @"Nothing to play";
                    }
                    else
                    {
                        BeatmapMetadata metadata = beatmap.Metadata;
                        title.Text = new LocalisedString((metadata.TitleUnicode, metadata.Title));
                        artist.Text = new LocalisedString((metadata.ArtistUnicode, metadata.Artist));
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
            };
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

        private class MusicIconButton : IconButton
        {
            public MusicIconButton()
            {
                AutoSizeAxes = Axes.Both;
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                HoverColour = colours.YellowDark.Opacity(0.6f);
                FlashColour = colours.Yellow;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                // works with AutoSizeAxes above to make buttons autosize with the scale animation.
                Content.AutoSizeAxes = Axes.None;
                Content.Size = new Vector2(DEFAULT_BUTTON_SIZE);
            }
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

        private class DragContainer : Container
        {
            protected override bool OnDragStart(DragStartEvent e)
            {
                return true;
            }

            protected override bool OnDrag(DragEvent e)
            {
                Vector2 change = e.MousePosition - e.MouseDownPosition;

                // Diminish the drag distance as we go further to simulate "rubber band" feeling.
                change *= change.Length <= 0 ? 0 : (float)Math.Pow(change.Length, 0.7f) / change.Length;

                this.MoveTo(change);
                return true;
            }

            protected override bool OnDragEnd(DragEndEvent e)
            {
                this.MoveTo(Vector2.Zero, 800, Easing.OutElastic);
                return base.OnDragEnd(e);
            }
        }

        /// <summary>
        /// Play the next random or playlist track.
        /// </summary>
        public void NextTrack() => next();
    }
}
