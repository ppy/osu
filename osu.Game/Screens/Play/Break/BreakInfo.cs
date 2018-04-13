// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Rulesets.Scoring;
using OpenTK;

namespace osu.Game.Screens.Play.Break
{
    public class BreakInfo : Container
    {
        public PercentageBreakInfoLine AccuracyDisplay;
        public BreakInfoLine<int> RankDisplay;
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
                        Text = "current progress".ToUpper(),
                        TextSize = 15,
                        Font = "Exo2.0-Black",
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
                            RankDisplay = new BreakInfoLine<int>("Rank"),
                            GradeDisplay = new BreakInfoLine<ScoreRank>("Grade"),
                        },
                    }
                },
            };
        }
    }
}
