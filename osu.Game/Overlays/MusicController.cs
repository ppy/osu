//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input;
using osu.Game.Database;
using osu.Game.Graphics;

namespace osu.Game.Overlays
{
    public class MusicController : OverlayContainer
    {
        private Sprite backgroundSprite;
        private Box progress;
        private SpriteText title, artist;
        private List<BeatmapSetInfo> playList;
        private BeatmapSetInfo currentPlay;
        public override void Load(BaseGame game)
        {
            base.Load(game);
            playList = (game as OsuGameBase).Beatmaps.Query<BeatmapSetInfo>().ToList();
            currentPlay = playList.FirstOrDefault();
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
                    Text = currentPlay?.Metadata.TitleUnicode ?? currentPlay?.Metadata.Title ?? @"Nothing to play"
                },
                artist = new SpriteText
                {
                    Origin = Anchor.TopCentre,
                    Anchor = Anchor.TopCentre,
                    Position = new Vector2(0, 45),
                    TextSize = 12,
                    Colour = Color4.White,
                    Text = currentPlay?.Metadata.ArtistUnicode ?? currentPlay?.Metadata.Artist ?? @"Nothing to play"
                },
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Colour = new Color4(0, 0, 0, 127)
                },
                new ClickableTextAwesome
                {
                    TextSize = 30,
                    Icon = FontAwesome.play_circle_o,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(0, 30)
                },
                new ClickableTextAwesome
                {
                    TextSize = 15,
                    Icon = FontAwesome.step_backward,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(-30, 30)
                },
                new ClickableTextAwesome
                {
                    TextSize = 15,
                    Icon = FontAwesome.step_forward,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomCentre,
                    Position = new Vector2(30, 30)
                },
                new ClickableTextAwesome
                {
                    TextSize = 15,
                    Icon = FontAwesome.bars,
                    Origin = Anchor.Centre,
                    Anchor = Anchor.BottomRight,
                    Position = new Vector2(20, 30)
                },
                progress = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 10,
                    Width = 0.5f,//placeholder
                    Origin = Anchor.BottomLeft,
                    Anchor = Anchor.BottomLeft,
                    Colour = Color4.Orange
                }
            };
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

        //placeholder for toggling
        protected override void PopIn() => FadeIn(500);

        protected override void PopOut() => FadeOut(500);
    }

    public class ClickableTextAwesome : TextAwesome
    {
        public Action<ClickableTextAwesome> Action;

        protected override bool OnClick(InputState state)
        {
            Action?.Invoke(this);
            return true;
        }
    }
}
