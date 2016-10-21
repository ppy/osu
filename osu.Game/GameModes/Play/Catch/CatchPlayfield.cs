//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework;

namespace osu.Game.GameModes.Play.Catch
{
    public class CatchPlayfield : Playfield
    {
        public CatchPlayfield()
        {
            RelativeSizeAxes = Axes.Y;
            Size = new Vector2(512, 0.9f);
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);

            Add(new Box { RelativeSizeAxes = Axes.Both, Alpha = 0.5f });
        }
    }
}