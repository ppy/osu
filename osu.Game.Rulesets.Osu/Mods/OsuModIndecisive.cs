// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Mods
{
    public class OsuModIndecisive : OsuModHidden
    {
        public override string Name => "Indecisive";
        public override string Acronym => "ID";
        public override IconUsage Icon => FontAwesome.Regular.QuestionCircle;
        public override ModType Type => ModType.DifficultyIncrease;
        public override string Description => "Nomod-Hidden alternation.";
        public override double ScoreMultiplier => 1;

        public override void ApplyToDrawableHitObjects(IEnumerable<DrawableHitObject> drawables)
        {
            base.ApplyToDrawableHitObjects(drawables.Where((x, i) => i % 2 == 1));

            // Hidden mod checks if first element should be skipped, this mod doesn't care
            if (IncreaseFirstObjectVisibility.Value)
                drawables.Skip(1).First().ApplyCustomUpdateState += ApplyHiddenState;
        }
    }
}
