//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Catch;
using osu.Game.Modes.Mania;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Taiko;

namespace osu.Game.Modes
{
    public abstract class Ruleset
    {
        public abstract ScoreOverlay CreateScoreOverlay();

        public abstract HitRenderer CreateHitRendererWith(List<HitObject> objects);

        public static Ruleset GetRuleset(PlayMode mode)
        {
            switch (mode)
            {
                default:
                    return new OsuRuleset();
                case PlayMode.Catch:
                    return new CatchRuleset();
                case PlayMode.Mania:
                    return new ManiaRuleset();
                case PlayMode.Taiko:
                    return new TaikoRuleset();
            }
        }
    }
}
