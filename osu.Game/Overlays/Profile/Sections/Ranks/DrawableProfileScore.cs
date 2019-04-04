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

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public abstract class DrawableProfileScore : DrawableProfileRow
    {
        private readonly ScoreModsContainer modsContainer;
        protected readonly ScoreInfo Score;

        protected DrawableProfileScore(ScoreInfo score)
        {
            Score = score;

            RelativeSizeAxes = Axes.X;
            Height = 60;
            Children = new Drawable[]
            {
                modsContainer = new ScoreModsContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.CentreRight,
                    Origin = Anchor.CentreRight,
                    Width = 60,
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

            RightFlowContainer.Add(text);
            RightFlowContainer.SetLayoutPosition(text, 1);

            LeftFlowContainer.Add(new BeatmapMetadataContainer(Score.Beatmap));
            LeftFlowContainer.Add(new DrawableDate(Score.Date));

            foreach (Mod mod in Score.Mods)
                modsContainer.Add(new ModIcon(mod) { Scale = new Vector2(0.5f) });
        }

        protected override Drawable CreateLeftVisual() => new DrawableRank(Score.Rank)
        {
            RelativeSizeAxes = Axes.Y,
            Width = 60,
            FillMode = FillMode.Fit,
        };
    }
}
