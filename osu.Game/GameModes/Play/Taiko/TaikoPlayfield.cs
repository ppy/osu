//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Play.Taiko
{
    public class TaikoPlayfield : Container
    {
        public TaikoPlayfield()
        {
            SizeMode = InheritMode.X;
            Size = new Vector2(1, 100);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        public override void Load()
        {
            base.Load();

            Add(new Box() { SizeMode = InheritMode.XY, Alpha = 0.5f });

            Add(new Sprite(Game.Textures.Get(@"menu-osu"))
            {
                Origin = Anchor.Centre,
                Scale = 0.2f,
                PositionMode = InheritMode.XY,
                Position = new Vector2(0.1f, 0.5f),
                Colour = Color4.Gray
            });
        }
    }
}