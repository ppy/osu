// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Overlays.BeatmapSet.Scores
{
    public partial class TopScoreStatisticsSection : CompositeDrawable
    {
        private const float margin = 10;
        private const float top_columns_min_width = 64;
        private const float bottom_columns_min_width = 45;

        private readonly FontUsage smallFont = OsuFont.GetFont(size: 16);
        private readonly FontUsage largeFont = OsuFont.GetFont(size: 22, weight: FontWeight.Light);

        private readonly TotalScoreColumn totalScoreColumn;
        private readonly TextColumn accuracyColumn;
        private readonly TextColumn maxComboColumn;

        [Resolved]
        private OsuConfigManager config { get; set; } = null!;

        private readonly SoloScoreInfo score;
        private readonly APIBeatmap beatmap;

        public TopScoreStatisticsSection(SoloScoreInfo score, APIBeatmap beatmap, Ruleset ruleset)
        {
            this.score = score;
            this.beatmap = beatmap;

            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            TextColumn ppColumn;

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
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
                            totalScoreColumn = new TotalScoreColumn(BeatmapsetsStrings.ShowScoreboardHeadersScoreTotal, largeFont, top_columns_min_width)
                            {
                                Current = score.GetBindableTotalScoreString(config),
                            },
                            accuracyColumn = new TextColumn(BeatmapsetsStrings.ShowScoreboardHeadersAccuracy, largeFont, top_columns_min_width)
                            {
                                Text = score.Accuracy.FormatAccuracy(),
                            },
                            maxComboColumn = new TextColumn(BeatmapsetsStrings.ShowScoreboardHeadersCombo, largeFont, top_columns_min_width)
                            {
                                Text = score.MaxCombo.ToLocalisableString(@"0\x"),
                            }
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
                            new FillFlowContainer<InfoColumn>
                            {
                                AutoSizeAxes = Axes.Both,
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(margin, 0),
                                ChildrenEnumerable = score.GetStatisticsForDisplay(ruleset).Select(createStatisticsColumn),
                            },
                            ppColumn = new TextColumn(BeatmapsetsStrings.ShowScoreboardHeaderspp, smallFont, bottom_columns_min_width)
                            {
                                Alpha = beatmap.Status.GrantsPerformancePoints() ? 1 : 0,
                            },
                            new ModsInfoColumn
                            {
                                Mods = score.Mods.Select(m => m.ToMod(ruleset)),
                            },
                        }
                    },
                }
            };

            // cross-reference: https://github.com/ppy/osu-web/blob/a6afee076f4f68bb56dea0cb8f18db63651763a7/resources/js/scores/pp-value.tsx#L19-L39
            if (!score.Ranked || !score.Preserve || (score.PP == null && score.Processed))
            {
                ppColumn.Drawable = new SpriteTextWithTooltip
                {
                    Text = "-",
                    Font = smallFont,
                    TooltipText = ScoresStrings.StatusNoPp
                };
            }
            else if (score.PP is not double pp)
            {
                ppColumn.Drawable = new SpriteIconWithTooltip
                {
                    Icon = FontAwesome.Solid.Sync,
                    Size = new Vector2(smallFont.Size),
                    TooltipText = ScoresStrings.StatusProcessing,
                };
            }
            else
                ppColumn.Text = pp.ToLocalisableString(@"N0");
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            if (score != null)
            {
                totalScoreColumn.Current = score.GetBindableTotalScoreString(config);

                if (score.Accuracy == 1.0)
                    accuracyColumn.TextColour = colours.GreenLight;

                if (score.MaxCombo == beatmap.MaxCombo)
                    maxComboColumn.TextColour = colours.GreenLight;
            }
        }

        private TextColumn createStatisticsColumn(HitResultDisplayStatistic stat) => new TextColumn(stat.DisplayName, smallFont, bottom_columns_min_width)
        {
            Text = stat.MaxCount == null ? stat.Count.ToLocalisableString(@"N0") : (LocalisableString)$"{stat.Count}/{stat.MaxCount}"
        };

        private partial class InfoColumn : CompositeDrawable
        {
            private readonly Box separator;
            private readonly OsuSpriteText text;

            public InfoColumn(LocalisableString title, Drawable content, float? minWidth = null)
            {
                AutoSizeAxes = Axes.Both;
                Margin = new MarginPadding { Vertical = 5 };

                InternalChild = new GridContainer
                {
                    AutoSizeAxes = Axes.Both,
                    ColumnDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize, minSize: minWidth ?? 0)
                    },
                    RowDimensions = new[]
                    {
                        new Dimension(GridSizeMode.AutoSize),
                        new Dimension(GridSizeMode.Absolute, 2),
                        new Dimension(GridSizeMode.AutoSize)
                    },
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            text = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(size: 10, weight: FontWeight.Bold),
                                Text = title.ToUpper(),
                                // 2px padding bottom + 1px vertical to compensate for the additional spacing because of 1.25 line-height in osu-web
                                Padding = new MarginPadding { Top = 1, Bottom = 3 }
                            }
                        },
                        new Drawable[]
                        {
                            separator = new Box
                            {
                                Anchor = Anchor.TopLeft,
                                RelativeSizeAxes = Axes.X,
                                Height = 2,
                            },
                        },
                        new[]
                        {
                            // osu-web has 4px margin here but also uses 0.9 line-height, reducing margin to 2px seems like a good alternative to that
                            content.With(c => c.Margin = new MarginPadding { Top = 2 })
                        }
                    }
                };
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                text.Colour = colourProvider.Foreground1;
                separator.Colour = colourProvider.Background3;
            }
        }

        private partial class TextColumn : InfoColumn
        {
            private readonly OsuTextFlowContainer text;

            public LocalisableString Text
            {
                set => text.Text = value;
            }

            public Colour4 TextColour
            {
                set => text.Colour = value;
            }

            public Drawable Drawable
            {
                set
                {
                    text.Clear();
                    text.AddArbitraryDrawable(value);
                }
            }

            public TextColumn(LocalisableString title, FontUsage font, float? minWidth = null)
                : this(title, new OsuTextFlowContainer(t => t.Font = font)
                {
                    AutoSizeAxes = Axes.Both
                }, minWidth)
            {
            }

            private TextColumn(LocalisableString title, OsuTextFlowContainer text, float? minWidth = null)
                : base(title, text, minWidth)
            {
                this.text = text;
            }
        }

        private partial class TotalScoreColumn : TextColumn
        {
            private readonly BindableWithCurrent<string> current = new BindableWithCurrent<string>();

            public TotalScoreColumn(LocalisableString title, FontUsage font, float? minWidth = null)
                : base(title, font, minWidth)
            {
            }

            public Bindable<string> Current
            {
                get => current;
                set => current.Current = value;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                Current.BindValueChanged(_ => Text = current.Value, true);
            }
        }

        private partial class ModsInfoColumn : InfoColumn
        {
            private readonly FillFlowContainer modsContainer;

            public ModsInfoColumn()
                : this(new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(1),
                    Height = 18f
                })
            {
            }

            private ModsInfoColumn(FillFlowContainer modsContainer)
                : base(BeatmapsetsStrings.ShowScoreboardHeadersMods, modsContainer)
            {
                this.modsContainer = modsContainer;
            }

            public IEnumerable<Mod> Mods
            {
                set
                {
                    modsContainer.Clear();
                    modsContainer.Children = value.AsOrdered().Select(mod => new ModIcon(mod)
                    {
                        AutoSizeAxes = Axes.Both,
                        Scale = new Vector2(0.25f),
                    }).ToList();
                }
            }
        }
    }
}
