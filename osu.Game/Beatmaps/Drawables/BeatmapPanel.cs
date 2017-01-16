//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.MathUtils;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Backgrounds;
using osu.Game.Graphics.UserInterface;
using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Input;

namespace osu.Game.Beatmaps.Drawables
{
    class BeatmapPanel : Panel
    {
        public BeatmapInfo Beatmap;
        private Sprite background;

        public Action<BeatmapPanel> GainedSelection;
        public Action<BeatmapPanel> StartRequested;

        protected override void Selected()
        {
            base.Selected();
            GainedSelection?.Invoke(this);

            background.ColourInfo = ColourInfo.GradientVertical(
                new Color4(20, 43, 51, 255),
                new Color4(40, 86, 102, 255));
        }

        protected override void Deselected()
        {
            base.Deselected();

            background.Colour = new Color4(20, 43, 51, 255);
        }

        protected override bool OnClick(InputState state)
        {
            if (State == PanelSelectedState.Selected)
                StartRequested?.Invoke(this);

            return base.OnClick(state);
        }

        public BeatmapPanel(BeatmapInfo beatmap)
        {
            Beatmap = beatmap;
            Height *= 0.60f;

            Children = new Drawable[]
            {
                background = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Triangles
                {
                    // The border is drawn in the shader of the children. Being additive, triangles would over-emphasize
                    // the border wherever they cross it, and thus they get their own masking container without a border.
                    Masking = true,
                    CornerRadius = Content.CornerRadius,
                    RelativeSizeAxes = Axes.Both,
                    BlendingMode = BlendingMode.Additive,
                    Colour = new Color4(20, 43, 51, 255),
                },
                new FlowContainer
                {
                    Padding = new MarginPadding(5),
                    Direction = FlowDirection.HorizontalOnly,
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Children = new Drawable[]
                    {
                        new DifficultyIcon(FontAwesome.fa_dot_circle_o, new Color4(159, 198, 0, 255))
                        {
                            Scale = new Vector2(1.8f),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                        },
                        new FlowContainer
                        {
                            Padding = new MarginPadding { Left = 5 },
                            Spacing = new Vector2(0, 5),
                            Direction = FlowDirection.VerticalOnly,
                            AutoSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                new FlowContainer
                                {
                                    Direction = FlowDirection.HorizontalOnly,
                                    AutoSizeAxes = Axes.Both,
                                    Spacing = new Vector2(4, 0),
                                    Children = new[]
                                    {
                                        new SpriteText
                                        {
                                            Font = @"Exo2.0-Medium",
                                            Text = beatmap.Version,
                                            TextSize = 20,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                        new SpriteText
                                        {
                                            Font = @"Exo2.0-Medium",
                                            Text = "mapped by",
                                            TextSize = 16,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                        new SpriteText
                                        {
                                            Font = @"Exo2.0-MediumItalic",
                                            Text = $"{(beatmap.Metadata ?? beatmap.BeatmapSet.Metadata).Author}",
                                            TextSize = 16,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft
                                        },
                                    }
                                },
                                new StarCounter { Count = beatmap.BaseDifficulty?.OverallDifficulty ?? 5, StarSize = 8 }
                            }
                        }
                    }
                }
            };
        }
    }
}
