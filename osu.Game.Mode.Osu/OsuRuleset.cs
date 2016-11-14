//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Osu
{
    public class OsuRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new OsuScoreOverlay();

        public override HitRenderer CreateHitRendererWith(List<HitObject> objects) => new UI.OsuHitRenderer { Objects = objects };
    }
}
