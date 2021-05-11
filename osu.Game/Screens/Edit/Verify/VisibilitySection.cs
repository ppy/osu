// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Verify
{
    internal class VisibilitySection : IssueSection
    {
        protected override string SectionName => "Visibility";

        public VisibilitySection(IssueList issueList)
            : base(issueList)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            var checkboxes = new List<SettingsCheckbox>();

            foreach (IssueType issueType in IssueList.ShowType.Keys)
            {
                var checkbox = new SettingsCheckbox
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    LabelText = issueType.ToString()
                };

                checkbox.Current.BindTo(IssueList.ShowType[issueType]);
                checkboxes.Add(checkbox);
            }

            Flow.AddRange(checkboxes);
        }
    }
}
