//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using OpenTK;
using osu.Framework;
using osu.Framework.Graphics.Sprites;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Play.Osu
{
    public class OsuPlayfield : Playfield
    {
        public OsuPlayfield()
        {
            Size = new Vector2(512, 384);
            Scale = new Vector2(1.6f);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            Add(new Box()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
                Alpha = 0.5f
            });
        }
    }
}