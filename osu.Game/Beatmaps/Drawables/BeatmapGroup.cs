//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Game.Database;

namespace osu.Game.Beatmaps.Drawables
{
    class BeatmapGroup : IStateful<BeatmapGroupState>
    {
        public BeatmapPanel SelectedPanel;

        /// <summary>
        /// Fires when one of our difficulties was selected. Will fire on first expand.
        /// </summary>
        public Action<BeatmapGroup, BeatmapInfo> SelectionChanged;

        public BeatmapSetHeader Header;

        private BeatmapGroupState state;

        public List<BeatmapPanel> BeatmapPanels;
        private WorkingBeatmap beatmap;

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
                        {
                            panel.Hidden = false;
                            panel.FadeIn(250);
                        }

                        Header.State = PanelSelectedState.Selected;
                        if (SelectedPanel != null)
                            SelectedPanel.State = PanelSelectedState.Selected;
                        break;
                    case BeatmapGroupState.Collapsed:
                        Header.State = PanelSelectedState.NotSelected;
                        if (SelectedPanel != null)
                            SelectedPanel.State = PanelSelectedState.NotSelected;

                        foreach (BeatmapPanel panel in BeatmapPanels)
                        {
                            panel.Hidden = true;
                            panel.FadeOut(250);
                        }

                        break;
                }
            }
        }

        public BeatmapGroup(WorkingBeatmap beatmap)
        {
            this.beatmap = beatmap;

            Header = new BeatmapSetHeader(beatmap)
            {
                GainedSelection = headerGainedSelection,
                RelativeSizeAxes = Axes.X,
            };

            BeatmapPanels = beatmap.BeatmapSetInfo.Beatmaps.Select(b => new BeatmapPanel(b)
            {
                GainedSelection = panelGainedSelection,
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
