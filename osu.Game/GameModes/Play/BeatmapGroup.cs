//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Beatmaps;
using osu.Framework.Graphics.Primitives;
using OpenTK;
using System.Linq;
using osu.Framework.Graphics.Drawables;
using osu.Framework.Graphics.Transformations;
using osu.Framework.Input;
using OpenTK.Graphics;

namespace osu.Game.GameModes.Play
{
    class BeatmapGroup : AutoSizeContainer
    {
        private const float collapsedAlpha = 0.3f;

        public event Action<BeatmapSet> SetSelected;
        public event Action<BeatmapSet, Beatmap> BeatmapSelected;
        public BeatmapSet BeatmapSet;
        private FlowContainer topContainer;
        private FlowContainer difficulties;
        private bool collapsed;
        public bool Collapsed
        {
            get { return collapsed; }
            set
            {
                if (collapsed == value)
                    return;
                collapsed = value;
                this.ClearTransformations();
                const float uncollapsedAlpha = 1;
                Transforms.Add(new TransformAlpha(Clock)
                {
                    StartValue = collapsed ? uncollapsedAlpha : collapsedAlpha,
                    EndValue = collapsed ? collapsedAlpha : uncollapsedAlpha,
                    StartTime = Time,
                    EndTime = Time + 250,
                });
                if (collapsed)
                    topContainer.Remove(difficulties);
                else
                    topContainer.Add(difficulties);
            }
        }

        public BeatmapGroup(BeatmapSet beatmapSet)
        {
            BeatmapSet = beatmapSet;
            Alpha = collapsedAlpha;
            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, 0);
            Children = new[]
            {
                topContainer = new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(1, 0),
                    Direction = FlowDirection.VerticalOnly,
                    Children = new[] { new BeatmapSetBox(beatmapSet) }
                }
            };
            difficulties = new FlowContainer // Deliberately not added to children
            {
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(1, 0),
                Margin = new MarginPadding { Top = 5 },
                Padding = new MarginPadding { Left = 25 },
                Spacing = new Vector2(0, 5),
                Direction = FlowDirection.VerticalOnly,
                Children = this.BeatmapSet.Beatmaps.Select(b => new BeatmapButton(this.BeatmapSet, b))
            };
            collapsed = true;
        }
        
        protected override bool OnClick(InputState state)
        {
            SetSelected?.Invoke(BeatmapSet);
            return true;
        }
    }
    
    class BeatmapSetBox : AutoSizeContainer
    {
        private BeatmapSet beatmapSet;

        public BeatmapSetBox(BeatmapSet beatmapSet)
        {
            this.beatmapSet = beatmapSet;
            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, 0);
            Children = new Drawable[]
            {
                new Box
                {
                    Colour = new Color4(85, 85, 85, 255), // TODO: Gradient, and beatmap texture
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(1),
                },
                new FlowContainer
                {
                    Direction = FlowDirection.VerticalOnly,
                    Spacing = new Vector2(0, 2),
                    Padding = new MarginPadding { Top = 3, Left = 20, Right = 20, Bottom = 3 },
                    Children = new[]
                    {
                        // TODO: Make these italic
                        new SpriteText
                        {
                            Text = this.beatmapSet.Metadata.TitleUnicode ?? this.beatmapSet.Metadata.Title,
                            TextSize = 20
                        },
                        new SpriteText
                        {
                            Text = this.beatmapSet.Metadata.ArtistUnicode ?? this.beatmapSet.Metadata.Artist,
                            TextSize = 16
                        },
                        new FlowContainer
                        {
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
                    Size = new Vector2(size),
                    Colour = color,
                    Icon = icon
                }
            };
        }
    }
}
