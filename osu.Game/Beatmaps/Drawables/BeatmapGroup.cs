// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;

namespace osu.Game.Beatmaps.Drawables
{
    public class BeatmapGroup : IStateful<BeatmapGroupState>
    {
        public event Action<BeatmapGroupState> StateChanged;

        public BeatmapPanel SelectedPanel;

        /// <summary>
        /// Fires when one of our difficulties was selected. Will fire on first expand.
        /// </summary>
        public Action<BeatmapGroup, BeatmapPanel> SelectionChanged;

        /// <summary>
        /// Fires when one of our difficulties is clicked when already selected. Should start playing the map.
        /// </summary>
        public Action<BeatmapInfo> StartRequested;

        public Action<BeatmapSetInfo> DeleteRequested;

        public Action<BeatmapSetInfo> RestoreHiddenRequested;

        public Action<BeatmapInfo> HideDifficultyRequested;

        public Action<BeatmapInfo> EditRequested;

        public BeatmapSetHeader Header;

        public List<BeatmapPanel> BeatmapPanels;

        public BeatmapSetInfo BeatmapSet;

        private BeatmapGroupState state;
        public BeatmapGroupState State
        {
            get { return state; }
            set
            {
                state = value;

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

                StateChanged?.Invoke(state);
            }
        }

        public BeatmapGroup(BeatmapSetInfo beatmapSet, BeatmapManager manager)
        {
            if (beatmapSet == null)
                throw new ArgumentNullException(nameof(beatmapSet));
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            BeatmapSet = beatmapSet;
            WorkingBeatmap beatmap = manager.GetWorkingBeatmap(BeatmapSet.Beatmaps.FirstOrDefault());

            Header = new BeatmapSetHeader(beatmap)
            {
                GainedSelection = headerGainedSelection,
                DeleteRequested = b => DeleteRequested(b),
                RestoreHiddenRequested = b => RestoreHiddenRequested(b),
                RelativeSizeAxes = Axes.X,
            };

            BeatmapPanels = BeatmapSet.Beatmaps.Where(b => !b.Hidden).OrderBy(b => b.StarDifficulty).Select(b => new BeatmapPanel(b)
            {
                Alpha = 0,
                GainedSelection = panelGainedSelection,
                HideRequested = p => HideDifficultyRequested?.Invoke(p),
                StartRequested = p => StartRequested?.Invoke(p.Beatmap),
                EditRequested = p => EditRequested?.Invoke(p.Beatmap),
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
                SelectionChanged?.Invoke(this, SelectedPanel);
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
