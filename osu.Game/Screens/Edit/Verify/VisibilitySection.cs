// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Verify
{
    internal class VisibilitySection : EditorRoundedScreenSettingsSection
    {
        [Resolved]
        private VerifyScreen verify { get; set; }

        protected override string Header => "Visibility";

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            foreach (IssueType issueType in verify.IssueList.ShowType.Keys)
            {
                var checkbox = new SettingsCheckbox
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    LabelText = issueType.ToString()
                };

                checkbox.Current.BindTo(verify.IssueList.ShowType[issueType]);
                checkbox.Current.BindValueChanged(_ => verify.IssueList.Refresh());
                Flow.Add(checkbox);
            }
        }
    }
}
