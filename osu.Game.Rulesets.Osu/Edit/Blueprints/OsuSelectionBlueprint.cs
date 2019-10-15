// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    public abstract class OsuSelectionBlueprint<T> : SelectionBlueprint
        where T : OsuHitObject
    {
        protected new T HitObject => (T)base.HitObject.HitObject;

        protected OsuSelectionBlueprint(DrawableHitObject hitObject)
            : base(hitObject)
        {
        }
    }
}
