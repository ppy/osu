// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawablePerformanceScore : DrawableProfileScore
    {
        private readonly double? weight;

        public DrawablePerformanceScore(Score score, double? weight = null)
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
                TextSize = 18,
                Font = "Exo2.0-BoldItalic",
            });

            if (weight.HasValue)
            {
                RightFlowContainer.Add(new OsuSpriteText
                {
                    Text = $"weighted: {pp * weight:0}pp ({weight:P0})",
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Colour = colour.GrayA,
                    TextSize = 11,
                    Font = "Exo2.0-RegularItalic",
                });
            }
        }
    }
}
