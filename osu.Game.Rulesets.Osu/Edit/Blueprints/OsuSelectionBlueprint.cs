// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
