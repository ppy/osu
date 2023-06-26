// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Edit.Blueprints
{
    public partial class SwellPlacementBlueprint : TaikoSpanPlacementBlueprint
    {
        public SwellPlacementBlueprint()
            : base(new Swell())
        {
        }
    }
}
