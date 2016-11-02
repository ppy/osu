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
        protected override Container Content => hitObjectContainer;

        private Container hitObjectContainer;

        public OsuPlayfield()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            Size = new Vector2(512, 384);

            AddInternal(new Box
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Colour = Color4.Black,
                Alpha = 0.5f
            });

            AddInternal(hitObjectContainer = new Container
            {
                RelativeSizeAxes = Axes.Both
            });
        }
    }
}