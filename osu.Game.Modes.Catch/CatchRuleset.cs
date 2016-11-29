//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Modes.Catch.UI;
using osu.Game.Modes.Objects;
using osu.Game.Modes.Osu.Objects;
using osu.Game.Modes.Osu.UI;
using osu.Game.Modes.UI;

namespace osu.Game.Modes.Catch
{
    public class CatchRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new OsuScoreOverlay();

        public override HitRenderer CreateHitRendererWith(List<HitObject> objects) => new CatchHitRenderer { Objects = objects };

        protected override PlayMode PlayMode => PlayMode.Catch;

        public override ScoreProcessor CreateScoreProcessor() => null;

        public override HitObjectParser CreateHitObjectParser() => new OsuHitObjectParser();
    }
}
