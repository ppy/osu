// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    public class OsuSelectionBlueprint : SelectionBlueprint
    {
        protected OsuHitObject OsuObject => (OsuHitObject)HitObject.HitObject;

        public OsuSelectionBlueprint(DrawableHitObject hitObject)
            : base(hitObject)
        {
        }
    }
}
