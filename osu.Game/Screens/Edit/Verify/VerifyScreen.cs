// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Verify
{
    public class VerifyScreen : RoundedContentEditorScreen
    {
        [Cached]
        private Bindable<Issue> selectedIssue = new Bindable<Issue>();

        public VerifyScreen()
            : base(EditorScreenMode.Verify)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Child = new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(),
                        new Dimension(GridSizeMode.Absolute, 200),
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new IssueList(),
                            new IssueSettings(),
                        },
                    }
                }
            };
        }
    }
}
