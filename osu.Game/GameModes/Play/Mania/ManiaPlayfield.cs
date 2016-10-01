//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Sprites;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Play.Mania
{
    public class ManiaPlayfield : Playfield
    {
        private readonly int columns;

        public ManiaPlayfield(int columns)
        {
            this.columns = columns;
            SizeMode = InheritMode.XY;
            Size = new Vector2(columns / 20f, 1f);
            Anchor = Anchor.BottomCentre;
            Origin = Anchor.BottomCentre;
        }

        public override void Load()
        {
            base.Load();

            Add(new Box() { SizeMode = InheritMode.XY, Alpha = 0.5f });

            for (int i = 0; i < columns; i++)
                Add(new Box()
                {
                    SizeMode = InheritMode.Y,
                    Size = new Vector2(2, 1),
                    PositionMode = InheritMode.XY,
                    Position = new Vector2((float)i / columns, 0),
                    Alpha = 0.5f,
                    Colour = Color4.Black
                });
        }
    }
}