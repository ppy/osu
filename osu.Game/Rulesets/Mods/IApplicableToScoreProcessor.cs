// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu-framework/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    public interface IApplicableToScoreProcessor
    {
        void ApplyToScoreProcessor(ScoreProcessor scoreProcessor);
    }
}
