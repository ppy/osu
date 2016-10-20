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

namespace osu.Game.GameModes.Play
{
    class BeatmapGroup : AutoSizeContainer
    {
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

        public event Action<BeatmapSetInfo> SetSelected;
        public event Action<BeatmapSetInfo, BeatmapInfo> BeatmapSelected;
        public BeatmapSetInfo BeatmapSet;
        private BeatmapSetBox setBox;
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
                setBox.ClearTransformations();
                setBox.Transforms.Add(new TransformSize(Clock)
                {
                    StartValue = new Vector2(collapsed ? 1 : collapsedWidth, -1),
                    EndValue = new Vector2(collapsed ? collapsedWidth : 1, -1),
                    StartTime = Time,
                    EndTime = Time + 200,
                });
                setBox.BorderColour = new Color4(
                    setBox.BorderColour.R,
                    setBox.BorderColour.G,
                    setBox.BorderColour.B,
                    collapsed ? 0 : 255);
                setBox.GlowRadius = collapsed ? 0 : 5;
            }
        }
        
        private void updateSelected(BeatmapInfo map)
        {
            int selected = BeatmapSet.Beatmaps.IndexOf(map);
            var buttons = difficulties.Children.ToList();
            for (int i = 0; i < buttons.Count; i++)
            {
                var button = buttons[i] as BeatmapButton;
                float targetWidth = 1 - Math.Abs((selected - i) * 0.025f);
                targetWidth = MathHelper.Clamp(targetWidth, 0.8f, 1);
                button.Transforms.Add(new TransformSize(Clock)
                {
                    StartValue = new Vector2(button.Size.X, -1),
                    EndValue = new Vector2(targetWidth, -1),
                    StartTime = Time,
                    EndTime = Time + 100,
                });
                button.Selected = selected == i;
            }
            BeatmapSelected?.Invoke(BeatmapSet, map);
        }

        public BeatmapGroup(BeatmapSetInfo beatmapSet, BeatmapResourceStore beatmapStore, TextureStore resources)
        {
            BeatmapSet = beatmapSet;
            selectedBeatmap = beatmapSet.Beatmaps[0];
            Alpha = collapsedAlpha;
            RelativeSizeAxes = Axes.X;
            Size = new Vector2(1, -1);
            float difficultyWidth = 1;
            Children = new[]
            {
                topContainer = new FlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Size = new Vector2(1, -1),
                    Direction = FlowDirection.VerticalOnly,
                    Children = new[]
                    {
                        setBox = new BeatmapSetBox(beatmapSet, beatmapStore, resources)
                        {
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(collapsedWidth, -1),
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                        }
                    }
                }
            };
            difficulties = new FlowContainer // Deliberately not added to children
            {
                RelativeSizeAxes = Axes.X,
                Size = new Vector2(1, -1),
                Margin = new MarginPadding { Top = 5 },
                Padding = new MarginPadding { Left = 75 },
                Spacing = new Vector2(0, 5),
                Direction = FlowDirection.VerticalOnly,
                Children = this.BeatmapSet.Beatmaps.Select(
                    b => {
                        float width = difficultyWidth;
                        if (difficultyWidth > 0.8f) difficultyWidth -= 0.025f;
                        return new BeatmapButton(this.BeatmapSet, b)
                        {
                            MapSelected = beatmap => updateSelected(beatmap),
                            Selected = width == 1,
                            Anchor = Anchor.TopRight,
                            Origin = Anchor.TopRight,
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(width, -1),
                        };
                    })
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
        private BeatmapSetInfo beatmapSet;
        private BeatmapResourceStore beatmapStore;
        private TextureStore resources;
        private Sprite backgroundImage;

        public BeatmapSetBox(BeatmapSetInfo beatmapSet, BeatmapResourceStore beatmapStore, TextureStore resources)
        {
            this.beatmapSet = beatmapSet;
            this.beatmapStore = beatmapStore;
            this.resources = resources;
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
                        backgroundImage = new Sprite
                        {
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(1, 0),
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
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
                    Children = new[]
                    {
                        // TODO: Make these italic
                        new SpriteText
                        {
                            Text = this.beatmapSet.Metadata.Title ?? this.beatmapSet.Metadata.TitleUnicode,
                            TextSize = 20
                        },
                        new SpriteText
                        {
                            Text = this.beatmapSet.Metadata.Artist ?? this.beatmapSet.Metadata.ArtistUnicode,
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
        
        public override void Load(Framework.BaseGame game)
        {
            base.Load(game);
            if (beatmapSet.Metadata.BackgroundFile != null)
            {
                Task.Factory.StartNew(() =>
                {
                    beatmapStore.AddBeatmap(beatmapSet);
                    var texture = resources.Get($@"{beatmapSet.BeatmapSetID}:{beatmapSet.Metadata.BackgroundFile}");
                    Scheduler.Add(() => backgroundImage.Texture = texture);
                });
            }
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
