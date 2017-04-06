// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Game.Database;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapGroup : IStateful<BeatmapGroupState>
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

        public BeatmapSetInfo BeatmapSet;

        public BeatmapGroupState State
        {
            get { return state; }
            set
            {
                switch (value)
                {
                    case BeatmapGroupState.Expanded:
                        Header.State = PanelSelectedState.Selected;
                        foreach (BeatmapPanel panel in BeatmapPanels)
                            panel.State = panel == SelectedPanel ? PanelSelectedState.Selected : PanelSelectedState.NotSelected;
                        break;
                    case BeatmapGroupState.Collapsed:
                        Header.State = PanelSelectedState.NotSelected;
                        foreach (BeatmapPanel panel in BeatmapPanels)
                            panel.State = PanelSelectedState.Hidden;
                        break;
                    case BeatmapGroupState.Hidden:
                        Header.State = PanelSelectedState.Hidden;
                        foreach (BeatmapPanel panel in BeatmapPanels)
                            panel.State = PanelSelectedState.Hidden;
                        break;
                }
                state = value;
            }
        }

        public BeatmapGroup(BeatmapSetInfo beatmapSet, BeatmapDatabase database)
        {
            BeatmapSet = beatmapSet;
            WorkingBeatmap beatmap = database.GetWorkingBeatmap(BeatmapSet.Beatmaps.FirstOrDefault());

            Header = new BeatmapSetHeader(beatmap)
            {
                GainedSelection = headerGainedSelection,
                RelativeSizeAxes = Axes.X,
            };

            BeatmapSet.Beatmaps = BeatmapSet.Beatmaps.OrderBy(b => b.StarDifficulty).ToList();
            BeatmapPanels = BeatmapSet.Beatmaps.Select(b => new BeatmapPanel(b)
            {
                Alpha = 0,
                GainedSelection = panelGainedSelection,
                StartRequested = p => { StartRequested?.Invoke(p.Beatmap); },
                RelativeSizeAxes = Axes.X,
            }).ToList();

            Header.AddDifficultyIcons(BeatmapPanels);
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
        Hidden,
    }
}
