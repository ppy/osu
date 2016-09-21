//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Containers;

namespace osu.Game.GameModes.Play
{
    public class Playfield : Container
    {
        public override void Load()
        {
            base.Load();

            Masking = true;
        }
    }
}
