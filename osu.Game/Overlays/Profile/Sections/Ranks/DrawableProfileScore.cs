// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osu.Game.Beatmaps;
using osu.Framework.Localisation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public abstract class DrawableProfileScore : DrawableProfileRow
    {
        private readonly FillFlowContainer modsContainer;
        protected readonly ScoreInfo Score;

        protected DrawableProfileScore(ScoreInfo score)
        {
            Score = score;

            RelativeSizeAxes = Axes.X;
            Height = 60;
            Children = new Drawable[]
            {
                modsContainer = new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Both,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Spacing = new Vector2(1),
                    Margin = new MarginPadding { Right = 160 }
                }
            };
        }

        [BackgroundDependencyLoader(true)]
        private void load(OsuColour colour)
        {
            var text = new OsuSpriteText
            {
                Text = $"accuracy: {Score.Accuracy:P2}",
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Colour = colour.GrayA,
                Font = OsuFont.GetFont(size: 11, weight: FontWeight.Regular, italics: true)
            };

            RightFlowContainer.Insert(1, text);

            LeftFlowContainer.Add(new ProfileScoreBeatmapMetadataContainer(Score.Beatmap));
            LeftFlowContainer.Add(new DrawableDate(Score.Date));

            foreach (Mod mod in Score.Mods)
                modsContainer.Add(new ModIcon(mod) { Scale = new Vector2(0.5f) });
        }

        protected override Drawable CreateLeftVisual() => new UpdateableRank(Score.Rank)
        {
            RelativeSizeAxes = Axes.Y,
            Width = 60,
            FillMode = FillMode.Fit,
        };

        private class ProfileScoreBeatmapMetadataContainer : BeatmapMetadataContainer
        {
            public ProfileScoreBeatmapMetadataContainer(BeatmapInfo beatmap)
                : base(beatmap)
            {
            }

            protected override Drawable[] CreateText(BeatmapInfo beatmap) => new Drawable[]
            {
                new OsuSpriteText
                {
                    Text = new LocalisedString((
                        $"{beatmap.Metadata.TitleUnicode ?? beatmap.Metadata.Title} [{beatmap.Version}] ",
                        $"{beatmap.Metadata.Title ?? beatmap.Metadata.TitleUnicode} [{beatmap.Version}] ")),
                    Font = OsuFont.GetFont(size: 15, weight: FontWeight.SemiBold, italics: true)
                },
                new OsuSpriteText
                {
                    Text = new LocalisedString((beatmap.Metadata.ArtistUnicode, beatmap.Metadata.Artist)),
                    Padding = new MarginPadding { Top = 3 },
                    Font = OsuFont.GetFont(size: 12, weight: FontWeight.Regular, italics: true)
                },
            };
        }
    }
}
