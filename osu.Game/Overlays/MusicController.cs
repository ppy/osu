//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
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
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.IO;
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

        private OsuGameBase osuGame;
        private List<BeatmapSetInfo> playList;
        private BeatmapDatabase database;
        private Bindable<WorkingBeatmap> beatmapSource;
        private AudioTrack CurrentTrack => beatmapSource.Value?.Track;

        public MusicController(BeatmapDatabase db = null)
        {
            database = db;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
            osuGame = game as OsuGameBase;

            beatmapSource = osuGame.Beatmap;
            if (database == null) database = osuGame.Beatmaps;
            playList = database.Query<BeatmapSetInfo>().ToList();

            Width = 400;
            Height = 130;
            CornerRadius = 5;
            Masking = true;

            Children = new Drawable[]
            {
                backgroundSprite = getScaledSprite(game.Textures.Get(@"Backgrounds/bg4")),//placeholder
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
                    TextSize = 20,
                    Colour = Color4.White,
                    Text = @"Nothing to play"
                },
                artist = new SpriteText
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Position = new Vector2(0, 45),
                    TextSize = 12,
                    Colour = Color4.White,
                    Text = @"Nothing to play"
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
                        if (CurrentTrack == null) return;
                        if (CurrentTrack.IsRunning)
                        {
                            CurrentTrack.Stop();
                            playButton.Icon = FontAwesome.play_circle_o;
                        }
                        else
                        {
                            CurrentTrack.Start();
                            playButton.Icon = FontAwesome.pause;
                        }
                    },
                    Children = new Drawable[]
                    {
                        playButton = new TextAwesome
                        {
                            TextSize = 30,
                            Icon = FontAwesome.play_circle_o,
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
                            Icon = FontAwesome.step_backward,
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
                            Icon = FontAwesome.step_forward,
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
                            Icon = FontAwesome.bars,
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

            if (beatmapSource.Value != null)
            {
                playButton.Icon = FontAwesome.pause;
                updateCurrent(beatmapSource, null);
            }
            else if (playList.Count > 0)
            {
                play(playList[0].Beatmaps[0], null);
            }
        }

        protected override void Update()
        {
            base.Update();
            if (CurrentTrack == null) return;
            progress.UpdatePosition((float)(CurrentTrack.CurrentTime / CurrentTrack.Length));
            if (CurrentTrack.HasCompleted) next();
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
            int i = findInPlaylist(beatmapSource.Value?.Beatmap);
            if (i == -1) return;
            i = (i - 1 + playList.Count) % playList.Count;
            play(playList[i].Beatmaps[0], false);
        }

        private void next()
        {
            int i = findInPlaylist(beatmapSource.Value?.Beatmap);
            if (i == -1) return;
            i = (i + 1) % playList.Count;
            play(playList[i].Beatmaps[0], true);
        }

        private void play(BeatmapInfo info, bool? isNext)
        {
            WorkingBeatmap working = database.GetWorkingBeatmap(info, beatmapSource.Value);
            beatmapSource.Value = working;
            updateCurrent(working, isNext);
        }

        private void updateCurrent(WorkingBeatmap beatmap, bool? isNext)
        {
            BeatmapMetadata metadata = beatmap.Beatmap.Metadata;
            title.Text = metadata.TitleUnicode ?? metadata.Title;
            artist.Text = metadata.ArtistUnicode ?? metadata.Artist;

            Sprite newBackground;


            newBackground = getScaledSprite(TextureLoader.FromStream(beatmap.Reader.ReadFile(metadata.BackgroundFile)));

            Add(newBackground);

            if (isNext == true)
            {
                newBackground.Position = new Vector2(400, 0);
                newBackground.MoveToX(0, 500, EasingTypes.OutCubic);
                backgroundSprite.MoveToX(-400, 500, EasingTypes.OutCubic);
                backgroundSprite.Expire();
            }
            else if (isNext == false)
            {
                newBackground.Position = new Vector2(-400, 0);
                newBackground.MoveToX(0, 500, EasingTypes.OutCubic);
                backgroundSprite.MoveToX(400, 500, EasingTypes.OutCubic);
                backgroundSprite.Expire();
            }
            else
            {
                Remove(backgroundSprite);
            }

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
            CurrentTrack?.Seek(CurrentTrack.Length * position);
            CurrentTrack?.Start();
        }

        //placeholder for toggling
        protected override void PopIn() => FadeIn(500);

        protected override void PopOut() => FadeOut(500);
    }
}
