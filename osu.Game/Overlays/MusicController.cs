//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK.Graphics;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays
{
    public class MusicController : OverlayContainer
    {
        private Sprite background;
        private Box progress;
        public override void Load(BaseGame game)
        {
            base.Load(game);
            Width = 400;
            Height = 130;
            CornerRadius = 5;
            Masking = true;
            Children = new Drawable[]
            {
                background = new Sprite
                {
                    RelativeSizeAxes = Axes.Both,
                    Texture = game.Textures.Get(@"Backgrounds/bg4")//placeholder
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = new Color4(0, 0, 0, 127)
                },
                new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 50,
                    Origin = Anchor.BottomCentre,
                    Anchor = Anchor.BottomCentre,
                    Colour = new Color4(0, 0, 0, 127)
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

        //placeholder for toggling
        protected override void PopIn() => FadeIn(500);

        protected override void PopOut() => FadeOut(500);
    }
}
