// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Screens.Play.Break
{
    public class BreakInfo : Container
    {
        public PercentageBreakInfoLine AccuracyDisplay;

        // Currently unused but may be revisited in a future design update (see https://github.com/ppy/osu/discussions/15185)
        // public BreakInfoLine<int> RankDisplay;
        public BreakInfoLine<ScoreRank> GradeDisplay;

        public BreakInfo()
        {
            AutoSizeAxes = Axes.Both;
            Child = new FillFlowContainer
            {
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(5),
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Text = "current progress".ToUpperInvariant(),
                        Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 15),
                    },
                    new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Origin = Anchor.TopCentre,
                        Anchor = Anchor.TopCentre,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            AccuracyDisplay = new PercentageBreakInfoLine("Accuracy"),

                            // See https://github.com/ppy/osu/discussions/15185
                            // RankDisplay = new BreakInfoLine<int>("Rank"),
                            GradeDisplay = new BreakInfoLine<ScoreRank>("Grade"),
                        },
                    }
                },
            };
        }
    }
}
