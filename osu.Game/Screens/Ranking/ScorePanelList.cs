// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Utils;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Ranking
{
    public class ScorePanelList : CompositeDrawable
    {
        private readonly Flow panels;
        private ScorePanel expandedPanel;

        public ScorePanelList()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChild = panels = new Flow
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Custom,
                Direction = FillDirection.Horizontal,
                AutoSizeAxes = Axes.Both,
            };
        }

        public void AddScore(ScoreInfo score)
        {
            var panel = new ScorePanel(score)
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.CentreLeft,
            };

            panel.StateChanged += s => onPanelStateChanged(panel, s);

            // Todo: Temporary
            panel.State = expandedPanel == null ? PanelState.Expanded : PanelState.Contracted;

            panels.Add(panel);
        }

        public void RemoveScore(ScoreInfo score) => panels.RemoveAll(p => p.Score == score);

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (expandedPanel != null)
            {
                var firstPanel = panels.FlowingChildren.First();
                var target = expandedPanel.DrawPosition.X - firstPanel.DrawPosition.X + expandedPanel.DrawSize.X / 2;

                panels.OriginPosition = new Vector2((float)Interpolation.Lerp(panels.OriginPosition.X, target, Math.Clamp(Math.Abs(Time.Elapsed) / 80, 0, 1)), panels.DrawHeight / 2);
            }
        }

        private void onPanelStateChanged(ScorePanel panel, PanelState state)
        {
            if (state == PanelState.Contracted)
                return;

            if (expandedPanel != null)
                expandedPanel.State = PanelState.Contracted;

            expandedPanel = panel;
        }

        private class Flow : FillFlowContainer<ScorePanel>
        {
            // Todo: Order is wrong.
            public override IEnumerable<Drawable> FlowingChildren => AliveInternalChildren.OfType<ScorePanel>().OrderBy(s => s.Score.TotalScore);
        }
    }
}
