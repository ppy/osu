// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.Select.Leaderboards;
using System.Linq;
using osu.Framework.Localisation;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawableScore : Container
    {
        private readonly FillFlowContainer<OsuSpriteText> stats;
        private readonly FillFlowContainer metadata;
        private readonly ModContainer modContainer;
        private readonly Score score;
        private readonly double? weight;

        public DrawableScore(Score score, double? weight = null)
        {
            this.score = score;
            this.weight = weight;

            Children = new Drawable[]
            {
                new DrawableRank(score.Rank)
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 60,
                    FillMode = FillMode.Fit,
                },
                stats = new FillFlowContainer<OsuSpriteText>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Direction = FillDirection.Vertical,
                },
                metadata = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Margin = new MarginPadding { Left = 70 },
                    Direction = FillDirection.Vertical,
                    Child = new OsuSpriteText
                    {
                        Text = score.Date.LocalDateTime.ToShortDateString(),
                        TextSize = 11,
                        Colour = OsuColour.Gray(0xAA),
                        Depth = -1,
                    },
                },
                modContainer = new ModContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Width = 60,
                    Margin = new MarginPadding { Right = 150 }
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, LocalisationEngine locale, BeatmapSetOverlay beatmapSetOverlay)
        {
            double pp = score.PP ?? 0;
            stats.Add(new OsuSpriteText
            {
                Text = $"{pp:0}pp",
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                TextSize = 18,
                Font = "Exo2.0-BoldItalic",
            });

            if (weight.HasValue)
            {
                stats.Add(new OsuSpriteText
                {
                    Text = $"weighted: {pp * weight:0}pp ({weight:P0})",
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = colour.GrayA,
                    TextSize = 11,
                    Font = "Exo2.0-RegularItalic",
                });
            }

            stats.Add(new OsuSpriteText
            {
                Text = $"accuracy: {score.Accuracy:P2}",
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Colour = colour.GrayA,
                TextSize = 11,
                Font = "Exo2.0-RegularItalic",
            });

            metadata.Add(new OsuHoverContainer
            {
                AutoSizeAxes = Axes.Both,
                Action = () =>
                {
                    if (score.Beatmap.OnlineBeatmapSetID.HasValue) beatmapSetOverlay.ShowBeatmapSet(score.Beatmap.OnlineBeatmapSetID.Value);
                },
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Current = locale.GetUnicodePreference(
                                $"{score.Beatmap.Metadata.TitleUnicode ?? score.Beatmap.Metadata.Title} [{score.Beatmap.Version}] ",
                                $"{score.Beatmap.Metadata.Title ?? score.Beatmap.Metadata.TitleUnicode} [{score.Beatmap.Version}] "
                            ),
                            TextSize = 15,
                            Font = "Exo2.0-SemiBoldItalic",
                        },
                        new OsuSpriteText
                        {
                            Current = locale.GetUnicodePreference(score.Beatmap.Metadata.ArtistUnicode, score.Beatmap.Metadata.Artist),
                            TextSize = 12,
                            Padding = new MarginPadding { Top = 3 },
                            Font = "Exo2.0-RegularItalic",
                        },
                    },
                },
            });

            foreach (Mod mod in score.Mods)
                modContainer.Add(new ModIcon(mod)
                {
                    AutoSizeAxes = Axes.Both,
                    Scale = new Vector2(0.5f),
                });
        }

        private class ModContainer : FlowContainer<ModIcon>
        {
            protected override IEnumerable<Vector2> ComputeLayoutPositions()
            {
                int count = FlowingChildren.Count();
                for (int i = 0; i < count; i++)
                    yield return new Vector2(DrawWidth * i * (count == 1 ? 0 : 1f / (count - 1)), 0);
            }
        }
    }
}
