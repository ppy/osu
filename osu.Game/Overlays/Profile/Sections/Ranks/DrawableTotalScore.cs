// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;

namespace osu.Game.Overlays.Profile.Sections.Ranks
{
    public class DrawableTotalScore : DrawableProfileScore
    {
        public DrawableTotalScore(ScoreInfo score)
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
                Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold, italics: true)
            });
        }
    }
}
