// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Objects
{
    public abstract class HitObjectParser
    {
        public abstract HitObject Parse(string text);
    }
}
