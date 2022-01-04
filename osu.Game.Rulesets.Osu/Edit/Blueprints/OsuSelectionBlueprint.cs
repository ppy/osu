// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Osu.Objects;
using osu.Game.Rulesets.Osu.Objects.Drawables;

namespace osu.Game.Rulesets.Osu.Edit.Blueprints
{
    public abstract class OsuSelectionBlueprint<T> : HitObjectSelectionBlueprint<T>
        where T : OsuHitObject
    {
        protected new DrawableOsuHitObject DrawableObject => (DrawableOsuHitObject)base.DrawableObject;

        protected override bool AlwaysShowWhenSelected => true;

        protected OsuSelectionBlueprint(T hitObject)
            : base(hitObject)
        {
        }
    }
}
