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
    public abstract class DrawableScore : Container
    {
        protected readonly FillFlowContainer<OsuSpriteText> Stats;
        private readonly FillFlowContainer metadata;
        private readonly ModContainer modContainer;
        protected readonly Score Score;

        protected DrawableScore(Score score)
        {
            Score = score;

            Children = new Drawable[]
            {
                new DrawableRank(score.Rank)
                {
                    RelativeSizeAxes = Axes.Y,
                    Width = 60,
                    FillMode = FillMode.Fit,
                },
                Stats = new FillFlowContainer<OsuSpriteText>
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

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colour, LocalisationEngine locale, BeatmapSetOverlay beatmapSetOverlay)
        {
            Stats.Add(new OsuSpriteText
            {
                Text = $"accuracy: {Score.Accuracy:P2}",
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Colour = colour.GrayA,
                TextSize = 11,
                Font = "Exo2.0-RegularItalic",
                Depth = -1,
            });

            metadata.Add(new OsuHoverContainer
            {
                AutoSizeAxes = Axes.Both,
                Action = () =>
                {
                    if (Score.Beatmap.OnlineBeatmapSetID.HasValue) beatmapSetOverlay?.ShowBeatmapSet(Score.Beatmap.OnlineBeatmapSetID.Value);
                },
                Child = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        new OsuSpriteText
                        {
                            Current = locale.GetUnicodePreference(
                                $"{Score.Beatmap.Metadata.TitleUnicode ?? Score.Beatmap.Metadata.Title} [{Score.Beatmap.Version}] ",
                                $"{Score.Beatmap.Metadata.Title ?? Score.Beatmap.Metadata.TitleUnicode} [{Score.Beatmap.Version}] "
                            ),
                            TextSize = 15,
                            Font = "Exo2.0-SemiBoldItalic",
                        },
                        new OsuSpriteText
                        {
                            Current = locale.GetUnicodePreference(Score.Beatmap.Metadata.ArtistUnicode, Score.Beatmap.Metadata.Artist),
                            TextSize = 12,
                            Padding = new MarginPadding { Top = 3 },
                            Font = "Exo2.0-RegularItalic",
                        },
                    },
                },
            });

            foreach (Mod mod in Score.Mods)
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
