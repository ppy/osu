// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;

namespace osu.Game.Rulesets.Mania.Edit.Checks
{
    public class CheckManiaConcurrentObjects : CheckConcurrentObjects
    {
        // Mania hitobjects are only considered concurrent if they also share the same column.
        protected override bool ConcurrentCondition(HitObject first, HitObject second) => (first as IHasColumn)?.Column != (second as IHasColumn)?.Column;
    }
}
