// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that make general adjustments to score processor.
    /// </summary>
    public interface IApplicableToScoreProcessor
    {
        void ApplyToScoreProcessor(ScoreProcessor scoreProcessor);
    }
}
