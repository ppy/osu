//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Framework;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Input;
using osu.Game.Database;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Beatmaps.Drawable
{
    class BeatmapGroup : Container, IStateful<BeatmapGroup.GroupState>
    {
        public enum GroupState
        {
            Collapsed,
            Expanded,
        }

        private const float collapsedAlpha = 0.5f;
        private const float collapsedWidth = 0.8f;

        private BeatmapPanel selectedPanel;

        /// <summary>
        /// Fires when one of our difficulties was selected. Will fire on first expand.
        /// </summary>
        public Action<BeatmapGroup, BeatmapInfo> SelectionChanged;

        public BeatmapSetInfo BeatmapSet;
        private BeatmapSetHeader header;
        private FlowContainer difficulties;
        private GroupState state;
        public GroupState State
        {
            get { return state; }
            set
            {
                state = value;
                switch (state)
                {
                    case GroupState.Expanded:
                        FadeTo(1, 250);
                        difficulties.Show();

                        //todo: header should probably have a state, with this logic moved inside it.
                        header.Width = 1;
                        header.GlowRadius = 5;
                        header.BorderColour = new Color4(header.BorderColour.R, header.BorderColour.G, header.BorderColour.B, 255);

                        if (selectedPanel == null)
                            (difficulties.Children.FirstOrDefault() as BeatmapPanel).Selected = true;
                        SelectionChanged?.Invoke(this, selectedPanel?.Beatmap);
                        break;
                    case GroupState.Collapsed:
                        FadeTo(collapsedAlpha, 250);
                        difficulties.Hide();

                        //todo: header should probably have a state, with this logic moved inside it.
                        header.Width = collapsedWidth;
                        header.GlowRadius = 0;
                        header.BorderColour = new Color4(header.BorderColour.R, header.BorderColour.G, header.BorderColour.B, 0);
                        break;
                }
            }
        }

        public BeatmapGroup(BeatmapSetInfo beatmapSet)
        {
            BeatmapSet = beatmapSet;
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
                        header = new BeatmapSetHeader(beatmapSet)
                        {
                            RelativeSizeAxes = Axes.X,
                            Width = collapsedWidth,
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
                            Children = BeatmapSet.Beatmaps.Select(b =>
                                new BeatmapPanel(BeatmapSet, b)
                                {
                                    GainedSelection = panelGainedSelection,
                                    Anchor = Anchor.TopRight,
                                    Origin = Anchor.TopRight,
                                    RelativeSizeAxes = Axes.X,
                                }
                            )
                        }
                    }
                }
            };
        }

        public override void Load(BaseGame game)
        {
            base.Load(game);
            State = GroupState.Collapsed;
        }

        private void panelGainedSelection(BeatmapPanel panel)
        {
            if (selectedPanel != null) selectedPanel.Selected = false;
            selectedPanel = panel;
            
            SelectionChanged?.Invoke(this, panel.Beatmap);
        }

        protected override bool OnClick(InputState state)
        {
            State = GroupState.Expanded;
            return true;
        }
    }
}
