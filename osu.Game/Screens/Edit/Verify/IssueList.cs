// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Checks.Components;
using osuTK;

namespace osu.Game.Screens.Edit.Verify
{
    public class IssueList : CompositeDrawable
    {
        private IssueTable table;

        [Resolved]
        private EditorClock clock { get; set; }

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private Bindable<Issue> selectedIssue { get; set; }

        private IBeatmapVerifier rulesetVerifier;
        private BeatmapVerifier generalVerifier;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            ShowNegligible = new Bindable<bool>();

            generalVerifier = new BeatmapVerifier();
            rulesetVerifier = beatmap.BeatmapInfo.Ruleset?.CreateInstance()?.CreateBeatmapVerifier();

            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Colour = colours.Background2,
                    RelativeSizeAxes = Axes.Both,
                },
                new OsuScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Child = table = new IssueTable(),
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    Margin = new MarginPadding(20),
                    Children = new Drawable[]
                    {
                        new TriangleButton
                        {
                            Text = "Refresh",
                            Action = refresh,
                            Size = new Vector2(120, 40),
                            Anchor = Anchor.BottomRight,
                            Origin = Anchor.BottomRight,
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            refresh();
        }

        private void refresh()
        {
            var issues = generalVerifier.Run(beatmap, workingBeatmap.Value);

            if (rulesetVerifier != null)
                issues = issues.Concat(rulesetVerifier.Run(beatmap, workingBeatmap.Value));

            table.Issues = issues
                           .OrderBy(issue => issue.Template.Type)
                           .ThenBy(issue => issue.Check.Metadata.Category);
        }
}
