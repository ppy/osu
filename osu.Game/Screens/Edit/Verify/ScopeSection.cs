// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Verify
{
    internal partial class ScopeSection : EditorRoundedScreenSettingsSection
    {
        protected override string HeaderText => "Scope";

        [BackgroundDependencyLoader]
        private void load(VerifyScreen verify)
        {
            Flow.Add(new SettingsEnumDropdown<CheckScope>
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                TooltipText = "Select which type of checks to display",
                Current = verify.VerifyChecksScope.GetBoundCopy()
            });
        }
    }
}
