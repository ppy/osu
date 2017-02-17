// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE


namespace osu.Game.Modes.Objects
{
    /// <summary>
    /// Returns null HitObjects but at least allows us to run.
    /// </summary>
    public class NullHitObjectParser : HitObjectParser
    {
        public override HitObject Parse(string text) => null;
    }
}
