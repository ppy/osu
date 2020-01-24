// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public class TopScoreStatisticsSection : CompositeDrawable
    {
        private const float margin = 10;

        private readonly FontUsage smallFont = OsuFont.GetFont(size: 20);
        private readonly FontUsage largeFont = OsuFont.GetFont(size: 25);

        private readonly TextColumn totalScoreColumn;
        private readonly TextColumn accuracyColumn;
        private readonly TextColumn maxComboColumn;
        private readonly TextColumn ppColumn;

        private readonly FillFlowContainer<InfoColumn> statisticsColumns;
        private readonly ModsInfoColumn modsColumn;

        public TopScoreStatisticsSection()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10, 0),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(margin, 0),
                        Children = new Drawable[]
                        {
                            statisticsColumns = new FillFlowContainer<InfoColumn>
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(margin, 0),
                            },
                            ppColumn = new TextColumn("pp", smallFont),
                            modsColumn = new ModsInfoColumn(),
                        }
                    },
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Horizontal,
                        Spacing = new Vector2(margin, 0),
                        Children = new Drawable[]
                        {
                            totalScoreColumn = new TextColumn("total score", largeFont),
                            accuracyColumn = new TextColumn("accuracy", largeFont),
                            maxComboColumn = new TextColumn("max combo", largeFont)
                        }
                    },
                }
            };
        }

        /// <summary>
        /// Sets the score to be displayed.
        /// </summary>
        public ScoreInfo Score
        {
            set
            {
                totalScoreColumn.Text = $@"{value.TotalScore:N0}";
                accuracyColumn.Text = $@"{value.Accuracy:P2}";
                maxComboColumn.Text = $@"{value.MaxCombo:N0}x";
                ppColumn.Text = $@"{value.PP:N0}";

                statisticsColumns.ChildrenEnumerable = value.Statistics.Select(kvp => createStatisticsColumn(kvp.Key, kvp.Value));
                modsColumn.Mods = value.Mods;
            }
        }

        private TextColumn createStatisticsColumn(HitResult hitResult, int count) => new TextColumn(hitResult.GetDescription(), smallFont)
        {
            Text = count.ToString()
        };

        private class InfoColumn : CompositeDrawable
        {
            private readonly Box separator;

            public InfoColumn(string title, Drawable content)
            {
                AutoSizeAxes = Axes.Both;

                InternalChild = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 2),
                    Children = new[]
                    {
                        new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12, weight: FontWeight.Black),
                            Text = title.ToUpper()
                        },
                        separator = new Box
                        {
                            RelativeSizeAxes = Axes.X,
                            Height = 2
                        },
                        content
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                separator.Colour = colours.Gray5;
            }
        }

        private class TextColumn : InfoColumn
        {
            private readonly SpriteText text;

            public TextColumn(string title, FontUsage font)
                : this(title, new OsuSpriteText { Font = font })
            {
            }

            private TextColumn(string title, SpriteText text)
                : base(title, text)
            {
                this.text = text;
            }

            public LocalisedString Text
            {
                set => text.Text = value;
            }
        }

        private class ModsInfoColumn : InfoColumn
        {
            private readonly FillFlowContainer modsContainer;

            public ModsInfoColumn()
                : this(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(1),
                })
            {
            }

            private ModsInfoColumn(FillFlowContainer modsContainer)
                : base("mods", modsContainer)
            {
                this.modsContainer = modsContainer;
            }

            public IEnumerable<Mod> Mods
            {
                set
                {
                    modsContainer.Clear();

                    foreach (Mod mod in value)
                    {
                        modsContainer.Add(new ModIcon(mod)
                        {
                            AutoSizeAxes = Axes.Both,
                            Scale = new Vector2(0.3f),
                        });
                    }
                }
            }
        }
    }
}
