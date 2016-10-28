//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Game.Database;
using OpenTK;

namespace osu.Game.Beatmaps.Drawable
{
    class BeatmapGroup : Container, IStateful<BeatmapGroupState>
    {
        public BeatmapPanel SelectedPanel;

        /// <summary>
        /// Fires when one of our difficulties was selected. Will fire on first expand.
        /// </summary>
        public Action<BeatmapGroup, BeatmapInfo> SelectionChanged;

        private BeatmapSetInfo beatmapSet;
        private BeatmapSetHeader header;
        private FlowContainer difficulties;

        private BeatmapGroupState state;

        public IEnumerable<BeatmapPanel> BeatmapPanels;

        public BeatmapGroupState State
        {
            get { return state; }
            set
            {
                state = value;
                switch (state)
                {
                    case BeatmapGroupState.Expanded:
                        FadeTo(1, 250);
                        difficulties.Show();

                        header.State = PanelSelectedState.Selected;

                        if (SelectedPanel == null)
                            ((BeatmapPanel)difficulties.Children.FirstOrDefault()).State = PanelSelectedState.Selected;
                        break;
                    case BeatmapGroupState.Collapsed:
                        FadeTo(0.5f, 250);

                        header.State = PanelSelectedState.NotSelected;
                        difficulties.Hide();
                        break;
                }
            }
        }

        public BeatmapGroup(BeatmapSetInfo beatmapSet)
        {
            this.beatmapSet = beatmapSet;

            Alpha = 0;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            BeatmapPanels = beatmapSet.Beatmaps.Select(b =>
                new BeatmapPanel(this.beatmapSet, b)
                {
                    GainedSelection = panelGainedSelection,
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                });
                

            Children = new[]
            {
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FlowDirection.VerticalOnly,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        header = new BeatmapSetHeader(beatmapSet)
                        {
                            GainedSelection = headerGainedSelection,
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        },
                        difficulties = new FlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Top = 5 },
                            Padding = new MarginPadding { Left = 75 },
                            Spacing = new Vector2(0, 5),
                            Direction = FlowDirection.VerticalOnly,
                            Alpha = 0,
                            Children = BeatmapPanels
                        }
                    }
                }
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
            State = BeatmapGroupState.Collapsed;
        }

        private void headerGainedSelection(BeatmapSetHeader panel)
        {
            State = BeatmapGroupState.Expanded;

            SelectionChanged?.Invoke(this, SelectedPanel.Beatmap);
        }

        private void panelGainedSelection(BeatmapPanel panel)
        {
            State = BeatmapGroupState.Expanded;

            if (SelectedPanel != null) SelectedPanel.State = PanelSelectedState.NotSelected;
            SelectedPanel = panel;

            SelectionChanged?.Invoke(this, panel.Beatmap);
        }
    }

    public enum BeatmapGroupState
    {
        Collapsed,
        Expanded,
    }
}
