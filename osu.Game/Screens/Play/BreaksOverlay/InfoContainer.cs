// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Screens.Play.BreaksOverlay
{
    public class InfoContainer : FillFlowContainer
    {
        public PercentageInfoLine AccuracyDisplay;
        public InfoLine<int> RankDisplay;
        public InfoLine<Grade> GradeDisplay;

        public InfoContainer()
        {
            AutoSizeAxes = Axes.Both;
            Alpha = 0;
            Direction = FillDirection.Vertical;
            Spacing = new Vector2(5);
            Children = new Drawable[]
            {
                new OsuSpriteText
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    Text = @"current progress".ToUpper(),
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
                        AccuracyDisplay = new PercentageInfoLine(@"Accuracy"),
                        RankDisplay = new InfoLine<int>(@"Rank", @"#"),
                        GradeDisplay = new InfoLine<Grade>(@"Grade"),
                    },
                }
            };
        }
    }
}
