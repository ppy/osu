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

        private readonly IssueType[] configurableIssueTypes =
        {
            IssueType.Warning,
            IssueType.Error,
            IssueType.Negligible
        };

        protected override string Header => "Visibility";

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            foreach (IssueType issueType in configurableIssueTypes)
            {
                var checkbox = new SettingsCheckbox
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    LabelText = issueType.ToString()
                };

                checkbox.Current.Default = !verify.HiddenIssueTypes.Contains(issueType);
                checkbox.Current.SetDefault();
                checkbox.Current.BindValueChanged(state =>
                {
                    if (!state.NewValue)
                        verify.HiddenIssueTypes.Add(issueType);
                    else
                        verify.HiddenIssueTypes.Remove(issueType);
                });

                Flow.Add(checkbox);
            }
        }
    }
}
