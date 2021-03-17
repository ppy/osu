// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;

namespace osu.Game.Screens.Ranking.Contracted
{
    public class ContractedPanelTopContent : CompositeDrawable
    {
        private readonly ScoreInfo score;

        public ContractedPanelTopContent(ScoreInfo score)
        {
            this.score = score;

            RelativeSizeAxes = Axes.Both;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChild = new OsuSpriteText
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                Y = 6,
                Text = score.Position != null ? $"#{score.Position}" : string.Empty,
                Font = OsuFont.GetFont(size: 18, weight: FontWeight.Bold)
            };
        }
    }
}
