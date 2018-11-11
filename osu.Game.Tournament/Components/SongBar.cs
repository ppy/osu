// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Menu;
using OpenTK;
using OpenTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class SongBar : CompositeDrawable
    {
        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                if (beatmap == value)
                    return;

                beatmap = value;
                update();
            }
        }

        private LegacyMods mods;

        public LegacyMods Mods
        {
            get => mods;
            set
            {
                mods = value;
                update();
            }
        }

        private Container panelContents;
        private Container innerPanel;
        private Container outerPanel;

        private const float main_width = 0.97f;

        public bool Expanded
        {
            set
            {
                if (value)
                {
                    innerPanel.ResizeWidthTo(0.7f, 800, Easing.OutQuint);
                    outerPanel.ResizeWidthTo(main_width, 800, Easing.OutQuint);
                }
                else
                {
                    innerPanel.ResizeWidthTo(1, 800, Easing.OutQuint);
                    outerPanel.ResizeWidthTo(0.3f, 800, Easing.OutQuint);
                }
            }
        }


        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;

            InternalChildren = new Drawable[]
            {
                outerPanel = new Container
                {
                    Masking = true,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Colour = Color4.Black.Opacity(0.2f),
                        Type = EdgeEffectType.Shadow,
                        Radius = 5,
                    },
                    RelativeSizeAxes = Axes.X,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                    RelativePositionAxes = Axes.X,
                    X = -(1 - main_width) / 2,
                    Y = -10,
                    Width = 0.95f,
                    Height = TournamentBeatmapPanel.HEIGHT,
                    CornerRadius = TournamentBeatmapPanel.HEIGHT / 2,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.Gray(0.93f),
                        },
                        innerPanel = new Container
                        {
                            Masking = true,
                            CornerRadius = TournamentBeatmapPanel.HEIGHT / 2,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.7f,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = OsuColour.Gray(0.86f),
                                },
                                panelContents = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                }
                            }
                        },
                        new OsuLogo
                        {
                            Triangles = false,
                            Colour = OsuColour.Gray(0.33f),
                            Scale = new Vector2(0.08f),
                            Margin = new MarginPadding(50),
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        }
                    }
                }
            };
        }

        private void update()
        {
            if (beatmap == null)
            {
                panelContents.Clear();
                return;
            }

            var bpm = beatmap.BeatmapSet.OnlineInfo.BPM;
            var length = beatmap.OnlineInfo.Length;
            string extra = "";

            var ar = beatmap.BaseDifficulty.ApproachRate;
            if ((mods & LegacyMods.HardRock) > 0) extra = "*";

            if ((mods & LegacyMods.DoubleTime) > 0)
            {
                //ar *= 1.5f;
                bpm *= 1.5f;
                length /= 1.5f;
                extra = "*";
            }

            panelContents.Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = $"Length {length}s",
                    Margin = new MarginPadding { Horizontal = 15, Vertical = 5 },
                    Colour = OsuColour.Gray(0.33f),
                    Anchor = Anchor.TopLeft,
                    Origin = Anchor.TopLeft,
                },
                new OsuSpriteText
                {
                    Text = $"BPM {bpm:0.#}",
                    Margin = new MarginPadding { Horizontal = 15, Vertical = 5 },
                    Colour = OsuColour.Gray(0.33f),
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft
                },
                new OsuSpriteText
                {
                    Text = $"CS{beatmap.BaseDifficulty.CircleSize:0.#} / AR {ar:0.#}{extra}",
                    Margin = new MarginPadding { Horizontal = 15, Vertical = 5 },
                    Colour = OsuColour.Gray(0.33f),
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight
                },
                new OsuSpriteText
                {
                    Text = $"Star Rating {beatmap.StarDifficulty:0.#}{extra}",
                    Margin = new MarginPadding { Horizontal = 15, Vertical = 5 },
                    Colour = OsuColour.Gray(0.33f),
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight
                },
                new TournamentBeatmapPanel(beatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        }
    }
}
