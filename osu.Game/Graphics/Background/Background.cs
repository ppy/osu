//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics.Containers;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Graphics.Background
{
    class Background : LargeContainer
    {
        protected Sprite BackgroundSprite;

        public override void Load()
        {
            base.Load();

            Add(BackgroundSprite = new Sprite
            {
                Texture = Game.Textures.Get(@"Menu/background"),
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Colour = Color4.DarkGray
            });
        }

        protected override void Update()
        {
            base.Update();
            BackgroundSprite.Scale = new Vector2(Math.Max(ActualSize.X / BackgroundSprite.Size.X, ActualSize.Y / BackgroundSprite.Size.Y));
        }
    }
}
