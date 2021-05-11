// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit.Verify
{
    internal class InterpretationSection : Section
    {
        public InterpretationSection(IssueList issueList)
            : base(issueList)
        {
        }

        protected override string Header => "Interpretation";

        [BackgroundDependencyLoader]
        private void load()
        {
            var dropdown = new SettingsEnumDropdown<DifficultyRating>
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                TooltipText = "Affects checks that depend on difficulty level"
            };
            dropdown.Current.BindTo(IssueList.InterpretedDifficulty);

            Flow.Add(dropdown);
        }
    }
}
