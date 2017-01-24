//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Transformations;
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

        /// <summary>
        /// Fires when one of our difficulties is clicked when already selected. Should start playing the map.
        /// </summary>
        public Action<BeatmapInfo> StartRequested;

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
                        foreach (BeatmapPanel panel in BeatmapPanels)
                            panel.FadeIn(250);

                        Header.State = PanelSelectedState.Selected;
                        if (SelectedPanel != null)
                            SelectedPanel.State = PanelSelectedState.Selected;
                        break;
                    case BeatmapGroupState.Collapsed:
                        Header.State = PanelSelectedState.NotSelected;
                        if (SelectedPanel != null)
                            SelectedPanel.State = PanelSelectedState.NotSelected;

                        foreach (BeatmapPanel panel in BeatmapPanels)
                            panel.FadeOut(300, EasingTypes.OutQuint);
                        break;
                }
            }
        }

        public BeatmapGroup(WorkingBeatmap beatmap, BeatmapSetInfo set = null)
        {
            Header = new BeatmapSetHeader(beatmap)
            {
                GainedSelection = headerGainedSelection,
                RelativeSizeAxes = Axes.X,
            };

            BeatmapPanels = beatmap.BeatmapSetInfo.Beatmaps.Select(b => new BeatmapPanel(b)
            {
                Alpha = 0,
                GainedSelection = panelGainedSelection,
                StartRequested = p => { StartRequested?.Invoke(p.Beatmap); },
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
