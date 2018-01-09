// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawableTotalScore : DrawableProfileScore
    {
        public DrawableTotalScore(Score score)
            : base(score)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RightFlowContainer.Add(new OsuSpriteText
            {
                Text = Score.TotalScore.ToString("#,###"),
                Anchor = Anchor.TopRight,
                Origin = Anchor.TopRight,
                TextSize = 18,
                Font = "Exo2.0-BoldItalic",
            });
        }
    }
}
