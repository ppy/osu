//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps.Objects;
using osu.Game.GameModes.Play.Catch;
using osu.Game.GameModes.Play.Mania;
using osu.Game.GameModes.Play.Osu;
using osu.Game.GameModes.Play.Taiko;

namespace osu.Game.GameModes.Play
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
