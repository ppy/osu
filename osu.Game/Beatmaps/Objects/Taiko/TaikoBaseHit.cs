//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.Objects.Taiko
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
