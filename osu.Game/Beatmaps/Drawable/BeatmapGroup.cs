//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Database;
using OpenTK;
using osu.Framework.Allocation;

namespace osu.Game.Beatmaps.Drawable
{
    class BeatmapGroup : IStateful<BeatmapGroupState>
    {
        public BeatmapPanel SelectedPanel;

        /// <summary>
        /// Fires when one of our difficulties was selected. Will fire on first expand.
        /// </summary>
        public Action<BeatmapGroup, BeatmapInfo> SelectionChanged;

        private BeatmapSetInfo beatmapSet;
        public BeatmapSetHeader Header;

        private BeatmapGroupState state;

        public List<BeatmapPanel> BeatmapPanels;

        public BeatmapGroupState State
        {
            get { return state; }
            set
            {
                state = value;
                switch (state)
                {
                    case BeatmapGroupState.Expanded:
                        //if (!difficulties.Children.All(d => IsLoaded))
                        //    Task.WhenAll(difficulties.Children.Select(d => d.Preload(Game))).ContinueWith(t => difficulties.Show());
                        //else
                        foreach (BeatmapPanel panel in BeatmapPanels)
                            panel.Show();

                        Header.State = PanelSelectedState.Selected;
                        if (SelectedPanel != null)
                            SelectedPanel.State = PanelSelectedState.Selected;
                        break;
                    case BeatmapGroupState.Collapsed:
                        Header.State = PanelSelectedState.NotSelected;
                        if (SelectedPanel != null)
                            SelectedPanel.State = PanelSelectedState.NotSelected;

                        foreach (BeatmapPanel panel in BeatmapPanels)
                            panel.Hide();

                        break;
                }
            }
        }

        public BeatmapGroup(BeatmapSetInfo beatmapSet, WorkingBeatmap working)
        {
            this.beatmapSet = beatmapSet;

            Header = new BeatmapSetHeader(beatmapSet, working)
            {
                GainedSelection = headerGainedSelection,
                RelativeSizeAxes = Axes.X,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
            };

            BeatmapPanels = beatmapSet.Beatmaps.Select(b => new BeatmapPanel(b)
            {
                GainedSelection = panelGainedSelection,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                RelativeSizeAxes = Axes.X,
            }).ToList();
        }

        private void headerGainedSelection(BeatmapSetHeader panel)
        {
            State = BeatmapGroupState.Expanded;

            //we want to make sure one of our children is selected in the case none have been selected yet.
            if (SelectedPanel == null)
                BeatmapPanels.First().State = PanelSelectedState.Selected;
            else
                SelectionChanged?.Invoke(this, SelectedPanel.Beatmap);
        }

        private void panelGainedSelection(BeatmapPanel panel)
        {
            try
            {
                if (SelectedPanel == panel) return;

                if (SelectedPanel != null)
                    SelectedPanel.State = PanelSelectedState.NotSelected;
                SelectedPanel = panel;
            }
            finally
            {
                State = BeatmapGroupState.Expanded;
                SelectionChanged?.Invoke(this, panel.Beatmap);
            }
        }
    }

    public enum BeatmapGroupState
    {
        Collapsed,
        Expanded,
    }
}
