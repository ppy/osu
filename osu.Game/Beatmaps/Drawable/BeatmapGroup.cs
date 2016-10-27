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
using osu.Game.Graphics;
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
        
        private BeatmapInfo selectedBeatmap;
        public BeatmapInfo SelectedBeatmap
        {
            get { return selectedBeatmap; }
            set
            {
                selectedBeatmap = value;
            }
        }

        public Action<BeatmapGroup, BeatmapInfo> BeatmapSelected;
        public BeatmapSetInfo BeatmapSet;
        private BeatmapSetHeader header;
        private FlowContainer difficulties;
        private bool collapsed;
        public GroupState State
        {
            get { return collapsed ? GroupState.Collapsed : GroupState.Expanded; }
            set
            {
                bool val = value == GroupState.Collapsed;
                if (collapsed == val)
                    return;
                collapsed = val;
                ClearTransformations();
                const float uncollapsedAlpha = 1;
                FadeTo(collapsed ? collapsedAlpha : uncollapsedAlpha, 250);
                if (collapsed)
                    difficulties.Hide();
                else
                    difficulties.Show();
                header.ClearTransformations();
                header.Width = collapsed ? collapsedWidth : 1; // TODO: Transform
                header.BorderColour = new Color4(
                    header.BorderColour.R,
                    header.BorderColour.G,
                    header.BorderColour.B,
                    collapsed ? 0 : 255);
                header.GlowRadius = collapsed ? 0 : 5;
            }
        }

        public BeatmapGroup(BeatmapSetInfo beatmapSet)
        {
            BeatmapSet = beatmapSet;
            selectedBeatmap = beatmapSet.Beatmaps[0];
            Alpha = collapsedAlpha;
            AutoSizeAxes = Axes.Y;
            RelativeSizeAxes = Axes.X;
            float difficultyWidth = 1;
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
                            Children = BeatmapSet.Beatmaps.Select(
                                b => {
                                    float width = difficultyWidth;
                                    if (difficultyWidth > 0.8f) difficultyWidth -= 0.025f;
                                    return new BeatmapPanel(BeatmapSet, b)
                                    {
                                        MapSelected = updateSelected,
                                        Selected = width == 1,
                                        Anchor = Anchor.TopRight,
                                        Origin = Anchor.TopRight,
                                        RelativeSizeAxes = Axes.X,
                                        Width = width,
                                    };
                                })
                        }
                    }
                }
            };
            
            collapsed = true;
        }
        
        private void updateSelected(BeatmapInfo map)
        {
            foreach (BeatmapPanel panel in difficulties.Children)
                panel.Selected = panel.Beatmap == map;
            BeatmapSelected?.Invoke(this, map);
        }
        
        protected override bool OnClick(InputState state)
        {
            BeatmapSelected?.Invoke(this, selectedBeatmap);
            return true;
        }
    }
}
