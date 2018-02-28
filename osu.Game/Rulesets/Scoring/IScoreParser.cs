// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.IO;

namespace osu.Game.Rulesets.Scoring
{
    public interface IScoreParser
    {
        Score Parse(Stream stream);
    }
}
