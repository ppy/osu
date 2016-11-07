//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Audio.Track;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;

namespace osu.Game.Overlays
{
    public class MusicController : OverlayContainer
    {
        private Sprite backgroundSprite;
        private DragBar progress;
        private TextAwesome playButton, listButton;
        private SpriteText title, artist;
        private Texture fallbackTexture;

        private List<BeatmapSetInfo> playList;
        private List<BeatmapInfo> playHistory;
        private int playListIndex;
        private int playHistoryIndex;

        private TrackManager trackManager;
        private BeatmapDatabase database;
        private Bindable<WorkingBeatmap> beatmapSource;
        private WorkingBeatmap current;

        public MusicController(BeatmapDatabase db = null)
        {
            database = db;
            Width = 400;
            Height = 130;
            CornerRadius = 5;
            Masking = true;
            Anchor = Anchor.TopRight;//placeholder
            Origin = Anchor.TopRight;
            Position = new Vector2(10, 60);
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0, 0, 0, 127)
                },
                title = new SpriteText
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.TopCentre,
                    Position = new Vector2(0, 40),
                    TextSize = 25,
                    Colour = Color4.White,
                    Text = @"Nothing to play",
                    Font = @"Exo2.0-MediumItalic"
                },
                artist = new SpriteText
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Position = new Vector2(0, 45),
                    TextSize = 15,
                    Colour = Color4.White,
                    Text = @"Nothing to play",
                    Font = @"Exo2.0-BoldItalic"
                },
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Colour = new Color4(0, 0, 0, 127)
                },
                new ClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(0, 30),
                    Action = () =>
                    {
                        if (current?.Track == null) return;
                        if (current.Track.IsRunning)
                            current.Track.Stop();
                        else
                            current.Track.Start();
                    },
                    Children = new Drawable[]
                    {
                        playButton = new TextAwesome
                        {
                            TextSize = 30,
                            Icon = FontAwesome.fa_play_circle_o,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                },
                new ClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(-30, 30),
                    Action = prev,
                    Children = new Drawable[]
                    {
                        new TextAwesome
                        {
                            TextSize = 15,
                            Icon = FontAwesome.fa_step_backward,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                },
                new ClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(30, 30),
                    Action = next,
                    Children = new Drawable[]
                    {
                        new TextAwesome
                        {
                            TextSize = 15,
                            Icon = FontAwesome.fa_step_forward,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                },
                new ClickableContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomRight,
                    Position = new Vector2(20, 30),
                    Children = new Drawable[]
                    {
                        listButton = new TextAwesome
                        {
                            TextSize = 15,
                            Icon = FontAwesome.fa_bars,
                            Origin = Anchor.Centre,
                            Anchor = Anchor.Centre
                        }
                    }
                },
                progress = new DragBar
                {
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Height = 10,
                    Colour = Color4.Orange,
                    SeekRequested = seek
                }
            };
        }

        protected override void Load(BaseGame game)
        {
            base.Load(game);
            var osuGame = game as OsuGameBase;

            if (osuGame != null)
            {
                if (database == null) database = osuGame.Beatmaps;
                trackManager = osuGame.Audio.Track;
            }

            beatmapSource = osuGame?.Beatmap ?? new Bindable<WorkingBeatmap>();
            playList = database.GetAllWithChildren<BeatmapSetInfo>();

            backgroundSprite = getScaledSprite(fallbackTexture = game.Textures.Get(@"Backgrounds/bg4"));
            AddInternal(backgroundSprite);
        }

        protected override void LoadComplete()
        {
            beatmapSource.ValueChanged += workingChanged;
            workingChanged();
            base.LoadComplete();
        }

        protected override void Update()
        {
            base.Update();
            if (current?.Track == null) return;

            progress.UpdatePosition((float)(current.Track.CurrentTime / current.Track.Length));
            playButton.Icon = current.Track.IsRunning ? FontAwesome.fa_pause_circle_o : FontAwesome.fa_play_circle_o;

            if (current.Track.HasCompleted && !current.Track.Looping) next();
        }

        private void workingChanged(object sender = null, EventArgs e = null)
        {
            if (beatmapSource.Value == current) return;
            current = beatmapSource.Value;
            updateCurrent(current, null);
        }

        private int findInPlaylist(Beatmap beatmap)
        {
            if (beatmap == null) return -1;
            for (int i = 0; i < playList.Count; i++)
                if (beatmap.BeatmapInfo.BeatmapSetID == playList[i].BeatmapSetID)
                    return i;
            return -1;
        }

        private void prev()
        {
            int i = findInPlaylist(current?.Beatmap);
            if (i == -1)
            {
                if (playList.Count > 0)
                    play(playList[0].Beatmaps[0], null);
                else return;
            }
            i = (i - 1 + playList.Count) % playList.Count;
            play(playList[i].Beatmaps[0], false);
        }

        private void next()
        {
            int i = findInPlaylist(current?.Beatmap);
            if (i == -1)
            {
                if (playList.Count > 0)
                    play(playList[0].Beatmaps[0], null);
                else return;
            }
            i = (i + 1) % playList.Count;
            play(playList[i].Beatmaps[0], true);
        }

        private void play(BeatmapInfo info, bool? isNext)
        {
            current = database.GetWorkingBeatmap(info, current);
            Task.Run(() =>
            {
                trackManager.SetExclusive(current.Track);
                current.Track.Start();
                beatmapSource.Value = current;
            });
            updateCurrent(current, isNext);
        }

        private void updateCurrent(WorkingBeatmap beatmap, bool? isNext)
        {
            BeatmapMetadata metadata = beatmap.Beatmap.Metadata;
            title.Text = metadata.TitleUnicode ?? metadata.Title;
            artist.Text = metadata.ArtistUnicode ?? metadata.Artist;

            Sprite newBackground = getScaledSprite(beatmap.Background ?? fallbackTexture);

            Add(newBackground);

            if (isNext == true)
            {
                newBackground.Position = new Vector2(400, 0);
                newBackground.MoveToX(0, 500, EasingTypes.OutCubic);
                backgroundSprite.MoveToX(-400, 500, EasingTypes.OutCubic);
            }
            else if (isNext == false)
            {
                newBackground.Position = new Vector2(-400, 0);
                newBackground.MoveToX(0, 500, EasingTypes.OutCubic);
                backgroundSprite.MoveToX(400, 500, EasingTypes.OutCubic);
            }
            backgroundSprite.Expire();

            backgroundSprite = newBackground;
        }

        private Sprite getScaledSprite(Texture background)
        {
            Sprite scaledSprite = new Sprite
            {
                Origin = Anchor.Centre,
                Anchor = Anchor.Centre,
                Texture = background,
                Depth = float.MinValue
            };
            scaledSprite.Scale = new Vector2(Math.Max(DrawSize.X / scaledSprite.DrawSize.X, DrawSize.Y / scaledSprite.DrawSize.Y));
            return scaledSprite;
        }

        private void seek(float position)
        {
            current?.Track?.Seek(current.Track.Length * position);
            current?.Track?.Start();
        }

        protected override bool OnClick(InputState state) => true;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnDragStart(InputState state) => true;

        //placeholder for toggling
        protected override void PopIn() => FadeIn(100);

        protected override void PopOut() => FadeOut(100);
    }
}
