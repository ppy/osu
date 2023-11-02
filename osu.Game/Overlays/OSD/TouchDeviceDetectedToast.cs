// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets;

namespace osu.Game.Overlays.OSD
{
    public partial class TouchDeviceDetectedToast : Toast
    {
        public TouchDeviceDetectedToast(RulesetInfo ruleset)
            : base(ruleset.Name, "Touch device detected", "Touch Device mod applied to score")
        {
        }
    }
}
