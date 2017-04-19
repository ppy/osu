// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Scoring
{
    public class ScoreStatistic
    {
        public readonly string Name;
        public readonly object Value;

        public ScoreStatistic(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
