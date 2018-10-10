// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModIndecisive : OsuModHidden
    {
        public override string Name => "Indecisive";
        public override string ShortenedName => "ID";
        public override FontAwesome Icon => FontAwesome.fa_question_circle;
        public override ModType Type => ModType.Fun;
        public override string Description => "Nomod-Hidden alternation.";
        public override double ScoreMultiplier => 1;

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            base.ApplyToDrawableHitObjects(drawables.Where((x, i) => i % 2 == 1));

            // Hidden mod checks if first element should be skipped, this mod doesn't care
            if (IncreaseFirstObjectVisibility)
                drawables.Skip(1).First().ApplyCustomUpdateState += ApplyHiddenState;
        }
    }
}
