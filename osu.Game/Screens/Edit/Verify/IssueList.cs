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
    [Cached]
    public class IssueList : CompositeDrawable
    {
        private IssueTable table;

        [Resolved]
        private IBindable<WorkingBeatmap> workingBeatmap { get; set; }

        [Resolved]
        private EditorBeatmap beatmap { get; set; }

        [Resolved]
        private VerifyScreen verify { get; set; }

        private IBeatmapVerifier rulesetVerifier;
        private BeatmapVerifier generalVerifier;
        private BeatmapVerifierContext context;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colours)
        {
            generalVerifier = new BeatmapVerifier();
            rulesetVerifier = beatmap.BeatmapInfo.Ruleset.CreateInstance().CreateBeatmapVerifier();

            context = new BeatmapVerifierContext(beatmap, workingBeatmap.Value, verify.InterpretedDifficulty.Value);
            verify.InterpretedDifficulty.BindValueChanged(difficulty => context.InterpretedDifficulty = difficulty.NewValue);

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

            verify.InterpretedDifficulty.BindValueChanged(_ => refresh());
            verify.HiddenIssueTypes.BindCollectionChanged((_, __) => refresh());

            refresh();
        }

        private void refresh()
        {
            var issues = generalVerifier.Run(context);

            if (rulesetVerifier != null)
                issues = issues.Concat(rulesetVerifier.Run(context));

            issues = filter(issues);

            table.Issues = issues
                           .OrderBy(issue => issue.Template.Type)
                           .ThenBy(issue => issue.Check.Metadata.Category);
        }

        private IEnumerable<Issue> filter(IEnumerable<Issue> issues)
        {
            return issues.Where(issue => !verify.HiddenIssueTypes.Contains(issue.Template.Type));
        }
    }
}
