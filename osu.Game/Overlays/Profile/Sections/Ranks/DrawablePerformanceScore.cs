// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawableWeightedScore : DrawableTotalScore
    {
        private readonly double weight;

        public DrawableWeightedScore(ScoreInfo score, double weight)
            : base(score)
        {
            this.weight = weight;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            double pp = Score.PP ?? 0;

            Accuracy.Origin = Anchor.BottomLeft;
            Accuracy.Margin = new MarginPadding { Bottom = 2 };

            Accuracy.Add(new Container
            {
                AutoSizeAxes = Axes.Y,
                Width = 50,
                Child = new OsuSpriteText
                {
                    Text = $"{pp * weight:0}pp",
                    Font = OsuFont.GetFont(weight: FontWeight.Bold, italics: true),
                }
            });
            InfoContainer.Add(new OsuSpriteText
            {
                Anchor = Anchor.CentreLeft,
                Origin = Anchor.TopLeft,
                Text = $"weighted {weight:P0}",
                Font = OsuFont.GetFont(size: 14, weight: FontWeight.SemiBold),
            });
        }
    }
}
