// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Containers;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public class ScorePanelList : CompositeDrawable
    {
        /// <summary>
        /// Normal spacing between all panels.
        /// </summary>
        private const float panel_spacing = 5;

        /// <summary>
        /// Spacing around both sides of the expanded panel. This is added on top of <see cref="panel_spacing"/>.
        /// </summary>
        private const float expanded_panel_spacing = 15;

        private readonly Flow flow;
        private readonly ScrollContainer<Drawable> scroll;

        private ScorePanel expandedPanel;

        public ScorePanelList(ScoreInfo initialScore)
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = scroll = new OsuScrollContainer(Direction.Horizontal)
            {
                RelativeSizeAxes = Axes.Both,
                Child = flow = new Flow
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(panel_spacing, 0),
                    AutoSizeAxes = Axes.Both,
                }
            };

            AddScore(initialScore);
            ShowScore(initialScore);
        }

        public void AddScore(ScoreInfo score)
        {
            flow.Add(new ScorePanel(score)
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            }.With(p =>
            {
                p.StateChanged += s =>
                {
                    if (s == PanelState.Expanded)
                        ShowScore(score);
                };
            }));
        }

        public void ShowScore(ScoreInfo score)
        {
            foreach (var p in flow.Where(p => p.Score != score))
                p.State = PanelState.Contracted;

            if (expandedPanel != null)
                expandedPanel.Margin = new MarginPadding(0);

            expandedPanel = flow.Single(p => p.Score == score);
            expandedPanel.State = PanelState.Expanded;
            expandedPanel.Margin = new MarginPadding { Horizontal = expanded_panel_spacing };

            float scrollOffset = flow.IndexOf(expandedPanel) * (ScorePanel.CONTRACTED_WIDTH + panel_spacing);
            scroll.ScrollTo(scrollOffset);
        }

        protected override void Update()
        {
            base.Update();

            flow.Padding = new MarginPadding { Horizontal = DrawWidth / 2f - expandedPanel.DrawWidth / 2f - expanded_panel_spacing };
        }

        private class Flow : FillFlowContainer<ScorePanel>
        {
            public override IEnumerable<Drawable> FlowingChildren => AliveInternalChildren.OfType<ScorePanel>().OrderByDescending(s => s.Score.TotalScore).ThenByDescending(s => s.Score.OnlineScoreID);
        }
    }
}
