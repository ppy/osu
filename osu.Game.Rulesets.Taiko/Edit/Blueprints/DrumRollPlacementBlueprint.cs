// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Edit.Blueprints
{
    public class DrumRollPlacementBlueprint : TaikoSpanPlacementBlueprint
    {
        public DrumRollPlacementBlueprint()
            : base(new DrumRoll())
        {
        }
    }
}
