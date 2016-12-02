//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Mania.UI;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Mania
{
    public class ManiaRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new OsuScoreOverlay();

        public override HitRenderer CreateHitRendererWith(List<HitObject> objects) => new ManiaHitRenderer { Objects = objects };

        protected override PlayMode PlayMode => PlayMode.Mania;

        public override ScoreProcessor CreateScoreProcessor() => null;

        public override HitObjectParser CreateHitObjectParser() => new OsuHitObjectParser();
    }
}
