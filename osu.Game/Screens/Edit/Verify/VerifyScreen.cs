// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Edit.Checks.Components;

namespace osu.Game.Screens.Edit.Verify
{
    [Cached]
    public class VerifyScreen : EditorRoundedScreen
    {
        public readonly Bindable<Issue> SelectedIssue = new Bindable<Issue>();

        public readonly Bindable<DifficultyRating> InterpretedDifficulty = new Bindable<DifficultyRating>();

        public readonly BindableList<IssueType> HiddenIssueTypes = new BindableList<IssueType> { IssueType.Negligible };

        public IssueList IssueList { get; private set; }

        public VerifyScreen()
            : base(EditorScreenMode.Verify)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InterpretedDifficulty.Default = BeatmapDifficultyCache.GetDifficultyRating(EditorBeatmap.BeatmapInfo.StarRating);
            InterpretedDifficulty.SetDefault();

            IssueList = new IssueList();
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
                            IssueList,
                            new IssueSettings(),
                        },
                    }
                }
            };
        }
    }
}
