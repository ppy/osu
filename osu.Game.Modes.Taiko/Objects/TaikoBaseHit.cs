//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Modes.Objects;

namespace osu.Game.Modes.Taiko.Objects
{
    public class TaikoBaseHit : HitObject
    {
        public float Scale = 1;

        public TaikoColour Type;
    }

    public enum TaikoColour
    {
        Red,
        Blue
    }
}
