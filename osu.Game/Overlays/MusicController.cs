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
using osu.Framework.MathUtils;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
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
        private List<BeatmapInfo> playHistory = new List<BeatmapInfo>();
        private int playListIndex;
        private int playHistoryIndex = -1;

        private TrackManager trackManager;
        private BeatmapDatabase database;
        private Bindable<WorkingBeatmap> beatmapSource;
        private Bindable<bool> preferUnicode;
        private OsuConfigManager config;
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
                config = osuGame.Config;
                preferUnicode = osuGame.Config.GetBindable<bool>(OsuConfig.ShowUnicode);
                preferUnicode.ValueChanged += preferUnicode_changed;
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

        void preferUnicode_changed(object sender, EventArgs e)
        {
            updateDisplay(current, false);
        }

        private void workingChanged(object sender = null, EventArgs e = null)
        {
            if (beatmapSource.Value == current) return;
            bool audioEquals = current?.BeatmapInfo.AudioEquals(beatmapSource.Value.BeatmapInfo) ?? false;
            current = beatmapSource.Value;
            updateDisplay(current, audioEquals ? null : (bool?)true);
            appendToHistory(current.BeatmapInfo);
        }

        private void appendToHistory(BeatmapInfo beatmap)
        {
            if (playHistoryIndex >= 0)
            {
                if (beatmap.AudioEquals(playHistory[playHistoryIndex]))
                    return;
                if (playHistoryIndex < playHistory.Count - 1)
                    playHistory.RemoveRange(playHistoryIndex + 1, playHistory.Count - playHistoryIndex - 1);
            }
            playHistory.Insert(++playHistoryIndex, beatmap);
        }

        private void prev()
        {
            if (playHistoryIndex > 0)
                play(playHistory[--playHistoryIndex], false);
        }

        private void next()
        {
            if (playHistoryIndex < playHistory.Count - 1)
                play(playHistory[++playHistoryIndex], true);
            else
            {
                if (playList.Count == 0) return;
                if (current != null && playList.Count == 1) return;
                //shuffle
                BeatmapInfo nextToPlay;
                do
                {
                    int j = RNG.Next(playListIndex, playList.Count);
                    if (j != playListIndex)
                    {
                        BeatmapSetInfo temp = playList[playListIndex];
                        playList[playListIndex] = playList[j];
                        playList[j] = temp;
                    }

                    nextToPlay = playList[playListIndex++].Beatmaps[0];
                    if (playListIndex == playList.Count) playListIndex = 0;
                }
                while (nextToPlay.AudioEquals(current?.BeatmapInfo));

                play(nextToPlay, true);
                appendToHistory(nextToPlay);
            }
        }

        private void play(BeatmapInfo info, bool isNext)
        {
            current = database.GetWorkingBeatmap(info, current);
            Task.Run(() =>
            {
                trackManager.SetExclusive(current.Track);
                current.Track.Start();
                beatmapSource.Value = current;
            });
            updateDisplay(current, isNext);
        }

        private void updateDisplay(WorkingBeatmap beatmap, bool? isNext)
        {
            BeatmapMetadata metadata = beatmap.Beatmap.BeatmapInfo.Metadata;
            title.Text = config.GetUnicodeString(metadata.Title, metadata.TitleUnicode);
            artist.Text = config.GetUnicodeString(metadata.Artist, metadata.ArtistUnicode);

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
        
        protected override void Dispose(bool isDisposing)
        {
            if (preferUnicode != null)
                preferUnicode.ValueChanged -= preferUnicode_changed;
            base.Dispose(isDisposing);
        }

        protected override bool OnClick(InputState state) => true;

        protected override bool OnMouseDown(InputState state, MouseDownEventArgs args) => true;

        protected override bool OnDragStart(InputState state) => true;

        //placeholder for toggling
        protected override void PopIn() => FadeIn(100);

        protected override void PopOut() => FadeOut(100);
    }
}
