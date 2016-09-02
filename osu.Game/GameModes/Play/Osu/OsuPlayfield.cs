//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using OpenTK;

namespace osu.Game.GameModes.Play.Osu
{
    public class OsuPlayfield : Container
    {
        public OsuPlayfield()
        {
            Size = new Vector2(512, 384);
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
        }

        public override void Load()
        {
            base.Load();

            Add(new Box() { SizeMode = InheritMode.XY, Alpha = 0.5f });
        }
    }
}