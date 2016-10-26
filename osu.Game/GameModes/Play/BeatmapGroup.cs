//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Framework.Graphics.Primitives;
using OpenTK;
using System.Linq;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK.Graphics;
using osu.Game.Beatmaps.IO;
using osu.Framework.Graphics.Textures;
using System.Threading.Tasks;
using osu.Framework;

namespace osu.Game.GameModes.Play
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
        private BeatmapSetHeader setBox;
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
                setBox.ClearTransformations();
                setBox.Width = collapsed ? collapsedWidth : 1; // TODO: Transform
                setBox.BorderColour = new Color4(
                    setBox.BorderColour.R,
                    setBox.BorderColour.G,
                    setBox.BorderColour.B,
                    collapsed ? 0 : 255);
                setBox.GlowRadius = collapsed ? 0 : 5;
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
                    Children = new Drawable[]
                    {
                        setBox = new BeatmapSetHeader(beatmapSet)
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
    
    class BeatmapSetHeader : Container
    {
        public BeatmapSetHeader(BeatmapSetInfo beatmapSet)
        {
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 5;
            BorderThickness = 2;
            BorderColour = new Color4(221, 255, 255, 0);
            GlowColour = new Color4(166, 221, 251, 0.5f); // TODO: Get actual color for this
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(85, 85, 85, 255),
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Size = Vector2.One,
                    Children = new Drawable[]
                    {
                        new Box // TODO: Gradient
                        {
                            Colour = new Color4(0, 0, 0, 100),
                            RelativeSizeAxes = Axes.Both,
                            Size = Vector2.One,
                        }
                    }
                },
                new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    Spacing = new Vector2(0, 2),
                    Padding = new MarginPadding { Top = 3, Left = 20, Right = 20, Bottom = 3 },
                    AutoSizeAxes = Axes.Both,
                    Children = new[]
                    {
                        // TODO: Make these italic
                        new SpriteText
                        {
                            Text = beatmapSet.Metadata.Title ?? beatmapSet.Metadata.TitleUnicode,
                            TextSize = 20
                        },
                        new SpriteText
                        {
                            Text = beatmapSet.Metadata.Artist ?? beatmapSet.Metadata.ArtistUnicode,
                            TextSize = 16
                        },
                        new FlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Children = new[]
                            {
                                new DifficultyIcon(FontAwesome.dot_circle_o, new Color4(159, 198, 0, 255)),
                                new DifficultyIcon(FontAwesome.dot_circle_o, new Color4(246, 101, 166, 255)),
                            }
                        }
                    }
                }
            };
        }
    }
    
    class DifficultyIcon : Container
    {
        public DifficultyIcon(FontAwesome icon, Color4 color)
        {
            const float size = 20;
            Size = new Vector2(size);
            Children = new[]
            {
                new TextAwesome
                {
                    Anchor = Anchor.Centre,
                    TextSize = size,
                    Colour = color,
                    Icon = icon
                }
            };
        }
    }
}
