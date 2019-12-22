// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawablePerformanceScore : DrawableProfileScore
    {
        private readonly double? weight;

        public DrawablePerformanceScore(ScoreInfo score, double? weight = null)
            : base(score)
        {
            this.weight = weight;
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            double pp = Score.PP ?? 0;
            RightFlowContainer.Add(new OsuSpriteText
            {
                Text = $"{pp:0}pp",
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold, italics: true)
            });

            if (weight.HasValue)
            {
                RightFlowContainer.Add(new OsuSpriteText
                {
                    Text = $"实得: {pp * weight:0}pp (权重:{weight:P0})",
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = colour.GrayA,
                    Font = OsuFont.GetFont(size: 15, weight: FontWeight.Regular, italics: true)
                });
            }
        }
    }
}
