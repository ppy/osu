// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

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
        private TournamentBeatmapPanel panel;

        private float panelWidth => expanded ? 0.6f : 1;

        private const float main_width = 0.97f;
        private const float inner_panel_width = 0.7f;

        private bool expanded;

        public bool Expanded
        {
            get => expanded;
            set
            {
                expanded = value;
                panel?.ResizeWidthTo(panelWidth, 800, Easing.OutQuint);

                if (expanded)
                {
                    innerPanel.ResizeWidthTo(inner_panel_width, 800, Easing.OutQuint);
                    outerPanel.ResizeWidthTo(main_width, 800, Easing.OutQuint);
                }
                else
                {
                    innerPanel.ResizeWidthTo(1, 800, Easing.OutQuint);
                    outerPanel.ResizeWidthTo(0.25f, 800, Easing.OutQuint);
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
                    Width = main_width,
                    Height = TournamentBeatmapPanel.HEIGHT,
                    CornerRadius = TournamentBeatmapPanel.HEIGHT / 2,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = OsuColour.Gray(0.93f),
                        },
                        new OsuLogo
                        {
                            Triangles = false,
                            Colour = OsuColour.Gray(0.33f),
                            Scale = new Vector2(0.08f),
                            Margin = new MarginPadding(50),
                            Anchor = Anchor.CentreRight,
                            Origin = Anchor.CentreRight,
                        },
                        innerPanel = new Container
                        {
                            Masking = true,
                            CornerRadius = TournamentBeatmapPanel.HEIGHT / 2,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            Width = inner_panel_width,
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
                        }
                    }
                }
            };

            Expanded = true;
        }

        private void update()
        {
            if (beatmap == null)
            {
                panelContents.Clear();
                return;
            }

            var bpm = beatmap.BeatmapSet.OnlineInfo.BPM;
            var length = beatmap.Length;
            string hardRockExtra = "";
            string srExtra = "";

            //var ar = beatmap.BaseDifficulty.ApproachRate;
            if ((mods & LegacyMods.HardRock) > 0)
            {
                hardRockExtra = "*";
                srExtra = "*";
            }

            if ((mods & LegacyMods.DoubleTime) > 0)
            {
                //ar *= 1.5f;
                bpm *= 1.5f;
                length /= 1.5f;
                srExtra = "*";
            }

            panelContents.Children = new Drawable[]
            {
                new DiffPiece(("Length", TimeSpan.FromMilliseconds(length).ToString(@"mm\:ss")))
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.BottomLeft,
                },
                new DiffPiece(("BPM", $"{bpm:0.#}"))
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.TopLeft
                },
                new DiffPiece(
                    //("CS", $"{beatmap.BaseDifficulty.CircleSize:0.#}{hardRockExtra}"),
                    //("AR", $"{ar:0.#}{srExtra}"),
                    ("OD", $"{beatmap.BaseDifficulty.OverallDifficulty:0.#}{hardRockExtra}"),
                    ("HP", $"{beatmap.BaseDifficulty.DrainRate:0.#}{hardRockExtra}")
                )
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.BottomRight
                },
                new DiffPiece(("Star Rating", $"{beatmap.StarDifficulty:0.#}{srExtra}"))
                {
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.TopRight
                },
                panel = new TournamentBeatmapPanel(beatmap)
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Both,
                    Size = new Vector2(panelWidth, 1)
                }
            };
        }

        public class DiffPiece : TextFlowContainer
        {
            public DiffPiece(params (string heading, string content)[] tuples)
            {
                Margin = new MarginPadding { Horizontal = 15, Vertical = 1 };
                AutoSizeAxes = Axes.Both;

                void cp(SpriteText s, Color4 colour)
                {
                    s.Colour = colour;
                    s.Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 15);
                }

                for (var i = 0; i < tuples.Length; i++)
                {
                    var tuple = tuples[i];

                    if (i > 0)
                    {
                        AddText(" / ", s =>
                        {
                            cp(s, OsuColour.Gray(0.33f));
                            s.Spacing = new Vector2(-2, 0);
                        });
                    }

                    AddText(new OsuSpriteText { Text = tuple.heading }, s => cp(s, OsuColour.Gray(0.33f)));
                    AddText(" ", s => cp(s, OsuColour.Gray(0.33f)));
                    AddText(new OsuSpriteText { Text = tuple.content }, s => cp(s, OsuColour.Gray(0.5f)));
                }
            }
        }
    }
}
