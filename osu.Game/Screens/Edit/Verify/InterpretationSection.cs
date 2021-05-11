// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Edit.Verify
{
    internal class InterpretationSection : CompositeDrawable
    {
        private readonly IssueList issueList;

        public InterpretationSection(IssueList issueList)
        {
            this.issueList = issueList;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            Padding = new MarginPadding(10);

            var dropdown = new SettingsEnumDropdown<DifficultyRating>
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
                LabelText = "Difficulty Interpretation",
                TooltipText = "Affects checks that depend on difficulty level"
            };

            dropdown.Current.BindTo(issueList.InterpretedDifficulty);
            InternalChildren = new Drawable[] { dropdown };
        }
    }
}
