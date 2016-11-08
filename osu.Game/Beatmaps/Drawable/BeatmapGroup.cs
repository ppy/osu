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
                        FadeTo(1, 250);

                        //if (!difficulties.Children.All(d => IsLoaded))
                        //    Task.WhenAll(difficulties.Children.Select(d => d.Preload(Game))).ContinueWith(t => difficulties.Show());
                        //else
                        difficulties.Show();

                        header.State = PanelSelectedState.Selected;
                        break;
                    case BeatmapGroupState.Collapsed:
                        FadeTo(0.8f, 250);

                        header.State = PanelSelectedState.NotSelected;
                        difficulties.Hide();
                        break;
                }
            }
        }

        public BeatmapGroup(BeatmapSetInfo beatmapSet, WorkingBeatmap working)
        {
            this.beatmapSet = beatmapSet;

            Alpha = 0;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;

            Children = new[]
            {
                new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FlowDirection.VerticalOnly,
                    Children = new Framework.Graphics.Drawable[]
                    {
                        header = new BeatmapSetHeader(beatmapSet, working)
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
                        }
                    }
                }
            };
        }

        [Initializer]
        private void Load(BaseGame game)
        {
            BeatmapPanels = beatmapSet.Beatmaps.Select(b => new BeatmapPanel(b)
            {
                GainedSelection = panelGainedSelection,
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                RelativeSizeAxes = Axes.X,
            }).ToList();

            //for the time being, let's completely load the difficulty panels in the background.
            //this likely won't scale so well, but allows us to completely async the loading flow.
            Task.WhenAll(BeatmapPanels.Select(panel => panel.Preload(game, p => difficulties.Add(panel)))).Wait();
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
