// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Replays.Legacy;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Replays.Types
{
    public interface IConvertibleReplayFrame
    {
        void ConvertFrom(LegacyReplayFrame legacyFrame, Score score, Beatmap beatmap);
    }
}
