// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Screens.Edit.Verify
{
    internal class VisibilitySection : IssueSection
    {
        private OsuCheckbox checkboxNegligible;

        protected override string SectionName => "Visibility";

        public VisibilitySection(IssueList issueList)
            : base(issueList)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Flow.AddRange(new Drawable[]
            {
                checkboxNegligible = new OsuCheckbox
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    LabelText = "Negligible"
                }
            });

            checkboxNegligible.Current.BindTo(IssueList.ShowNegligible);
        }
    }
}
