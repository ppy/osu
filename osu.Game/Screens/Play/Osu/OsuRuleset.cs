//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Beatmaps.Objects;

namespace osu.Game.Screens.Play.Osu
{
    class OsuRuleset : Ruleset
    {
        public override ScoreOverlay CreateScoreOverlay() => new ScoreOverlayOsu();

        public override HitRenderer CreateHitRendererWith(List<HitObject> objects) => new OsuHitRenderer { Objects = objects };
    }}
