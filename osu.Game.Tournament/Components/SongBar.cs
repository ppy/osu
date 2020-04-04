// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Legacy;
using osu.Game.Graphics;
using osu.Game.Rulesets;
using osu.Game.Screens.Menu;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Tournament.Components
{
    public class SongBar : CompositeDrawable
    {
        private BeatmapInfo beatmap;

        private const float height = 145;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; }

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

        private FillFlowContainer flow;

        private bool expanded;

        public bool Expanded
        {
            get => expanded;
            set
            {
                expanded = value;
                flow.Direction = expanded ? FillDirection.Full : FillDirection.Vertical;
            }
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChildren = new Drawable[]
            {
                flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    LayoutDuration = 500,
                    LayoutEasing = Easing.OutQuint,
                    Direction = FillDirection.Full,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                }
            };

            Expanded = true;
        }

        private void update()
        {
            if (beatmap == null)
            {
                flow.Clear();
                return;
            }

            var bpm = beatmap.BeatmapSet.OnlineInfo.BPM;
            var length = beatmap.Length;
            string hardRockExtra = "";
            string srExtra = "";

            var ar = beatmap.BaseDifficulty.ApproachRate;

            if ((mods & LegacyMods.HardRock) > 0)
            {
                hardRockExtra = "*";
                srExtra = "*";
            }

            if ((mods & LegacyMods.DoubleTime) > 0)
            {
                // temporary local calculation (taken from OsuDifficultyCalculator)
                double preempt = (int)BeatmapDifficulty.DifficultyRange(ar, 1800, 1200, 450) / 1.5;
                ar = (float)(preempt > 1200 ? (1800 - preempt) / 120 : (1200 - preempt) / 150 + 5);

                bpm *= 1.5f;
                length /= 1.5f;
                srExtra = "*";
            }

            (string heading, string content)[] stats;

            switch (ruleset.Value.ID)
            {
                default:
                    stats = new (string heading, string content)[]
                    {
                        ("CS", $"{beatmap.BaseDifficulty.CircleSize:0.#}{hardRockExtra}"),
                        ("AR", $"{ar:0.#}{hardRockExtra}"),
                        ("OD", $"{beatmap.BaseDifficulty.OverallDifficulty:0.#}{hardRockExtra}"),
                    };
                    break;

                case 1:
                case 3:
                    stats = new (string heading, string content)[]
                    {
                        ("OD", $"{beatmap.BaseDifficulty.OverallDifficulty:0.#}{hardRockExtra}"),
                        ("HP", $"{beatmap.BaseDifficulty.DrainRate:0.#}{hardRockExtra}")
                    };
                    break;

                case 2:
                    stats = new (string heading, string content)[]
                    {
                        ("CS", $"{beatmap.BaseDifficulty.CircleSize:0.#}{hardRockExtra}"),
                        ("AR", $"{ar:0.#}"),
                    };
                    break;
            }

            flow.Children = new Drawable[]
            {
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    Height = height / 2,
                    Width = 0.5f,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,

                    Children = new Drawable[]
                    {
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,

                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new DiffPiece(stats),
                                            new DiffPiece(("Star Rating", $"{beatmap.StarDifficulty:0.#}{srExtra}"))
                                        }
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Direction = FillDirection.Vertical,
                                        Children = new Drawable[]
                                        {
                                            new DiffPiece(("Length", TimeSpan.FromMilliseconds(length).ToString(@"mm\:ss"))),
                                            new DiffPiece(("BPM", $"{bpm:0.#}"))
                                        }
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            new Box
                                            {
                                                Colour = Color4.Black,
                                                RelativeSizeAxes = Axes.Both,
                                                Alpha = 0.1f,
                                            },
                                            new OsuLogo
                                            {
                                                Triangles = false,
                                                Scale = new Vector2(0.08f),
                                                Margin = new MarginPadding(50),
                                                X = -10,
                                                Anchor = Anchor.CentreRight,
                                                Origin = Anchor.CentreRight,
                                            },
                                        }
                                    },
                                },
                            }
                        }
                    }
                },
                new TournamentBeatmapPanel(beatmap)
                {
                    RelativeSizeAxes = Axes.X,
                    Width = 0.5f,
                    Height = height / 2,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                }
            };
        }

        public class DiffPiece : TextFlowContainer
        {
            public DiffPiece(params (string heading, string content)[] tuples)
            {
                Margin = new MarginPadding { Horizontal = 15, Vertical = 1 };
                AutoSizeAxes = Axes.Both;

                static void cp(SpriteText s, bool bold)
                {
                    s.Font = OsuFont.Torus.With(weight: bold ? FontWeight.Bold : FontWeight.Regular, size: 15);
                }

                for (var i = 0; i < tuples.Length; i++)
                {
                    var (heading, content) = tuples[i];

                    if (i > 0)
                    {
                        AddText(" / ", s =>
                        {
                            cp(s, false);
                            s.Spacing = new Vector2(-2, 0);
                        });
                    }

                    AddText(new TournamentSpriteText { Text = heading }, s => cp(s, false));
                    AddText(" ", s => cp(s, false));
                    AddText(new TournamentSpriteText { Text = content }, s => cp(s, true));
                }
            }
        }
    }
}
