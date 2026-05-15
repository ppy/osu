// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Edit.Checks;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Osu.Objects;

namespace osu.Game.Rulesets.Osu.Edit.Checks
{
    public class CheckOsuFewHitsounds : CheckFewHitsounds
    {
        protected override bool IsExcludedFromHitsounding(HitObject hitObject) => hitObject is Spinner;
    }
}
